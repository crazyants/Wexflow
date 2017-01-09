using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Threading;
using System.Xml.Linq;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using Npgsql;
using System.IO;
using System.Data.OleDb;
using Teradata.Client.Provider;

namespace Wexflow.Tasks.Sql
{
    public enum Type
    {
        SqlServer,
        Access,
        Oracle,
        MySql,
        Sqlite,
        PostGreSql,
        Teradata
    }

    // TODO Oracle, Teradata, Access?

    public class Sql:Task
    {
        public Type DbType {get; private set;}
        public string ConnectionString { get; private set; }
        public string SqlScript { get; private set; }

        public Sql(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.DbType = (Type)Enum.Parse(typeof(Type), this.GetSetting("type"), true);
            this.ConnectionString = this.GetSetting("connectionString");
            this.SqlScript = this.GetSetting("sql", string.Empty);
        }

        public override void Run()
        {
            this.Info("Executing SQL scripts...");

            // Execute this.SqlScript if necessary
            try
            {
                if (!string.IsNullOrEmpty(this.SqlScript))
                {
                    ExecuteSql(this.SqlScript);
                    this.Info("The script has been executed through the sql option of the task.");
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.ErrorFormat("An error occured while executing sql script. Error: {0}", e.Message);
            }

            // Execute SQL files scripts
            foreach (FileInf file in this.SelectFiles())
            {
                try
                {
                    string sql = File.ReadAllText(file.Path);
                    ExecuteSql(sql);
                    this.InfoFormat("The script {0} has been executed.", file.Path);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while executing sql script {0}. Error: {1}", file.Path, e.Message);
                }
            }

            this.Info("Task finished.");
        }

        private void ExecuteSql(string sql)
        {
            switch (this.DbType)
            {
                case Type.SqlServer:
                    using (SqlConnection conn = new SqlConnection(this.ConnectionString))
                    {
                        SqlCommand comm = new SqlCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.Access:
                    using (OleDbConnection conn = new OleDbConnection(this.ConnectionString))
                    {
                        OleDbCommand comm = new OleDbCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.Oracle:
                    using (OracleConnection conn = new OracleConnection(this.ConnectionString))
                    {
                        OracleCommand comm = new OracleCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.MySql:
                    using (MySqlConnection conn = new MySqlConnection(this.ConnectionString))
                    {
                        MySqlCommand comm = new MySqlCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.Sqlite:
                    using (SQLiteConnection conn = new SQLiteConnection(this.ConnectionString))
                    {
                        SQLiteCommand comm = new SQLiteCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.PostGreSql:
                    using (NpgsqlConnection conn = new NpgsqlConnection(this.ConnectionString))
                    {
                        NpgsqlCommand comm = new NpgsqlCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
                case Type.Teradata:
                    using (TdConnection conn = new TdConnection(this.ConnectionString))
                    {
                        TdCommand comm = new TdCommand(sql, conn);
                        conn.Open();
                        comm.ExecuteNonQuery();
                    }
                    break;
            }
        }
    }
}
