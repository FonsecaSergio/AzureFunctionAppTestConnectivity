using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;

namespace FunctionAppTestConnection
{
    public static class Function_ConnectSQL_MSI
    {
        [FunctionName("Function_ConnectSQL_MSI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ServerName = req.Query["ServerName"];
            string DatabaseName = req.Query["DatabaseName"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            ServerName = ServerName ?? data?.ServerName;
            DatabaseName = DatabaseName ?? data?.DatabaseName;



            string ConnectionString = "Server=tcp:" + ServerName + ";Initial Catalog=" + DatabaseName + ";Persist Security Info=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string result = "NOT CONNECTED";

            SqlConnection sqlConnection = new SqlConnection();

            sqlConnection = new SqlConnection(ConnectionString);

            ////////////////////////////
            ///https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet#asal
            ////////////////////////////

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/");
            sqlConnection.AccessToken = accessToken;
            ////////////////////////////

            SqlCommand sqlCommand = new SqlCommand("SELECT SERVERNAME = @@SERVERNAME", sqlConnection);
            string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            try
            {
                sqlConnection.Open();
                result = sqlCommand.ExecuteScalar().ToString();
                result = CurrentTime + " CONNECTION SUCCESS - @@SERVERNAME = " + result;
            }
            catch (Exception ex)
            {
                //throw ex;
                result = ex.Message;
                result = CurrentTime + " CONNECTION ERROR - " + result;

            }
            finally
            {
                sqlConnection.Close();

            }


            return ServerName != null && DatabaseName != null
                ? (ActionResult)new OkObjectResult($"{result}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
