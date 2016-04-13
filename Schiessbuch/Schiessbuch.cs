﻿using System;
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
using System.Globalization;
using System.Diagnostics;
using System.IO;

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

        string connStrLocal = "server=localhost;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
        string connStrRemote = "server=192.168.178.202;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
        string backupDestination = "localhost";
        //string backupDestination = "192.168.178.202";

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: Diese Codezeile lädt Daten in die Tabelle "siusclubDataSet.schuetzenliste". Sie können sie bei Bedarf verschieben oder entfernen.
            //this.schuetzenlisteTableAdapter1.Fill(this.siusclubDataSet.schuetzenliste);
            // TODO: Diese Codezeile lädt Daten in die Tabelle "siusclubDataSet11.schuetzenliste". Sie können sie bei Bedarf verschieben oder entfernen.
            this.schuetzenlisteTableAdapter1.Fill(this.siusclubDataSet.schuetzenliste);
            //string connStr = "server=localhost;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
            string connStr = connStrLocal;
            //string _connectionStringName = "schiessbuch.Properties.Settings.siusclubConnectionString";
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //MessageBox.Show(Properties.Settings.Default.siusclubConnectionString);
            config.ConnectionStrings.ConnectionStrings[0].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.ConnectionStrings.ConnectionStrings[1].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.ConnectionStrings.ConnectionStrings[2].ConnectionString = connStr; // Properties.Settings.Default.siusclubConnectionString;
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
            schiessbuchTableAdapter.Connection.ConnectionString = connStr;
            schuetzenTableAdapter.Connection.ConnectionString = connStr;
            trefferTableAdapter.Connection.ConnectionString = connStr;
            vereineTableAdapter.Connection.ConnectionString = connStr;
            //MessageBox.Show("schiessbuchTableAdapter: " + schiessbuchTableAdapter.Connection.State.ToString());
            //MessageBox.Show("schuetzenTableAdapter: " + schuetzenTableAdapter.Connection.State.ToString());
            //MessageBox.Show("trefferTableAdapter: " + trefferTableAdapter.Connection.State.ToString());
            //MessageBox.Show("vereineTableAdapter: " + vereineTableAdapter.Connection.State.ToString());

        // global::schiessbuch.Properties.Settings.Default.siusclubConnectionString
        retry:
            int numAllRead = 0; // Das dient zur Überprüfung, ob Daten aus der Datenbank kommen
            try
            {
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

                ereignisse_count = GetEreignisseCount();
                treffer_count = GetTrefferCount();

                AktualisiereSchiessjahrMenu();

                PruefeJahrespokalAbend();
                FillWanderPokalTermine();
                splitContainer1.SplitterDistance = splitContainer1.Width / 2;
                splitContainer2.SplitterDistance = splitContainer2.Height / 2;
                splitContainer3.SplitterDistance = splitContainer3.Height / 2;


                //KoenigSKGridView_SetWidts();
                ResizeAllKoenigGridViews();
                UpdateKoenig();

                for (int stand = 1; stand <= 6; stand++)
                    for (int schuss = 0; schuss < 40; schuss++)
                        setSchussValue(stand, schuss, schuss.ToString());
            }
            catch (MySqlException mysqle)
            {
                if (global::schiessbuch.Properties.Settings.Default.siusclubConnectionString.Equals(connStrRemote))
                {
                    // Aktuell wird möglicherweise auf den Server zugegriffen, Connectionstring ändern und neu versuchen
                    // global::schiessbuch.Properties.Settings.Default
                }
                MessageBox.Show("Konnte Datenbank nicht öffnen.");
            }
        }

        private void PruefeJahrespokalAbend()
        {
            MySqlConnection conn;
            MySqlCommand cmdJahre;

            conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            cmdJahre = new MySqlCommand();
            cmdJahre.Connection = conn;

            // prüfe, ob heute Wanderpokalschiessen ist
            cmdJahre.CommandText = "SELECT COUNT(id) FROM termine_wanderpokal WHERE Termin = '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            int wps = int.Parse(cmdJahre.ExecuteScalar().ToString());
            if (wps > 0)
                lblWanderpokalschiessen.Visible = true;
            else
                lblWanderpokalschiessen.Visible = false;

            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
        }

        private void AktualisiereSchiessjahrMenu()
        {
            // zuerst das schiessjahrAuswählenToolStripMenuItem leeren
            schießjahrAuswählenToolStripMenuItem.DropDownItems.Clear();
            MySqlConnection conn;
            MySqlCommand cmdJahre;
            conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            cmdJahre = new MySqlCommand("SELECT * FROM schiessjahr", conn);
            conn.Open();
            MySqlDataReader reader = cmdJahre.ExecuteReader();
            while (reader.Read())
            {
                // Liste der Schiessjahre aktualisieren
                ToolStripMenuItem tsi = new ToolStripMenuItem();
                tsi.Text = reader["Schiessjahr"].ToString();
                tsi.Tag = reader["idSchiessjahr"];
                tsi.Click += Tsi_Click;
                schießjahrAuswählenToolStripMenuItem.DropDownItems.Add(tsi);
            }
            reader.Close();
            reader.Dispose();
            // Lies das Schießjahr mit dem höchsten Datum aus der Datenbank und setze das als das aktuelle
            cmdJahre.CommandText = "SELECT idSchiessjahr FROM schiessjahr ORDER BY SchiessjahrBeginn DESC LIMIT 1";
            int activeID = int.Parse(cmdJahre.ExecuteScalar().ToString());
            foreach (ToolStripMenuItem tsi in schießjahrAuswählenToolStripMenuItem.DropDownItems)
            {
                if (int.Parse(tsi.Tag.ToString()) == activeID)
                {
                    tsi.PerformClick();
                }
            }
            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
        }

        private void FillWanderPokalTermine()
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmdWanderPokal = new MySqlCommand("SELECT Termin FROM termine_wanderpokal WHERE fkSchiessjahr='" + aktuellesSchiessjahrID + "' ORDER BY Termin ASC", conn);
            MySqlDataReader reader = cmdWanderPokal.ExecuteReader();
            int counter = 0;
            while (reader.Read())
            {
                switch (counter)
                {
                    case 0: dateTimeWPTermin1.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 1: dateTimeWPTermin2.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 2: dateTimeWPTermin3.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 3: dateTimeWPTermin4.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 4: dateTimeWPTermin5.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 5: dateTimeWPTermin6.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 6: dateTimeWPTermin7.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 7: dateTimeWPTermin8.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 8: dateTimeWPTermin9.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 9: dateTimeWPTermin10.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 10: dateTimeWPTermin11.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                    case 11: dateTimeWPTermin12.Value = DateTime.Parse(reader["Termin"].ToString()); break;
                }
                counter++;
            }
            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
        }

        private void Tsi_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            aktuellesSchiessjahrID = Int32.Parse(((ToolStripMenuItem)sender).Tag.ToString());
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT SchiessjahrBeginn FROM schiessjahr WHERE idSchiessjahr='" + aktuellesSchiessjahrID + "'", conn);
            dtJahrBeginn = (DateTime)cmd.ExecuteScalar();
            // Das Ende wird bestimmt, indem der Eintrag gesucht wird, in dem das nächsthöhere Datum abgespeichert ist
            cmd.CommandText = "SELECT SchiessjahrBeginn FROM schiessjahr WHERE SchiessjahrBeginn > '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' LIMIT 1";
            try
            {
                dtJahrEnde = (DateTime)cmd.ExecuteScalar();
            }
            catch (NullReferenceException)
            {
                dtJahrEnde = dtJahrBeginn;
            }
            foreach (ToolStripMenuItem tsi in schießjahrAuswählenToolStripMenuItem.DropDownItems)
            {
                tsi.Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;
            populateSchiessjahrFilter();
            CheckSchiessjahr();
            ComboBoxSelectionChange();
            FillWanderPokalTermine();
            UpdateKoenig();
        }

        private int aktuellesSchiessjahrID;
        public DateTime dtJahrBeginn;
        public DateTime dtJahrEnde;

        private long GetEreignisseCount()
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            try {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM schiessbuch", conn);
                long tmpCount;
                tmpCount = long.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
                return tmpCount;
            }
            catch (MySqlException mysqle)
            {
                MessageBox.Show("Konnte keine Verbindung zur Datenbank herstellen. Methode: GetEreignisseCount()");
                return 0;
            }
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

        string strSchiessjahrFilter;
        private void populateSchiessjahrFilter()
        {
            if (dtJahrBeginn == dtJahrEnde)
                strSchiessjahrFilter = " HAVING Date >= '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "'";
            else
                strSchiessjahrFilter = " HAVING Date >= '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' AND Date < '" + dtJahrEnde.ToString("yyyy-MM-dd") + "'";
        }

        private void printSchiessAuswertung(string strDisziplin, TextBox textbox, string Zeile1, string Zeile2)
        {
            // Erzeuge Auswertungen
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY Date DESC", conn);
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
            cmd.CommandText = "SELECT COUNT(*) AS count, SUM(ergebnis) AS summe, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter; // + " GROUP BY id";
            reader = cmd.ExecuteReader(CommandBehavior.Default);
            while (reader.Read())
            {
                int summe = 0;
                if (Int32.TryParse(reader["summe"].ToString(), out summe))
                {
                    textbox.Text += String.Format("Anzahl: {0:6}", reader["count"].ToString()) + Environment.NewLine;
                    textbox.Text += String.Format("Summe: {0:7}", summe);
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
            MySqlCommand cmd = new MySqlCommand("SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15", conn);
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
            cmd.CommandText = "SELECT SUM(ergebnis) AS summe FROM (SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15) T2";
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
                CheckSchiessjahr();
            }
            else
            {
                SchiessabendPicker_ValueChanged(this, e);
            }
        }

        private void CheckSchiessjahr()
        {
            if (dtJahrBeginn == dtJahrEnde)
                schiessbuchBindingSource.Filter = "dt >= '" + dtJahrBeginn.ToShortDateString() + "'";
            else
                schiessbuchBindingSource.Filter = "dt >= '" + dtJahrBeginn.ToShortDateString() + "' AND dt < '" + dtJahrEnde.ToShortDateString() + "'";
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



                // Für neue König-Ansicht
                // erstelle Zeitfilter
                string ZeitFilter;
                if (dtJahrBeginn == dtJahrEnde) // aktuelles Jahr?
                {
                    ZeitFilter = " HAVING Datum > '" + dtJahrBeginn.ToShortDateString() + "' ";
                } else
                {
                    ZeitFilter = " HAVING (Datum >= '" + dtJahrBeginn.ToShortDateString() + "' AND Datum < '" + dtJahrEnde.ToShortDateString() + "') ";
                }
                //cmd.CommandText = "set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2";
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig SK';set @d1a='LG';set @d2='LP Koenig SK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                cmd.CommandText = "set @row=0;set @d1='LG Koenig';set @d1a='LG';set @d2='LP Koenig';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                int row = 0;
                while (reader.Read())
                {
                    row = KoenigSKGridView.Rows.Add();
                    KoenigSKGridView.Rows[row].Cells["Position"].Value = reader["Rang"].ToString();
                    KoenigSKGridView.Rows[row].Cells["Fullname"].Value = reader["Schuetze"].ToString();
                    KoenigSKGridView.Rows[row].Cells["Datum"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
                    KoenigSKGridView.Rows[row].Cells["Teiler"].Value = reader["Teiler"].ToString();
                    KoenigSKGridView.Rows[row].Cells["Typ"].Value = reader["Typ"].ToString();
                    row++;
                }
                reader.Close();
                cmd.CommandText = "set @row=0;set @d1='LG Koenig DK';set @d1a='LG';set @d2='LP Koenig DK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    row = KoenigDKGridView.Rows.Add();
                    KoenigDKGridView.Rows[row].Cells["PositionDK"].Value = reader["Rang"].ToString();
                    KoenigDKGridView.Rows[row].Cells["FullnameDK"].Value = reader["Schuetze"].ToString();
                    KoenigDKGridView.Rows[row].Cells["DatumDK"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
                    KoenigDKGridView.Rows[row].Cells["TeilerDK"].Value = reader["Teiler"].ToString();
                    KoenigDKGridView.Rows[row].Cells["TypDK"].Value = reader["Typ"].ToString();
                    row++;
                }
                reader.Close();
                cmd.CommandText = "set @row=0;set @d1='LG Koenig JUG';set @d1a='LG';set @d2='LP Koenig JUG';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    row = KoenigJUGGridView.Rows.Add();
                    KoenigJUGGridView.Rows[row].Cells["PositionJUG"].Value = reader["Rang"].ToString();
                    KoenigJUGGridView.Rows[row].Cells["FullnameJUG"].Value = reader["Schuetze"].ToString();
                    KoenigJUGGridView.Rows[row].Cells["DatumJUG"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
                    KoenigJUGGridView.Rows[row].Cells["TeilerJUG"].Value = reader["Teiler"].ToString();
                    KoenigJUGGridView.Rows[row].Cells["TypJUG"].Value = reader["Typ"].ToString();
                    row++;
                }
                reader.Close();
                cmd.CommandText = "set @row=0;set @d1='LG Koenig Auflage';set @d1a='LG';set @d2='LP Koenig Auflage';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    row = KoenigAuflageGridView.Rows.Add();
                    KoenigAuflageGridView.Rows[row].Cells["PositionAuflage"].Value = reader["Rang"].ToString();
                    KoenigAuflageGridView.Rows[row].Cells["FullnameAuflage"].Value = reader["Schuetze"].ToString();
                    KoenigAuflageGridView.Rows[row].Cells["DatumAuflage"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
                    KoenigAuflageGridView.Rows[row].Cells["TeilerAuflage"].Value = reader["Teiler"].ToString();
                    KoenigAuflageGridView.Rows[row].Cells["TypAuflage"].Value = reader["Typ"].ToString();
                    row++;
                }
                reader.Close();






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
            col_id.ReadOnly = true;
            Schiessabend.Columns.Add(col_id);
            col_name.Name = "Name";
            col_name.CellTemplate = cell;
            col_name.ReadOnly = true;
            Schiessabend.Columns.Add(col_name);
            col_vorname.Name = "Vorname";
            col_vorname.CellTemplate = cell;
            col_vorname.ReadOnly = true;
            Schiessabend.Columns.Add(col_vorname);
            col_id.Dispose();
            col_name.Dispose();
            col_vorname.Dispose();
            //MessageBox.Show(Properties.Settings.Default.siusclubConnectionString);
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            try
            {
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
                    col.ReadOnly = true;
                    Schiessabend.Columns.Add(col);
                    i++;
                    col.Dispose();
                }
                reader.Close();

                DataGridViewColumn colKasse = new DataGridViewColumn();
                colKasse.Name = "Kasse";
                colKasse.HeaderText = "Kasse";
                colKasse.CellTemplate = cell;
                colKasse.ReadOnly = false;
                colKasse.DefaultCellStyle.BackColor = Color.LightYellow;
                colKasse.DefaultCellStyle.ForeColor = Color.Red;
                colKasse.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                colKasse.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                colKasse.DefaultCellStyle.Format = "C2";
                colKasse.DefaultCellStyle.NullValue = "0";
                colKasse.ValueType = Type.GetType("System.Decimal");
                colKasse.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Yellow;
                colKasse.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

                Schiessabend.Columns.Add(colKasse);
                colKasse.Dispose();

                //reader.Dispose();
                //cmd.Cancel();
                //cmd.Dispose();
                Schiessabend.SuspendLayout();
                cmd.CommandText = "SELECT DISTINCT schuetzen.id as SID, name, vorname, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id WHERE status='beendet' HAVING Date='" + filterDateStr + "' ORDER BY name, vorname";
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
                        string cmdstr = "select MAX(ergebnis) AS ergebnis FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + Schiessabend.Columns[j + 3].Name + "' AND id='" + reader["SID"] + "' AND status='beendet' HAVING Date='" + filterDateStr + "') T";
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
                    // Betrag anzeigen
                    string betragstr = "SELECT Betrag FROM kasse WHERE idSchuetze='" + reader["SID"] + "' AND DatumBezahlt='" + filterDateStr + "'";
                    MySqlCommand cmd3 = new MySqlCommand(betragstr, conn2);
                    float fBetrag;
                    object o = cmd3.ExecuteScalar();
                    if (o == null)
                        fBetrag = 0f;
                    else
                        if (!float.TryParse(o.ToString(), out fBetrag)) fBetrag = 0f;
                    Schiessabend["Kasse", newRow].Value = fBetrag;
                    cmd3.Cancel();
                    cmd3.Dispose();
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
            catch (MySqlException mysqle)
            {
                MessageBox.Show("Konnte MySQL-Datenbank nicht öffnen: " + mysqle.Message);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ErstelleAuswertung();
        }

        PrintDocument pdTagesauswertung;

        private void btnTagesAuswertungDrucken_Click(object sender, EventArgs e)
        {
            if (pdTagesauswertung == null)
            {
                pdTagesauswertung = new PrintDocument();
                pdTagesauswertung.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();
            //PrintPreviewControl printPreviewControl1 = new PrintPreviewControl();
            //if (printPreviewControl1.Document != null)
            //    printPreviewControl1.Document.Dispose();
            ppdlg.Document = pdTagesauswertung;
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

            int disc = Schiessabend.Columns.Count - 3 - 1; // ID, Name, Vorname und Kasse sind keine Disziplinen

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
                    int disziplinen = Schiessabend.Columns.Count - 3 - 1; // ID, Name, Vorname und Kasse sind keine Disziplinen
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
            TagesAuswertungListeConnection.Close();
            TagesAuswertungListeConnection.Dispose();
            MySqlConnection.ClearPool(TagesAuswertungListeConnection);
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

        private void splitContainer1_Resize(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = splitContainer1.Width / 2;
        }

        private void splitContainer2_Resize(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Height / 2;
        }

        private void splitContainer3_Resize(object sender, EventArgs e)
        {
            splitContainer3.SplitterDistance = splitContainer3.Height / 2;
        }

        private void neuesSchießjahrBeginnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NeuesSchiessjahr neuSj = new NeuesSchiessjahr();
            neuSj.ShowDialog();
            AktualisiereSchiessjahrMenu();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Befülle die Termine für das Wanderpokalschießen für das aktuelle Schießjahr (über ein Jahr hinweg)
            // finde ersten Termin: der letzte Freitag des Monats, an dem das Schießjahr beginnt.
            // Ist der Beginn des Schießjahres nach dem letzten Freitag dieses Monats, suche den letzten Freitag des darauffolgenden Monats
            int iMonat = dtJahrBeginn.Month;
            // erster Tag des nächsten Monats - 1 ist letzter Tag dieses Monats
            DateTime lastDayFirstMonth;
            DateTime reference = dtJahrBeginn;
            DateTime lastFridayThisMonth;
            int counter = 0;
            for (int i = 0; true; i++) // Endebedingung durch counter-Variable bestimmt
            {
                if (reference.Month != 12)
                    lastDayFirstMonth = new DateTime(reference.Year, reference.Month + 1, 1);
                else
                    lastDayFirstMonth = new DateTime(reference.Year + 1, 1, 1);
                lastDayFirstMonth += new TimeSpan(-1, 0, 0, 0);
                int diffDays = (int)(lastDayFirstMonth.DayOfWeek) - 5;
                if (diffDays < 0) diffDays += 7;
                lastFridayThisMonth = lastDayFirstMonth.AddDays(-diffDays);
                // setzen der pickers
                if (lastFridayThisMonth >= dtJahrBeginn && ((dtJahrBeginn == dtJahrEnde) || (lastFridayThisMonth < dtJahrEnde)))
                {
                    switch (counter)
                    {
                        case 0: dateTimeWPTermin1.Value = lastFridayThisMonth; break;
                        case 1: dateTimeWPTermin2.Value = lastFridayThisMonth; break;
                        case 2: dateTimeWPTermin3.Value = lastFridayThisMonth; break;
                        case 3: dateTimeWPTermin4.Value = lastFridayThisMonth; break;
                        case 4: dateTimeWPTermin5.Value = lastFridayThisMonth; break;
                        case 5: dateTimeWPTermin6.Value = lastFridayThisMonth; break;
                        case 6: dateTimeWPTermin7.Value = lastFridayThisMonth; break;
                        case 7: dateTimeWPTermin8.Value = lastFridayThisMonth; break;
                        case 8: dateTimeWPTermin9.Value = lastFridayThisMonth; break;
                        case 9: dateTimeWPTermin10.Value = lastFridayThisMonth; break;
                        case 10: dateTimeWPTermin11.Value = lastFridayThisMonth; break;
                        case 11: dateTimeWPTermin12.Value = lastFridayThisMonth; break;
                    }
                    counter++;
                }
                if ((counter > 11) || (dtJahrBeginn != dtJahrEnde && lastFridayThisMonth > dtJahrEnde))
                {
                    for (i = counter; i < 12; i++)
                    {
                        switch (i)
                        {
                            case 0: dateTimeWPTermin1.Enabled = false; break;
                            case 1: dateTimeWPTermin2.Enabled = false; break;
                            case 2: dateTimeWPTermin3.Enabled = false; break;
                            case 3: dateTimeWPTermin4.Enabled = false; break;
                            case 4: dateTimeWPTermin5.Enabled = false; break;
                            case 5: dateTimeWPTermin6.Enabled = false; break;
                            case 6: dateTimeWPTermin7.Enabled = false; break;
                            case 7: dateTimeWPTermin8.Enabled = false; break;
                            case 8: dateTimeWPTermin9.Enabled = false; break;
                            case 9: dateTimeWPTermin10.Enabled = false; break;
                            case 10: dateTimeWPTermin11.Enabled = false; break;
                            case 11: dateTimeWPTermin12.Enabled = false; break;
                        }
                    }
                    break;
                }
                // nächstes Monat
                reference = reference.AddMonths(1);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // alte Termine in der Datenbank löschen
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("DELETE FROM termine_wanderpokal WHERE fkSchiessjahr='" + aktuellesSchiessjahrID + "'", conn);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO termine_wanderpokal (Termin, fkSchiessjahr) VALUES (@Termin, @schiessjahr)";
            cmd.Prepare();

            cmd.Parameters.Add("@Termin", MySqlDbType.DateTime).Value = dateTimeWPTermin1.Value;
            cmd.Parameters.Add("@schiessjahr", MySqlDbType.Int32).Value = aktuellesSchiessjahrID;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin2.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin3.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin4.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin5.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin6.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin7.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin8.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin9.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin10.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin11.Value;
            cmd.ExecuteNonQuery();

            cmd.Parameters["@Termin"].Value = dateTimeWPTermin12.Value;
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
        }

        private void KoenigSKGridView_Resize(object sender, EventArgs e)
        {
            ResizeAllKoenigGridViews();
        }

        private void ResizeAllKoenigGridViews()
        {
            KoenigSKGridView_SetWidts();
            KoenigDKGridView_SetWidts();
            KoenigJUGGridView_SetWidts();
            KoenigAuflageGridView_SetWidts();
        }

        private void KoenigSKGridView_SetWidts()
        {
            KoenigSKGridView.Columns["Position"].Width = 30;
            KoenigSKGridView.Columns["Teiler"].Width = 40;
            KoenigSKGridView.Columns["Typ"].Width = 30;
            KoenigSKGridView.Columns["Datum"].Width = 100;
            KoenigSKGridView.Columns["Fullname"].Width = KoenigSKGridView.Width -
                KoenigSKGridView.Columns["Position"].Width -
                KoenigSKGridView.Columns["Teiler"].Width -
                KoenigSKGridView.Columns["Datum"].Width -
                KoenigSKGridView.Columns["Typ"].Width - 20;
        }
        private void KoenigDKGridView_SetWidts()
        {
            KoenigDKGridView.Columns["PositionDK"].Width = 30;
            KoenigDKGridView.Columns["TeilerDK"].Width = 40;
            KoenigDKGridView.Columns["TypDK"].Width = 30;
            KoenigDKGridView.Columns["DatumDK"].Width = 100;
            KoenigDKGridView.Columns["FullnameDK"].Width = KoenigSKGridView.Width -
                KoenigDKGridView.Columns["PositionDK"].Width -
                KoenigDKGridView.Columns["TeilerDK"].Width -
                KoenigDKGridView.Columns["DatumDK"].Width -
                KoenigDKGridView.Columns["TypDK"].Width - 20;
        }
        private void KoenigJUGGridView_SetWidts()
        {
            KoenigJUGGridView.Columns["PositionJUG"].Width = 30;
            KoenigJUGGridView.Columns["TeilerJUG"].Width = 40;
            KoenigJUGGridView.Columns["TypJUG"].Width = 30;
            KoenigJUGGridView.Columns["DatumJUG"].Width = 100;
            KoenigJUGGridView.Columns["FullnameJUG"].Width = KoenigSKGridView.Width -
                KoenigJUGGridView.Columns["PositionJUG"].Width -
                KoenigJUGGridView.Columns["TeilerJUG"].Width -
                KoenigJUGGridView.Columns["DatumJUG"].Width -
                KoenigJUGGridView.Columns["TypJUG"].Width - 20;
        }
        private void KoenigAuflageGridView_SetWidts()
        {
            KoenigAuflageGridView.Columns["PositionAuflage"].Width = 30;
            KoenigAuflageGridView.Columns["TeilerAuflage"].Width = 40;
            KoenigAuflageGridView.Columns["TypAuflage"].Width = 30;
            KoenigAuflageGridView.Columns["DatumAuflage"].Width = 100;
            KoenigAuflageGridView.Columns["FullnameAuflage"].Width = KoenigSKGridView.Width -
                KoenigAuflageGridView.Columns["PositionAuflage"].Width -
                KoenigAuflageGridView.Columns["TeilerAuflage"].Width -
                KoenigAuflageGridView.Columns["DatumAuflage"].Width -
                KoenigAuflageGridView.Columns["TypAuflage"].Width - 20;
        }

        private void tableLayoutPanel9_Resize(object sender, EventArgs e)
        {
            SuspendLayout();
            Stand1SplitContainer.SuspendLayout();
            Stand2SplitContainer.SuspendLayout();
            Stand3SplitContainer.SuspendLayout();
            Stand4SplitContainer.SuspendLayout();
            Stand5SplitContainer.SuspendLayout();
            Stand6SplitContainer.SuspendLayout();
            Stand1SchussPanel.SuspendLayout();
            Stand2SchussPanel.SuspendLayout();
            Stand3SchussPanel.SuspendLayout();
            Stand4SchussPanel.SuspendLayout();
            Stand5SchussPanel.SuspendLayout();
            Stand6SchussPanel.SuspendLayout();
            Stand1SplitContainer.SplitterDistance = Stand1SplitContainer.Panel1.Height;
            Stand2SplitContainer.SplitterDistance = Stand2SplitContainer.Panel1.Height;
            Stand3SplitContainer.SplitterDistance = Stand3SplitContainer.Panel1.Height;
            Stand4SplitContainer.SplitterDistance = Stand4SplitContainer.Panel1.Height;
            Stand5SplitContainer.SplitterDistance = Stand5SplitContainer.Panel1.Height;
            Stand6SplitContainer.SplitterDistance = Stand6SplitContainer.Panel1.Height;
            Stand1SchussPanel.Width = Stand1SplitContainer.Width - Stand1SplitContainer.SplitterDistance;
            Stand1SchussPanel.Height = (int)(Stand1SchussPanel.Width * (8/5));
            Stand2SchussPanel.Width = Stand2SplitContainer.Width - Stand2SplitContainer.SplitterDistance;
            Stand2SchussPanel.Height = (int)(Stand2SchussPanel.Width * (8 / 5));
            Stand3SchussPanel.Width = Stand3SplitContainer.Width - Stand3SplitContainer.SplitterDistance;
            Stand3SchussPanel.Height = (int)(Stand3SchussPanel.Width * (8 / 5));
            Stand4SchussPanel.Width = Stand4SplitContainer.Width - Stand4SplitContainer.SplitterDistance;
            Stand4SchussPanel.Height = (int)(Stand4SchussPanel.Width * (8 / 5));
            Stand5SchussPanel.Width = Stand5SplitContainer.Width - Stand5SplitContainer.SplitterDistance;
            Stand5SchussPanel.Height = (int)(Stand5SchussPanel.Width * (8 / 5));
            Stand6SchussPanel.Width = Stand6SplitContainer.Width - Stand6SplitContainer.SplitterDistance;
            Stand6SchussPanel.Height = (int)(Stand6SchussPanel.Width * (8 / 5));
            //stand1Zielscheibe.Width = tabl
            Stand1SplitContainer.ResumeLayout();
            Stand2SplitContainer.ResumeLayout();
            Stand3SplitContainer.ResumeLayout();
            Stand4SplitContainer.ResumeLayout();
            Stand5SplitContainer.ResumeLayout();
            Stand6SplitContainer.ResumeLayout();
            Stand1SchussPanel.ResumeLayout();
            Stand2SchussPanel.ResumeLayout();
            Stand3SchussPanel.ResumeLayout();
            Stand4SchussPanel.ResumeLayout();
            Stand5SchussPanel.ResumeLayout();
            Stand6SchussPanel.ResumeLayout();
            ResumeLayout();
        }

        private void setSchussValue(int iStand, int iSchussNummer, string strWert)
        {
            int iReihe = iSchussNummer / 5;
            int iSpalte = iSchussNummer % 5;
            //this.Controls["txtSchuss" + iStand + iSpalte + iReihe].Text = strWert;
            //UebersichtTableLayoutPanel.Controls["Stand" + iStand + "SplitContainer"];
            //UebersichtTableLayoutPanel.Controls["Stand" + iStand + "SplitContainer"].Controls[1].Controls["Stand" + iStand + "SchussPanel"]
            ((TableLayoutPanel)UebersichtTableLayoutPanel.Controls["Stand" + iStand + "SplitContainer"].Controls[1].Controls["Stand" + iStand + "SchussPanel"]).GetControlFromPosition(iSpalte, iReihe).Text = strWert;
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            ErstelleAuswertung();
        }

        private void Schiessabend_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            // stelle fest, ob eine Zelle in der Spalte "Kasse" verlassen wurde. Nur dann soll nämlich ein Update in der Datenbank durchgeführt werden.
            if (Schiessabend.Columns[e.ColumnIndex].HeaderText.Equals("Kasse"))
            {
                try {
                    Schiessabend.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    string strDatum = dateTimePicker1.Value.ToString("yyyy-MM-dd"); // Datum, an dem bezahlt wurde (das Datum, für das die Tagesansicht erzeugt wird)
                    int iSchuetze = int.Parse(Schiessabend["ID", e.RowIndex].Value.ToString()); // der Schütze, der bezahlt hat
                    MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO kasse (idSchuetze, DatumBezahlt, Betrag) VALUES (@id, @datum, @betrag) ON DUPLICATE KEY UPDATE Betrag=@betrag", conn);
                    cmd.Prepare();
                    cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = iSchuetze;
                    cmd.Parameters.Add("@datum", MySqlDbType.Date).Value = dateTimePicker1.Value;
                    cmd.Parameters.Add("@betrag", MySqlDbType.Float).Value = Schiessabend[e.ColumnIndex, e.RowIndex].Value;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    MySqlConnection.ClearPool(conn);
                }
                catch (FormatException fe)
                {
                    MessageBox.Show("Es darf nur ein Betrag eingegeben werden. Soll eine Zahlung gelöscht werden, dann 0 eintragen.", "Falsches Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        PrintDocument pdKasse;
        private void btnKassenbericht_Click(object sender, EventArgs e)
        {
            if (pdKasse == null)
            {
                pdKasse = new PrintDocument();
                pdKasse.PrintPage += new PrintPageEventHandler(pd_KassePrintPage);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count + 1; // extra Zeile für die Summe
            //pd = new PrintDocument();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();
            //PrintPreviewControl printPreviewControl1 = new PrintPreviewControl();
            //if (printPreviewControl1.Document != null)
            //    printPreviewControl1.Document.Dispose();
            ppdlg.Document = pdKasse;
            ppdlg.ShowDialog();
            //printPreviewControl1.Document = pd;
            //button2.Enabled = true;
            //PrintPreviewDialog pvd = new PrintPreviewDialog();
            //pvd.Document = pd;
            //pvd.ShowDialog();
            //pd.Print();

        }

        private void pd_KassePrintPage(object sender, PrintPageEventArgs ev)
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
            str = String.Format("Kassenbericht vom {0}", dateTimePicker1.Value.ToShortDateString());
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

            int disziplinen=0;
            while (count < linesPerPage && currentLinesPrinted < linesCount)
            {
                if ((currentLinesPrinted < linesCount - 1) && Schiessabend["ID", currentLinesPrinted].Value != null)
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
                    disziplinen = Schiessabend.Columns.Count - 4; // ID, Name, Vorname, Kasse sind keine Disziplinen, deshalb von den Spalten 4 abziehen
                    for (int i = 0; i < disziplinen; i++)
                    {
                        if (Schiessabend[3+i, currentLinesPrinted].Value.ToString().Length != 0)
                            ev.Graphics.DrawString("x",
                                printFont,
                                Brushes.Black,
                                leftMargin + 220 + i * 40,
                                yPos,
                                new StringFormat());

                        //ev.Graphics.DrawString(Schiessabend[3 + i, currentLinesPrinted].Value.ToString(),
                        //    printFont,
                        //    Brushes.Black,
                        //    leftMargin + 220 + i * 40,
                        //    yPos,
                        //    new StringFormat());
                    }
                    string EinzelBetrag = String.Format(
                            "{0:c}",
                            float.Parse(Schiessabend[disziplinen + 3, currentLinesPrinted].Value.ToString()));
                    float EinzelbetragBreite = ev.Graphics.MeasureString(EinzelBetrag, printFont).Width;
                    ev.Graphics.DrawString(
                        EinzelBetrag, 
                        printFont, 
                        Brushes.Black, 
                        leftMargin + 220 + disziplinen * 40 + 50 - EinzelbetragBreite, 
                        yPos, 
                        new StringFormat());
                    count++;
                    currentLinesPrinted++;
                }
                if (currentLinesPrinted == linesCount - 1)
                {
                    // Summe der Beträge anzeigen
                    string sum = "Summe:";
                    yPos += printFont.GetHeight(ev.Graphics);
                    ev.Graphics.DrawString(sum, printFont, Brushes.Black, leftMargin + 220 + disziplinen * 40 - ev.Graphics.MeasureString(sum, printFont).Width - 10, yPos, new StringFormat());
                    float fSumme = 0.0f;
                    for (int i = 0; i < Schiessabend.Rows.Count; i++)
                    {
                        fSumme += float.Parse(Schiessabend["Kasse", i].Value.ToString());
                    }
                    string Gesamtbetrag = String.Format("{0:c}", float.Parse(fSumme.ToString()));
                    float GesamtbetragBreite = ev.Graphics.MeasureString(Gesamtbetrag, printFont).Width;
                    ev.Graphics.DrawString(Gesamtbetrag, printFont, Brushes.Black, leftMargin + 220 + disziplinen * 40 + 50 - GesamtbetragBreite, yPos, new StringFormat());
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

        private void deleteEntry_Click(object sender, EventArgs e)
        {
            if (mouseLocation != null)
            {
                //schiessbuchTableAdapter.Delete(schiessbuchDataGridView["session", mouseLocation.RowIndex].Value.ToString());
                //schiessbuchTableAdapter.UpdateQuerySetStatusForSession("ungültig", );
                schiessbuchTableAdapter.UpdateQuerySetStatusForSessionAndOldStatus("ungültig", schiessbuchDataGridView["session", mouseLocation.RowIndex].Value.ToString(), "beendet");
                //schiessbuchTableAdapter.Update(siusclubDataSet.schiessbuch);
                //schiessbuchBindingSource.ResetCurrentItem();
                schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                schiessbuchDataGridView.Invalidate();
            }
        }

        DataGridViewCellEventArgs mouseLocation;

        private void schiessbuchDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            mouseLocation = e;
        }

        private void schiessbuchDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DialogResult res = MessageBox.Show("Eintrag wirklich löschen?", "Bestätigung", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            if (res == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void eintratgültigSetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mouseLocation != null)
            {
                //schiessbuchTableAdapter.Delete(schiessbuchDataGridView["session", mouseLocation.RowIndex].Value.ToString());
                //schiessbuchTableAdapter.UpdateQuerySetStatusForSession("beendet", );
                schiessbuchTableAdapter.UpdateQuerySetStatusForSessionAndOldStatus("beendet", schiessbuchDataGridView["session", mouseLocation.RowIndex].Value.ToString(), "ungültig");
                //schiessbuchTableAdapter.Update(siusclubDataSet.schiessbuch);
                schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                schiessbuchDataGridView.Invalidate();
            }
        }

        private void moveEntry_Click(object sender, EventArgs e)
        {
            SchiesserieVerschieben dlg = new SchiesserieVerschieben();
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                DateTime dt = dlg.dateTimePicker1.Value;
                string[] compoNow = dt.ToLongTimeString().Split(':');
                TimeSpan toMidnight = new TimeSpan(int.Parse(compoNow[0]), int.Parse(compoNow[1]), int.Parse(compoNow[2]));
                dt = dt.Subtract(toMidnight);
                String strTime = ((DateTime)schiessbuchDataGridView["dt", mouseLocation.RowIndex].Value).ToLongTimeString();
                string[] components = strTime.Split(':');
                TimeSpan ts = new TimeSpan(int.Parse(components[0]), int.Parse(components[1]), int.Parse(components[2]));
                dt = dt.Add(ts);
                CultureInfo enus = new CultureInfo("en-US");
                schiessbuchTableAdapter.UpdateQuerySetDatumForSession(dt.ToString("ddd MMM dd yyyy HH:mm:ss G'M'T+0200 (CEST)", enus), schiessbuchDataGridView["session", mouseLocation.RowIndex].Value.ToString());
                schiessbuchTableAdapter.Fill(siusclubDataSet.schiessbuch);
                schiessbuchDataGridView.Invalidate();
                dlg.Dispose();
            }
        }

        private void Schiessbuch_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult res = MessageBox.Show("Soll eine Sicherung der Datenbank erstellt werden?", "Sicherung erstellen", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                if (Properties.Settings.Default.BackupFileName.Length > 0 && Properties.Settings.Default.BackupDirectory.Length > 0)
                {
                    Process externalProcess = new Process();
                    externalProcess.StartInfo.FileName = Properties.Settings.Default.BackupFileName;
                    externalProcess.StartInfo.Arguments = "--add-drop-database --add-drop-table --add-drop-trigger --add-locks --complete-insert --create-options --extended-insert --single-transaction  --dump-date -u siusclub --host=" + backupDestination + " --password=\"siusclub\" siusclub";
                    externalProcess.StartInfo.UseShellExecute = false;
                    externalProcess.StartInfo.RedirectStandardOutput = true;
                    externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    externalProcess.StartInfo.CreateNoWindow = true;
                    externalProcess.Start();
                    string output = externalProcess.StandardOutput.ReadToEnd();
                    File.WriteAllText(Properties.Settings.Default.BackupDirectory + "\\backup-" + DateTime.Now.ToShortDateString() + ".sql", output);
                    //MessageBox.Show(output);
                    externalProcess.WaitForExit();
                    MessageBox.Show("Backup erstellt.");
                }
                else
                {
                    MessageBox.Show("Es wurden keine Einstellungen für die Sicherungsverzeichnisse gefunden. Bitte Verzeichnisse festlegen.");
                }
            }
        }

        private void ordnerFürSicherungenFestlegenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            if (Properties.Settings.Default.BackupDirectory.Length > 0)
                fbd.SelectedPath = Properties.Settings.Default.BackupDirectory;
            DialogResult res = fbd.ShowDialog();
            if (res == DialogResult.OK)
            {
                Properties.Settings.Default.BackupDirectory = fbd.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void pfadZuMysqldumpFestlegenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (Properties.Settings.Default.BackupFileName.Length > 0)
                ofd.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.BackupFileName);
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK)
            {
                Properties.Settings.Default.BackupFileName = ofd.FileName;
                Properties.Settings.Default.Save();
            }
        }
    }
}
