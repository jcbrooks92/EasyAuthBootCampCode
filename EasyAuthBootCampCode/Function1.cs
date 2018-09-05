using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace EasyAuthBootCampCode
{
    public static class Function1
    {
        private static readonly HttpClient client = new HttpClient();
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            AuthenticationResult authenticationResult;
            string authority = "https://login.microsoftonline.com/microsoft.onmicrosoft.com/";
            var authenticationContext = new AuthenticationContext(authority);
            string clientSecret= "";
            string clientId = "";
            string resourceId = "";
            string accessToken = "";


            if (ConfigurationManager.AppSettings["clientSecret"] != null && ConfigurationManager.AppSettings["clientId"] != null && ConfigurationManager.AppSettings["resourceId"] != null)
            {
                clientSecret = ConfigurationManager.AppSettings["clientSecret"];
                clientId = ConfigurationManager.AppSettings["clientId"];
                resourceId = ConfigurationManager.AppSettings["resourceId"];
                log.Info("Using app settings for authentication.");
            }
            else 
            {
                log.Info("One of app settings clientSecret, resourceId or clientId are null while the other is not, please check your app settings. Using hardcoded values if this is running locally.");
                clientSecret = "";
                clientId = "";
                resourceId = "";

            }
            try
            {
                var clientCredential = new ClientCredential(clientId, clientSecret);
           
                //Resource you are getting an access token to, if you don’t specify this the call will fail due to an incorrect scope
                authenticationResult = await authenticationContext.AcquireTokenAsync("https://" + resourceId, clientCredential);
                accessToken = authenticationResult.AccessToken;
                log.Info("Bearer Token = " + accessToken);
                //End of getting the access token
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.ToString());
            }
            

            //Try to connect to management endpoint
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync("https://" + resourceId);
                var contents = await response.Content.ReadAsStringAsync();

                log.Info(response.ToString());
                //log.Info(contents);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.ToString());
            }

            return req.CreateErrorResponse(HttpStatusCode.OK, "Successfully Authenticated");
        }
    }
}
