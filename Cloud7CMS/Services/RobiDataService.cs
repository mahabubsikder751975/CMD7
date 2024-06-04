using Cloud7CMS.Helper;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace Cloud7CMS.Services
{
    public class RobiDataService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BASE_URL_PAYMENT = "https://api.payment-app.info";
        private const string CANCEL_ENDPOINT = "/robi/billing/cancel-subscription";
        private const string CANCEL_SECRET_KEY = "secretKeyb1DEYmhXrTYiyU65EWI8U1h";
        private const string CANCEL_IV_SECRET = "ivSec1HJFhYrhcr5";
        private const string X_API_KEY = "Sv0390cf388d6bc429d9fd09741b0abf7c8T";
        public RobiDataService() {           
        }

        //public string GetGPTelenorSubsriptionsData(string name) {
        //    DataTable table = new DataTable();
        //    return (CDA.DataTableToJSONWithJSONNet(table));
        //}
        //public DataTable GetGPTelenorRenewalData(string name)
        //{
        //    DataTable table = new DataTable();
        //    return table;
        //}

        public string GetServices(string name)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                sql = $"SELECT DISTINCT SERVICE_ID  AS Value, Service_Name AS Text FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap]\r\nWHERE ( Service_Name like '%Cloud7%') \r\nUNION \r\nSELECT DISTINCT SERVICE_ID  AS Value, Service_Name AS Text FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap]\r\nWHERE SERVICE_ID IN ('MED570643','MED277519','MED553484') \r\nORDER BY SERVICE_ID ASC";
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

        internal string GetActivationData(string serviceIds, DateTime fromDate, DateTime toDate, string dataType)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(s.LastUpdate AS DATE) SubscriptionDate,Service_ID,  sm.Service_Name [sDescription],3.00 sPrice, COUNT(*) [sCount], COUNT(*) * 3.00 [sChargeBDT] ";

            string strGroupByCount = $" GROUP BY CAST(s.LastUpdate AS DATE),Service_ID, sm.[Service_Name] ";

            string strSelectionDetails = $" DISTINCT CAST(s.LastUpdate AS DATE) ActivationDate ,MSISDN,s.SERVICE_ID, SERVICE_NAME Description ,'ACT' ActionType, 'New Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $";WITH cte_totalbase as (\r\n" +
                           $"SELECT Service_Id,count(distinct MSISDN) totalBase FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] \r\n" +
                           $"WHERE [Reg_Status]=1  and cast(LastUpdate as date)<cast('{fromDate}' as date)\r\n" +
                           $"GROUP BY Service_Id\r\n) --select* from cte_totalbase\r\n" +
                           $",cte_current_activation as (\r\n" +
                           $"SELECT {strSelectionCount}" +
                           $"FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] AS s \r\n" +
                           $"OUTER APPLY (SELECT Service_Name FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap] WHERE Service_Id = s.Service_ID) as sm\r\n" +
                           $"WHERE s.[Reg_Status]=1 and s.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\n" +
                           $"AND CAST(s.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' \r\n" +
                           $"{strGroupByCount}\r\n)\r\n" +
                           $"select * from cte_current_activation x inner join cte_totalbase y\r\nON x.Service_ID=y.Service_ID\r\n" +
                           $"ORDER BY CAST(x.SubscriptionDate AS DATE), [sDescription]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT  {strSelectionDetails} \r\n" +
                        $" FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] AS s \r\n" +
                        $"OUTER APPLY (SELECT Service_Name FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap] WHERE Service_Id = s.Service_ID) as sm\r\n" +
                        $"WHERE s.[Reg_Status]=1 and s.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\n" +
                        $"AND CAST(s.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' ";
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
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            
            string strSelectionCount = $" CAST(rh.[RequestDate] AS DATE) RenewalDate,[Service_Name] [rDescription],[ChargedAmount] rPrice\r\n" +
                $",COUNT(*) [rCount],COUNT(*) * cast(isnull([ChargedAmount],0) as numeric(13,2)) [rChargeBDT] ";

            string strGroupByCount = $" GROUP BY CAST(rh.[RequestDate] AS DATE),[Service_Name],[ChargedAmount]";

            string strSelectionDetails = $"  distinct rh.[RequestDate] RenewalDate,MSISDN,rh.Service_ID ServiceID,Service_Name [rDescription],ChargedAmount,'REN' ActionType, 'Charge' ActionMessage ";

            string strGroupByDetails = $" ";


            try
            {
                if (dataType?.ToLower() == "count")
                {
                    sql = $"SELECT {strSelectionCount} \r\n" +                            
                            $"FROM (SELECT * FROM [CelcomDB_Archive].[dbo].[tbl_RobiDCBRenewalCharge_Partitioned] AS rh WITH (NOLOCK)\r\n" +
                            $"UNION ALL\r\n" +
                            $"SELECT * FROM [CelcomDB_Archive].[dbo].[tbl_RobiDCBRenewalChargeFunbox_Partitioned] AS rh WITH (NOLOCK)\r\n) AS rh  \r\n" +
                            $"INNER JOIN [CelcomDB].[dbo].[tbl_CelcomServiceMap] AS s ON s.Service_Id = rh.Service_ID \r\n" +
                            $"WHERE rh.[Service_Id] IN ('{serviceIds.Replace(",", "','")}') AND [ErrorCode] = '00' \r\n" +
                            $"AND CAST(rh.[RequestDate] AS DATE) BETWEEN  '{fromDate}'AND '{toDate}' \r\n" +
                            $" {strGroupByCount} " +
                            $"ORDER BY [RenewalDate],[rDescription] ";
                       

                }
                else if (dataType?.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails} \r\n" +                            
                            $"FROM (SELECT * FROM [CelcomDB_Archive].[dbo].[tbl_RobiDCBRenewalCharge_Partitioned] AS rh WITH (NOLOCK)\r\n" +
                            $"UNION ALL\r\n" +
                            $"SELECT * FROM [CelcomDB_Archive].[dbo].[tbl_RobiDCBRenewalChargeFunbox_Partitioned] AS rh WITH (NOLOCK)\r\n) AS rh  \r\n" +
                            $"INNER JOIN [CelcomDB].[dbo].[tbl_CelcomServiceMap] AS s ON s.Service_Id = rh.Service_ID \r\n" +
                            $"WHERE rh.[Service_Id] IN ('{serviceIds.Replace(",", "','")}') AND [ErrorCode] = '00' \r\n" +
                            $"AND CAST(rh.[RequestDate] AS DATE) BETWEEN  '{fromDate}'AND '{toDate}' \r\n" +
                            $" {strGroupByDetails} " +
                            $"ORDER BY [RenewalDate],[rDescription] ";
                      
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

        internal string GetChurnData(string serviceIds, DateTime fromDate, DateTime toDate, string dataType)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(g.LastUpdate AS DATE) AS Churndate,g.Service_Id\r\n" +
                   $",s.Service_Name AS [Description],COUNT(g.MSISDN) AS Churn\r\n";

            string strGroupByCount = $" GROUP BY CAST(g.LastUpdate AS DATE) ,s.Service_Name ,g.Service_Id";

            string strSelectionDetails = $" DISTINCT CAST(LastUpdate AS DATE) Churndate ,MSISDN,Service_ID,Service_Name Description ,'ACT' ActionType, 'Churn Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $";WITH cte_totalchurn as (\r\n" +
                   $"SELECT Service_Id,count(distinct MSISDN) totalChurn FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] \r\n" +
                   $"WHERE [Reg_Status]=0  and cast(LastUpdate as date)<cast('{fromDate}' as date)\r\n" +
                   $"GROUP BY Service_Id\r\n) --select* from cte_totalbase\r\n" +
                   $",cte_current_churn as (\r\n" +
                   $"SELECT {strSelectionCount}\r\n" +
                   $"FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] AS g\r\n" +
                   $"OUTER APPLY (SELECT Service_Name FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap] WHERE Service_Id = g.Service_ID) as s\r\n" +
                   $" WHERE g.[Reg_Status]=0 and g.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\n" +
                   $"AND CAST(g.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' \r\n" +
                   $"{strGroupByCount})\r\n" +
                   $"select * from cte_current_churn x inner join cte_totalchurn y\r\n" +
                   $"ON x.Service_ID=y.Service_ID\r\nORDER BY Churndate, [Description]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails} \r\n" +
                        $"FROM [CelcomDB].[dbo].[tbl_CelcomSubscription] AS g\r\nOUTER APPLY (SELECT Service_Name FROM [CelcomDB].[dbo].[tbl_CelcomServiceMap] WHERE Service_Id = g.Service_ID) as s\r\n " +
                        $"WHERE g.[Reg_Status]=0 and g.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\nAND CAST(g.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' ";
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
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            StringBuilder stringBuilder = new StringBuilder();
            var jsonArray = "";
            try
            {
                sql = $"EXEC [dbo].[sp_RobiTrafficMonitorCloud7] 'Robi Funbox DCB';";

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item)+",");
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
        /// Robi Admin Panel
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="reportDate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal string GetSubscriptionDetailsData(string msisdn, DateTime reportDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT\r\n    sub.MSISDN,\r\n    srv.SERVICE_NAME ServiceName,\r\n    '' AS ServicePlan,srv.SERVICE_ID ServiceId,sub" +
                    $".AocTransID SubscriptionId,\r\n    MAX(sub.Reg_Date) AS ActivationDate,\r\n FORMAT(MAX(sub.Reg_Date), 'HH:mm:ss') AS ActivationTime" +
                    $",\r\n\tMAX(dateadd(dd,0,sub.Reg_Date) ) AS ExpiryDate\r\n\t,case when Reg_Status=0 then sub.LastUpdate end CancellationDate" +
                    $", sub.Reg_Description CancelSource,\r\n    sub.Reg_Source AS ActivationChannel,\r\n    sub.Reg_Status AS CurrentStatus,\r\n\r\n\t" +
                    $"DNDStatus = CASE WHEN EXISTS (\r\n\t\t\t\t\tSELECT 1 \r\n\t\t\t\t\tFROM [CelcomDB].[dbo].[BlockedUsers] b \r\n\t\t\t\t\t" +
                    $"WHERE b.MSISDN = sub.MSISDN \r\n\t\t\t\t\tAND b.ServiceId = srv.SERVICE_ID\r\n\t\t\t\t\t) THEN 'Yes' ELSE 'No' END,\r\n" +
                    $"CAST(CASE \r\n\tWHEN (srv.SERVICE_NAME like 'daily' OR srv.SERVICE_NAME like 'One Day') " +
                    $"AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE) THEN DATEADD(DD,1,MAX(sub.Reg_Date)) \r\n\t" +
                    $"WHEN srv.SERVICE_NAME like 'week' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(DD,7,MAX(sub.Reg_Date)) \r\n\t" +
                    $"WHEN srv.SERVICE_NAME like 'month' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(MM,1,MAX(sub.Reg_Date)) \r\n\t" +
                    $"WHEN srv.SERVICE_NAME like 'year' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(YYYY,1,MAX(sub.Reg_Date)) \r\n\tend as Date) AS NextRenewal\r\n" +
                    $"FROM\r\n    [CelcomDB].[dbo].[tbl_CelcomSubscription] AS sub\r\nJOIN\r\n    [CelcomDB].[dbo].tbl_CelcomServiceMap AS srv " +
                    $"ON srv.SERVICE_ID = sub.Service_ID\r\n" +
                    $"WHERE\r\n    sub.MSISDN = '{msisdn}'\r\n    AND sub.Reg_Date = (\r\n        " +
                    $"SELECT MAX(Reg_Date)\r\n        FROM [CelcomDB].[dbo].[tbl_CelcomSubscription]\r\n        " +
                    $"WHERE [Service_ID] = sub.[Service_ID]\r\n          AND MSISDN = sub.MSISDN\r\n    )\r\n" +
                    $"GROUP BY\r\n    sub.MSISDN,\r\n    srv.SERVICE_NAME,\r\n    srv.SERVICE_ID,sub.AocTransID,sub.Reg_Source,\r\n    sub.Reg_Status" +
                    $",case when Reg_Status=0 then sub.LastUpdate end,sub.Reg_Description;";
                   

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
            var dbName = "RobiConnection";
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
                    responseMessage = response.Content.ReadAsStringAsync().Result;

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
            var dbName = "RobiConnection";
            string affectedRows = "";
            try
            {
                sql = $"INSERT INTO [CelcomDB].[dbo].[BlockedUsers]([ServiceId],[Msisdn])\r\nVALUES ('{serviceId}','{msisdn}')";

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
            var dbName = "RobiConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT MSISDN\r\n\t,s.SERVICE_ID ServiceId\r\n\t,s.SERVICE_NAME ServiceName\r\n\t,s.ServiceType SubscriptionType\r\n\t" +
                    $",rh.ChargedAmount TotalAmountCharged\r\n\t,CASE \r\n\tWHEN s.Service_Name like '%Daily%' THEN 1 \r\n\t" +
                    $"WHEN s.Service_Name like '%Weekly%' THEN 7\r\n\tWHEN s.Service_Name like '%Monthly%' THEN 30\r\n\t" +
                    $"WHEN s.Service_Name like '%Yearly%' THEN 365\r\n\tEND  SubscriptionPeriod\r\n\t," +
                    $"CASE WHEN rh.ErrorCode='00' THEN 'Charged' else ErrorCode end RenewalStatus\r\n\t,rh.ErrorMessage Description\r\n\t,'' Currency\r\n\t" +
                    $",rh.RequestDate ProcessDate\r\n\t,rh.ResponseTime LastUpdated\r\n" +
                    $"FROM [CelcomDB_Archive].[dbo].[tbl_RobiDCBRenewalChargeFunbox_Partitioned] AS rh WITH (NOLOCK)\r\n" +
                    $"INNER JOIN [CelcomDB].[dbo].[tbl_CelcomServiceMap] AS s ON s.Service_Id = rh.Service_Id\r\n" +
                    $"WHERE rh.MSISDN = '{msisdn}' AND\r\n\t CAST(rh.ResponseTime AS DATE) BETWEEN DATEADD(DD, - 30, '{reportDate}')\r\n\t\t" +
                    $"AND CAST('{reportDate}' AS DATE)\r\nORDER BY ProcessDate DESC,rh.MSISDN ";
                   

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
            { "msisdn", msisdn },
            { "serviceid", serviceId }
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
