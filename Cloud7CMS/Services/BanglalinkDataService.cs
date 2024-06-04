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
    public class BanglalinkDataService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BASE_URL_PAYMENT = "https://api.payment-app.info";
        private const string CANCEL_ENDPOINT = "/banglalink/billing/cancel-subscription";
        private const string CANCEL_SECRET_KEY = "secretKeyb1DEYmhXrTYiyU65EWI8U1h";
        private const string CANCEL_IV_SECRET = "ivSec1HJFhYrhcr5";
        private const string X_API_KEY = "Sv0390cf388d6bc429d9fd09741b0abf7c8T";


        public BanglalinkDataService() {           
        }     

        public string GetServices(string name)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                //sql = $"SELECT DISTINCT SERVICE_ID  AS Value, MAX(SERVICE_NAME) AS Text  from BL6D_DCBDB.[dbo].[tbl_BLSDP_ServiceMap_Gakk] \r\nWHERE (SERVICE_NAME LIKE '%Funbox%' OR SERVICE_NAME LIKE '%Football%'  OR SERVICE_NAME LIKE '%Daily Hub%')  \r\nAND CPName = 'Cloud 7 Ltd'\r\nGROUP BY SERVICE_ID\r\nORDER BY SERVICE_ID";
                sql = $"SELECT DISTINCT SERVICE_ID  AS Value, MAX(SERVICE_NAME) AS Text  from BL6D_DCBDB.[dbo].[tbl_BLSDP_ServiceMap_Gakk] \r\nWHERE \r\n(\r\nSERVICE_NAME LIKE '%Funbox%' \r\nOR SERVICE_NAME LIKE '%Football%'  \r\nOR SERVICE_NAME LIKE '%Daily Hub%'\r\nOR SERVICE_NAME LIKE '%Magplus%'\r\nOR SERVICE_NAME LIKE '%Media Games%'\r\nOR SERVICE_NAME LIKE '%Gamehub Today%'\r\nOR SERVICE_NAME LIKE '%Sports Locker%'\r\nOR SERVICE_NAME LIKE '%Roll On%'\r\nOR SERVICE_NAME LIKE '%Care Plus%'\r\n ) \r\nGROUP BY SERVICE_ID\r\nORDER BY SERVICE_ID";

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
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            string strSelectionCount = $"CAST(u.Reg_Date AS DATE) SubscriptionDate,s.[SERVICE_NAME] [sDescription],s.SERVICE_ID,s.ChargeAmount sPrice,COUNT(*) [sCount],COUNT(*) * Convert(numeric(18,2),s.ChargeAmount) [sChargeBDT]";

            string strGroupByCount = $" GROUP BY CAST(u.Reg_Date AS DATE),s.[SERVICE_NAME],s.SERVICE_ID,s.ChargeAmount)\r\n ";

            string strSelectionDetails = $" DISTINCT CAST(u.LastUpdate AS DATE) SubscriptionDate ,MSISDN,s.Service_Id,s.[SERVICE_NAME] [sDescription],'ACT' ActionType, 'New Activation' ActionMessage ";

            string strGroupByDetails = $" ";


            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $";WITH cte_totalbase\r\nAS (\r\n\tSELECT Service_ID,count(DISTINCT MSISDN) totalBase\r\n\t" +
                      $"FROM [BL6D_DCBDB].[dbo].[UserSubscriptions]\r\n\t" +
                      $"WHERE [Reg_Status] = 1 AND cast(Reg_Date AS DATE) < cast('{fromDate}' AS DATE)\r\n\t" +
                      $"GROUP BY Service_ID\t\r\n\t)\r\n\t,cte_current_activation\r\nAS (\r\n" +
                      $"SELECT {strSelectionCount}\r\nFROM [BL6D_DCBDB].[dbo].[UserSubscriptions] u\r\n" +
                      $"INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] s ON u.Service_ID = s.SERVICE_ID\r\n" +
                      $"WHERE u.Reg_Status = 1\r\n\tAND s.SERVICE_ID IN ('{serviceIds.Replace(",", "','")}')\r\n\t" +
                      $"AND CAST(u.Reg_Date AS DATE) BETWEEN '{fromDate}'\r\n\t\tAND '{toDate}'\r\n" +
                      $"{strGroupByCount}\r\n\r\n" +
                      $"SELECT SubscriptionDate,sDescription,isnull(y.totalBase,0) totalBase,sPrice,[sCount],[sChargeBDT] FROM cte_current_activation x\r\n" +
                      $"LEFT OUTER JOIN cte_totalbase y ON x.Service_ID = y.Service_ID\r\n" +
                      $"ORDER BY CAST(SubscriptionDate AS DATE)\r\n\t,[sDescription]\r\n";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails}\r\nFROM [BL6D_DCBDB].[dbo].[UserSubscriptions] u\r\n" +
                        $"INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] s ON u.Service_ID = s.SERVICE_ID\r\n" +
                        $"WHERE u.Reg_Status = 1\r\n\tAND s.SERVICE_ID IN ('{serviceIds.Replace(",", "','")}')\r\n\t" +
                        $"AND CAST(u.Reg_Date AS DATE) BETWEEN '{fromDate}' AND '{toDate}'";
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
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            string strSelectionCount = $" CAST(rh.[Timestamp] AS DATE) RenewalDate"+
                                        $",[ProductName] [rDescription],[ChargeAmount] rPrice,COUNT(*)[rCount],COUNT(*) * [ChargeAmount][rChargeBDT]  ";

            string strGroupByCount = $" GROUP BY CAST(rh.[Timestamp] AS DATE),[ProductName],[ChargeAmount]";

            string strSelectionDetails = $"  distinct CAST(rh.[Timestamp] AS DATE) RenewalDate,MSISDN,Service_ID ServiceID,[ProductName] [rDescription],[ChargeAmount],'REN' ActionType, 'Charge' ActionMessage ";

            string strGroupByDetails = $" ";


            try
            {
                if (dataType?.ToLower() == "count")
                {
                    sql =   $"SELECT {strSelectionCount} \r\n" +
                            $" FROM [BL6D_DCBDB_Archive].[dbo].[tbl_BLSDP_CallBackDetails_Partitioned] AS rh WITH (NOLOCK) " +
                            $" INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] AS s ON s.SERVICE_ID = rh.SubscriptionOfferID  " +
                            $"WHERE rh.[SubscriptionOfferID] IN ('{serviceIds.Replace(",", "','")}') AND [CallBackValue5] = 'Charging success' " +
                            $" AND CAST(rh.[Timestamp] AS DATE) BETWEEN '{fromDate}'AND '{toDate}'  AND CallBackValue7='REN' " +
                            $" {strGroupByCount} " +
                            $"ORDER BY [RenewalDate],[rDescription]";                  


                }
                else if (dataType?.ToLower() == "details")
                {
                    sql =   $"SELECT {strSelectionDetails} \r\n" +
                            $" FROM [BL6D_DCBDB_Archive].[dbo].[tbl_BLSDP_CallBackDetails_Partitioned] AS rh WITH (NOLOCK) " +
                            $" INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] AS s ON s.SERVICE_ID = rh.SubscriptionOfferID  " +
                            $" WHERE rh.[SubscriptionOfferID] IN ('{serviceIds.Replace(",", "','")}') AND [CallBackValue5] = 'Charging success' " +
                            $" AND CAST(rh.[Timestamp] AS DATE) BETWEEN '{fromDate}'AND '{toDate}'  AND CallBackValue7='REN' " +
                            $" {strGroupByDetails} " +
                            $"ORDER BY [RenewalDate],[rDescription]";                    

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
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(u.Reg_Date AS DATE) AS Churndate,u.Service_ID,s.[SERVICE_NAME] AS [Description],COUNT(u.MSISDN) AS Churn,a.totalChurn ";

            string strGroupByCount = $" GROUP BY CAST(u.Reg_Date AS DATE) ,s.[SERVICE_NAME] ,u.Service_ID, a.totalChurn  ";

            string strSelectionDetails = $" DISTINCT CAST(u.LastUpdate AS DATE) Churndate ,MSISDN,s.SERVICE_ID,s.[SERVICE_NAME]  Description ,'ACT' ActionType, 'Churn Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $"SELECT {strSelectionCount}  " +
                   $"from [BL6D_DCBDB].[dbo].[UserSubscriptions] u INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] s " +
                   $"ON u.Service_ID=s.SERVICE_ID " +
                   $" CROSS APPLY (SELECT count(distinct MSISDN) totalChurn FROM [BL6D_DCBDB].[dbo].[UserSubscriptions] " +
                   $"WHERE [Reg_Status]=0 AND Service_Id = s.SERVICE_ID and cast(Reg_Date as date)<cast('{fromDate}' as date)) as a " +
                   $"WHERE u.Reg_Status=0  and u.Service_Id IN('{serviceIds.Replace(",", "','")}') " +
                   $"AND CAST(u.Reg_Date AS DATE) BETWEEN '{fromDate}' AND '{toDate}' " +
                   $" {strGroupByCount} " +
                   $"ORDER BY Churndate, [Description]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails}  " +
                 $"from [BL6D_DCBDB].[dbo].[UserSubscriptions] u INNER JOIN [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] s " +
                 $"ON u.Service_ID=s.SERVICE_ID " +
                 $"WHERE u.Reg_Status=0  and u.Service_Id IN('{serviceIds.Replace(",", "','")}') " +
                 $"AND CAST(u.Reg_Date AS DATE) BETWEEN '{fromDate}' AND '{toDate}' " +
                 $" {strGroupByDetails} " +
                 $"ORDER BY Churndate, [Description]";

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
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); 
            StringBuilder stringBuilder = new StringBuilder();
            var jsonArray = "";
            try
            {
                sql = $"EXEC [dbo].[sp_BL6D_DCBDBTrafficMonitor] 'BL Funbox DCB';";

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item)+",");
                }

                sql = $"EXEC [dbo].[sp_BL6D_DCBDBTrafficMonitor] 'BL Footballers DCB';";

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
        /// BL Admin Panel
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="reportDate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal string GetSubscriptionDetailsData(string msisdn, DateTime reportDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT\r\n    sub.MSISDN,\r\n    srv.SERVICE_NAME ServiceName,\r\n    srv.ProductName AS ServicePlan,srv.SERVICE_ID ServiceId,sub.AocTransID SubscriptionId,\r\n    " +
                    $"FORMAT(MAX(sub.Reg_Date), 'yyyy-MM-dd HH:mm:ss') AS FirstActivationDate,\r\n\t" +
                    $"FORMAT(MAX(sub.LastUpdate), 'yyyy-MM-dd HH:mm:ss') AS LastActivationDate,\r\n\t" +
                    $"MAX(dateadd(dd,ISNULL(srv.Duration,0),sub.Reg_Date) ) AS ExpiryDate\r\n\t" +
                    $",case when Reg_Status=0 then sub.LastUpdate end CancellationDate, sub.CancelReason CancelSource,\r\n    '' AS ActivationChannel,\r\n    sub.Reg_Status AS CurrentStatus" +
                    $",\r\n\r\n\tDNDStatus = CASE WHEN EXISTS (\r\n\t\t\t\t\tSELECT 1 \r\n\t\t\t\t\tFROM [BL6D_DCBDB].[dbo].[BlockedUsers] b \r\n\t\t\t\t\tWHERE b.MSISDN = sub.MSISDN \r\n\t\t\t\t\t" +
                    $"AND b.ServiceId = srv.SERVICE_ID\r\n\t\t\t\t\t) THEN 'Yes' ELSE 'No' END,\r\nCAST(CASE \r\n\t" +
                    $"WHEN (Validity like 'daily' OR Validity like 'One Day') AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE) " +
                    $"THEN DATEADD(DD,1,MAX(sub.Reg_Date)) \r\n\tWHEN Validity like 'week' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(DD,7,MAX(sub.Reg_Date)) \r\n\tWHEN Validity like 'month' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(MM,1,MAX(sub.Reg_Date)) \r\n\tWHEN Validity like 'year' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(YYYY,1,MAX(sub.Reg_Date)) \r\n\tend as Date) AS NextRenewal\r\nFROM\r\n    [BL6D_DCBDB].[dbo].[UserSubscriptions] AS sub\r\n    " +
                    $"CROSS APPLY (SELECT top 1 * FROM [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] x WHERE x.Service_Id = sub.Service_ID) AS srv \r\n" +
                    $"WHERE\r\n    sub.MSISDN = '{msisdn}'\r\n    AND sub.Reg_Date = (\r\n        " +
                    $"SELECT MAX(Reg_Date)\r\n        FROM [BL6D_DCBDB].[dbo].[UserSubscriptions]\r\n        WHERE [Service_ID] = sub.[Service_ID]\r\n          AND MSISDN = sub.MSISDN\r\n    )\r\n" +
                    $"GROUP BY\r\n    sub.MSISDN,\r\n    srv.SERVICE_NAME,\r\n    srv.ProductName,srv.SERVICE_ID,sub.AocTransID,Validity,\r\n    " +
                    $"sub.Reg_Status,case when Reg_Status=0 then sub.LastUpdate end,sub.CancelReason\r\n" +
                    $"ORDER BY FirstActivationDate DESC;"; 
                                  
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
            var dbName = "BanglalinkConnection";
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
            var dbName = "BanglalinkConnection";
            string affectedRows = "";
            try
            {
                sql = $"INSERT INTO [BL6D_DCBDB].[dbo].[BlockedUsers]([ServiceId],[Msisdn])\r\nVALUES ('{serviceId}','{msisdn}')";

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
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT  MSISDN\r\n\t,s.SERVICE_ID ServiceId\r\n\t,s.SERVICE_NAME ServiceName\r\n\t,s.SubscriptionType\r\n\t" +
                    $",s.ChargeAmount TotalAmountCharged\r\n\t,ISNULL(s.Duration,0) SubscriptionPeriod\r\n\t,rh.CallBackValue5 RenewalStatus\r\n\t" +
                    $",rh.[CallBackValue7] ChargingType,s.Validity Description\r\n\t,'' Currency\r\n\t,(rh.TimeStamp) ProcessDate\r\n\t,(rh.TimeStamp) LastUpdated\r\n" +
                    $"FROM [BL6D_DCBDB_Archive].[dbo].[tbl_BLSDP_CallBackDetails_Partitioned] AS rh WITH (NOLOCK)\r\n" +
                    $"CROSS APPLY (SELECT top 1 * FROM [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk] x WHERE x.Service_Id = rh.SubscriptionOfferID) AS s \r\n" +
                    $"WHERE rh.MSISDN = '{msisdn}' AND \r\n\t " +
                    $"CAST(rh.TimeStamp AS DATE) BETWEEN DATEADD(DD, - 30, '{reportDate}')\r\n\t\tAND CAST('{reportDate}' AS DATE)\t\t  \r\n" +
                    $"ORDER BY ProcessDate DESC,rh.MSISDN \r\n" ;

                   
                ds = cda.GetDataSet(sql, dbName);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }



            return CDA.DataTableToJSONWithJSONNet(dt);
        }


        internal string GetLiveRenewalData(string productName, int setTimeoutInterval)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT  cast(TIMESTAMP AS DATE) ReportDate\r\n\t\t\t,datepart(hh, [Timestamp]) ReportHour\r\n\t\t\t,datepart(MINUTE, [Timestamp]) ReportMinute\r\t\t\t,MAX(datepart(SECOND, [Timestamp])) ReportSecond\r\n\t\t\t,COUNT(case when [CallBackValue5] = 'Charging success' then 1 end) TotalSuccess\r\n\t\t\t,COUNT(MSISDN) TotalTraffic\r\n\t\tFROM (\r\n\t\t\tSELECT SubscriptionOfferID,[CallBackValue5]\r\n\t\t\t\t,MSISDN\r\n\t\t\t\t,[Timestamp]\r\n\t\t\tFROM [BL6D_DCBDB].[dbo].[tbl_BLSDP_CallBackDetails]  WITH (NOLOCK)\r\n\t\t\tWHERE \r\n\t\t\t\t SubscriptionOfferID IN (\r\n\t\t\t\t\tSELECT DISTINCT SERVICE_ID\r\n\t\t\t\t\tFROM [BL6D_DCBDB].[dbo].[tbl_BLSDP_ServiceMap_Gakk]  WITH (NOLOCK)\r\n\t\t\t\t\tWHERE [ProductName] LIKE '%'+'Funbox'+'%'\r\n\t\t\t\t\t)\r\n\t\t\t\tAND cast([Timestamp] AS DATETIME) BETWEEN CAST(DATEADD(SECOND, -(" + setTimeoutInterval + "/1000), GETDATE()) AS DATETIME) AND CAST(GETDATE() AS DATETIME)\r\n\t\t\t) r\r\n\t\tGROUP BY cast(TIMESTAMP AS DATE)\r\n\t\t\t,datepart(hh, [Timestamp]),datepart(MINUTE, [Timestamp])\t\t\t\r\n\t\t\tORDER BY cast(TIMESTAMP AS DATE)\r\n\t\t\t,datepart(hh, [Timestamp]),datepart(MINUTE, [Timestamp])";


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

        internal string GetTrafficDataMoreFun(string serviceIds, DateTime dateTime1, DateTime dateTime2)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BanglalinkConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            StringBuilder stringBuilder = new StringBuilder();
            var jsonArray = "";
            try
            {
                sql = $"EXEC [BL6D_DCBDB_Archive].[dbo].[sp_BL6D_DCBDBTrafficMonitor] 'BL Funbox DCB';";

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

            return stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);
        }
    }
}
