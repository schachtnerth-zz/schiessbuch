using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace schiessbuch
{
    public sealed class MySqlConnectionWrapper
    {
        private static volatile MySqlConnectionWrapper instance;
        private static object syncRoot = new Object();

        private MySqlConnection _conn;
        private string _connStr;

        private MySqlConnectionWrapper() {
            string strMySQLServer = Properties.Settings.Default.MySQLServer;
            _connStr = string.Format("server={0};user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true;Connection Timeout=1;", strMySQLServer);
            _conn = new MySqlConnection(_connStr);
            try
            {
                _conn.Open();
                _conn.Close();
            }
            catch(MySqlException mysqle)
            {
                    AlternativerMySQLServer altServer = new AlternativerMySQLServer();
                altServer.tbAlternativeServer.Text = strMySQLServer;
                altServer.tbAlternativeServer.Focus();
                DialogResult dr = altServer.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    strMySQLServer = altServer.tbAlternativeServer.Text;
                }
            }
            _connStr = string.Format("server={0};user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true;Connection Timeout=1;", strMySQLServer);
            _conn.ConnectionString = _connStr;
            try
            {
                _conn.Open();
            }
            catch (MySqlException)
            {
                MessageBox.Show("Eine Exception ist aufgetreten beim Öffnen der Datenbank!");
                Application.Exit();
            }

        }

        public static MySqlConnectionWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MySqlConnectionWrapper();
                    }
                }

                return instance;
            }
        }

        public bool IsConnected() { return _conn.State == System.Data.ConnectionState.Open; }
        public string getConnectionString() { return _connStr; }
        public MySqlConnection getConnectionObject() { return _conn; }
        public MySqlDataReader doMySqlReaderQuery (string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _conn);
            return cmd.ExecuteReader();
        }
        public object doMySqlScalarQuery(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _conn);
            return cmd.ExecuteScalar();
        }
        public void closeMySqlReaderQuery(MySqlDataReader reader)
        {
            reader.Close();
        }
    }
}
