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


//NEED .NET CORE 2.1 / sqlclient 4.5.1 bug sql client


namespace FunctionAppTestConnection
{
    public static class Function_ConnectSQL
    {
        [FunctionName("Function_ConnectSQL")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ConnectionString = req.Query["ConnectionString"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            ConnectionString = ConnectionString ?? data?.name;

            string result = "NOT CONNECTED";

            SqlConnection sqlConnection = new SqlConnection();

            sqlConnection = new SqlConnection(ConnectionString);


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


            return ConnectionString != null
                ? (ActionResult)new OkObjectResult($"{result}")
                : new BadRequestObjectResult("Please pass a ConnectionString on the query string or in the request body");
        }
    }
}
