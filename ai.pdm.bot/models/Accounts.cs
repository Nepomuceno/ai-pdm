using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ai.pdm.bot.models
{
    public interface IAccountsRepository
    {
        Task<List<AccountMapping>> MyAccounts(string name);
    }


    public class SearchResult
    {
        public string odatacontext { get; set; }
        public SearchAccount[] value { get; set; }
    }

    public class SearchAccount
    {
        public float searchscore { get; set; }
        public SearchHighlights searchhighlights { get; set; }
        public string MSCustomerGUID { get; set; }
        public string OrganizationName { get; set; }
        public object City { get; set; }
        public object Country { get; set; }
        public string TPID { get; set; }
        public bool IsEA { get; set; }
        public bool IsDirect { get; set; }
        public object Website { get; set; }
        public bool IsManagedAccount { get; set; }
        public int NumberOfSubscriptions { get; set; }
        public int TopXRank { get; set; }
        public string Classification { get; set; }
        public float TotalAzureRevenue { get; set; }
        public object SegmentName { get; set; }
        public object VerticalCategoryName { get; set; }
        public object IndustryName { get; set; }
        public bool IsAHUB { get; set; }
        public string SubsidiaryId { get; set; }
        public string TPIDList { get; set; }
        public float CXPScore { get; set; }
        public float ConsumptionRiskScore { get; set; }
    }

    public class SearchHighlights
    {
        public string OrganizationNameodatatype { get; set; }
        public string[] OrganizationName { get; set; }
    }

    public class AccountsRepository : IAccountsRepository
    {
        private List<Account> _accounts;
        private List<AccountMapping> _accountMappings;
        public AccountsRepository()
        {

            _accounts = JsonConvert.DeserializeObject<Request>(File.ReadAllText("./data/accounts.json")).body.accounts.ToList();
            _accountMappings = JsonConvert.DeserializeObject<List<AccountMapping>>(File.ReadAllText("./data/accountmapping.json"));
        }

        public async Task<List<AccountMapping>> MyAccounts(string name)
        {
            var result = new List<AccountMapping>();
            var imediate = _accounts.Where(a => a.responsible.Contains(name)).ToList();
            foreach (var item in imediate)
            {
                var account = await GetAcountDetail(item.name,item);
                if(account != null)
                {
                    result.Add(account);
                }
            }
            
            return result;
        }

        public async Task<AccountMapping> GetAcountDetail(string name, Account accountSource)
        {
            var account = _accountMappings.SingleOrDefault(a => a.CRMID == name);
            if(account == null)
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Origin", "https://cecustomers.microsoftonline.com");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("api-key", "DE8FC7CD7E9C6B38C58F57B94D2CFF14");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayIsImtpZCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayJ9.eyJhdWQiOiJodHRwczovL21zaXRhYWQub25taWNyb3NvZnQuY29tL2dyYWJwb3J0YWxhcGkiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTI3ODYxNjY5LCJuYmYiOjE1Mjc4NjE2NjksImV4cCI6MTUyNzg2NTU2OSwiYWNyIjoiMSIsImFpbyI6IkFVUUF1LzhIQUFBQU1iSUVmQTlrZWZsZnRQN3lKSUFiVDZ5NUZYSEFscVAyeS83aC9hUE81Sk42YUptSmdZanc4YzQ4Ry9NeE9iQm1VUjd0d2xXRW5Rem9paDM4SjZmV0JRPT0iLCJhbXIiOlsid2lhIiwibWZhIl0sImFwcGlkIjoiZTUzMDRhNzktNDk3MC00YzM2LWI2MTEtZjdhYTU2Y2Y1OWNkIiwiYXBwaWRhY3IiOiIxIiwiZV9leHAiOjI2MjgwMCwiZmFtaWx5X25hbWUiOiJOZXBvbXVjZW5vIiwiZ2l2ZW5fbmFtZSI6IkdhYnJpZWwiLCJpcGFkZHIiOiI4MS4xNDUuMTgzLjIxMSIsIm5hbWUiOiJHYWJyaWVsIE5lcG9tdWNlbm8iLCJvaWQiOiJkOThmZWIxNi0zZTM0LTQxMzctYThjOC03OWQ4ODM2MDllMDYiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTcyMTI1NDc2My00NjI2OTU4MDYtMTUzODg4MjI4MS0zOTY2NDM1Iiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwic3ViIjoiem9BVHA2d2MxUFhWMW5pMGZVeWpSN0JvVU9fS0RiTnZ4cHlGZVkweG8wayIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInVuaXF1ZV9uYW1lIjoiZ2FuZXBvbXVAbWljcm9zb2Z0LmNvbSIsInVwbiI6ImdhbmVwb211QG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJUQ3BvZDJ4ejFFUzZaT25rdkdBSkFBIiwidmVyIjoiMS4wIn0.nOtvdRY34l5-Gi65sqpBQJUfH54AsycYxLWrqNmKX26o72RGUWXbuZwZiHg4X6sNr58nyftizeZxuTHzqHFYOzUa0XwWcfIyJSOwnI2lJhAZc_N0vrBwMeEnCdL8dr-FjRn1gCgDjZ3hwQ2vNTYzjQSEPoX_UIESbC2IL4-3tvpe2FpWVJXxQTP81P75_UqDK55sPcyS6ndS-cx0l3dLDmXLgmbRebKS2paiCOOCvb7ucnjkQ4yMugmRf4cK9Zz_9FjSsTJ7aPiix2EDBOIIOMBxNO9Afg4IHgUGQlhQ3D2cLUnY_qd7LKf-9OvS-R4bW9mEixiiVhnWCGy-PVbJWA");
                var nameSearch = name.Replace("(United Kingdom)", "").Trim().Split(' ')[0];
                account = new AccountMapping();
                try
                {
                    var result = await client.GetStringAsync($"https://aiportal.search.windows.net/indexes/customerprofile20180601000228/docs?api-version=2016-09-01&searchMode=all&search={nameSearch}*&searchFields=MSCustomerGUID%2COrganizationName%2CEnrollmentNumber%2CTPID%2CCloudCustomerIds%2CSubscriptionIds%2CBillableAccountNumber%2CTPNames%2CPreferredAnsiExternalNames%2CPCNs%2COrgNames%2CTPIDList&highlight=MSCustomerGUID%2COrganizationName&%24select=MSCustomerGUID%2COrganizationName%2CCity%2CCountry%2CTPID%2CIsEA%2CIsDirect%2CWebsite%2CIsManagedAccount%2CNumberOfSubscriptions%2CTopXRank%2CClassification%2CTotalAzureRevenue%2CSegmentName%2CVerticalCategoryName%2CIndustryName%2CIsAHUB%2CSubsidiaryId%2CTPIDList%2CCXPScore%2CConsumptionRiskScore&%24top=15&%24orderby=IsEA%20desc%2CNumberOfSubscriptions%20desc%2CTopXRank%2COrganizationName&_=1527859949333");
                    var searchResult = JsonConvert.DeserializeObject<SearchResult>(result);
                    account.CSEID = searchResult.value[0].MSCustomerGUID;
                    account.SearchAccount = searchResult.value[0];
                }
                catch
                {

                }
                account.CRMID = name;
                account.Account = accountSource;
                _accountMappings.Add(account);
                File.WriteAllText("./data/accountmapping.json", JsonConvert.SerializeObject(_accountMappings));
            }
            return account;
        }
    }

    public class AccountBase
    {
        [JsonProperty(PropertyName = "account")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "Modified On")]
        public string ModifiedOn { get; set; }
        [JsonProperty(PropertyName = "Account Name")]
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
