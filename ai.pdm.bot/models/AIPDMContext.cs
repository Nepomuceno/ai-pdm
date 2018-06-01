using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ai.pdm.bot.models
{
    public class AIPDMContext : TurnContextWrapper
    {
        /// <summary>
        /// AlarmBot recognized Intents for the incoming activity
        /// </summary>
        public IRecognizedIntents RecognizedIntents { get { return this.Services.Get<IRecognizedIntents>(); } }

        public AIPDMContext(ITurnContext context) : base(context)
        {
            
        }
    }
}
