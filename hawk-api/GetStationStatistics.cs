using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;


namespace SmartVision.Function
{
    public static class GetStationStatistics
    {
        [FunctionName("GetStationStatistics")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string station = req.Query["station"];
            string model_name = req.Query["modelName"];
            string model_version = req.Query["modelVersion"];

            string connString = Environment.GetEnvironmentVariable("SQL_DATABASE_CONNECTION_STRING");

            try{
                DataTable datatable = GetStatistics(connString, station, model_name, model_version, log);

                string JsonResponse = string.Empty;
                JsonResponse = JsonConvert.SerializeObject(datatable);

                return new OkObjectResult(JsonResponse);
            }
            catch (Exception ex){
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }


        public static DataTable GetStatistics(string connString, string station, string model_name, string model_version, ILogger log){
            using(var conn = new SqlConnection(connString)){
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "stat.GetAnnotationToolStatistics";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@station", System.Data.SqlDbType.VarChar, 255){Value=station});
                    cmd.Parameters.Add(new SqlParameter("@model_name", System.Data.SqlDbType.VarChar, 255){Value=model_name});
                    cmd.Parameters.Add(new SqlParameter("@model_version", System.Data.SqlDbType.VarChar, 255){Value=model_version});
                    cmd.Connection = conn;
                    conn.Open();
                    
                    SqlDataReader rdr = cmd.ExecuteReader();
                    var datatable = new DataTable();
                    datatable.Load(rdr);

                    conn.Close();
                    return datatable;
                    
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                    throw ex;
                }
            }
        }
    }
}
