using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;

namespace SmartVision.Function
{
    public static class InsertStationStatistics
    {

        public static void InsertStatistics(string connString, string station, string item, string imageName, string truePositive, string falsePositive, string falseNegative, string modelName, string modelVersion)
        {
            using(var conn = new SqlConnection(connString)){
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "stat.InsertAnnotationToolStatistics";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@station", System.Data.SqlDbType.VarChar, 255){Value=station});
                    cmd.Parameters.Add(new SqlParameter("@item", System.Data.SqlDbType.VarChar, 255){Value=item});
                    cmd.Parameters.Add(new SqlParameter("@imageName", System.Data.SqlDbType.VarChar, 255){Value=imageName});
                    cmd.Parameters.Add(new SqlParameter("@truePositive", System.Data.SqlDbType.VarChar, 255){Value=truePositive});
                    cmd.Parameters.Add(new SqlParameter("@falsePositive", System.Data.SqlDbType.VarChar, 255){Value=falsePositive});
                    cmd.Parameters.Add(new SqlParameter("@falseNegative", System.Data.SqlDbType.VarChar, 255){Value=falseNegative});
                    cmd.Parameters.Add(new SqlParameter("@modelName", System.Data.SqlDbType.VarChar, 255){Value=modelName});
                    cmd.Parameters.Add(new SqlParameter("@modelVersion", System.Data.SqlDbType.VarChar, 255){Value=modelVersion});

                    cmd.Connection = conn;

                    conn.Open();
                    cmd.ExecuteNonQuery(); 
                    conn.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [FunctionName("InsertStationStatistics")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            try {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string connString = Environment.GetEnvironmentVariable("SQL_DATABASE_CONNECTION_STRING");

                foreach (var s in data){
                    string station = s.station;
                    string item = s.item;
                    string imageName = s.imageName;
                    string truePositive = s.truePositive;
                    string falsePositive = s.falsePositive;
                    string falseNegative = s.falseNegative;
                    string modelName = s.modelName;
                    string modelVersion = s.modelVersion;


                    InsertStatistics(connString, station, item, imageName, truePositive, falsePositive, falseNegative, modelName, modelVersion);
                }

                string name = "";
                name = name ?? data?.name;

                string responseMessage = "All annotations were written correctly in the database.";

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}