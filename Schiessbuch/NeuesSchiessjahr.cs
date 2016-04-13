using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace schiessbuch
{
    public partial class NeuesSchiessjahr : Form
    {
        public NeuesSchiessjahr()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ein neues Schießjahr soll in der Datenbank angelegt werden
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            try
            { 
                conn.Open();
                if (SchiessjahrTextbox.Text.Length != 0)
                {
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO schiessjahr (Schiessjahr, SchiessjahrBeginn) VALUES (@Schiessjahr, @SchiessjahrBeginn)", conn);
                    cmd.Prepare();
                    cmd.Parameters.Add("@Schiessjahr", MySqlDbType.VarChar, 45).Value = SchiessjahrTextbox.Text;
                    cmd.Parameters.Add("@SchiessjahrBeginn", MySqlDbType.Date).Value = SchiessjahrBeginn.Value.Date;
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                conn.Dispose();
                MySqlConnection.ClearPool(conn);
                Close();
            }
            catch (MySqlException mysqle)
            {
                MessageBox.Show("Ein Fehler ist aufgetreten beim Anlegen eines neuen Schießjahres (" + mysqle.Message + ").");
            }
        }
    }
}
