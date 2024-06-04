using Cloud7CMS.Helper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace Cloud7CMS.Services
{
    public class GPDataService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BASE_URL_PAYMENT = "https://api.payment-app.info";
        private const string CANCEL_ENDPOINT = "/grameenphone/billing/cancel-subscription";
        private const string CANCEL_SECRET_KEY = "secretKeyb1DEYmhXrTYiyU65EWI8U1h";
        private const string CANCEL_IV_SECRET = "ivSec1HJFhYrhcr5";
        private const string X_API_KEY = "Sv0390cf388d6bc429d9fd09741b0abf7c8T";

        public GPDataService() {           
        }  

        public string GetServices(string name)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                sql = $"SELECT DISTINCT SERVICEID AS Value\r\n\t,SERVICEDESCRIPTION AS Text\r\nFROM [TELENORDOB].[DBO].[SERVICEMAPS]\r\nWHERE (\r\n\t\tSERVICEDESCRIPTION LIKE '%FUNBOX%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%MXPLAYER%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%MAGPLUS%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%FOOTBALLERS%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%EMAGAZINE%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%Mjunoon%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%GameSwiper%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%SportsLocker%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%DroidGame%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%ComedyClub%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%SachinSaga%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%MarathonofWishes%' \r\n\t\tOR SERVICEDESCRIPTION LIKE '%CarePlus%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%WIN%'\r\n\t\tOR SERVICEDESCRIPTION LIKE '%SuperGameRoll%'\r\n\t\t )\r\nORDER BY SERVICEID ASC";
                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

                   
            return CDA.DataTableToJSONWithJSONNet(dt);
            //return ds;
        }

        internal string GetActivationData(string serviceIds, DateTime fromDate, DateTime toDate, string dataType = "Count")
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $"SubscriptionDate,sDescription,y.totalBase,sPrice,[sCount],[sChargeBDT]";

            string strGroupByCount = $" GROUP BY CAST(s.SubscriptionDate AS DATE), s.[Description],s.ServiceId, s.TotalAmountCharged";

            string strSelectionDetails = $" DISTINCT CAST(s.SubscriptionDate AS DATE) SubscriptionDate ,MSISDN,ServiceId,s.[Description] [sDescription],'ACT' ActionType, 'New Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $";WITH cte_totalbase\r\nAS (\r\n\tSELECT ServiceId,count(DISTINCT MSISDN) totalBase\r\n\t" +
                        $"FROM Subscriptions\r\n\tWHERE [SubscriptionStatus] = 1\t\t\r\n\t\t" +
                        $"AND cast(SubscriptionDate AS DATE) < cast('{fromDate}' AS DATE)\r\n\t" +
                        $"GROUP BY ServiceId\t\r\n\t)\r\n\t" +
                        $",cte_current_activation\r\nAS (\r\n" +
                        $"SELECT CAST(s.SubscriptionDate AS DATE) SubscriptionDate\r\n\t,s.ServiceId\r\n\t" +
                        $",s.[Description] [sDescription]\r\n\t,s.TotalAmountCharged sPrice\r\n\t,COUNT(*) [sCount]\r\n\t" +
                        $",COUNT(*) * s.TotalAmountCharged [sChargeBDT]\r\nFROM Subscriptions AS s\r\n" +
                        $"WHERE s.[SubscriptionStatus] = 1\r\n\tAND s.[SubscriptionType] = 2\r\n\tAND s.ServiceId IN ('{serviceIds.Replace(",", "','")}')\r\n\t" +
                        $"AND CAST(s.SubscriptionDate AS DATE) BETWEEN '{fromDate}'\r\n\t\t" +
                        $"AND '{toDate}'\r\n" +
                        $"{strGroupByCount})\r\n" +
                        $"SELECT {strSelectionCount} FROM cte_current_activation x\r\n" +
                        $"INNER JOIN cte_totalbase y ON x.ServiceID = y.ServiceID\r\n" +
                        $"ORDER BY CAST(SubscriptionDate AS DATE)\r\n\t" +
                        $",[sDescription]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails} " +
                        $" FROM Subscriptions AS s" +
                        $" CROSS APPLY (SELECT count(distinct MSISDN) totalBase FROM Subscriptions " +
                        $"WHERE [SubscriptionStatus]=1 AND ServiceId = s.ServiceId and cast(SubscriptionDate as date)<cast('{fromDate}' as date)) as a " +
                        $" WHERE s.[SubscriptionStatus]=1 and s.[SubscriptionType]=2 and s.ServiceId IN('{serviceIds.Replace(",", "','")}')" +
                        $" AND CAST(s.SubscriptionDate AS DATE) BETWEEN '{fromDate}' AND '{toDate}'" +
                        $" {strGroupByDetails}" +
                        $" ORDER BY CAST(s.SubscriptionDate AS DATE), s.[Description]";
                }

                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

          
          
            return CDA.DataTableToJSONWithJSONNet(dt);
        }

        internal string GetRenewalData(string serviceIds, DateTime fromDate, DateTime toDate, string dataType = "Count")
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(rh.[ProcessDate] AS DATE) RenewalDate,[ServiceDescription] [rDescription],[TotalAmountCharged] rPrice" +
              $",COUNT(*) [rCount]" +
              $",COUNT(*) * [TotalAmountCharged] [rChargeBDT]";

            string strGroupByCount = $" GROUP BY CAST(rh.[ProcessDate] AS DATE)" +
              $",[ServiceDescription]" +
              $",[TotalAmountCharged]";

            string strSelectionDetails = $"  DISTINCT Processdate RenewalDate,MSISDN,rh.ServiceId,[Description] [rDescription],'REN' ActionType, 'Charge' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql =   $"SELECT {strSelectionCount}" +
                            $" FROM [TelenorDOB_Archive].[dbo].[RenewalProcess_Partitioned] AS rh WITH (NOLOCK)" +
                            $" INNER JOIN [TelenorDOB].[dbo].[ServiceMaps] AS s ON s.NewServiceId = rh.ServiceId" +
                            $" WHERE rh.[ServiceId] IN ('{serviceIds.Replace(",", "','")}')" +
                            $" AND [RenewalStatus] = 'Charged'" +
                            $" AND CAST(rh.[ProcessDate] AS DATE) BETWEEN '{fromDate}'AND '{toDate}'" +
                            $" {strGroupByCount} " +
                            $" ORDER BY [RenewalDate],[rDescription]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails}" +
                         $" FROM [TelenorDOB_Archive].[dbo].[RenewalProcess_Partitioned] AS rh WITH (NOLOCK)" +
                         $" INNER JOIN [TelenorDOB].[dbo].[ServiceMaps] AS s ON s.NewServiceId = rh.ServiceId" +
                         $" WHERE rh.[ServiceId] IN ('{serviceIds.Replace(",", "','")}')" +
                         $" AND [RenewalStatus] = 'Charged'" +
                         $" AND CAST(rh.[ProcessDate] AS DATE) BETWEEN '{fromDate}'AND '{toDate}'" +
                         $" {strGroupByDetails} " +
                         $" ORDER BY [RenewalDate],[rDescription]";
                }


                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

             

          
            return CDA.DataTableToJSONWithJSONNet(dt);
        }

        internal string GetChurnData(string serviceIds, DateTime fromDate, DateTime toDate, string dataType="Count")
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" COALESCE(CAST(g.CancellationDate AS DATE), CAST(g.SubscriptionDate AS DATE)) AS Churndate" +
                    $",g.ServiceId,g.Description AS [Description],COUNT(g.MSISDN) AS Churn,a.totalChurn ";

            string strGroupByCount = $" GROUP BY COALESCE(CAST(g.CancellationDate AS DATE), CAST(g.SubscriptionDate AS DATE)) ,g.Description ,g.ServiceId, a.totalChurn";

            string strSelectionDetails = $" DISTINCT CAST(SubscriptionDate AS DATE) Churndate ,MSISDN,ServiceId,Description ,'ACT' ActionType, 'Churn Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $"SELECT {strSelectionCount} " +
                   $" FROM [TelenorDOB].dbo.Subscriptions AS g " +
                   $" CROSS APPLY (SELECT count(distinct MSISDN) totalChurn FROM [TelenorDOB].dbo.Subscriptions " +
                   $"WHERE [SubscriptionStatus]=0 AND ServiceId = g.ServiceId and cast(SubscriptionDate as date)<cast('{fromDate}' as date)) as a " +
                   $" WHERE g.[SubscriptionStatus]=0 and g.ServiceId IN('{serviceIds.Replace(",", "','")}')" +
                   $" AND CAST(g.SubscriptionDate AS DATE) BETWEEN '{fromDate}' AND '{toDate}'" +
                   $" {strGroupByCount} " +
                   $" ORDER BY Churndate, [Description]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $";SELECT {strSelectionDetails} " +
                    $" FROM [TelenorDOB].dbo.Subscriptions AS g " +
                    $" CROSS APPLY (SELECT count(distinct MSISDN) totalChurn FROM [TelenorDOB].dbo.Subscriptions " +
                    $"WHERE [SubscriptionStatus]=0 AND ServiceId = g.ServiceId and cast(SubscriptionDate as date)<cast('{fromDate}' as date)) as a " +
                    $" WHERE g.[SubscriptionStatus]=0 and g.ServiceId IN('{serviceIds.Replace(",", "','")}')" +
                    $" AND CAST(g.SubscriptionDate AS DATE) BETWEEN '{fromDate}' AND '{toDate}'" +
                    $" {strGroupByDetails} " +
                    $" ORDER BY Churndate, [Description]";
                }

                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

          

            return CDA.DataTableToJSONWithJSONNet(dt);
        }

        internal string GetTrafficData(string serviceIds, DateTime fromDate, DateTime toDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            StringBuilder stringBuilder = new StringBuilder();
            var jsonArray = "";
            try
            {
                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP Funbox DOB';" ;

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item)+",");
                }

                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP MXPlayer DOB';" ;

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
                }

                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP Magplus DOB';" ;

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
                }

                sql =$"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP Footballers DOB';" ;

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
                }

                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP eMagazine DOB';";

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
                }

				sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP MjunoonTV DOB';";

				ds = cda.GetDataSet(sql, dbName);

				foreach (DataTable item in ds.Tables)
				{
					stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
				}

				sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP DroidGame DOB';";

				ds = cda.GetDataSet(sql, dbName);

				foreach (DataTable item in ds.Tables)
				{
					stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
				}
                
                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP MarathonOfWishes DOB';";

				ds = cda.GetDataSet(sql, dbName);

				foreach (DataTable item in ds.Tables)
				{
					stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
				}     
                
                sql = $"EXEC [dbo].[sp_GPTelenorDOBTrafficMonitor] 'GP Win DOB';";

				ds = cda.GetDataSet(sql, dbName);

				foreach (DataTable item in ds.Tables)
				{
					stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item) + ",");
				}




			}
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        
          

            return  stringBuilder.ToString().Substring(0,stringBuilder.ToString().Length-1);
           //dt = ds.Tables[0];
           //return CDA.DataTableToJSONWithJSONNet(dt);
        }
        /// <summary>
        /// GP Admin Panel
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="reportDate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal string GetSubscriptionDetailsData(string msisdn, DateTime reportDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT\r\n    sub.MSISDN,\r\n    srv.ServiceName,\r\n    srv.ServiceDescription AS ServicePlan,srv.ServiceId,sub.SubscriptionId,\r\n    " +
                    $"MAX(sub.SubscriptionDate) AS ActivationDate,\r\n FORMAT(MAX(sub.SubscriptionDate), 'HH:mm:ss') AS ActivationTime,\r\n\t" +
                    $"MAX(ExpiryDate) AS ExpiryDate,sub.CancellationDate,sub.CancelSource,\r\n    MAX(srv.Channel) AS ActivationChannel,\r\n    sub.SubscriptionStatus AS CurrentStatus,\r\n" +
                    $"\r\n\tDNDStatus = CASE WHEN EXISTS (\r\nSELECT 1 \r\nFROM [TelenorDOB].[dbo].[BlockedUsers] b \r\nWHERE b.MSISDN = sub.MSISDN \r\nAND b.ServiceId = srv.ServiceId\r\n) THEN 'Yes' ELSE 'No' END," +
                    $"CAST(CASE \r\n\t" +
                    $"WHEN (ServiceName like 'daily' OR ServiceName like 'One Day') AND MAX(ExpiryDate)>=CAST('{reportDate}' as DATE) THEN DATEADD(DD,1,MAX(sub.SubscriptionDate)) \r\n\t" +
                    $"WHEN ServiceName like 'week' AND MAX(ExpiryDate)>=CAST('{reportDate}' as DATE)  THEN DATEADD(DD,7,MAX(sub.SubscriptionDate)) \r\n\t" +
                    $"WHEN ServiceName like 'month' AND MAX(ExpiryDate)>=CAST('{reportDate}' as DATE)  THEN DATEADD(MM,1,MAX(sub.SubscriptionDate)) \r\n\t" +
                    $"WHEN ServiceName like 'year' AND MAX(ExpiryDate)>=CAST('{reportDate}' as DATE)  THEN DATEADD(YYYY,1,MAX(sub.SubscriptionDate)) \r\n\tend as Date) AS NextRenewal\r\n" +
                    $"FROM\r\n    [TelenorDOB].[dbo].[Subscriptions] AS sub\r\nJOIN\r\n    [TelenorDOB].[dbo].[ServiceMaps] AS srv ON srv.[NewServiceId] = sub.[ServiceId]\r\n" +
                    $"WHERE\r\n    sub.MSISDN = '{msisdn}'\r\n    AND sub.SubscriptionDate = (\r\n        SELECT MAX(SubscriptionDate)\r\n        " +
                    $"FROM [TelenorDOB].[dbo].[Subscriptions]\r\n        WHERE [ServiceId] = sub.[ServiceId]\r\n          AND MSISDN = sub.MSISDN\r\n    )\r\n" +
                    $"GROUP BY\r\n    sub.MSISDN,\r\n    srv.ServiceName,\r\n    srv.ServiceDescription,srv.ServiceId,sub.SubscriptionId,\r\n    sub.SubscriptionStatus,sub.CancellationDate,sub.CancelSource;";

                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }



            return CDA.DataTableToJSONWithJSONNet(dt);
        }

        internal async Task<string> DeactivateMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId, string reason)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            string responseMessage = "";
            string affectedRows = "";
            try
            {
                HttpResponseMessage response = await CancelGPSubscription(msisdn, serviceId);
                
                if (response.IsSuccessStatusCode)
                {
                    // Successful response
                    // Access response content if needed: string responseContent = await response.Content.ReadAsStringAsync();
                    //responseMessage = response.StatusCode.ToString();
                    responseMessage = response.Content.ReadAsStringAsync().Result;
                    sql = $"UPDATE sub\r\nSET sub.SubscriptionStatus=0, sub.CancellationDate=Getdate(), sub.CancelSource='{reason}'\r\n" +
                    $"FROM [TelenorDOB].[dbo].[Subscriptions] sub\r\n" +
                    $"WHERE sub.MSISDN = '{msisdn}' AND sub.ServiceId='{serviceId}' AND SubscriptionId={subscriptionId};";

                    affectedRows = cda.ExecuteNonQuery(sql, dbName);

                    // Console.WriteLine($"Success! Status Code: {statusCode}");
                }
                else
                {
                    responseMessage =  response.Content.ReadAsStringAsync().Result;

                    // Error response
                    //responseMessage = response.StatusCode.ToString();
                    
                }

                if (responseMessage.Length == 0)
                {
                    responseMessage = response.ReasonPhrase;
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }



            return (responseMessage);
        }

        internal int DNDMSISDNByServiceId(string msisdn, string serviceId, string subscriptionId)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            string affectedRows = "";
            try
            {
                sql = $"INSERT INTO [TelenorDOB].[dbo].[BlockedUsers]([ServiceId],[Msisdn])\r\nVALUES ('{serviceId}','{msisdn}')";

                affectedRows = cda.ExecuteNonQuery(sql, dbName);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }



            return (int.Parse(affectedRows.ToString()) > 0) ? 1 : 0; ;
        }

        internal string GetRenewalDetailsData(string msisdn, DateTime reportDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "TelenorConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT MSISDN,s.ServiceId,s.ServiceName,s.SubscriptionType,TotalAmountCharged,SubscriptionPeriod,RenewalStatus, Description" +
                    $",s.Currency,ProcessDate,LastUpdated\r\nFROM [TelenorDOB_Archive].[dbo].[RenewalProcess_Partitioned] AS rh WITH (NOLOCK)\r\n" +
                    $"INNER JOIN [TelenorDOB].[dbo].[ServiceMaps] AS s ON s.NewServiceId = rh.ServiceId\r\n" +
                    $"WHERE rh.MSISDN='{msisdn}' AND \r\nCAST(rh.[ProcessDate] AS DATE) BETWEEN DATEADD(DD,-30,'{reportDate}') AND CAST('{reportDate}' as DATE) ORDER BY ProcessDate DESC";

                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }



            return CDA.DataTableToJSONWithJSONNet(dt);
        }

        internal async Task<HttpResponseMessage> CancelGPSubscription(string msisdn, string serviceId)
        {
            string cancelUrl = $"{BASE_URL_PAYMENT}{CANCEL_ENDPOINT}";

            Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "MSISDN", msisdn },
            { "ServiceId", serviceId }
        };

            string encryptedBody = EncryptAES_CBC(JsonSerialize(parameters), CANCEL_SECRET_KEY, CANCEL_IV_SECRET);

            string encryptionJsonBody = $"\"{encryptedBody}\"";
            StringContent content = new StringContent(encryptionJsonBody, Encoding.UTF8, "application/json");
            // Add custom header
            client.DefaultRequestHeaders.Add("x-api-key", X_API_KEY);
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");


            return await client.PostAsync(cancelUrl, content);
        }

        // Function to serialize an object to JSON
        static string JsonSerialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        // Function to encrypt a string using AES CBC
        static string EncryptAES_CBC(string plaintext, string key, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                // Set key size to 256 bits
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor();

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plaintext);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }


       
    }
}
