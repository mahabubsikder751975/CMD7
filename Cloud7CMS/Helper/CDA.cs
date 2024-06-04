using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cloud7CMS.Helper
{
    public class CDA
    {
        Hashtable connTable = new Hashtable();
        SqlConnection myConnection = null;
        SqlCommand cmd = null;
        SqlDataAdapter adapter = null;
        DataSet ds = null;
        public CDA()
        {

            connTable.Add("TelenorConnection", "Data Source=167.71.213.46,7953;Initial Catalog=TelenorDOB;User=Grameenphone; Password=P@$$phr@se#2023;Connect Timeout=120;");
            connTable.Add("BanglalinkConnection", "Data Source=167.71.213.46,7953;Initial Catalog=BL6D_DCBDB;User=Banglalink; Password=5Tr0ngP@$$!word;Connect Timeout=120;");
            connTable.Add("RobiConnection", "Data Source=159.223.40.204,1433;Initial Catalog=CelcomDB;User=Robi; Password=S3cureP@ss#2023;Connect Timeout=120;");
            connTable.Add("BkashConnection", "Data Source=27.131.15.19;Initial Catalog=CloudPaymentDB;User=Bkash; Password=C0mpl3x#Passw0rd;Connect Timeout=120;");

        }
        public string RawConnectionStr()
        {
            const string ConStr = "Data Source=167.71.213.46,7953;Initial Catalog=TelenorDOB;User=Grameenphone; Password=P@$$phr@se#2023;Connect Timeout=120;";
            return ConStr;
        }
        public SqlDataReader getList(string query, string dbName)
        {
            myConnection = new SqlConnection(connTable[dbName].ToString());
            SqlCommand cmd = new SqlCommand(query, myConnection);
            SqlDataReader dr;
            myConnection.Open();
            dr = cmd.ExecuteReader();
            cmd = null;
            return dr;
        }
        public string ExecuteNonQuery(string query, string dbName)
        {
            string rValue = string.Empty;
            myConnection = new SqlConnection(connTable[dbName].ToString());
            try
            {
                cmd = new SqlCommand(query, myConnection);
                myConnection.Open();
                rValue = cmd.ExecuteNonQuery().ToString();

                if (rValue == "-1") { throw new Exception(); }
            }
            catch (Exception ex)
            {
                rValue = ex.Message.ToString();

            }
            finally
            {
                myConnection.Close();
                cmd = null;
                myConnection = null;
                query = null;
            }
            return rValue;
        }
        public object getSingleValue(string query, string dbName)
        {
            myConnection = new SqlConnection(connTable[dbName].ToString());
            try
            {
                cmd = new SqlCommand(query, myConnection);
                myConnection.Open();
                object retValue = cmd.ExecuteScalar();
                return retValue;
            }
            catch (Exception ex)
            {
                return (object)ex.Message.ToString();
            }
            finally
            {
                myConnection.Close();
                cmd = null;
                myConnection = null;
                query = null;
                dbName = null;
            }
        }
        public bool IsNumeric(string strTextEntry)
        {
            bool bIsNumeric = true;
            try
            {
                System.Text.RegularExpressions.Regex objNotWholePattern = new Regex("[^0-9]");
                bIsNumeric = !objNotWholePattern.IsMatch(strTextEntry);
            }
            catch
            {
                bIsNumeric = false;
            }
            return bIsNumeric;
        }
        public DataSet GetDataSet(string query, string dbName)
        {
            myConnection = new SqlConnection(connTable[dbName].ToString());
            ds = new DataSet();
            try
            {
                cmd = new SqlCommand(query, myConnection);
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                return ds;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                adapter = null;
                cmd = null;
                myConnection = null;
                query = null;
            }
        }
        public string[] SplitByString(string testString, string split)
        {
            int offset = 0;
            int index = 0;
            int[] offsets = new int[testString.Length + 1];
            while (index < testString.Length)
            {
                int indexOf = testString.IndexOf(split, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + split.Length);
                }
                else
                {
                    index = testString.Length;
                }
            }
            string[] final = new string[offset + 1];
            if (offset == 0)
            {
                final[0] = testString;
            }
            else
            {
                offset--;
                final[0] = testString.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = testString.Substring(offsets[i] + split.Length, offsets[i + 1] - offsets[i] - split.Length);
                }
                final[offset + 1] = testString.Substring(offsets[offset] + split.Length);
            }
            return final;
        }

        public static string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
    }
}
