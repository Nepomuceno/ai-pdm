namespace ai.pdm.bot.models
{
    public class AccountMapping
    {
        public string CRMID { get; set; }
        public string CSEID { get; set; }

        public SearchAccount SearchAccount { get; set; }
        public Account Account { get; set; }


    }
}