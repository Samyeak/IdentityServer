using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DiscoveryDocumentResponse disco = await DiscoverEndpoints();
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            TokenResponse tokenResponse = await RequestToken(disco.TokenEndpoint);
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);

            GetResource(tokenResponse.AccessToken);

            Console.ReadKey();
        }

        static private async Task<DiscoveryDocumentResponse> DiscoverEndpoints()
        {
            var identityUrl = "http://localhost:5000";
            var client = new HttpClient();
            return await client.GetDiscoveryDocumentAsync(identityUrl);
        }

        static private async Task<TokenResponse> RequestToken(string tokenEndpoint)
        {
            var client = new HttpClient();
            return await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });
        }

        static private async void GetResource(string accessToken)
        {
            string url = "http://localhost:5001/identity";
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(accessToken);

            HttpResponseMessage response = await apiClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
