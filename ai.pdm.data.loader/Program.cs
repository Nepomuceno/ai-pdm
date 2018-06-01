using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await GetToken());
            _accessToken = tokenResponse.access_token;
            await DoWork();
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
