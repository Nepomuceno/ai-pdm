// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;

namespace ai.pdm.bot
{
    public class Entity
    {
        public string GroupName { get; set; }
        public double Score { get; set; }

        public object Value { get; set; }
    }
    public interface IRecognizedIntents
    {
        Intent TopIntent { get; set; }
        IList<Intent> Intents { get; set; }
    }
    public class IntentRecognition : IRecognizedIntents
    {
        public IntentRecognition()
        {
        }

        public Intent TopIntent { get; set; }
        public IList<Intent> Intents { get; set; } = new Intent[0];
    }

    public class Intent
    {
        public string Name { get; set; }
        public double Score { get; set; }

        public IList<Entity> Entities { get; } = new List<Entity>();
    }


    public class IntentRecognizerMiddleware : IMiddleware
    {
        public delegate Task<Boolean> IntentDisabler(ITurnContext context);
        public delegate Task<IList<Intent>> IntentRecognizer(ITurnContext context);
        public delegate Task IntentResultMutator(ITurnContext context, IList<Intent> intents);

        private readonly LinkedList<IntentDisabler> _intentDisablers = new LinkedList<IntentDisabler>();
        private readonly LinkedList<IntentRecognizer> _intentRecognizers = new LinkedList<IntentRecognizer>();
        private readonly LinkedList<IntentResultMutator> _intentResultMutators = new LinkedList<IntentResultMutator>();

        /// <summary>
        /// method for accessing recognized intents added by middleware to context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IRecognizedIntents Get(ITurnContext context) { return context.Services.Get<IRecognizedIntents>(); }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);

            var intents = await this.Recognize(context);
            var result = new IntentRecognition();
            if (intents.Count != 0)
            {
                result.Intents = intents;
                var topIntent = FindTopIntent(intents);
                if (topIntent.Score > 0.0)
                {
                    result.TopIntent = topIntent;
                }
            }
            context.Services.Add((IRecognizedIntents)result);
            await next().ConfigureAwait(false);
        }

        public async Task<IList<Intent>> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);

            bool isEnabled = await IsRecognizerEnabled(context).ConfigureAwait(false);
            if (isEnabled)
            {
                var allRecognizedIntents = await RunRecognizer(context).ConfigureAwait(false);
                await RunFilters(context, allRecognizedIntents);
                return allRecognizedIntents;
            }
            else
            {
                return new List<Intent>();
            }
        }

        private async Task<IList<Intent>> RunRecognizer(ITurnContext context)
        {
            List<Intent> allRecognizedIntents = new List<Intent>();

            foreach (var recognizer in _intentRecognizers)
            {
                IList<Intent> intents = await recognizer(context).ConfigureAwait(false);
                if (intents != null && intents.Count > 0)
                {
                    allRecognizedIntents.AddRange(intents);
                }
            }

            return allRecognizedIntents;
        }

        private async Task<Boolean> IsRecognizerEnabled(ITurnContext context)
        {
            foreach (var userCode in _intentDisablers)
            {
                bool isEnabled = await userCode(context).ConfigureAwait(false);
                if (isEnabled == false)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task RunFilters(ITurnContext context, IList<Intent> intents)
        {
            foreach (var filter in _intentResultMutators)
            {
                await filter(context, intents);
            }
        }

        /// <summary>
        /// An IntentDisabler that's registered here will fire BEFORE the intent recognizer code
        /// is run, and will have the oppertunity to prevent the recognizer from running. 
        /// 
        /// As soon as one function returns 'Do Not Run' no further methods will be called. 
        /// 
        /// Enabled/Disabled methods that are registered are run in the order registered. 
        /// </summary>        
        public IntentRecognizerMiddleware OnEnabled(IntentDisabler preCondition)
        {
            if (preCondition == null)
                throw new ArgumentNullException(nameof(preCondition));

            _intentDisablers.AddLast(preCondition);

            return this;
        }

        /// <summary>
        /// Recognizer methods are run in the ordered registered.
        /// </summary>
        public IntentRecognizerMiddleware OnRecognize(IntentRecognizer recognizer)
        {
            if (recognizer == null)
                throw new ArgumentNullException(nameof(recognizer));

            _intentRecognizers.AddLast(recognizer);

            return this;
        }

        /// <summary>
        /// Filter method are run in REVERSE order registered. That is, they are run from "last -> first". 
        /// </summary>
        public IntentRecognizerMiddleware OnFilter(IntentResultMutator postCondition)
        {
            if (postCondition == null)
                throw new ArgumentNullException(nameof(postCondition));

            _intentResultMutators.AddFirst(postCondition);

            return this;
        }

        public static Intent FindTopIntent(IList<Intent> intents)
        {
            if (intents == null)
                throw new ArgumentNullException(nameof(intents));

            var enumerator = intents.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new ArgumentException($"No Intents on '{nameof(intents)}'");

            var topIntent = enumerator.Current;
            var topScore = topIntent.Score;

            while (enumerator.MoveNext())
            {
                var currVal = enumerator.Current.Score;

                if (currVal.CompareTo(topScore) > 0)
                {
                    topScore = currVal;
                    topIntent = enumerator.Current;
                }
            }
            return topIntent;
        }
        public static string CleanString(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();
        }

    }
    public class RegExpRecognizerSettings
    {
        /// <summary>
        /// Minimum score, on a scale from 0.0 to 1.0, that should be returned for a matched 
        /// expression.This defaults to a value of 0.0. 
        /// </summary>
        public double MinScore { get; set; } = 0.0;
    }

    public class RegExLocaleMap
    {
        private Dictionary<string, List<Regex>> _map = new Dictionary<string, List<Regex>>();
        private const string Default_Key = "*";

        public RegExLocaleMap()
        {
        }

        public RegExLocaleMap(List<Regex> items)
        {
            _map[Default_Key] = items;
        }

        public List<Regex> GetLocale(string locale)
        {
            if (_map.ContainsKey(locale))
                return _map[locale];
            else if (_map.ContainsKey(Default_Key))
                return _map[Default_Key];
            else
                return new List<Regex>();
        }

        public Dictionary<string, List<Regex>> Map
        {
            get { return _map; }
        }

    }

    public class RegExpRecognizerMiddleware : IntentRecognizerMiddleware
    {
        private RegExpRecognizerSettings _settings;
        private Dictionary<string, RegExLocaleMap> _intents = new Dictionary<string, RegExLocaleMap>();
        public const string DefaultEntityType = "string";

        public RegExpRecognizerMiddleware() : this(new RegExpRecognizerSettings() { MinScore = 0.0 })
        {
        }

        public RegExpRecognizerMiddleware(RegExpRecognizerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException("settings");
            if (_settings.MinScore < 0 || _settings.MinScore > 1.0)
            {
                throw new ArgumentException($"RegExpRecognizerMiddleware: a minScore of {_settings.MinScore} is out of range.");
            }

            this.OnRecognize(async (context) =>
            {
                IList<Intent> intents = new List<Intent>();
                string utterance = CleanString(context.Activity.Text);
                double minScore = _settings.MinScore;

                foreach (var name in _intents.Keys)
                {
                    var map = _intents[name];
                    List<Regex> expressions = GetExpressions(context, map);
                    Intent top = null;
                    foreach (Regex exp in expressions)
                    {
                        List<string> entityTypes = new List<string>();
                        Intent intent = Recognize(utterance, exp, entityTypes, minScore);
                        if (intent != null)
                        {
                            if (top == null)
                            {
                                top = intent;
                            }
                            else if (intent.Score > top.Score)
                            {
                                top = intent;
                            }
                        }

                        if (top != null)
                        {
                            top.Name = name;
                            intents.Add(top);
                        }
                    }
                }
                return intents;
            });
        }

        public RegExpRecognizerMiddleware AddIntent(string intentName, Regex regex)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");

            return AddIntent(intentName, new List<Regex> { regex });
        }
        public RegExpRecognizerMiddleware AddIntent(string intentName, List<Regex> regexList)
        {
            if (regexList == null)
                throw new ArgumentNullException("regexList");

            return AddIntent(intentName, new RegExLocaleMap(regexList));
        }

        public RegExpRecognizerMiddleware AddIntent(string intentName, RegExLocaleMap map)
        {
            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException("intentName");

            if (_intents.ContainsKey(intentName))
                throw new ArgumentException($"RegExpRecognizer: an intent name '{intentName}' already exists.");

            _intents[intentName] = map;

            return this;
        }
        private List<Regex> GetExpressions(ITurnContext context, RegExLocaleMap map)
        {

            var locale = string.IsNullOrWhiteSpace(context.Activity.Locale) ? "*" : context.Activity.Locale;
            var entry = map.GetLocale(locale);
            return entry;
        }

        public static Intent Recognize(string text, Regex expression, double minScore)
        {
            return Recognize(text, expression, new List<string>(), minScore);
        }
        public static Intent Recognize(string text, Regex expression, List<string> entityTypes, double minScore)
        {
            // Note: Not throwing here, as users enter whitespace all the time. 
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (expression == null)
                throw new ArgumentNullException("expression");

            if (entityTypes == null)
                throw new ArgumentNullException("entity Types");

            if (minScore < 0 || minScore > 1.0)
                throw new ArgumentOutOfRangeException($"RegExpRecognizer: a minScore of '{minScore}' is out of range for expression '{expression.ToString()}'");

            var match = expression.Match(text);
            //var matches = expression.Matches(text);
            if (match.Success)
            {
                double coverage = (double)match.Length / (double)text.Length;
                double score = minScore + ((1.0 - minScore) * coverage);

                Intent intent = new Intent()
                {
                    Name = expression.ToString(),
                    Score = score
                };

                for (int i = 0; i < match.Groups.Count; i++)
                {
                    if (i == 0)
                        continue;   // First one is always the entire capture, so just skip

                    string groupName = DefaultEntityType;
                    if (entityTypes.Count > 0)
                    {
                        // If the dev passed in group names, use them. 
                        groupName = (i - 1) < entityTypes.Count ? entityTypes[i - 1] : DefaultEntityType;
                    }
                    else
                    {
                        groupName = expression.GroupNameFromNumber(i);
                        if (string.IsNullOrEmpty(groupName))
                        {
                            groupName = DefaultEntityType;
                        }
                    }

                    Group g = match.Groups[i];
                    if (g.Success)
                    {
                        Entity newEntity = new Entity()
                        {
                            GroupName = groupName,
                            Score = 1.0,
                            Value = match.Groups[i].Value
                        };
                        intent.Entities.Add(newEntity);
                    }
                }

                return intent;
            }

            return null;
        }
    }
}