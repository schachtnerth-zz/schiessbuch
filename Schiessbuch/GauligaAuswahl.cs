using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace schiessbuch
{
    public partial class GauligaAuswahlDlg : Form
    {
        public GauligaAuswahlDlg()
        {
            InitializeComponent();
        }

        private string aktuellesDatum;

        private string mySqlDateTimeFromGermanDate(string germanDate)
        {
            return DateTime.ParseExact(germanDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string connStr;

        private void GauligaAuswahl_Load(object sender, EventArgs e)
        {
            //connStr = "server = localhost; user id = siusclub; password = siusclub; database = siusclub; persistsecurityinfo = True; Allow User Variables = true";
            connStr = "server = localhost; user id = siusclub; password = siusclub; database = siusclub; persistsecurityinfo = True; Allow User Variables = true";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            // Zeile für Zeile sowohl bei der Heim- als auch bei der Gastmannschaft durchgehen und überprüfen, ob sie in der Datenbank stehen
            int ZeilenGesamt = GauligaDGVGastverein.Rows.Count + GauligaDGVHeimverein.Rows.Count;
            //List<string> sessions = new List<string>();
            string dts;
            foreach (DataGridViewRow row in GauligaDGVHeimverein.Rows)
            {
                string thisSession = row.Cells["HeimSession"].Value.ToString();
                dts = mySqlDateTimeFromGermanDate(row.Cells["DatumHeim"].Value.ToString()); // DateTime.ParseExact(row.Cells["DatumHeim"].Value.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                aktuellesDatum = dts;
                string sql = "SELECT idschuetze, ergebnis, datum, SE, session FROM gauliga WHERE idschuetze='" + row.Cells["idschuetzeHeim"].Value.ToString() + "' AND ergebnis='" + row.Cells["ergebnisHeim"].Value.ToString() + "' AND datum='" + dts + "' AND session='" + row.Cells["HeimSession"].Value.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    row.Cells["HeimWertung"].Value = "T";
                    if (reader["SE"] == DBNull.Value) row.Cells["SE_Heim"].Value = "";
                    else
                    {
                        if (reader["SE"].ToString().Equals("S"))
                            row.Cells["SE_Heim"].Value = "Stammschütze";
                        if (reader["SE"].ToString().Equals("E"))
                            row.Cells["SE_Heim"].Value = "Ersatzschütze";
                    }
                }
                reader.Close();
                reader.Dispose();
                cmd.Dispose();
            }

            dts=""; // Wird bei ersten Schützen des Heimvereins gesetzt

            foreach (DataGridViewRow row in GauligaDGVGastverein.Rows)
            {
                string thisSession = row.Cells["GastSession"].Value.ToString();
                dts = mySqlDateTimeFromGermanDate(row.Cells["DatumGast"].Value.ToString()); // DateTime.ParseExact(row.Cells["DatumGast"].Value.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                string sql = "SELECT idschuetze, ergebnis, datum, SE, session FROM gauliga WHERE idschuetze='" + row.Cells["idschuetzeGast"].Value.ToString() + "' AND ergebnis='" + row.Cells["ergebnisGast"].Value.ToString() + "' AND datum='" + dts + "' AND session='" + row.Cells["GastSession"].Value.ToString() + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    row.Cells["GastWertung"].Value = "T";
                    if (reader["SE"] == DBNull.Value) row.Cells["SE_Gast"].Value = "";
                    else
                    {
                        if (reader["SE"].ToString().Equals("S"))
                            row.Cells["SE_Gast"].Value = "Stammschütze";
                        if (reader["SE"].ToString().Equals("E"))
                            row.Cells["SE_Gast"].Value = "Ersatzschütze";
                    }
                }
                reader.Close();
                reader.Dispose();
                cmd.Dispose();
            }

            {
                string sql = "SELECT RundeNr, Gruppe FROM gauligameta WHERE datum='" + dts + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["RundeNr"] != DBNull.Value)
                        tbRunde.Text = reader["RundeNr"].ToString();
                    if (reader["Gruppe"] != DBNull.Value)
                        tbGruppe.Text = reader["Gruppe"].ToString();
                }
                reader.Close();
                reader.Dispose();
            }

            conn.Close();
            conn.Dispose();
        }

        private void GauligaDGVHeimverein_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == HeimWertung.Index  && e.RowIndex != -1)
            {
                GauligaDGVHeimverein.EndEdit();
            }
        }

        private void GauligaDGVHeimverein_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == HeimWertung.Index && e.RowIndex != -1)
            {
                if (GauligaDGVHeimverein[e.ColumnIndex, e.RowIndex].Value.ToString() == "T")
                {
                    // schau, ob mehr als 4 Einträge angehakt sind. Wenn ja, unchecke box
                    int numSchuetzen = 0;
                    foreach (DataGridViewRow row in GauligaDGVHeimverein.Rows)
                    {
                        if (row.Cells["HeimWertung"].Value != null && row.Cells["HeimWertung"].Value.ToString() == "T") numSchuetzen++;
                    }
                    if (numSchuetzen > 5) GauligaDGVHeimverein[e.ColumnIndex, e.RowIndex].Value = "F";
                }
            }
        }

        private void GauligaDGVGastverein_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == HeimWertung.Index && e.RowIndex != -1)
            {
                GauligaDGVGastverein.EndEdit();
            }
        }

        private void GauligaDGVGastverein_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == GastWertung.Index && e.RowIndex != -1)
            {
                if (GauligaDGVGastverein[e.ColumnIndex, e.RowIndex].Value.ToString() == "T")
                {
                    // schau, ob mehr als 4 Einträge angehakt sind. Wenn ja, unchecke box
                    int numSchuetzen = 0;
                    foreach (DataGridViewRow row in GauligaDGVGastverein.Rows)
                    {
                        if (row.Cells["GastWertung"].Value != null && row.Cells["GastWertung"].Value.ToString() == "T") numSchuetzen++;
                    }
                    if (numSchuetzen > 5) GauligaDGVGastverein[e.ColumnIndex, e.RowIndex].Value = "F";
                }
            }
        }

        private void gauligaOKBtn_Click(object sender, EventArgs e)
        {
            // Alle Einträge aus der Gauliga-Tabelle löschen, die das heutige Datum haben, dann die markierten wieder anlegen
            // das aktuelle Datum steht in aktuellesDatum (hoffentlich)
            if (aktuellesDatum == "") MessageBox.Show("Datum nicht bekannt.");
            string sql = "DELETE FROM gauliga WHERE datum='" + aktuellesDatum + "'";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            sql = "DELETE FROM gauligameta WHERE datum='" + aktuellesDatum + "'";
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "INSERT INTO gauligameta (RundeNr, Gruppe, datum) VALUES ('" + tbRunde.Text + "','" + tbGruppe.Text + "','" + aktuellesDatum + "')";
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            // Bauen des INSERT SQL-Kommandos
            sql = "INSERT INTO gauliga (idschuetze, fullname, verein, ergebnis, datum, SE, session) VALUES ";
            bool first = true;
            foreach (DataGridViewRow row in GauligaDGVHeimverein.Rows)
            {
                if (row.Cells["HeimWertung"].Value.ToString() == "T")
                {
                    if (!first)
                        sql += ",";
                    else
                        first = false;
                    string SE = "";
                    if (row.Cells["SE_Heim"].Value.ToString().Equals("Stammschütze")) SE = "S";
                    if (row.Cells["SE_Heim"].Value.ToString().Equals("Ersatzschütze")) SE = "E";
                    sql += "('" + row.Cells["idschuetzeHeim"].Value.ToString() + "','" + row.Cells["schuetzeName2"].Value.ToString() + "', '" + labelHeimVerein.Text + "', '" + row.Cells["ergebnisHeim"].Value.ToString() + "','" + aktuellesDatum + "','" + SE + "','" + row.Cells["HeimSession"].Value.ToString() + "')";
                }
            }
            foreach (DataGridViewRow row in GauligaDGVGastverein.Rows)
            {
                if (row.Cells["GastWertung"].Value.ToString() == "T")
                {
                    if (!first)
                        sql += ",";
                    else
                        first = false;
                    string SE = "";
                    if (row.Cells["SE_Gast"].Value.ToString().Equals("Stammschütze")) SE = "S";
                    if (row.Cells["SE_Gast"].Value.ToString().Equals("Ersatzschütze")) SE = "E";
                    sql += "('" + row.Cells["idschuetzeGast"].Value.ToString() + "','" + row.Cells["schuetzeNameGast"].Value.ToString() + "', '" + labelGastVerein.Text + "', '" + row.Cells["ergebnisGast"].Value.ToString() + "','" + aktuellesDatum + "','" + SE + "','" + row.Cells["GastSession"].Value.ToString() + "')";
                }
            }
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            DialogResult = DialogResult.OK;
            Close();
            Dispose();
        }

        private void gauligaCancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            Dispose();
        }
    }
}
