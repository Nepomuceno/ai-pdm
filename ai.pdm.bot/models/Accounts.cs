using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ai.pdm.bot.models
{
    public interface IAccountsRepository
    {
        List<Account> MyAccounts(string name);
    }
    public class AccountsRepository : IAccountsRepository
    {
        private List<Account> _accounts;
        public AccountsRepository()
        {
            _accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText("./data/convertcsv.json"));
        }

        public List<Account> MyAccounts(string name)
        {
            var result = _accounts.Where(a => a.PDMContacts == name || a.PTS.TEContacts == name).ToList();
            return result;
        }
    }

    public class Account
    {
        [JsonProperty(PropertyName ="account")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "Modified On")]
        public string ModifiedOn { get; set; }
        public string AccountName { get; set; }
        [JsonProperty(PropertyName = "OCP CRM ID")]
        public string OCPCRMID { get; set; }
        [JsonProperty(PropertyName = "PartnerOne Name")]
        public string PartnerOneName { get; set; }
        [JsonProperty(PropertyName = "Partner Specialization")]
        public string PartnerSpecialization { get; set; }
        public string Tier { get; set; }
        public string Businessunit { get; set; }
        [JsonProperty(PropertyName = "PDM Contacts")]
        public string PDMContacts { get; set; }
        public PTS PTS { get; set; }
        [JsonProperty(PropertyName = "Account Status")]
        public string AccountStatus { get; set; }
    }

    public class PTS
    {
        [JsonProperty(PropertyName = "TE Contacts")]
        public string TEContacts { get; set; }
    }

}
