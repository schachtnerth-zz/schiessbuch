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

namespace schiessbuch
{
    public partial class Schiessbuch : Form
    {
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
            string _connectionStringName = "siusclubConnectionString";
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings[_connectionStringName].ConnectionString = Properties.Settings.Default.siusclubConnectionString;
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
            Druckansicht da = new Druckansicht();
            da.ShowDialog();
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
    }
}
