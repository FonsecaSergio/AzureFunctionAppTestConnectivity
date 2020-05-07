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

namespace FunctionAppConnectionLimits
{
    public static class Function_ConnectSQL_PoolLimit
    {

        ///SAMPLE1 - MAX POOL SIZE 50
        ///  Server=tcp:SERVERNAME.database.windows.net,1433;Initial Catalog=sandbox;Persist Security Info=False;User ID=User;Password=XXXXXXXXX;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=10;


        [FunctionName("Function_ConnectSQL_PoolLimit")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ConnectionString = req.Query["ConnectionString"];
            int LoopLimit = Convert.ToInt32(req.Query["LoopLimit"]);

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

                log.LogInformation(result);

                int Aux = 0;
                while (Aux++ < LoopLimit)
                {
                    log.LogInformation(String.Format("Loop {0}", Aux));
                    SqlConnection sqlConnection2 = new SqlConnection(ConnectionString);
                    sqlConnection2.Open();
                }
                
            }
            catch (Exception ex)
            {
                //throw ex;
                result = ex.Message;
                result = CurrentTime + " CONNECTION ERROR - " + result;
                log.LogError(result);

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
