using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Drawing.Printing;

namespace schiessbuch
{
    public partial class Schiessbuch : Form
    {
        PrintDocument pd;
        private Font printFont;
        private int linesCount;
        private int currentLinesPrinted;

        public Schiessbuch()
        {
            InitializeComponent();
        }

        private void schuetzenBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.schuetzenBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.siusclubDataSet);

        }

        long ereignisse_count = 0;
        long treffer_count = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            string connStr = "server=localhost;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
            //string _connectionStringName = "schiessbuch.Properties.Settings.siusclubConnectionString";
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MessageBox.Show(Properties.Settings.Default.siusclubConnectionString);
            config.ConnectionStrings.ConnectionStrings[0].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.ConnectionStrings.ConnectionStrings[1].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.ConnectionStrings.ConnectionStrings[2].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");

            int numAllRead = 0; // Das dient zur Überprüfung, ob Daten aus der Datenbank kommen
            // TODO: Diese Codezeile lädt Daten in die Tabelle "siusclubDataSet1.Vereine". Sie können sie bei Bedarf verschieben oder entfernen.
            numAllRead += this.vereineTableAdapter.Fill(this.siusclubDataSet1.Vereine);
            // TODO: This line of code loads data into the 'siusclubDataSet.treffer' table. You can move, or remove it, as needed.
            numAllRead += this.trefferTableAdapter.Fill(this.siusclubDataSet.treffer);
            // TODO: This line of code loads data into the 'siusclubDataSet.schiessbuch' table. You can move, or remove it, as needed.
            numAllRead += this.schiessbuchTableAdapter.Fill(this.siusclubDataSet.schiessbuch);
            // TODO: This line of code loads data into the 'siusclubDataSet.schuetzen' table. You can move, or remove it, as needed.
            numAllRead += this.schuetzenTableAdapter.Fill(this.siusclubDataSet.schuetzen);

            if (numAllRead == 0)
            {
                MessageBox.Show("Keine Datensätze aus der Datenbank gelesen. Möglicherweise keine Datenbankverbindung.");
            }

            DoUpdates.Checked = true;
            UpdateKoenig();
            ereignisse_count = GetEreignisseCount();
            treffer_count = GetTrefferCount();
        }

        private long GetEreignisseCount()
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM schiessbuch",conn);
            long tmpCount;
            tmpCount = long.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return tmpCount;
        }

        private long GetTrefferCount()
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM treffer", conn);
            long tmpCount;
            tmpCount = long.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return tmpCount;
        }

        private void schuetzenBindingNavigatorSaveItem_Click_1(object sender, EventArgs e)
        {
            this.Validate();
            this.schuetzenBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.siusclubDataSet);

        }

        private void schuetzenBindingNavigatorSaveItem_Click_2(object sender, EventArgs e)
        {
            this.Validate();
            this.schuetzenBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.siusclubDataSet);

        }

        private void SchiessabendPicker_ValueChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(schiessbuchBindingSource.Filter);
            //MessageBox.Show(SchiessabendPicker.Value.ToShortDateString());
            schiessbuchBindingSource.Filter = "Datum = '" + SchiessabendPicker.Value.ToShortDateString() + "' OR Datum='" + SchiessabendPicker.Value.AddDays(1).ToShortDateString() + "'";
        }

        private void printSchiessAuswertung(string strDisziplin, TextBox textbox, string Zeile1, string Zeile2)
        {
            // Erzeuge Auswertungen
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'", conn);
            MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
            textbox.Text = Zeile1 + Environment.NewLine;
            textbox.Text += Zeile2 + Environment.NewLine;
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            textbox.Text += "Datum       Ring" + Environment.NewLine;
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            while (reader.Read())
            {
                string line = String.Format("{0:dd.MM.yyyy}   {1:6}{2}", reader["Date"], reader["ergebnis"], Environment.NewLine);
                textbox.Text += line;
            }
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            reader.Close();
            cmd.CommandText = "SELECT COUNT(*) AS count, SUM(ergebnis) AS summe FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet' GROUP BY id";
            reader = cmd.ExecuteReader(CommandBehavior.Default);
            while (reader.Read())
            {
                if (Int16.Parse(reader["summe"].ToString()) > 0)
                {
                    textbox.Text += String.Format("Anzahl: {0:6}", reader["count"].ToString()) + Environment.NewLine;
                    textbox.Text += String.Format("Summe: {0:7}", reader["summe"].ToString());
                }
            }
            reader.Close();
            conn.Close();
        }

        private void printSchiessAuswertungBest15(string strDisziplin, TextBox textbox, string Zeile1, string Zeile2)
        {
            // Erzeuge Auswertungen
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet' ORDER BY ergebnis DESC LIMIT 15", conn);
            MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
            textbox.Text = Zeile1 + Environment.NewLine;
            textbox.Text += Zeile2 + Environment.NewLine;
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            textbox.Text += "Datum       Ring" + Environment.NewLine;
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            while (reader.Read())
            {
                string line = String.Format("{0:dd.MM.yyyy}   {1:6}{2}", reader["Date"], reader["ergebnis"], Environment.NewLine);
                textbox.Text += line;
            }
            textbox.Text += "----------------";
            textbox.Text += Environment.NewLine;
            reader.Close();
            cmd.CommandText = "SELECT SUM(ergebnis) AS summe FROM (SELECT ergebnis FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet' ORDER BY ergebnis DESC LIMIT 15) T";
            //cmd.CommandText = "SELECT COUNT(*) AS count, SUM(ergebnis) AS summe FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet' GROUP BY id ORDER";
            reader = cmd.ExecuteReader(CommandBehavior.Default);
            while (reader.Read())
            {
                if (reader["summe"]!=System.DBNull.Value)
                    textbox.Text += String.Format("Summe: {0:7}", reader["summe"].ToString());
            }
            reader.Close();
            conn.Close();
        }

        private void ComboBoxSelectionChange()
        {
            checkBox1.Checked = false;
            printSchiessAuswertung("LG 20 Schuss", AuswertungLG20, "20 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 20 Schuss", AuswertungLG20_15, "20 Schuss", "15 Beste");
            printSchiessAuswertung("LG 30 Schuss", AuswertungLG30, "30 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 30 Schuss", AuswertungLG30_15, "30 Schuss", "15 Beste");
            printSchiessAuswertung("LG 40 Schuss", AuswertungLG40, "40 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 40 Schuss", AuswertungLG40_15, "40 Schuss", "15 Beste");

            printSchiessAuswertung("LG 20 Schuss Auflage", AuswertungLG20A, "20 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 20 Schuss Auflage", AuswertungLG20A_15, "20 Schuss", "15 Beste");
            printSchiessAuswertung("LG 30 Schuss Auflage", AuswertungLG30A, "30 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 30 Schuss Auflage", AuswertungLG30A_15, "30 Schuss", "15 Beste");
            printSchiessAuswertung("LG 40 Schuss Auflage", AuswertungLG40A, "40 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LG 40 Schuss Auflage", AuswertungLG40A_15, "40 Schuss", "15 Beste");

            printSchiessAuswertung("LP 20 Schuss", AuswertungLP20, "20 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LP 20 Schuss", AuswertungLP20_15, "20 Schuss", "15 Beste");
            printSchiessAuswertung("LP 30 Schuss", AuswertungLP30, "30 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LP 30 Schuss", AuswertungLP30_15, "30 Schuss", "15 Beste");
            printSchiessAuswertung("LP 40 Schuss", AuswertungLP40, "40 Schuss", "Alle Ergebnisse:");
            printSchiessAuswertungBest15("LP 40 Schuss", AuswertungLP40_15, "40 Schuss", "15 Beste");
        }

        private void fullnameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxSelectionChange();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                SchiessabendPicker.Enabled = true;
            else
                SchiessabendPicker.Enabled = false;
        }

        private void SchiessabendPicker_EnabledChanged(object sender, EventArgs e)
        {
            if (SchiessabendPicker.Enabled == false)
            {
                schiessbuchBindingSource.Filter = "";
            }
            else
            {
                SchiessabendPicker_ValueChanged(this, e);
            }
        }

        private void trefferDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (trefferDataGridView.SelectedRows.Count > 0)
            {
                //pictureBox1.Invalidate();
                //   MessageBox.Show("Selection changed.");
                string zielscheibe = trefferDataGridView.SelectedRows[0].Cells["zielscheibe"].Value.ToString();
                if (zielscheibe.Equals("DSB Luftpistole 10m") || zielscheibe.Equals("DSB Luftpistole 10m rot"))
                    pictureBox1.Image = schiessbuch.Properties.Resources.Luftpistole;
                //pictureBox1.Image = Properties.Resources.Luftpistole;
                if (zielscheibe.Equals("DSB Luftgewehr 10m"))
                    pictureBox1.Image = schiessbuch.Properties.Resources.Luftgewehr;

                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // zuerst überprüfen, ob überaupt alle Schüsse auf die gleiche Zielscheibe abgegeben wurden.
            // wenn nämlich nicht, dann macht es keinen Sinn, sie alle in der gleichen Scheibe anzuzeigen
            string zielscheibe = "";
            foreach (DataGridViewRow row in trefferDataGridView.SelectedRows)
            {
                if (zielscheibe != row.Cells["zielscheibe"].Value.ToString())
                {
                    if (zielscheibe == "")
                        zielscheibe = row.Cells["zielscheibe"].Value.ToString();
                    // noch keine Scheibe gesetzt. Setze die Scheibe als Referenz für alle Schüsse
                    else
                    {
                        MessageBox.Show("Schüsse auf verschiedene Scheiben!", "Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // unterschiedliche Zielscheiben, fahre nicht fort mit Zeichnen
                    }
                }
            }
            Pen pen = new Pen(Color.Red, 1f);
            Brush brush = new SolidBrush(Color.Red);
            Brush bluebrush = new SolidBrush(Color.Blue);
            float kaliber_mm = 4.5f;
            float pt_per_mm = 23.622f;
            float kaliber_pt = kaliber_mm * pt_per_mm;
            // Verkleinerung durch Zoom berechnen
            float real_pt_size = pictureBox1.Image.Width; // Properties.Resources.Luftpistole.Width;
            float render_points = pictureBox1.Width;
            float zoom_factor = real_pt_size / render_points;

            foreach (DataGridViewRow row in trefferDataGridView.SelectedRows)
            {
                float x_l_o = (float.Parse(row.Cells["xrahmeninmm"].Value.ToString()) - kaliber_mm / 2) * pt_per_mm;
                float y_l_o = (float.Parse(row.Cells["yrahmeninmm"].Value.ToString()) - kaliber_mm / 2) * pt_per_mm;

                
                e.Graphics.FillEllipse(brush, new Rectangle((int)(x_l_o / zoom_factor + pictureBox1.Width / 2), (int)(y_l_o / zoom_factor + pictureBox1.Height / 2), (int)(kaliber_pt / zoom_factor), (int)(kaliber_pt / zoom_factor)));
                //gr.FillEllipse(bluebrush, new Rectangle((int)((-kaliber_pt / 2) / zoom_factor), (int)((-kaliber_pt / 2) / zoom_factor), (int)(kaliber_pt / zoom_factor), (int)(kaliber_pt / zoom_factor)));

            }

        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // trefferBindingSource.ResetBindings(false);
            // schuetzenBindingSource.ResetBindings(false);
            // schiessbuchBindingSource.ResetBindings(false);
            if (GetEreignisseCount() != ereignisse_count)
            {
                ereignisse_count = GetEreignisseCount();
                treffer_count = GetTrefferCount();
                int SchiessbuchScrollposition = schiessbuchDataGridView.FirstDisplayedScrollingRowIndex;
                int TrefferScrollposition = trefferDataGridView.FirstDisplayedScrollingRowIndex;
                List<string> eventsSelected = new List<string>();
                List<long> trefferSelected = new List<long>();
                foreach (DataGridViewRow row in schiessbuchDataGridView.SelectedRows)
                {
                    eventsSelected.Add(row.Cells["session"].Value.ToString());
                }
                foreach (DataGridViewRow row in trefferDataGridView.SelectedRows)
                {
                    trefferSelected.Add(long.Parse(row.Cells["id"].Value.ToString()));
                }

                //schuetzenTableAdapter.Fill(siusclubDataSet.schuetzen);
                schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                trefferTableAdapter.Fill(siusclubDataSet.treffer);
                foreach (DataGridViewRow row in schiessbuchDataGridView.Rows)
                {
                    //row.Selected = false;
                    foreach (string selItem in eventsSelected)
                    {
                        if (row.Cells["session"].Value.ToString() == selItem)
                        {
                            row.Selected = true;
                            schiessbuchBindingSource.Position = row.Index;
                        }
                    }
                }
                foreach (DataGridViewRow row in trefferDataGridView.Rows)
                {
                    row.Selected = false;
                    foreach (long selId in trefferSelected)
                    {
                        if (long.Parse(row.Cells["id"].Value.ToString()) == selId)
                        {
                            row.Selected = true;
                        }
                    }
                }
                //schiessbuchBindingSource.ResetBindings(false);
                //trefferBindingSource.ResetBindings(false);
                if (SchiessbuchScrollposition != -1)
                    schiessbuchDataGridView.FirstDisplayedScrollingRowIndex = SchiessbuchScrollposition;
                if (TrefferScrollposition != -1)
                    trefferDataGridView.FirstDisplayedScrollingRowIndex = TrefferScrollposition;
                //siusclubDataSet.Reset();
                //schiessbuchBindingSource.ResetBindings(false);

                //schiessbuchDataGridView.DataSource = null;
                //schiessbuchDataGridView.DataSource = schiessbuchBindingSource;

                //MessageBox.Show("Tick");
                //schiessbuchBindingSource.ResetBindings(false);
                //trefferBindingSource.ResetBindings(false);
                //schuetzenBindingSource.ResetBindings(false);

                //schiessbuchDataGridView.Refresh();
                //schiessbuchDataGridView.Invalidate();

                //siusclubDataSet.Reset();
                //trefferTableAdapter.Fill(siusclubDataSet.treffer);
                //schuetzenTableAdapter.Fill(siusclubDataSet.schuetzen);
                //schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                //schiessbuchDataGridView.Invalidate();
                // SELECT für König:
                // set @row=0;select @row:=@row+1 AS Rang, Schütze, Teiler, Typ FROM (SELECT Schütze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schütze, schuetzen.id as ID, ergebnis AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin="LG Koenig" UNION select CONCAT(name, ', ', vorname) AS Schütze, schuetzen.id as ID, ergebnis / 2.6 AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin="LP Koenig") T GROUP BY ID ORDER BY Teiler ASC ) T2
                // ComboBoxSelectionChange(); (unsure if needed) update Statistics regularly
                // Was ist mit den Datagridviews? werden die automatisch aktualisiert?
                //UpdateKoenig();
            }
            else
            {
                if (GetTrefferCount() != treffer_count)
                {
                    treffer_count = GetTrefferCount();
                    int TrefferScrollposition = trefferDataGridView.FirstDisplayedScrollingRowIndex;
                    List<long> trefferSelected = new List<long>();
                    foreach (DataGridViewRow row in trefferDataGridView.SelectedRows)
                    {
                        trefferSelected.Add(long.Parse(row.Cells["id"].Value.ToString()));
                    }

                    trefferTableAdapter.Fill(siusclubDataSet.treffer);
                    foreach (DataGridViewRow row in trefferDataGridView.Rows)
                    {
                        row.Selected = false;
                        foreach (long selId in trefferSelected)
                        {
                            if (long.Parse(row.Cells["id"].Value.ToString()) == selId)
                            {
                                row.Selected = true;
                            }
                        }
                    }
                    //schiessbuchBindingSource.ResetBindings(false);
                    //trefferBindingSource.ResetBindings(false);
                    if (TrefferScrollposition != -1)
                        trefferDataGridView.FirstDisplayedScrollingRowIndex = TrefferScrollposition;
                    //siusclubDataSet.Reset();
                    //schiessbuchBindingSource.ResetBindings(false);

                    //schiessbuchDataGridView.DataSource = null;
                    //schiessbuchDataGridView.DataSource = schiessbuchBindingSource;

                    //MessageBox.Show("Tick");
                    //schiessbuchBindingSource.ResetBindings(false);
                    //trefferBindingSource.ResetBindings(false);
                    //schuetzenBindingSource.ResetBindings(false);

                    //schiessbuchDataGridView.Refresh();
                    //schiessbuchDataGridView.Invalidate();

                    //siusclubDataSet.Reset();
                    //trefferTableAdapter.Fill(siusclubDataSet.treffer);
                    //schuetzenTableAdapter.Fill(siusclubDataSet.schuetzen);
                    //schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                    //schiessbuchDataGridView.Invalidate();
                    // SELECT für König:
                    // set @row=0;select @row:=@row+1 AS Rang, Schütze, Teiler, Typ FROM (SELECT Schütze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schütze, schuetzen.id as ID, ergebnis AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin="LG Koenig" UNION select CONCAT(name, ', ', vorname) AS Schütze, schuetzen.id as ID, ergebnis / 2.6 AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin="LP Koenig") T GROUP BY ID ORDER BY Teiler ASC ) T2
                    // ComboBoxSelectionChange(); (unsure if needed) update Statistics regularly
                    // Was ist mit den Datagridviews? werden die automatisch aktualisiert?
                    UpdateKoenig();
                }
            }
        }

        private void UpdateKoenig()
        {
            KoenigTextBox.Clear();
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            try {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2", conn);
                //MySqlCommand cmd = new MySqlCommand("select * from schuetzen", conn);
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
                KoenigTextBox.Font = new Font("Courier New", 16);
                while (reader.Read())
                {
                    KoenigTextBox.Text += String.Format("{0,3}  {1,-30}     {2,6}    {3,6}", reader["Rang"].ToString(), reader["Schuetze"].ToString(), reader["Teiler"].ToString(), reader["Typ"].ToString()) + Environment.NewLine;
                }
                reader.Close();
                reader.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch (MySqlException mysqle)
            {
                MessageBox.Show("Kann Datenbank nicht öffnen. (" + mysqle.Message + ")");
                Application.Exit();
            }
        }

        private void trefferDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void trefferDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                int schritt = Int16.Parse(trefferDataGridView["schritt", e.RowIndex].Value.ToString());
                string disziplin = trefferDataGridView["disziplin", e.RowIndex].Value.ToString();
                if (schritt == 0)
                {

                    if (disziplin.Equals("LG 20 Schuss"))
                        trefferDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gray;
                    if (disziplin.Equals("LP 20 Schuss"))
                        trefferDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gray;
                    if (disziplin.Equals("LG 20 Schuss Auflage"))
                        trefferDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gray;
                }
            }
        }

        private void schießabendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Druckansicht da = new Druckansicht();
            //da.ShowDialog();
        }

        private void AuswertungLG30_TextChanged(object sender, EventArgs e)
        {

        }

        private void fullNameComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void DoUpdates_CheckedChanged(object sender, EventArgs e)
        {
            if (DoUpdates.Checked)
            {
                DoUpdates.Image = Properties.Resources.refresh40;
                RefreshTimer.Start();
            }
            else
            {
                DoUpdates.Image = Properties.Resources.refresh40bw;
                RefreshTimer.Stop();
            }
        }

        private void schiessbuchDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ManuellNachtragen manuell = new ManuellNachtragen();
            manuell.id = idTextBox.Text;
            manuell.ShowDialog();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            GauligaAuswertung gauliga = new GauligaAuswertung();
            gauliga.ShowDialog();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text.Equals("König"))
                UpdateKoenig();
            if (tabControl1.SelectedTab.Text.Equals("Tagesauswertung"))
            {
                ErstelleAuswertung();
            }
        }

        private void bearbeitungsmodusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEnableDisableEditControls(((ToolStripMenuItem)bearbeitungsmodusToolStripMenuItem).Checked);
        }

        private void SetEnableDisableEditControls(bool enable)
        {
            if (enable)
            {
                nameTextBox.ReadOnly = false;
                vornameTextBox.ReadOnly = false;
                emailTextBox.ReadOnly = false;
                //vereinTextBox.ReadOnly = false;
                vereinTextBox.Visible = false;
                vereinComboBox.Visible = true;
                bindingNavigatorAddNewItem.Enabled = true;
                bindingNavigatorDeleteItem.Enabled = true;
                //saveToolStripButton.Enabled = true;
                //saveToolStripButton1.Enabled = true;
            }
            else
            {
                nameTextBox.ReadOnly = true;
                vornameTextBox.ReadOnly = true;
                emailTextBox.ReadOnly = true;
                vereinTextBox.ReadOnly = true;
                vereinComboBox.Visible = false;
                vereinTextBox.Visible = true;
                bindingNavigatorAddNewItem.Enabled = false;
                bindingNavigatorDeleteItem.Enabled = false;
                //saveToolStripButton.Enabled = false;

            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            schuetzenBindingSource.EndEdit();
            schuetzenTableAdapter.Update(siusclubDataSet.schuetzen);
            saveToolStripButton1.Enabled = false;
            ((ToolStripMenuItem)bearbeitungsmodusToolStripMenuItem).Checked = false;
            SetEnableDisableEditControls(false);
            schuetzenTableAdapter.Fill(siusclubDataSet.schuetzen);
        }

        private void nameTextBox_Leave(object sender, EventArgs e)
        {

        }

        private void bindingNavigatorDeleteItem1_Click(object sender, EventArgs e)
        {
            if (siusclubDataSet.HasChanges())
                saveToolStripButton1.Enabled = true;
        }

        private void fillByToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.schuetzenTableAdapter.FillBy(this.siusclubDataSet.schuetzen);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            saveToolStripButton1.Enabled = true;
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)bearbeitungsmodusToolStripMenuItem).Checked == true)
                saveToolStripButton1.Enabled = true;
        }

        private void ErstelleAuswertung()
        {
            Schiessabend.Columns.Clear();
            DataGridViewColumn col_id = new DataGridViewColumn();
            DataGridViewColumn col_name = new DataGridViewColumn();
            DataGridViewColumn col_vorname = new DataGridViewColumn();
            col_id.Name = "ID";
            DataGridViewCell cell = new DataGridViewTextBoxCell();
            col_id.CellTemplate = cell;
            col_id.Width = 20;
            Schiessabend.Columns.Add(col_id);
            col_name.Name = "Name";
            col_name.CellTemplate = cell;
            Schiessabend.Columns.Add(col_name);
            col_vorname.Name = "Vorname";
            col_vorname.CellTemplate = cell;
            Schiessabend.Columns.Add(col_vorname);
            col_id.Dispose();
            col_name.Dispose();
            col_vorname.Dispose();
            //MessageBox.Show(Properties.Settings.Default.siusclubConnectionString);
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            string filterDateStr = dateTimePicker1.Value.Year + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Day;
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT DISZIPLIN, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch HAVING Date='" + filterDateStr + "'", conn);
            MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
            //DataGridViewColumn[] cols = new DataGridViewColumnCollection();
            int i = 0;
            Schiessabend.SuspendLayout();
            while (reader.Read())
            {
                DataGridViewColumn col = new DataGridViewColumn();
                col.Name = reader["disziplin"].ToString();
                col.CellTemplate = cell;
                Schiessabend.Columns.Add(col);
                i++;
                col.Dispose();
            }
            reader.Close();
            //reader.Dispose();
            //cmd.Cancel();
            //cmd.Dispose();
            cmd.CommandText = "SELECT DISTINCT schuetzen.id as SID, name, vorname, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id HAVING Date='" + filterDateStr + "' ORDER BY name, vorname";
            reader = cmd.ExecuteReader(CommandBehavior.Default);
            while (reader.Read())
            {
                //DataGridViewRow row = n
                //row.Cells["ID"].Value = reader["SID"].ToString();
                //row.Cells["Name"].Value = reader["name"];
                //row.Cells["Vorname"].Value = reader["vorname"];

                // Die ersten drei Spalten stehen fest. Alles ab Spalte 4 ist eine Disziplin
                int disziplinen = Schiessabend.Columns.Count - 3;
                int newRow = Schiessabend.Rows.Add(reader["SID"], reader["name"], reader["vorname"]);
                MySqlConnection conn2 = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
                conn2.Open();
                MySqlDataReader reader2;
                for (int j = 0; j < disziplinen; j++)
                {

                    //MessageBox.Show(Schiessabend.Columns[j + 3].Name);
                    string cmdstr = "SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + Schiessabend.Columns[j + 3].Name + "' AND id='" + reader["SID"] + "' HAVING Date='" + filterDateStr + "'";
                    MySqlCommand cmd2 = new MySqlCommand(cmdstr, conn2);
                    reader2 = cmd2.ExecuteReader();
                    int count = 0;
                    string result = "";
                    while (reader2.Read())
                    {
                        if (count == 0)
                            result = reader2["ergebnis"].ToString();
                        else
                        {
                            result += ", ";
                            result += reader2["ergebnis"].ToString();
                        }
                    }
                    reader2.Close();
                    //reader2.Dispose();
                    //cmd2.Cancel();
                    //cmd2.Dispose();
                    //conn2.Close();
                    //conn2.Dispose();

                    Schiessabend[j + 3, newRow].Value = result;

                }
                conn2.Close();
                MySqlConnection.ClearPool(conn2);
            }
            reader.Close();
            //reader.Dispose();
            //cmd.Cancel();
            //cmd.Dispose();
            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
            Schiessabend.ResumeLayout();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ErstelleAuswertung();
        }

        private void btnTagesAuswertungDrucken_Click(object sender, EventArgs e)
        {
            if (pd == null)
            {
                pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();
            //PrintPreviewControl printPreviewControl1 = new PrintPreviewControl();
            //if (printPreviewControl1.Document != null)
            //    printPreviewControl1.Document.Dispose();
            ppdlg.Document = pd;
            ppdlg.ShowDialog();
            //printPreviewControl1.Document = pd;
            //button2.Enabled = true;
            //PrintPreviewDialog pvd = new PrintPreviewDialog();
            //pvd.Document = pd;
            //pvd.ShowDialog();
            //pd.Print();

        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            //ev.Graphics.Clear(Color.White);
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            //string line = null;

            // calculate the number of lines per page
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

            string str = "Schützengesellschaft Edelweiß Eltheim e. V.";
            Font headFont = new Font("Arial", 20f);
            Font headFont2 = new Font("Arial", 14);
            int strl = (int)ev.Graphics.MeasureString(str, headFont).Width;
            int headHeight = (int)ev.Graphics.MeasureString(str, headFont).Height;
            ev.Graphics.DrawString(str, headFont, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);
            topMargin += headHeight;
            str = String.Format("Auswertung vom {0}", dateTimePicker1.Value.ToShortDateString());
            strl = (int)ev.Graphics.MeasureString(str, headFont2).Width;
            headHeight = (int)ev.Graphics.MeasureString(str, headFont2).Height;
            ev.Graphics.DrawString(str, headFont2, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);
            topMargin += headHeight;

            int disc = Schiessabend.Columns.Count - 3;

            // berechne maximale Textlänge der Disziplinen
            int maxLen = 0;
            for (int i = 0; i < disc; i++)
            {
                float l = ev.Graphics.MeasureString(Schiessabend.Columns[i + 3].Name, printFont).Width;
                if ((int)l > maxLen)
                    maxLen = (int)l;
            }

            yPos = topMargin + maxLen;
            ev.Graphics.DrawString("Nr.", printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            ev.Graphics.DrawString("Name", printFont, Brushes.Black, leftMargin + 35, yPos, new StringFormat());

            for (int i = 0; i < disc; i++)
            {
                ev.Graphics.TranslateTransform(leftMargin + 220 + i * 40, yPos);
                ev.Graphics.RotateTransform(-90);
                ev.Graphics.DrawString(Schiessabend.Columns[i + 3].Name,
                    printFont,
                    Brushes.Black,
                    0,
                    0,
                    new StringFormat());
                ev.Graphics.RotateTransform(90);
                ev.Graphics.TranslateTransform(-(leftMargin + 220 + i * 40), -yPos);
            }


            while (count < linesPerPage && currentLinesPrinted < linesCount)
            {
                if (Schiessabend["ID", currentLinesPrinted].Value != null)
                {
                    string idstr = Schiessabend["ID", currentLinesPrinted].Value.ToString();
                    yPos = topMargin + maxLen + ((count + 1) * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(idstr, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                    ev.Graphics.DrawString(
                        Schiessabend["name", currentLinesPrinted].Value.ToString() + ", " + Schiessabend["vorname", currentLinesPrinted].Value.ToString(),
                        printFont,
                        Brushes.Black,
                        leftMargin + 35,
                        yPos,
                        new StringFormat());
                    int disziplinen = Schiessabend.Columns.Count - 3;
                    for (int i = 0; i < disziplinen; i++)
                    {
                        ev.Graphics.DrawString(Schiessabend[3 + i, currentLinesPrinted].Value.ToString(),
                            printFont,
                            Brushes.Black,
                            leftMargin + 220 + i * 40,
                            yPos,
                            new StringFormat());
                    }
                    count++;
                    currentLinesPrinted++;
                }
            }
            if (currentLinesPrinted < Schiessabend.Rows.Count)
                ev.HasMorePages = true;
            else
            {
                ev.HasMorePages = false;
                currentLinesPrinted = 0;
            }
        }

        MySqlConnection TagesAuswertungListeConnection;
        MySqlDataReader TagesAuswertungListeDataReader;

        private void btnTagesAuswertungListeDrucken_Click(object sender, EventArgs e)
        {
            string filterDateStr = dateTimePicker1.Value.Year + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Day;
            if (pd == null)
            {
                pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPageListe);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();

            TagesAuswertungListeConnection = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            TagesAuswertungListeConnection.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT `name`, vorname, concat(name, ', ', vorname) as fullname, disziplin, ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id WHERE status='beendet' HAVING Date='" + filterDateStr + "'  order by fullname", TagesAuswertungListeConnection);
            TagesAuswertungListeDataReader = cmd.ExecuteReader();

            //PrintPreviewControl printPreviewControl1 = new PrintPreviewControl();
            //if (printPreviewControl1.Document != null)
            //    printPreviewControl1.Document.Dispose();
            ppdlg.Document = pd;
            ppdlg.ShowDialog();
            //printPreviewControl1.Document = pd;
            //button2.Enabled = true;
            //PrintPreviewDialog pvd = new PrintPreviewDialog();
            //pvd.Document = pd;
            //pvd.ShowDialog();
            //pd.Print();
        }

        private void pd_PrintPageListe(object sender, PrintPageEventArgs ev)
        {
            //ev.Graphics.Clear(Color.White);
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            //string line = null;
            int heightCount = 0;

            

            string str = "Schützengesellschaft Edelweiß Eltheim e. V.";
            Font headFont = new Font("Arial", 20f);
            Font headFont2 = new Font("Arial", 14);
            int strl = (int)ev.Graphics.MeasureString(str, headFont).Width;
            int headHeight = (int)ev.Graphics.MeasureString(str, headFont).Height;
            ev.Graphics.DrawString(str, headFont, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);
            topMargin += headHeight;
            heightCount += headHeight;
            str = String.Format("Auswertung vom {0}", dateTimePicker1.Value.ToShortDateString());
            strl = (int)ev.Graphics.MeasureString(str, headFont2).Width;
            headHeight = (int)ev.Graphics.MeasureString(str, headFont2).Height;
            ev.Graphics.DrawString(str, headFont2, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);
            topMargin += headHeight;
            heightCount += headHeight;

            


            yPos = topMargin;
            Font UeberschriftFont = new Font("Arial", 12, FontStyle.Bold);
            ev.Graphics.DrawString("Name", UeberschriftFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            ev.Graphics.DrawString("Disziplin", UeberschriftFont, Brushes.Black, leftMargin + 200, yPos, new StringFormat());
            ev.Graphics.DrawString("Ergebnis", UeberschriftFont, Brushes.Black, leftMargin + 400, yPos, new StringFormat());
            yPos += (int)ev.Graphics.MeasureString("NrName.", UeberschriftFont).Height;

            // calculate the number of lines per page
            linesPerPage = (ev.MarginBounds.Height - heightCount) / printFont.GetHeight(ev.Graphics) - 2; // zwei Zeilen Rand halten unten
            //yPos = topMargin + heightCount;

            while (TagesAuswertungListeDataReader.Read())
            {
                if (count <= linesPerPage)
                {
                    string s_fullname;
                    s_fullname = TagesAuswertungListeDataReader["fullname"].ToString();
                    ev.Graphics.DrawString(s_fullname, printFont, Brushes.Black, leftMargin, yPos);
                    string s_disziplin;
                    s_disziplin = TagesAuswertungListeDataReader["disziplin"].ToString();
                    ev.Graphics.DrawString(s_disziplin, printFont, Brushes.Black, leftMargin + 200, yPos);
                    string s_ergebnis;
                    s_ergebnis = TagesAuswertungListeDataReader["ergebnis"].ToString();
                    ev.Graphics.DrawString(s_ergebnis, printFont, Brushes.Black, leftMargin + 400, yPos);
                    int deltay = (int)ev.Graphics.MeasureString(s_fullname, printFont).Height;
                    int tmpy;
                    tmpy = (int)ev.Graphics.MeasureString(s_disziplin, printFont).Height;
                    if (tmpy > deltay) deltay = tmpy;
                    tmpy = (int)ev.Graphics.MeasureString(s_ergebnis, printFont).Height;
                    if (tmpy > deltay) deltay = tmpy;
                    yPos += deltay;
                    count++;
                } else
                {
                    ev.HasMorePages = true;
                    return;
                }
            }

            ev.HasMorePages = false;
            TagesAuswertungListeDataReader.Close();
            TagesAuswertungListeDataReader.Dispose();
            TagesAuswertungListeConnection.Close();
            TagesAuswertungListeConnection.Dispose();
            MySqlConnection.ClearPool(TagesAuswertungListeConnection);
        }
    }
}
