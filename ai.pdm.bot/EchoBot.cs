using AdaptiveCards;
using ai.pdm.bot.models;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai.pdm.bot
{
    public class EchoBot : IBot
    {
        private readonly IAccountsRepository _account;

        public EchoBot(IAccountsRepository account)
        {
            _account = account;
        }
        public async Task OnTurn(ITurnContext context)
        {
            var pdmcontext = new AIPDMContext(context);
            IMessageActivity activity = context.Activity.CreateReply();

            switch (context.Activity.Type)
            {
                // Note: This Sample is soon to be deleted. I've added in some easy testing here
                // for now, although we do need a better solution for testing the BotFrameworkConnector in the
                // near future

                case ActivityTypes.Message:
                    switch (pdmcontext.RecognizedIntents.TopIntent?.Name)
                    {
                        case "mypartners":
                            var myaccounts = await _account.MyAccounts(context.Activity.From.Name);
                            var card = new AdaptiveCard();
                            card.Body = new List<CardElement>();
                            card.Body.Add(new TextBlock()
                            {
                                Text = "Your partners are"
                            });
                            card.Body.AddRange(myaccounts.Select(s => new TextBlock()
                            {
                                Size = TextSize.Normal,
                                Color = s.SearchAccount?.ConsumptionRiskScore > 0.3 ? s.SearchAccount?.ConsumptionRiskScore > 0.6 ? TextColor.Warning : TextColor.Attention : TextColor.Default,
                                Text = s.Account?.name
                            }));
                            activity.Attachments.Add(new Attachment(AdaptiveCard.ContentType, content: card));
                            card.Actions.Add(new SubmitAction() { Title = "partner", DataJson = "{ Action:'Submit' }" });
                            await context.SendActivity(activity);
                            break;

                        case "mystarts":

                            var mystarts = await _account.MyAccounts(context.Activity.From.Name);
                            var realstars = mystarts.OrderByDescending(x => x.SearchAccount?.TopXRank).Take(5);
                            StringBuilder sb = new StringBuilder();
                            foreach (var star in realstars)
                            {
                                sb.AppendLine($"# {star.Account?.name}  / ${star.SearchAccount?.TopXRank} / {star.SearchAccount?.ConsumptionRiskScore.ToString("p")} / {star.SearchAccount?.Classification} \r\n");
                            }
                            await context.SendActivity($"# Your stars are:\r\n {sb}");

                            break;
                        default:
                            await context.SendActivity($"Sorry I couldn't understand you but I am learning every day!");
                            break;
                    }
                    /*await context.SendActivity($"You sent '{context.Activity.Text}'");
                    if (context.Activity.Text.ToLower() == "getmembers")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var members = await b.GetConversationMembers(context);
                        await context.SendActivity($"Members Found: {members.Count} ");
                        foreach (var m in members)
                        {
                            await context.SendActivity($"Member Id: {m.Id} Name: {m.Name} Role: {m.Role}");
                        }
                    }

                    if (context.Activity.Text.ToLower() == "getaccounts")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var members = await b.GetConversationMembers(context);
                        await context.SendActivity($"Members Found: {members.Count} ");
                        foreach (var m in members)
                        {
                            await context.SendActivity($"Member Id: {m.Id} Name: {m.Name} Role: {m.Role}");
                        }
                    }

                    else if (context.Activity.Text.ToLower() == "getactivitymembers")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var members = await b.GetActivityMembers(context);
                        await context.SendActivity($"Members Found: {members.Count} ");
                        foreach (var m in members)
                        {
                            await context.SendActivity($"Member Id: {m.Id} Name: {m.Name} Role: {m.Role}");
                        }
                    }
                    else if (context.Activity.Text.ToLower() == "getconversations")
                    {
                        BotFrameworkAdapter b = (BotFrameworkAdapter)context.Adapter;
                        var conversations = await b.GetConversations(context);
                        await context.SendActivity($"Conversations Found: {conversations.Conversations.Count} ");
                        foreach (var m in conversations.Conversations)
                        {
                            await context.SendActivity($"Conversation Id: {m.Id} Member Count: {m.Members.Count}");
                        }
                    }
                    */

                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the echo bot.");
                        }
                    }
                    break;
            }
        }
    }
}
