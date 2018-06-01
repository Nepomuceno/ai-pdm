using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ai.pdm.data.loader
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendAsJsonAsync<T>(this HttpClient client, HttpMethod method, string requestUri, T value)
        {
            var content = value.GetType().Name.Equals("JObject") ?
                value.ToString() :
                JsonConvert.SerializeObject(value, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });

            HttpRequestMessage request = new HttpRequestMessage(method, requestUri) { Content = new StringContent(content) };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return client.SendAsync(request);
        }
    }
    [DataContract]
    public class TokenResponse
    {
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string ext_expires_in { get; set; }
        [DataMember]
        public string expires_on { get; set; }
        [DataMember]
        public string not_before { get; set; }
        [DataMember]
        public string resource { get; set; }
        [DataMember]
        public string access_token { get; set; }
    }
    class Program
    {
        //Azure Application / Client ID
        private const string ClientId = "4e1201c3-5d77-46e2-aedb-0a1d5c191ec9";
        //Azure Application Client Key / Secret
        private const string ClientSecret = "a5wqE6gCqOUkKPvCYL9iItdV/RedRhXqMuMqj3YcpbU=";

        //Resource / CRM Url
        private const string Resource = "https://ocp.crm.dynamics.com/";

        //Guid is your Azure Active Directory Tenant Id
        private const string Authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/token";

        private static string _accessToken;

        static async Task Main(string[] args)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Origin", "https://cecustomers.microsoftonline.com");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("api-key", "DE8FC7CD7E9C6B38C58F57B94D2CFF14");
            /*
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetAdvisorData/msCustomerGuid/fdbcabce-d545-44b4-b143-706264f6bc1f?_=1527850278066
             * https://cecustomersapi.trafficmanager.net/customerapi/GetACRDetails/msCustomerGuid/fdbcabce-d545-44b4-b143-706264f6bc1f?_=1527850278065
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetSearchIndexProfileInformation?_=1527850278058
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetRevenueSummaries/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f?_=1527850278068
             * https://aiportal.search.windows.net/indexes/customerprofile20180601000228/docs?api-version=2016-09-01&searchMode=all&%24select=MSCustomerGUID%2COrganizationName%2CTPID%2CCity%2CCountry%2CIsEA%2CIsDirect%2CWebsite%2CIsManagedAccount%2CNumberOfSubscriptions%2CTopXRank%2CCrmAccountId%2CDUNSNumber%2CClassification%2CTotalAzureRevenue%2CSegmentName%2CVerticalCategoryName%2CIndustryName%2CIsAHUB%2CEnrollmentNumber%2CSubsidiaryId%2CTPIDList%2CCXPScore%2CConsumptionRiskScore&%24top=1&%24orderby=IsEA%20desc%2CNumberOfSubscriptions%20desc%2CTopXRank%2COrganizationName&%24filter=MSCustomerGUID%20eq%20%27fdbcabce-d545-44b4-b143-706264f6bc1f%27&_=1527850278063
             * https://aiportal.search.windows.net/indexes/salesblockerv1/docs?api-version=2016-09-01&searchMode=all&%24top=10&search=Axel%20Springer%20SE%2051831483%20axelspringer%20%2B%20Azure%20%7C%20Axel%20Springer%20SE%2051831483%20axelspringer&_=1527850278072
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetCustomerLifetime/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f/lifetimetype/1/granularity/30/start/2015-06-01/end/2018-06-01?_=1527850278082
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetCustomerSubscriptionSummary/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f?_=1527850593085
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetCustomerLifetime/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f/lifetimetype/2/granularity/30/start/2015-06-01/end/2018-06-01?_=1527850593088
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetComputeUsage/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f/granularity/Daily/start/2018-05-02/end/2018-06-01?_=1527850593091
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetModelDetails/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f/start/2018-05-01/end/2018-06-01?_=1527850593102
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetCloudCustomers/msCustomerGuid/fdbcabce-d545-44b4-b143-706264f6bc1f/startDate/2015-06-01?_=1527850593105
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetRankOfCustomer/IdentifierType/MSCustomer/identifierValue/fdbcabce-d545-44b4-b143-706264f6bc1f/startDate/2015-06-01/endDate/2018-06-01?_=1527850593111
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetServiceStartDate/customerId/fdbcabce-d545-44b4-b143-706264f6bc1f?_=1527850593112
             * https://cecustomersapi.trafficmanager.net/dashboardapi/GetStaticMappings?_=1527850593060
             */
            
            client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayIsImtpZCI6ImlCakwxUmNxemhpeTRmcHhJeGRacW9oTTJZayJ9.eyJhdWQiOiJodHRwczovL21zaXRhYWQub25taWNyb3NvZnQuY29tL2dyYWJwb3J0YWxhcGkiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTI3ODU5NjA1LCJuYmYiOjE1Mjc4NTk2MDUsImV4cCI6MTUyNzg2MzUwNSwiYWNyIjoiMSIsImFpbyI6IkFVUUF1LzhIQUFBQUVMWWZld2pkNGU2cGdSc2xOVGRaN0VIVGJhRElEdFVuR0VtT2dlVWM0b2xSWDRGUDczdmEyUC81MFJnSktlWERtVUNGNlp6YzNxZFV1MHhGeXkzTm13PT0iLCJhbXIiOlsid2lhIiwibWZhIl0sImFwcGlkIjoiZTUzMDRhNzktNDk3MC00YzM2LWI2MTEtZjdhYTU2Y2Y1OWNkIiwiYXBwaWRhY3IiOiIxIiwiZV9leHAiOjI2MjgwMCwiZmFtaWx5X25hbWUiOiJOZXBvbXVjZW5vIiwiZ2l2ZW5fbmFtZSI6IkdhYnJpZWwiLCJpcGFkZHIiOiI4MS4xNDUuMTgzLjIxMSIsIm5hbWUiOiJHYWJyaWVsIE5lcG9tdWNlbm8iLCJvaWQiOiJkOThmZWIxNi0zZTM0LTQxMzctYThjOC03OWQ4ODM2MDllMDYiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMTcyMTI1NDc2My00NjI2OTU4MDYtMTUzODg4MjI4MS0zOTY2NDM1Iiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwic3ViIjoiem9BVHA2d2MxUFhWMW5pMGZVeWpSN0JvVU9fS0RiTnZ4cHlGZVkweG8wayIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInVuaXF1ZV9uYW1lIjoiZ2FuZXBvbXVAbWljcm9zb2Z0LmNvbSIsInVwbiI6ImdhbmVwb211QG1pY3Jvc29mdC5jb20iLCJ1dGkiOiI0VC1HZ3lHcGprdVhIYTc5OUZrS0FBIiwidmVyIjoiMS4wIn0.oMzJ3XW_iF0B2WwILYCv_vRqkQamlGhRK15TcU4o4MKemVngFkpV8kKsrfoDZDGDl9_sPUFdZj3BTAHGIvodDkOBQLuoD2gvoK8laiHaoG8Ed9E5ZC247gE_K-V1cEsYxU2Gjcs_ws3gYYr3lKw-mtoG9c4G6YqE4UXVxPVy5vb0Lq3lWthM6cqvwZBtQD05_c0RwraRn8rQWyFTPuC8g3l_9npx4yf3FXIpQyhnJNheBBAz81IS5xjJIhGwQCbzi_uArbt2nH4Gb3F5zqPpbNmjTZkyJI2JYVpzsorC2w_9H8nvGsLWT2LtWSTYWXFKH2mJ0oIoqC6JOgOeFRO13Q");
            string name = "guestline";
            var responde = await client.GetStringAsync("https://cecustomersapi.trafficmanager.net/dashboardapi/GetStaticMappings?_=1527850593060");
            responde = await client.GetStringAsync($"https://aiportal.search.windows.net/indexes/customerprofile20180601000228/docs?api-version=2016-09-01&searchMode=all&search={name}*&searchFields=MSCustomerGUID%2COrganizationName%2CEnrollmentNumber%2CTPID%2CCloudCustomerIds%2CSubscriptionIds%2CBillableAccountNumber%2CTPNames%2CPreferredAnsiExternalNames%2CPCNs%2COrgNames%2CTPIDList&highlight=MSCustomerGUID%2COrganizationName&%24select=MSCustomerGUID%2COrganizationName%2CCity%2CCountry%2CTPID%2CIsEA%2CIsDirect%2CWebsite%2CIsManagedAccount%2CNumberOfSubscriptions%2CTopXRank%2CClassification%2CTotalAzureRevenue%2CSegmentName%2CVerticalCategoryName%2CIndustryName%2CIsAHUB%2CSubsidiaryId%2CTPIDList%2CCXPScore%2CConsumptionRiskScore&%24top=15&%24orderby=IsEA%20desc%2CNumberOfSubscriptions%20desc%2CTopXRank%2COrganizationName&_=1527859949333");
            responde = await client.GetStringAsync("https://aiportal.search.windows.net/indexes/customerprofile20180601000228/docs?api-version=2016-09-01&searchMode=all&search=Gui*&searchFields=MSCustomerGUID%2COrganizationName%2CEnrollmentNumber%2CTPID%2CCloudCustomerIds%2CSubscriptionIds%2CBillableAccountNumber%2CTPNames%2CPreferredAnsiExternalNames%2CPCNs%2COrgNames%2CTPIDList&highlight=MSCustomerGUID%2COrganizationName&%24select=MSCustomerGUID%2COrganizationName%2CCity%2CCountry%2CTPID%2CIsEA%2CIsDirect%2CWebsite%2CIsManagedAccount%2CNumberOfSubscriptions%2CTopXRank%2CClassification%2CTotalAzureRevenue%2CSegmentName%2CVerticalCategoryName%2CIndustryName%2CIsAHUB%2CSubsidiaryId%2CTPIDList%2CCXPScore%2CConsumptionRiskScore&%24top=15&%24orderby=IsEA%20desc%2CNumberOfSubscriptions%20desc%2CTopXRank%2COrganizationName&_=1527845067368");
            //TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await GetToken());
            //_accessToken = tokenResponse.access_token;
            //await DoWork();
        }

        private static async Task<string> GetToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("resource", Resource),
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                HttpResponseMessage response = await httpClient.PostAsync(Authority, formContent);

                return !response.IsSuccessStatusCode ? null
                    : response.Content.ReadAsStringAsync().Result;
            }
        }

        private static async Task DoWork()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Resource);
                httpClient.Timeout = new TimeSpan(0, 2, 0);
                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);

                //Unbound Function
                HttpResponseMessage whoAmIResponse =
                    await httpClient.GetAsync("api/data/v9.0/WhoAmI");
                Guid userId;
                var whoResponse = await whoAmIResponse.Content.ReadAsStringAsync();
                if (whoAmIResponse.IsSuccessStatusCode)
                {
                    JObject jWhoAmIResponse =
                        JObject.Parse(whoAmIResponse.Content.ReadAsStringAsync().Result);
                    userId = (Guid)jWhoAmIResponse["UserId"];
                    Console.WriteLine("WhoAmI " + userId);
                }
                else
                    return;

                //Retrieve 
                HttpResponseMessage retrieveResponse =
                    await httpClient.GetAsync("api/data/v8.2/systemusers(" +
                                              userId + ")?$select=fullname");
                if (retrieveResponse.IsSuccessStatusCode)
                {
                    JObject jRetrieveResponse =
                        JObject.Parse(retrieveResponse.Content.ReadAsStringAsync().Result);
                    string fullname = jRetrieveResponse["fullname"].ToString();
                    Console.WriteLine("Fullname " + fullname);
                }
                else
                    return;

                //Create
                JObject newAccount = new JObject
                {
                    {"name", "CSharp Test"},
                    {"telephone1", "111-888-7777"}
                };

                HttpResponseMessage createResponse =
                    await httpClient.SendAsJsonAsync(HttpMethod.Post, "api/data/v8.2/accounts", newAccount);

                Guid accountId = new Guid();
                if (createResponse.IsSuccessStatusCode)
                {
                    string accountUri = createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault();
                    if (accountUri != null)
                        accountId = Guid.Parse(accountUri.Split('(', ')')[1]);
                    Console.WriteLine("Account '{0}' created.", newAccount["name"]);
                }
                else
                    return;

                //Update 
                newAccount.Add("fax", "123-456-7890");

                HttpResponseMessage updateResponse =
                    await httpClient.SendAsJsonAsync(new HttpMethod("PATCH"), "api/data/v8.2/accounts(" + accountId + ")", newAccount);
                if (updateResponse.IsSuccessStatusCode)
                    Console.WriteLine("Account '{0}' updated", newAccount["name"]);

                //Delete
                HttpResponseMessage deleteResponse =
                    await httpClient.DeleteAsync("api/data/v8.2/accounts(" + accountId + ")");

                if (deleteResponse.IsSuccessStatusCode)
                    Console.WriteLine("Account '{0}' deleted", newAccount["name"]);

                Console.ReadLine();
            }
        }
    }
}
