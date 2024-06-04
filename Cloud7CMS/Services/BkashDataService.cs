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
    public class BkashDataService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BASE_URL_PAYMENT = "https://api.payment-app.info";
        private const string CANCEL_ENDPOINT = "/bkash/billing/cancel_subscription";
        private const string CANCEL_SECRET_KEY = "secretKeyb1DEYmhXrTYiyU65EWI8U1h";
        private const string CANCEL_IV_SECRET = "ivSec1HJFhYrhcr5";
        private const string X_API_KEY = "Sv0390cf388d6bc429d9fd09741b0abf7c8T";
        public BkashDataService() {           
        }    

        public string GetServices(string name)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                sql = $"SELECT DISTINCT SERVICE_ID  AS Value, Service_Name AS Text FROM [CloudPaymentDB].[dbo].[tbl_ServiceMap]\r\nWHERE Service_ID IN ('4057','4058','4059','4010','4011','4012')\r\nORDER BY SERVICE_ID ASC";
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
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(s.Reg_Date AS DATE) SubscriptionDate,sm.Service_Name [sDescription],s.Service_ID,sc.ChargeAmount sPrice,COUNT(*) [sCount],COUNT(*) * sc.ChargeAmount [sChargeBDT] ";

            string strGroupByCount = $" GROUP BY CAST(s.Reg_Date AS DATE),sm.Service_Name,s.Service_ID,sc.ChargeAmount";

            string strSelectionDetails = $" DISTINCT CAST(s.Reg_Date AS DATE) SubscriptionDate ,MSISDN,Service_ID,sm.Service_Name [sDescription],'ACT' ActionType, 'New Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $";WITH cte_totalbase\r\nAS (\r\n\t" +
                       $"SELECT Service_ID,count(DISTINCT MSISDN) totalBase\r\n\t" +
                       $"FROM [CloudPaymentDB].[dbo].[tbl_Subscription]\r\n\t" +
                       $"WHERE Reg_Status = 1 AND cast(Reg_Date AS DATE) < cast('{fromDate}' AS DATE)\r\n\t" +
                       $"GROUP BY Service_ID\t\r\n\t)\r\n\t,cte_current_activation\r\nAS (\r\n" +
                       $"SELECT {strSelectionCount}\r\n" +
                       $" FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS s\r\n" +
                       $"CROSS APPLY (\r\n\tSELECT Service_Name\r\n\tFROM [CloudPaymentDB].[dbo].[tbl_ServiceMap]\r\n\t" +
                       $"WHERE Service_ID = s.Service_ID\r\n\t) sm\r\nCROSS APPLY (\r\n\t" +
                       $"SELECT TRY_PARSE(ChargeAmount AS INT) ChargeAmount\r\n\tFROM [CloudPaymentDB].[dbo].[tbl_ServiceCharge]\r\n\t" +
                       $"WHERE Service_ID = s.Service_ID\r\n\t) sc\r\nWHERE s.Reg_Status = 1\r\n\t" +
                       $"AND s.Service_Id IN ('{serviceIds.Replace(",", "','")}')\r\n\t" +
                       $"AND CAST(s.Reg_Date AS DATE) BETWEEN '{fromDate}'\r\n\t\tAND '{toDate}'\r\n" +
                       $"{strGroupByCount})\r\n\r\n" +
                       $"SELECT SubscriptionDate,sDescription,y.totalBase,sPrice,[sCount],[sChargeBDT] FROM cte_current_activation x\r\n" +
                       $"INNER JOIN cte_totalbase y ON x.Service_ID = y.Service_ID\r\nORDER BY CAST(SubscriptionDate AS DATE)\r\n\t" +
                       $",[sDescription]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT \r\n{strSelectionDetails} \r\n" +
                        $"FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS s\r\n" +
                        $"CROSS APPLY (\r\n\tSELECT Service_Name\r\n\tFROM [CloudPaymentDB].[dbo].[tbl_ServiceMap]\r\n\t" +
                        $"WHERE Service_ID = s.Service_ID\r\n\t) sm\r\n" +
                        $"WHERE s.Reg_Status = 1\r\n\tAND s.Service_Id IN ('{serviceIds.Replace(",", "','")}')\r\n\t" +
                        $"AND CAST(s.Reg_Date AS DATE) BETWEEN '{fromDate}'\r\n\t\tAND '{toDate}'";
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
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(np.LastUpdate AS DATE) RenewalDate,sm.Service_Name [rDescription],sc.ChargeAmount rPrice\r\n" +
                $",COUNT(*) [rCount],COUNT(*) * sc.ChargeAmount [rChargeBDT]  ";

            string strGroupByCount = $" GROUP BY CAST(np.LastUpdate AS DATE),sm.Service_Name,sc.ChargeAmount";

            string strSelectionDetails = $"  DISTINCT np.LastUpdate RenewalDate,MSISDN,ServiceID,sm.Service_Name [rDescription],ChargeAmount,'REN' ActionType, 'Charge' ActionMessage ";

            string strGroupByDetails = $" ";


            try
            {
                if (dataType?.ToLower() == "count")
                {
                    sql = $"SELECT  {strSelectionCount} \r\n" +
                    $"FROM [CloudPaymentDB].[dbo].[tbl_bKInitiatePaymentReQuery] ipr WITH (NOLOCK) \r\n" +
                    $"INNER JOIN \r\n[CloudPaymentDB_Archive].[dbo].[tbl_bKNotifyPayment_Partitioned] np WITH (NOLOCK)  ON np.SubsRequestID=ipr.InvoiceID\r\n" +
                    $"CROSS APPLY (select Service_Name FROM [CloudPaymentDB].[dbo].[tbl_ServiceMap] WHERE Service_ID=ipr.ServiceID) sm\r\n" +
                    $"CROSS APPLY (select TRY_PARSE(ChargeAmount as int) ChargeAmount from [CloudPaymentDB].[dbo].[tbl_ServiceCharge] " +
                    $"WHERE Service_ID=ipr.ServiceID) sc\r\n" +
                    $"WHERE ipr.[ServiceId] IN ('{serviceIds.Replace(",", "','")}') AND PaymentStatus like '%SUCCEEDED_PAYMENT%' \r\n" +
                    $"AND CAST(np.LastUpdate AS DATE) BETWEEN  '{fromDate}'AND '{toDate}' \r\n" +
                    $"AND MSISDN NOT IN (SELECT DISTINCT MSISDN FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS s WHERE s.Reg_Status = 1 " +
                    $"AND s.Service_Id IN ('{serviceIds.Replace(",", "','")}') AND CAST(s.Reg_Date AS DATE) = CAST(np.LastUpdate AS DATE)) \r\n" +
                    $" {strGroupByCount} " + 
                    $"ORDER BY [RenewalDate],[rDescription]";
                   
                }
                else if (dataType?.ToLower() == "details")
                {
                    sql = $"SELECT  {strSelectionDetails} \r\n" +
                    $"FROM [CloudPaymentDB].[dbo].[tbl_bKInitiatePaymentReQuery] ipr WITH (NOLOCK) \r\n" +
                    $"INNER JOIN \r\n[CloudPaymentDB_Archive].[dbo].[tbl_bKNotifyPayment_Partitioned] np WITH (NOLOCK)  ON np.SubsRequestID=ipr.InvoiceID\r\n" +
                    $"CROSS APPLY (select Service_Name FROM [CloudPaymentDB].[dbo].[tbl_ServiceMap] WHERE Service_ID=ipr.ServiceID) sm\r\n" +
                    $"CROSS APPLY (select TRY_PARSE(ChargeAmount as int) ChargeAmount from [CloudPaymentDB].[dbo].[tbl_ServiceCharge] " +
                    $"WHERE Service_ID=ipr.ServiceID) sc\r\n" +
                    $"WHERE ipr.[ServiceId] IN ('{serviceIds.Replace(",", "','")}') AND PaymentStatus like '%SUCCEEDED_PAYMENT%' \r\n" +
                    $"AND CAST(np.LastUpdate AS DATE) BETWEEN  '{fromDate}'AND '{toDate}' \r\n" +
                    $"AND MSISDN NOT IN (SELECT DISTINCT MSISDN FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS s WHERE s.Reg_Status = 1 " +
                    $"AND s.Service_Id IN ('{serviceIds.Replace(",", "','")}') AND CAST(s.Reg_Date AS DATE) = CAST(np.LastUpdate AS DATE)) \r\n" +
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
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string strSelectionCount = $" CAST(g.LastUpdate AS DATE) AS Churndate\r\n,g.Service_Id,sm.Service_Name AS [Description],COUNT(g.MSISDN) AS Churn, a.totalChurn ";

            string strGroupByCount = $" GROUP BY CAST(g.LastUpdate AS DATE) ,sm.Service_Name ,g.Service_Id, a.totalChurn  ";

            string strSelectionDetails = $" DISTINCT CAST(LastUpdate AS DATE) Churndate ,MSISDN,Service_ID,Service_Name Description ,'ACT' ActionType, 'Churn Activation' ActionMessage ";

            string strGroupByDetails = $" ";

            try
            {
                if (dataType.ToLower() == "count")
                {
                    sql = $"SELECT  {strSelectionCount} \r\n" +
                       $"FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS g  \r\n" +
                       $"CROSS APPLY (select Service_Name FROM [CloudPaymentDB].[dbo].[tbl_ServiceMap] WHERE Service_ID=g.Service_ID) sm\r\n" +
                       $" CROSS APPLY (SELECT count(distinct MSISDN) totalChurn FROM [CloudPaymentDB].[dbo].[tbl_Subscription] " +
                       $"WHERE [Reg_Status]=0 AND Service_ID = g.SERVICE_ID and cast(LastUpdate as date)<cast('{fromDate}' as date)) as a " +
                       $"WHERE g.Reg_Status=0 and g.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\n" +
                       $"AND CAST(g.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' \r\n" +
                       $" {strGroupByCount} \r\n" +
                       $"ORDER BY Churndate, [Description]";
                }
                else if (dataType.ToLower() == "details")
                {
                    sql = $"SELECT {strSelectionDetails}\r\n" +
                        $"FROM [CloudPaymentDB].[dbo].[tbl_Subscription] AS g  \r\n" +
                        $"CROSS APPLY (select Service_Name FROM [CloudPaymentDB].[dbo].[tbl_ServiceMap] WHERE Service_ID=g.Service_ID) sm\r\n " +
                        $"CROSS APPLY (SELECT count(distinct MSISDN) totalChurn FROM [CloudPaymentDB].[dbo].[tbl_Subscription] " +
                        $"WHERE [Reg_Status]=0 AND Service_ID = g.SERVICE_ID and cast(LastUpdate as date)<cast('2/7/2024 12:00:00 AM' as date)) as a " +
                        $"WHERE g.Reg_Status=0 and g.Service_ID IN('{serviceIds.Replace(",", "','")}') \r\n" +
                        $"AND CAST(g.LastUpdate AS DATE) BETWEEN '{fromDate}' AND '{toDate}' \r\n" +
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
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            StringBuilder stringBuilder = new StringBuilder();
            var jsonArray = "";
            try
            {
                sql = $"EXEC [dbo].[sp_BkashTrafficMonitorCloud7] 'Bkash Funbox DCB';";

                ds = cda.GetDataSet(sql, dbName);

                foreach (DataTable item in ds.Tables)
                {
                    stringBuilder.Append(CDA.DataTableToJSONWithJSONNet(item)+",");
                }

                sql = $"EXEC [dbo].[sp_BkashTrafficMonitorCloud7] 'Bkash MXPlayer DCB';";

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
        /// bKash Admin Panel
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="reportDate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal string GetSubscriptionDetailsData(string msisdn, DateTime reportDate)
        {
            CDA cda = new CDA();
            var sql = "";
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $";;WITH cte_msisdn as (select top 1 SubscriptionID from [CloudPaymentDB_Archive].[dbo].[tbl_bKNotifyPayment_01] WHERE Payer like '%{msisdn.Replace("8801","")}%')" +
                    $" SELECT\r\n    sub.MSISDN,\r\n    srv.SERVICE_NAME ServiceName,\r\n    '' AS ServicePlan,srv.SERVICE_ID ServiceId" +
                    $",sub.AocTransID SubscriptionId,\r\n    MAX(sub.Reg_Date) AS ActivationDate,\r\n FORMAT(MAX(sub.Reg_Date), 'HH:mm:ss') AS ActivationTime,\r\n\t" +
                    $"MAX(dateadd(dd,0,sub.Reg_Date) ) AS ExpiryDate\r\n\t,case when Reg_Status=0 then sub.LastUpdate end CancellationDate" +
                    $", sub.Reg_Description CancelSource,\r\n    sub.Reg_Source AS ActivationChannel,\r\n    sub.Reg_Status AS CurrentStatus,\r\n\r\n\t" +
                    $"DNDStatus = CASE WHEN EXISTS (\r\n\t\t\t\t\tSELECT 1 \r\n\t\t\t\t\tFROM [CloudPaymentDB].[dbo].[BlockedUsers] b \r\n\t\t\t\t\t" +
                    $"WHERE b.MSISDN = sub.MSISDN \r\n\t\t\t\t\tAND b.ServiceId = srv.SERVICE_ID\r\n\t\t\t\t\t) THEN 'Yes' ELSE 'No' END,\r\nCAST(CASE \r\n\t" +
                    $"WHEN (srv.SERVICE_NAME like 'daily' OR srv.SERVICE_NAME like 'One Day') AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=" +
                    $"CAST('{reportDate}' as DATE) THEN DATEADD(DD,1,MAX(sub.Reg_Date)) \r\n\tWHEN srv.SERVICE_NAME like 'week' AND MAX(CASE WHEN Reg_Status=0 " +
                    $"THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  THEN DATEADD(DD,7,MAX(sub.Reg_Date)) \r\n\t" +
                    $"WHEN srv.SERVICE_NAME like 'month' AND MAX(CASE WHEN Reg_Status=0 THEN LastUpdate end)>=CAST('{reportDate}' as DATE)  " +
                    $"THEN DATEADD(MM,1,MAX(sub.Reg_Date)) \r\n\tWHEN srv.SERVICE_NAME like 'year' AND MAX(CASE WHEN Reg_Status=0 THEN " +
                    $"LastUpdate end)>=CAST('{reportDate}' as DATE)  THEN DATEADD(YYYY,1,MAX(sub.Reg_Date)) \r\n\tend as Date) AS NextRenewal\r\nFROM\r\n    " +
                    $"[CloudPaymentDB].[dbo].[tbl_Subscription] AS sub\r\nJOIN\r\n    " +
                    $"[CloudPaymentDB].[dbo].tbl_ServiceMap AS srv ON srv.SERVICE_ID = sub.Service_ID\r\nWHERE\r\n    (sub.MSISDN = '{msisdn}' OR sub.AocTransID = (select SubscriptionID from cte_msisdn))  AND\r\n     " +
                    $"sub.Reg_Date = (\r\n" +
                    $"SELECT MAX(Reg_Date)\r\nFROM [CloudPaymentDB].[dbo].[tbl_Subscription]\r\nWHERE [Service_ID] = sub.[Service_ID]\r\n          " +
                    $"AND MSISDN = sub.MSISDN\r\n)\r\nGROUP BY\r\n sub.MSISDN,\r\n    srv.SERVICE_NAME,\r\n    srv.SERVICE_ID,sub.AocTransID,sub.Reg_Source,\r\n" +
                    $"    sub.Reg_Status,case when Reg_Status=0 then sub.LastUpdate end,sub.Reg_Description ORDER BY 6;";

                   
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
            var dbName = "BkashConnection";
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
            var dbName = "BkashConnection";
            string affectedRows = "";
            try
            {
                sql = $"INSERT INTO [CloudPaymentDB].[dbo].[BlockedUsers]([ServiceId],[Msisdn])\r\nVALUES ('{serviceId}','{msisdn}')";

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
            var dbName = "BkashConnection";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(); ;

            try
            {
                sql = $"SELECT ipr.MSISDN\r\n\t,s.SERVICE_ID ServiceId\r\n\t,s.SERVICE_NAME ServiceName\r\n\t,s.ServiceType SubscriptionType\r\n\t" +
                    $",rh.ChargedAmount TotalAmountCharged\r\n\t,CASE \r\n\tWHEN s.Service_Name like '%Daily%' THEN 1 \r\n\tWHEN s.Service_Name like '%Weekly%' THEN 7\r\n\t" +
                    $"WHEN s.Service_Name like '%Monthly%' THEN 30\r\n\tWHEN s.Service_Name like '%Yearly%' THEN 365\r\n\tEND  SubscriptionPeriod\r\n\t" +
                    $",rh.PaymentStatus RenewalStatus\r\n\t,ipr.Remarks Description\r\n\t,'' Currency\r\n\t,rh.TransactionDate ProcessDate\r\n\t,rh.LastUpdate LastUpdated\r\n" +
                    $"FROM [CloudPaymentDB].[dbo].[tbl_bKInitiatePaymentReQuery] ipr WITH (NOLOCK)\r\n" +
                    $"INNER JOIN [CloudPaymentDB_Archive].[dbo].[tbl_bKNotifyPayment_Partitioned] rh WITH (NOLOCK) ON rh.SubsRequestID = ipr.InvoiceID\r\n" +
                    $"INNER JOIN [CloudPaymentDB].[dbo].[tbl_ServiceMap] AS s ON s.Service_Id = ipr.ServiceId\r\n" +
                    $"WHERE (ipr.MSISDN = '{msisdn}' OR rh.Payer like '%{msisdn.Replace("8801","")}%') AND\r\n\t " +
                    $"CAST(rh.LastUpdate AS DATE) BETWEEN DATEADD(DD, - 30, '{reportDate}')\r\n\t\tAND CAST('{reportDate}' AS DATE)\r\n" +
                    $"ORDER BY ProcessDate DESC ,rh.Payer ;";
                    
                   

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
                { "userCode", msisdn },
                { "serviceId", serviceId }
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
