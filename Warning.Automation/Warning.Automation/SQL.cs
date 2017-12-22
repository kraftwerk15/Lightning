using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;

namespace Thunder
{
    /// <summary>
    /// 
    /// </summary>
    public class SQL
    {
        /// <summary>
        /// Open a SQL Connection.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        public static SqlConnection ConnectionOpen(string dbConnection)
        {
            SqlConnection newConn = new SqlConnection(dbConnection);
            if (dbConnection != null)
            {
                try
                {
                    newConn.Open();
                }
                catch
                {
                    throw new ArgumentException("Database Connection Failed");
                }
            }
            return newConn;
        }
        /// <summary>
        /// Create a string to use as an INSERT INTO command for SQL Databases.
        /// </summary>
        /// <param name="Table">The path as a string to the Table.</param>
        /// <param name="Columns">The column names as a list of strings.</param>
        /// <param name="Values">The values to be inserted as a list of list.</param>
        /// <returns>A string containing the INSERT INTO commmand.</returns>
        /// <search>sql, database, insert, inject, add</search>

        public static List<string> InsertInto(string Table, string[] Columns, dynamic[][] Values)
        {
            List<string> holding = new List<string>();
            string start = "INSERT INTO ";
            string table = start + Table + "(";
            string column = "";
            string lastColumn = Columns.Last();
            foreach (var k in Columns)
            {
                if (k.Equals(lastColumn))
                    column = column + k + ") VALUES (";
                else
                    column = column + k + ",";
            }
            table = table + column;
            dynamic lastValue = Values.Last();
            foreach(var i in Values)
            {
                string value = "";
                dynamic hold = i.Last();
                foreach (var c in i)
                {
                    if (c.Equals(hold))
                        value = value + "\'" + c + "\');";
                    else
                        value = value + "\'" + c + "\',";
                }
                holding.Add(table + value);
            }
            return holding;
        }
        /// <summary>
        /// Select from a Database some Data.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="statement"></param>
        /// <returns>Returns information as strings.</returns>
        public static List<dynamic> Select(SqlConnection Connection, string statement)
        {
            List<dynamic> tempConn = new List<dynamic>();
            SqlCommand getRunStat = new SqlCommand(statement, Connection);
            SqlDataReader theReader = null;
            try
            {
                theReader = getRunStat.ExecuteReader();
                while (theReader.Read())
                {
                    object temp = theReader[0];
                    tempConn.Add(temp);
                }
            }
            catch
            {
                throw new ArgumentException("Database Connected Successfully, but reading the Connection Failed.");
            }
            return tempConn;
        }

        internal static List<int> InsertInto(SqlConnection newConn, string prjNum, DateTime dateTime)
        {
            List<int> type = new List<int>();
            string sql = "INSERT INTO drRunStat(rsDateStart) VALUES (@param2)";
            SqlCommand set = new SqlCommand(sql, newConn);
            set.Parameters.Add("@param2", SqlDbType.DateTime).Value = dateTime;
            set.CommandType = CommandType.Text;
            int id = set.ExecuteNonQuery();
            type.Add(id);
            string sqlPrj = "INSERT INTO drPrjStat(psPrjNumber,psRunKey) VALUES (@param2,@param3)";
            SqlCommand setPrj = new SqlCommand(sqlPrj, newConn);
            setPrj.Parameters.Add("@param2", SqlDbType.VarChar).Value = prjNum;
            setPrj.Parameters.Add("@param3", SqlDbType.Int).Value = id;
            setPrj.CommandType = CommandType.Text;
            int backID = setPrj.ExecuteNonQuery();
            type.Add(backID);
            return type;
        }

        internal static void Update(DateTime dateEx, int key, SqlConnection newConn)
        {
            try
            {
                SqlCommand update = new SqlCommand();
                update.CommandText = "UPDATE drRunStat SET rsDateEnd = @dateCom, rsRunFailure = @fail WHERE rsDateStart = @dateEx and rsKey = @Key";
                DateTime dateCom = DateTime.Now;

                using (update)
                {

                    update.Parameters.Add("@dateCom", SqlDbType.DateTime).Value = dateCom;
                    update.Parameters.Add("@fail", SqlDbType.Bit).Value = true;
                    update.Parameters.Add("@dateEx", SqlDbType.DateTime).Value = dateEx;
                    update.Parameters.Add("@Key", SqlDbType.Int).Value = key;
                    update.ExecuteNonQuery();
                    //consider tracking rows affected
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine("SQL Line 154 : " + e.ToString());
                //throw new ArgumentException("Database Could Not Accept the Insert as Requested. " + e);
            }
        }

        /// <summary>
        /// Closes a SQL Connection.
        /// </summary>
        /// <param name="newConn">The Connection as a SQLConnection object.</param>
        public static void ConnectionClose(SqlConnection newConn)
        {
            try
            {
                newConn.Close();
            }
            catch (Exception)
            {
                throw new ArgumentException("Closing the Database Connection Failed");
            }
        }

    }
}
