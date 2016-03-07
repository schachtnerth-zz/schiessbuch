using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace schiessbuch
{
    public partial class ManuellNachtragen : Form
    {
        public ManuellNachtragen()
        {
            InitializeComponent();
        }

        public string id;

        private void nachtragen_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("INSERT INTO schiessbuch (id, disziplin, ergebnis, datum, stand, status, session) VALUES (@id,@disziplin,@ergebnis,@datum,@stand,@status,@session)", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@disziplin", disziplin.SelectedItem);
            cmd.Parameters.AddWithValue("@ergebnis", ergebnis.Text);
            CultureInfo ci = new CultureInfo("en-US");
            cmd.Parameters.AddWithValue("@datum", datum.Value.ToString("ddd MMM dd yyyy 00:00:00 G\\MT +0000 (G\\MT)", ci));
            cmd.Parameters.AddWithValue("@stand", "extern");
            cmd.Parameters.AddWithValue("@status", "manuell");
            cmd.Parameters.AddWithValue("@session", Guid.NewGuid().ToString());
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
            conn.Dispose();
            Close();
        }
    }
}
