using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace schiessbuch
{
    public partial class Schiessbuch : Form
    {

        /// <summary>
        /// Properties
        /// </summary>
        PrintDocument pd;
        private Font printFont;
        private int linesCount;
        private int currentLinesPrinted;
        long ereignisse_count = 0;
        long treffer_count = 0;
        //private string connStr;
        /// <summary>
        /// Die ID des aktuell ausgewählten Schießjahrs. Diese ID entspricht der ID in der entsprechenden Datenbanktabelle.
        /// </summary>
        private int aktuellesSchiessjahrID;
        public DateTime dtJahrBeginn;
        public DateTime dtJahrEnde;
        string strSchiessjahrFilter;
        private int databaseRequestCounter = 0; // wenn 0, dann wird die Datenbank gelesen (sorgt dafür, dass die Datenbank (z. B. Schiessbuch) nur bei jedem n. Mal durch den Timer gelesen wird. Die restlichen Male werden nur die aktuellen Schiessergebnisse der momentan schießenden Schützen abgefragt
        private int standFuerEinzelScheibe;
        private bool generateEinzelScheibe;
        PrintDocument pdTagesauswertung;
        MySqlConnection TagesAuswertungListeConnection;
        MySqlDataReader TagesAuswertungListeDataReader;
        bool TagesAuswertungConnectionIsActive = false;
        PrintDocument pdKasse;
        DataGridViewCellEventArgs mouseLocation;
        private List<SchussInfo>[] aktuelleTreffer;
        private TabPage EinzelScheibe;
        private string strSchuetzenListeBindingSourceFilter;
        private bool generateOverview;
        private Bitmap[] StandZielscheiben;

        /// <summary>
        /// Konstanten
        /// </summary>
        // string connStrLocal = "server=localhost;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
        // string connStrRemote = "server=192.168.178.202;user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true";
        string backupDestination = "192.168.178.202";
        //string backupDestination = "localhost";
        const string strZielscheibeLuftgewehr = "DSB Luftgewehr 10m";
        const string strZielscheibeLuftpistole = "DSB Luftpistole 10m";
        const string strZielscheibeLuftpistoleRot = "DSB Luftpistole 10m rot";
        const string strZielscheibeLuftgewehrBlattlRot = "DSB Luftgewehr 10m Blattl rot";
        const string strZielscheibeLuftpistoleBlattlRot = "DSB Luftpistole 10m Blattl rot";
        const string strZielscheibeLuftgewehrBlattl = "DSB Luftgewehr 10m Blattl schwarz";
        const string strZielscheibeLuftpistoleBlattl = "DSB Luftpistole 10m Blattl schwarz";
        const string strZielscheibeLuftgewehrBlattlBlau = "DSB Luftgewehr 10m Blattl blau";
        const string strZielscheibeLuftpistoleBlattlBlau = "DSB Luftpistole 10m Blattl blau";


        /// <summary>
        /// Konstruktor
        /// </summary>
        public Schiessbuch()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Eventhandler für das Load-Event des Hauptformulars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private bool IsMySQLServerReachable(string strMySQLServer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hier wird geprüft, ob heute ein Termin zum Jahrespokalschießen ist.
        /// Wenn ja, dann wird eine entsprechende Meldung im Hauptfenster angezeigt.
        /// Wenn nein, dann wird diese Meldung im Hauptfenster versteckt.
        /// </summary>
        private void PruefeJahrespokalAbend()
        {
            MySqlConnection conn;
            MySqlCommand cmdJahre;
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            conn = new MySqlConnection(connStr);
            conn.Open();
            cmdJahre = new MySqlCommand();
            cmdJahre.Connection = conn;

            // prüfe, ob heute Wanderpokalschiessen ist
            cmdJahre.CommandText = "SELECT COUNT(id) FROM termine_wanderpokal WHERE Termin = '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            //int wps = int.Parse(mysql.doMySqlScalarQuery("SELECT COUNT(id) FROM termine_wanderpokal WHERE Termin = '" + DateTime.Now.ToString("yyyy-MM-dd") + "'").ToString());
            int wps = int.Parse(cmdJahre.ExecuteScalar().ToString());
            if (wps > 0)
                lblWanderpokalschiessen.Visible = true;
            else
                lblWanderpokalschiessen.Visible = false;

            conn.Close();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
        }

        /// <summary>
        /// Hier werden alle Termine für das Jahrespokalschießen aus der Datenbank gelesen
        /// Diese Werte werden dann im entsprechenden Reiter im Hauptfenster eingetragen
        /// und können von dort aus eingesehen und verändert werden.
        /// </summary>
        private void FillWanderPokalTermine()
        {
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmdWanderPokal = new MySqlCommand("SELECT Termin FROM termine_wanderpokal WHERE fkSchiessjahr='" + aktuellesSchiessjahrID + "' ORDER BY Termin ASC", conn);
            MySqlDataReader reader = cmdWanderPokal.ExecuteReader();
            //MySqlDataReader reader = mysql.doMySqlReaderQuery("SELECT Termin FROM termine_wanderpokal WHERE fkSchiessjahr='" + aktuellesSchiessjahrID + "' ORDER BY Termin ASC");
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
            //mysql.closeMySqlReaderQuery(reader);
            reader.Close();
        }

        /// <summary>
        /// Hier werden alle Einträge aus dem Schießbuch gezählt und zurückgeliefert.
        /// Dieser Wert kann in eine Variable geschrieben werden und dazu verwendet werden, zu überprüfen,
        /// ob sich seit dem letzten Aufruf im Schießbuch etwas geändert hat. Wenn nein, muss keine Aktualisierung
        /// aus der Datenbank und keine Aktualisierung des Schießbuch-UI erfolgen.
        /// </summary>
        /// <returns>Anzahl der Einträge im Schießbuch</returns>
        private long GetEreignisseCount()
        {
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance; // nur, um den ConnectionString zu kriegen
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM schiessbuch", conn);
                long tmpCount;
                // tmpCount = long.Parse(mysql.doMySqlScalarQuery("SELECT COUNT(*) FROM schiessbuch").ToString());
                tmpCount = long.Parse(cmd.ExecuteScalar().ToString());
                conn.Close();
                return tmpCount;
            }
            catch (MySqlException)
            {
                MessageBox.Show("Konnte keine Verbindung zur Datenbank herstellen. Methode: GetEreignisseCount()");
                return 0;
            }
        }

        /// <summary>
        /// Hier werden alle Einträge aus der Treffertabelle in der Datenbank gezählt und zurückgeliefert.
        /// Dieser Wert kann in eine Variable geschrieben werden und dazu verwenden werden, zu überprüfen,
        /// ob sich seit dem letzten Aufruf in der Treffertabelle etwas geändert hat. Wenn nein, muss keine Aktualisierung
        /// aus der Datengank und keine Aktualisierung der Trefferliste im Hauptdialog (Schießbuch-Tab) erfolgen.
        /// </summary>
        /// <returns>Anzahl der Einträge in der Treffer-Tabelle der Datenbank</returns>
        private long GetTrefferCount()
        {
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;

            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM treffer", conn);
            long tmpCount;
            //tmpCount = long.Parse(mysql.doMySqlScalarQuery("SELECT COUNT(*) FROM treffer").ToString());
            tmpCount = long.Parse(cmd.ExecuteScalar().ToString());
            conn.Close();
            return tmpCount;
        }

        /// <summary>
        /// Das Menü, in dem die einzelnen Schießjahre ausgewählt werden können, wird hier aus der Datenbank befüllt bzw. aktualisiert
        /// </summary>
        private void AktualisiereSchiessjahrMenu()
        {
            // zuerst das schiessjahrAuswählenToolStripMenuItem leeren
            schießjahrAuswählenToolStripMenuItem.DropDownItems.Clear();

            // Verbindung zur Datenbank aufbauen
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            // Alle Schießjahre auswählen
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM schiessjahr", conn);
            //MySqlDataReader reader = mysql.doMySqlReaderQuery("SELECT * FROM schiessjahr");
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // Liste der Schiessjahre aktualisieren
                ToolStripMenuItem tsi = new ToolStripMenuItem();
                tsi.Text = reader["Schiessjahr"].ToString();
                tsi.Tag = reader["idSchiessjahr"];
                // OnClick Eventhandler hinzufügen
                tsi.Click += Tsi_Click;
                // Eintrag zu Menü hinzufügen
                schießjahrAuswählenToolStripMenuItem.DropDownItems.Add(tsi);
            }
            // Datenbankverbindung aufräumen
            //mysql.closeMySqlReaderQuery(reader);
            reader.Close();

            // Lies das Schießjahr mit dem höchsten Datum aus der Datenbank und setze das als das aktuelle
            cmd.CommandText = "SELECT idSchiessjahr FROM schiessjahr ORDER BY SchiessjahrBeginn DESC LIMIT 1";
            int activeID = int.Parse(cmd.ExecuteScalar().ToString());
            //int activeID = int.Parse(mysql.doMySqlScalarQuery("SELECT idSchiessjahr FROM schiessjahr ORDER BY SchiessjahrBeginn DESC LIMIT 1").ToString());

            // Gehe durch alle Schießjahre im Menü und schau, welcher Eintrag dem aktuellen Schießjahr entspricht. Dieser Eintrag soll dann ausgewählt werden.
            foreach (ToolStripMenuItem tsi in schießjahrAuswählenToolStripMenuItem.DropDownItems)
            {
                if (int.Parse(tsi.Tag.ToString()) == activeID)
                {
                    // Schießjahr auswählen (und entsprechende EventHandler abfeuern)
                    tsi.PerformClick();
                }
            }
        }

        /// <summary>
        /// Auf die für die Darstellung des Schießbuchs verwendete BindingSource (schuetzenListeBindingSource) wird ein Filter angewandt, um
        /// nur die Einträge des aktuell ausgewählten Schießjahrs sehen zu können
        /// </summary>
        private void populateSchiessjahrFilter()
        {
            if (dtJahrBeginn == dtJahrEnde)
                strSchiessjahrFilter = " HAVING Date >= '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "'";
            else
                strSchiessjahrFilter = " HAVING Date >= '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' AND Date < '" + dtJahrEnde.ToString("yyyy-MM-dd") + "'";
            strSchuetzenListeBindingSourceFilter = "  Jahr = '" + dtJahrBeginn.ToString("yyyy") + "'";
            schuetzenListeBindingSource.Filter = strSchuetzenListeBindingSourceFilter;

            if (dtJahrBeginn == dtJahrEnde)
                schuetzenlisteschiessbuchBindingSource.Filter = "dt >= '" + dtJahrBeginn.ToShortDateString() + "'";
            else
                schuetzenlisteschiessbuchBindingSource.Filter = "dt >= '" + dtJahrBeginn.ToShortDateString() + "' AND dt < '" + dtJahrEnde.ToShortDateString() + "'";

        }

        /// <summary>
        /// Wenn sich bei Rahmendaten (z. B. ausgewählter Schütze oder Schießjahr) etwas ändert,
        /// müssen auch die zu diesem Schützen gehörenden Statistiken aktualisiert werden.
        /// Das wird hier erledigt.
        /// </summary>
        private void AktuellerSchuetzeStatistikAktualisieren()
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

        private void schuetzenBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.schuetzenBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.siusclubDataSet);

        }

        /// <summary>
        /// Die Schützenliste (mit Geschlecht, Geburtsdatum und Schützenklasse) wird neu aus der Datenbank gelesen
        /// und die zugehörigen TableAdapters werden aktualisiert.
        /// </summary>
        private void UpdateSchuetzenListe()
        {
            // Die Schützenliste wird neu aus der Datenbank gelesen

            // Vielleicht brauche ich das gar nicht mehr???
            //this.schuetzenListeBindingSourceA.ResetBindings(false);
            this.siusclubDataSet.schuetzenliste.Clear();
            //this.siusclubDataSet.EnforceConstraints = false;
            this.schuetzenlisteTableAdapter.Fill(this.siusclubDataSet.schuetzenliste);
        }

        private void Tsi_Click(object sender, EventArgs e)
        {
            int aktuellerSchuetze = -1;
            // Zwischenspeichern des aktuell ausgewählten Schützen, da dieser nach Ändern des Schießjahres wieder ausgewählt werden soll
            if (int.Parse(schuetzenListeBindingSource.Count.ToString()) != 0)
                aktuellerSchuetze = int.Parse(((DataRowView)schuetzenListeBindingSource.Current)["id"].ToString());
            //int aktuellerSchuetze = schuetzenListeBindingSource.Position;

            // Befülle oder aktualisiere die globale Variable aktuellesSchiessjahrID, in der die ID des aktuellen Schießjahrs gespeichert wird.
            aktuellesSchiessjahrID = Int32.Parse(((ToolStripMenuItem)sender).Tag.ToString());

            // Öffne die Datenbank und lese die einzelnen Anfangsdaten der Schießjahre
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT SchiessjahrBeginn FROM schiessjahr WHERE idSchiessjahr = '" + aktuellesSchiessjahrID + "'", conn);
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            //dtJahrBeginn = (DateTime)mysql.doMySqlScalarQuery("SELECT SchiessjahrBeginn FROM schiessjahr WHERE idSchiessjahr='" + aktuellesSchiessjahrID + "'");
            dtJahrBeginn = (DateTime)cmd.ExecuteScalar();

            // Das Ende wird bestimmt, indem der Eintrag gesucht wird, in dem das nächsthöhere Datum abgespeichert ist
            try
            {
                // Wenn es keinen Eintrag in der Datenbank mit einem höheren Datum gibt, dann ist dieses Schießjahr das aktuelle.
                // In diesem Fall wird eine Exception ausgelöst und wir setzen Anfangsdatum und Enddatum gleich, um auszudrücken, dass
                // es sich um das aktuelle Schiessjahr handelt.
                // dtJahrEnde = (DateTime)mysql.doMySqlScalarQuery("SELECT SchiessjahrBeginn FROM schiessjahr WHERE SchiessjahrBeginn > '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' LIMIT 1");
                cmd.CommandText = "SELECT SchiessjahrBeginn FROM schiessjahr WHERE SchiessjahrBeginn > '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' LIMIT 1";
                dtJahrEnde = (DateTime)cmd.ExecuteScalar();
            }
            catch (NullReferenceException)
            {
                // Es handelt sich um das aktuelle Schießjahr (s. o.)
                dtJahrEnde = dtJahrBeginn;
            }
            foreach (ToolStripMenuItem tsi in schießjahrAuswählenToolStripMenuItem.DropDownItems)
            {
                // Entferne den Haken bei allen Schießjahren im Menü
                // tsi.Checked = false;

                if (menuStrip1.InvokeRequired)
                {
                    menuStrip1.BeginInvoke(new MethodInvoker(delegate () { tsi.Checked = false; }));
                }
                else
                {
                    tsi.Checked = false;
                }
            }
            // Setze einen Haken beim aktuell ausgewählten Schießjahr
            if (menuStrip1.InvokeRequired)
            {
                menuStrip1.BeginInvoke(new MethodInvoker(delegate () { ((ToolStripMenuItem)sender).Checked = true; }));
            }
            else
            {
                ((ToolStripMenuItem)sender).Checked = true;
            }

            // Einige zusätzliche Aufgaben müssen erledigt werden, wenn ein neues Schießjahr ausgewählt wird.

            // Der Filter für die schuetzenListeBindingSource wird mit dem aktuellen Jahr aktualisiert
            populateSchiessjahrFilter();

            // Da sich das aktuelle Schießjahr geändert hat, sollen auch für den ausgewählten Schützen
            // die Statistiken des ausgewählten Schießjahres angezeigt werden. Das soll hier angestoßen werden.
            AktuellerSchuetzeStatistikAktualisieren();
            //TODO: schauen, warum das so lange dauert

            // Die Jahrespokaltermine für dieses Jahr müssen ebenfalls aus der Datenbank gelesen werden und
            // ins Hauptformular eingetragen werden:
            FillWanderPokalTermine();

            // Die Auswertung für den Schießkönig soll aktualisiert werden, um das aktuelle Jahr anzuzeigen
            UpdateKoenig();

            // Da sich durch die Änderung des Schießjahres auch die Zugehörigkeit von Schützen zu Schützenklassen ändern kann,
            // soll die Schützenliste neu eingelesen werden.
            UpdateSchuetzenListe();
            //TODO: Schauen, warum das so lange dauert

            // Die Änderungen sollen im DataGridView des Schießbuches reflektiert werden,
            // deshalb wird die entsprechende Tabelle neu befüllt.
            schiessbuchTableAdapter.Fill(this.siusclubDataSet.schiessbuch);
            schuetzenListeBindingSource.Position = schuetzenListeBindingSource.Find("id", aktuellerSchuetze);
        }

        /// <summary>
        /// Eventhandler, der beim Auswählen eines Eintrags aus der Schießjahrauswahl aufgerufen wird (ToolStripMenuItem)
        /// </summary>
        public void Tsi_ClickWorker(object parameter)
        {
        }

        /// <summary>
        /// Die Anzeige im Schießbuch kann auf nur einen Tag beschränkt werden. Dazu muss der entsprechende DateTimePicker aktiviert werden
        /// (Checkbox) und dann das gewünschte Datum eingetragen werden. Nach Eintragen des Datums wird dieser Eventhandler aufgerufen, der
        /// die Beschränkung der Anzeige auf diesen einen Tag vornimmt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SchiessabendPicker_ValueChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(schiessbuchBindingSource.Filter);
            //MessageBox.Show(SchiessabendPicker.Value.ToShortDateString());
            schiessbuchBindingSource.Filter = "Datum = '" + SchiessabendPicker.Value.ToShortDateString() + "' OR Datum='" + SchiessabendPicker.Value.AddDays(1).ToShortDateString() + "'";
        }

        /// <summary>
        /// Im Tab "Auswertung" wird eine Übersicht der Schießergebnisse des aktuellen Jahres angezeigt. Diese Methode erzeugt diese Übersicht
        /// abhängig von den mitgegebenen Argumenten (wie z. B. die Disziplin)
        /// </summary>
        /// <param name="strDisziplin">Disziplin, für die die Auswertung erstellt werden soll</param>
        /// <param name="textbox">TextBox, in die die Auswertung geschrieben werden soll</param>
        /// <param name="Zeile1">Erste Zeile der Überschrift/Beschreibung dieser Auswertung</param>
        /// <param name="Zeile2">Zweite Zeile der Überschrift/Beschreibung dieser Auswertung</param>
        private void printSchiessAuswertung(string strDisziplin, TextBox textbox, string Zeile1, string Zeile2)
        {
            string strSelectedSchiessjahr="";
            this.Invoke((MethodInvoker)delegate ()
            {
                if (fullnameComboBox.SelectedValue != null)
                    strSelectedSchiessjahr = fullnameComboBox.SelectedValue.ToString();
            });
            // Erzeuge Auswertungen
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + strSelectedSchiessjahr + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY Date DESC", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            //MySqlDataReader reader = mysql.doMySqlReaderQuery("SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + strSelectedSchiessjahr + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY Date DESC");
            textbox.Text = Zeile1 + Environment.NewLine;
            textbox.Text += Zeile2 + Environment.NewLine;
            textbox.Text += "--------------------";
            textbox.Text += Environment.NewLine;
            textbox.Text += "    Datum       Ring" + Environment.NewLine;
            textbox.Text += "--------------------";
            textbox.Text += Environment.NewLine;
            int zaehler = 0;
            while (reader.Read())
            {
                zaehler++;
                string line = String.Format("{3,2}. {0:dd.MM.yyyy}   {1:6}{2}", reader["Date"], reader["ergebnis"], Environment.NewLine, zaehler);
                textbox.Text += line;
            }
            textbox.Text += "--------------------";
            textbox.Text += Environment.NewLine;
            //mysql.closeMySqlReaderQuery(reader);
            reader.Close();

            string strFullNameComboBoxValue="";
            this.Invoke((MethodInvoker)delegate ()
            {
                if (fullnameComboBox.SelectedValue != null)
                    strFullNameComboBoxValue = fullnameComboBox.SelectedValue.ToString();
            });

            cmd.CommandText = "SELECT COUNT(*) AS count, SUM(ergebnis) AS summe, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + strFullNameComboBoxValue + "' AND status='beendet'" + strSchiessjahrFilter;
            reader = cmd.ExecuteReader();
            //reader = mysql.doMySqlReaderQuery("SELECT COUNT(*) AS count, SUM(ergebnis) AS summe, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + strFullNameComboBoxValue + "' AND status='beendet'" + strSchiessjahrFilter);
            while (reader.Read())
            {
                if (Int16.Parse(reader["summe"].ToString()) > 0)
                {
                    textbox.Text += String.Format("Anzahl: {0:6}", reader["count"].ToString()) + Environment.NewLine;
                    textbox.Text += String.Format("Summe: {0:7}", reader["summe"].ToString());
                }
            }
            //mysql.closeMySqlReaderQuery(reader);
            reader.Close();
        }

        /// <summary>
        /// Im Tab "Auswertung" wird eine Übersicht der Schießergebnisse des aktuellen Jahres angezeigt. Diese Methode erzeugt die Übersicht über die
        /// 15 besten Ergebnisse abhängig von den mitgegebenen Argumenten (wie z. B. die Disziplin)
        /// </summary>
        /// <param name="strDisziplin">Disziplin, für die die Auswertung erstellt werden soll</param>
        /// <param name="textbox">TextBox, in die die Auswertung geschrieben werden soll</param>
        /// <param name="Zeile1">Erste Zeile der Überschrift/Beschreibung dieser Auswertung</param>
        /// <param name="Zeile2">Zweite Zeile der Überschrift/Beschreibung dieser Auswertung</param>
        private void printSchiessAuswertungBest15(string strDisziplin, TextBox textbox, string Zeile1, string Zeile2)
        {
            string strFullNameComboBoxValue = "";
            this.Invoke((MethodInvoker)delegate ()
            {
                if (fullnameComboBox.SelectedValue != null)
                    strFullNameComboBoxValue = fullnameComboBox.SelectedValue.ToString();
            });

            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            // MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) AS Wertungen FROM (SELECT DISTINCT STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" +
                    strDisziplin +
                    "' AND id='" +
                    strFullNameComboBoxValue +
                    "' AND status='beendet'" +
                    strSchiessjahrFilter +
                    ") T;", conn);
            // Erzeuge Auswertungen
            int Wertungen;
            int.TryParse(cmd.ExecuteScalar().ToString(), out Wertungen);
            //Object o = cmd.ExecuteScalar();
            //int.TryParse(
            //
            //    mysql.doMySqlScalarQuery("SELECT COUNT(*) AS Wertungen FROM (SELECT DISTINCT STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + 
            //        strDisziplin + 
            //"' AND id='" + 
            //strFullNameComboBoxValue + 
            //"' AND status='beendet'" + 
            //strSchiessjahrFilter + 
            //") T;").ToString(), 
            //    out Wertungen);
            if (Wertungen >= 15)
            {
                cmd.CommandText = "SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15";
                MySqlDataReader reader = cmd.ExecuteReader();
                // MySqlDataReader reader = mysql.doMySqlReaderQuery("SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15");
                textbox.Text = Zeile1 + Environment.NewLine;
                textbox.Text += Zeile2 + Environment.NewLine;
                textbox.Text += "--------------------";
                textbox.Text += Environment.NewLine;
                textbox.Text += "    Datum       Ring" + Environment.NewLine;
                textbox.Text += "--------------------";
                textbox.Text += Environment.NewLine;
                int zaehler = 0;
                while (reader.Read())
                {
                    zaehler++;
                    string line = String.Format("{3,2}. {0:dd.MM.yyyy}   {1:6}{2}", reader["Date"], reader["ergebnis"], Environment.NewLine, zaehler);
                    textbox.Text += line;
                }
                textbox.Text += "--------------------";
                textbox.Text += Environment.NewLine;
                reader.Close();
                //cmd.CommandText = "SELECT COUNT(*) AS count, SUM(ergebnis) AS summe FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet' GROUP BY id ORDER";
                cmd.CommandText = "SELECT SUM(ergebnis) AS summe FROM (SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15) T2";
                //reader = mysql.doMySqlReaderQuery("SELECT SUM(ergebnis) AS summe FROM (SELECT MAX(ergebnis) AS ergebnis, Date FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + strDisziplin + "' AND id='" + fullnameComboBox.SelectedValue + "' AND status='beendet'" + strSchiessjahrFilter + ") T GROUP BY Date ORDER BY ergebnis DESC LIMIT 15) T2");
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["summe"] != System.DBNull.Value)
                        textbox.Text += String.Format("Summe: {0:7}", reader["summe"].ToString());
                }
                //mysql.closeMySqlReaderQuery(reader);
                reader.Close();
            } else
            {
                textbox.Text = "Zu wenige Wertungen." + Environment.NewLine + "Anzahl Wertungen: " + Wertungen;
            }

        }

        /// <summary>
        /// Wenn ein neuer Schütze in der fullnameComboBox ausgewählt wird, müssen auch alle Statistiken neu erstellt werden.
        /// Die Anzeige von Schießbuch und Trefferliste wird durch das DataBinding automatisch bewerkstelligt.
        /// Dieser EventHandler SelectedIndexChanged von fullnameComboBox kümmert sich um die Aktualisierung der Statistiken.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullnameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AktuellerSchuetzeStatistikAktualisieren();
        }

        /// <summary>
        /// Dieser EventHandler aktiviert oder deaktiviert den DateTimePicker, mit dem die Anzeige im Schießbuch auf nur einen Tag
        /// beschränkt werden kann.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                SchiessabendPicker.Enabled = true;
            else
                SchiessabendPicker.Enabled = false;
        }

        /// <summary>
        /// Je nach dem, ob die CheckBox aktiviert oder deaktiviert wurde, wird hier festgelegt, ob die Anzeige im Schießbuch sich auf
        /// das aktuelle Schießjahr oder auf das aktuell im Picker ausgewählte Datum beschränken soll.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SchiessabendPicker_EnabledChanged(object sender, EventArgs e)
        {
            if (SchiessabendPicker.Enabled == false)
            {
                // Das ganze Jahr soll angezeigt werden.
                populateSchiessjahrFilter();
            }
            else
            {
                // nur ein Tag soll angezeigt werden. Dazu wird der EventHandler ValueChanged des Pickers aufgerufen, so als ob man gerade ein
                // anderes Datum ausgewählt hätte. Das ist zwar nicht der Fall, wenn er Picker aktiviert wird, aber im Endeffekt ändert sich das Datum
                // auch von "disabled" auf Tag xxx
                SchiessabendPicker_ValueChanged(this, e);
            }
        }

        private void trefferDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (trefferDataGridView.SelectedRows.Count > 0)
            {
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
                if (row.Cells["zielscheibe"].Value != null && zielscheibe != row.Cells["zielscheibe"].Value.ToString())
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

            Pen pen = new Pen(Color.White, 1f);
            Brush brush = new SolidBrush(Color.FromArgb(150, Color.Red));
            Brush bluebrush = new SolidBrush(Color.Blue);
            float kaliber_mm = 4.5f;
            float pt_per_mm = 23.622f;
            float kaliber_pt = kaliber_mm * pt_per_mm;
            // Verkleinerung durch Zoom berechnen
            float real_pt_size = pictureBox1.Image.Width; // Properties.Resources.Luftpistole.Width;
            float render_points = pictureBox1.Width;
            float zoom_factor = real_pt_size / render_points;

            List<SchussInfo>[] schussliste = new List<SchussInfo>[1];
            schussliste[0] = new List<SchussInfo>();

            if (trefferDataGridView.SelectedRows.Count > 0) // Es macht nur Sinn, etwas zu zeichnen, wenn auch mindestens ein Schuss ausgewählt ist
            {

                foreach (DataGridViewRow row in trefferDataGridView.SelectedRows)
                {
                    SchussInfo si = new SchussInfo(
                        float.Parse(row.Cells["xrahmeninmm"].Value.ToString()),
                        float.Parse(row.Cells["yrahmeninmm"].Value.ToString()),
                        int.Parse(row.Cells["Ring"].Value.ToString()),
                        int.Parse(row.Cells["schussnummer"].Value.ToString()),
                        row.Cells["zielscheibe"].Value.ToString(),
                        int.Parse(row.Cells["schuetze"].Value.ToString()),
                        row.Cells["schuetze"].Value.ToString(), false);
                    schussliste[0].Add(si);


                    //float x_l_o = (float.Parse(row.Cells["xrahmeninmm"].Value.ToString()) - kaliber_mm / 2) * pt_per_mm;
                    //float y_l_o = (float.Parse(row.Cells["yrahmeninmm"].Value.ToString()) - kaliber_mm / 2) * pt_per_mm;
                    //float y_l_u = (float.Parse(row.Cells["yrahmeninmm"].Value.ToString()) + kaliber_mm / 2) * pt_per_mm;


                    //e.Graphics.FillEllipse(
                    //    brush, 
                    //    new Rectangle(
                    //        (int)(x_l_o / zoom_factor + pictureBox1.Width / 2), 
                    //        (int)(y_l_o / zoom_factor + pictureBox1.Height / 2), 
                    //        (int)(kaliber_pt / zoom_factor), 
                    //        (int)(kaliber_pt / zoom_factor)));
                    //gr.FillEllipse(bluebrush, new Rectangle((int)((-kaliber_pt / 2) / zoom_factor), (int)((-kaliber_pt / 2) / zoom_factor), (int)(kaliber_pt / zoom_factor), (int)(kaliber_pt / zoom_factor)));

                }
                Bitmap[] zielscheibeStr = new Bitmap[1];
                //zielscheibe[0] = new Bitmap;

                if (schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftgewehr) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftgewehrBlattl) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftgewehrBlattlRot) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftgewehrBlattlBlau))
                {
                    zielscheibeStr[0] = Properties.Resources.Luftgewehr;
                }
                if (schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftpistole) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftpistoleBlattl) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftpistoleBlattlRot) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftpistoleRot) || schussliste[0][0].strZielscheibe.Equals(strZielscheibeLuftpistoleBlattlBlau))
                {
                    zielscheibeStr[0] = Properties.Resources.Luftpistole;
                }

                Graphics g = e.Graphics;
                ZeichneTrefferInZielscheibe(pictureBox1, g, 0, schussliste, zielscheibeStr, false);
//                g.Dispose();
            }
        }

        private Thread RefreshTimerWorkerThread = null;
        private delegate void RefreshTimerTickDelegate();
        private RefreshTimerTickDelegate refreshTimerTickDelegate = null;
        private bool bTimerUpdateStillRunning=false;

        private System.Windows.Forms.Timer tmBildUpdateTimer = new System.Windows.Forms.Timer();
        bool bInRefreshTimerTick = false;
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (!bInRefreshTimerTick)
            {
                bInRefreshTimerTick = true;
                //  this.RefreshTimerWorkerThread = new Thread(RefreshTimerWorker);
                //  this.RefreshTimerWorkerThread.Start();

                //this.refreshTimerTickDelegate = new RefreshTimerTickDelegate(RefreshTimerWorker);
                RefreshTimerWorker();
                bInRefreshTimerTick = false;
            }
        }

        private void RefreshTimerWorker()
        {
            if (!bTimerUpdateStillRunning)
            {
                bTimerUpdateStillRunning = true;
                if (generateEinzelScheibe)
                {
                    setzeZielscheibeInEinzelansicht(standFuerEinzelScheibe);
                    UpdateStandTrefferDaten(standFuerEinzelScheibe);
                }
                if (generateOverview)
                {
                    updateOverview();
                    if (ergebnisbilder[0].bIsChanged) stand1Zielscheibe.Invalidate();
                    if (ergebnisbilder[1].bIsChanged) stand2Zielscheibe.Invalidate();
                    if (ergebnisbilder[2].bIsChanged) stand3Zielscheibe.Invalidate();
                    if (ergebnisbilder[3].bIsChanged) stand4Zielscheibe.Invalidate();
                    if (ergebnisbilder[4].bIsChanged) stand5Zielscheibe.Invalidate();
                    if (ergebnisbilder[5].bIsChanged) stand6Zielscheibe.Invalidate();
                }
                if (databaseRequestCounter == 0)
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

                        if (SchiessbuchScrollposition != -1 && schiessbuchDataGridView.RowCount > 0)
                            schiessbuchDataGridView.FirstDisplayedScrollingRowIndex = SchiessbuchScrollposition;
                        if (TrefferScrollposition != -1 && trefferDataGridView.RowCount > 0)
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
                            UpdateKoenig();
                        }
                    }
                }
                databaseRequestCounter = (databaseRequestCounter + 1) % Properties.Settings.Default.DatabaseInterval;
                bTimerUpdateStillRunning = false;
            }   
        }



        /// <summary>
        /// Hier wird die Übersichtsseite aktualisiert, also neu gezeichnet.
        /// </summary>
        private void updateOverview()
        {
            Pen pen = new Pen(Color.Red, 1f);
            Brush brush = new SolidBrush(Color.Red);
            Brush brush2 = new SolidBrush(Color.Blue);
            Brush brush3 = new SolidBrush(Color.Green);
            Brush brush4 = new SolidBrush(Color.LightGray);
            float num = 4.5f;
            float num2 = 23.622f;
            float num3 = num * num2;
            try
            {
                for (int stand = 1; stand <= 6; stand++)
                    UpdateStandTrefferDaten(stand);

            }
            catch (WebException)
            {

                MessageBox.Show("Keine Verbindung zum Schießstand-Computer. Aktualisierung wurde abgeschaltet.\nUm sie wieder einzuschalten, bitte im Reiter \"Schiessbuch\" die Schaltfläche mit den grünen Pfeilen wieder drücken.");
                DoUpdates.Checked = false;
            }            // Zeichne die richtigen Zielscheiben
            for (int iStand = 0; iStand < 6; iStand++)
                this.setzeZielscheibeInUebersicht(iStand);
        }

        private void UpdateStandTrefferDaten(int stand)
        {
            this.aktuelleTreffer[stand - 1].Clear();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.178.202/trefferliste?stand=" + stand.ToString());
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/trefferliste.xml");
            request.Method = "GET";
            XDocument document = new XDocument();
            //document = XDocument.Load(@"C:\Users\Thomas\Downloads\trefferliste.xml");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                document = XDocument.Load(response.GetResponseStream());
                string str = "";
                int iSchuetze = 0;
                string strZielscheibe = "";
                string strDisziplin = "";
                int schritt = -1;
                foreach (XElement element in document.Root.Elements())
                {
                    if (element.Name.ToString().Equals("treffer"))
                    {
                        float xrahmeninmm = 0f;
                        float yrahmeninmm = 0f;
                        string str2 = "";
                        int ring = 0;
                        int schussnummer = 0;
                        CultureInfo provider = new CultureInfo("en-US");
                        foreach (XElement element2 in element.Elements())
                        {
                            if (element2.Name.ToString().Equals("schuetze"))
                            {
                                if (iSchuetze == 0)
                                {
                                    iSchuetze = int.Parse(element2.Value);
                                }
                                else
                                {
                                    if (iSchuetze != int.Parse(element2.Value))
                                    {
                                        MessageBox.Show("verschiedene Schützen auf Stand vorhanden. Das sollte nicht vorkommen. Bitte genaue Umstände festhalten. Software muss angepasst werden.");
                                    }
                                }
                            }
                            if (element2.Name.ToString().Equals("disziplin"))
                            {
                                if (strDisziplin.Length == 0)
                                {
                                    strDisziplin = element2.Value;
                                }
                                else
                                {
                                    if (!element2.Value.Equals(strDisziplin))
                                    {
                                        MessageBox.Show("verschiedene Disziplinen auf Stand vorhanden. Das sollte nicht vorkommen. Bitte genaue Umstände festhalten. Software muss angepasst werden.");
                                    }
                                }
                            }
                            if (element2.Name.ToString().Equals("ring"))
                            {
                                ring = int.Parse(element2.Value);
                            }
                            if (element2.Name.ToString().Equals("zielscheibe"))
                            {
                                str2 = element2.Value;
                                if (strZielscheibe.Length == 0)
                                {
                                    strZielscheibe = element2.Value;
                                }
                                else
                                {
                                    if (!strZielscheibe.Equals(element2.Value))
                                    {
                                        // nicht für Gemeindemeisterschaft MessageBox.Show("verschiedene Zielscheiben auf Stand vorhanden. Das sollte nicht vorkommen. Bitte genaue Umstände festhalten. Software muss angepasst werden.");
                                    }
                                }
                            }
                            if (element2.Name.ToString().Equals("xRahmenInMm"))
                            {
                                xrahmeninmm = float.Parse(element2.Value.ToString(), provider);
                            }
                            if (element2.Name.ToString().Equals("yRahmenInMm"))
                            {
                                yrahmeninmm = float.Parse(element2.Value.ToString(), provider);
                            }
                            if (element2.Name.ToString().Equals("schussnummer"))
                            {
                                schussnummer = int.Parse(element2.Value);
                                if (schussnummer == 1) aktuelleTreffer[stand - 1].Clear();
                            }
                            if (element2.Name.ToString().Equals("schritt"))
                            {
                                schritt = int.Parse(element2.Value);
                            }
                        }
                        if ((str.Length == 0) || str.Equals(str2))
                        {
                            bool bProbe = false;
                            // MessageBox.Show(strDisziplin);
                            if (
                                (schritt == 0) && (
                                    strDisziplin.Equals("LG Gemeindemeisterschaft")
                                    || strDisziplin.Equals("LP Gemeindemeisterschaft")
                                    || strDisziplin.Equals("LG 20 Schuss")
                                    || strDisziplin.Equals("LG 20 Schuss Auflage")
                                    || strDisziplin.Equals("LG 30 Schuss Auflage")
                                    || strDisziplin.Equals("LG 40 Schuss")
                                    || strDisziplin.Equals("LP 20 Schuss")
                                    || strDisziplin.Equals("LP 40 Schuss")
                                    || strDisziplin.Equals("Gauliga")
                                    || strDisziplin.Equals("LG Auflage Gemeindemeisterschaft")
                                    || strDisziplin.Equals("LP Auflage Gemeindemeisterschaft")
                                    || strDisziplin.Equals("LG GMM 20 Schuss")
                                    || strDisziplin.Equals("LP GMM 20 Schuss")
                                    || strDisziplin.Equals("LG Auflage GMM 20 Schuss")
                                    || strDisziplin.Equals("LP Auflage GMM 20 Schuss")
                                    )
                                ) bProbe = true;
                            this.aktuelleTreffer[stand - 1].Add(new SchussInfo(xrahmeninmm, yrahmeninmm, ring, schussnummer, strZielscheibe, iSchuetze, strDisziplin, bProbe));
                        }
                    }
                }
            }
            for (int i = 0; i < 6; i++)
            {   // Wenn sich bei einem Stand entweder der Schütze oder die Anzahl der geschossenen Schüsse verändert,
                // vermerke das in einem Flag in der extra dafür vorgesehenen Struktur sSchusszahlVeraendert und sorge
                // damit für ein Neuzeichnen der Schusstabelle
                sSchusszahlVeraendert[i].stand = i + 1;
                if (aktuelleTreffer[i].Count > 0)
                {
                    int iTmp = aktuelleTreffer[i][0].schuetze;
                    if (sSchusszahlVeraendert[i].schuetze != iTmp)
                    {
                        sSchusszahlVeraendert[i].veraendert = true;
                        sSchusszahlVeraendert[i].schuetze = iTmp;
                    }
                    iTmp = aktuelleTreffer[i].Count;
                    if (sSchusszahlVeraendert[i].schusszahl != iTmp)
                    {
                        sSchusszahlVeraendert[i].veraendert = true;
                        sSchusszahlVeraendert[i].schusszahl = iTmp;
                    }
                }
            }
        }

        private class SchusszahlInfo
        {
            public bool bValid;
            public int stand;
            public int schuetze;
            public int schusszahl;
            public bool veraendert;
        }

        private SchusszahlInfo[] sSchusszahlVeraendert;

        public void TestZielscheibe()
        {

        }

        Ergebnisbild[] ergebnisbilder;

        private void CreateGraphicsForAnzeige(Ergebnisbild ergebnis, float boxWidth, float boxHeight, float imgWidth, float imgHeight, int stand, List<SchussInfo>[] trefferliste)
        {
            Pen pen = new Pen(Color.Red, 1f);
            Font font = new Font("Arial", 1f);
            float kaliber = 4.5f;
            float millimeterToPixel = 23.622f;
            float schusslochDurchmesser = kaliber * millimeterToPixel;
            Graphics graphics;
            float zoomFactor = imgWidth / boxWidth;
            int stand1 = stand + 1;
            int AnzTreffer = Properties.Settings.Default.AnzLetzteTreffer;
            int anzeigenAb = 0;
            if (trefferliste != null)
            {
                anzeigenAb = trefferliste[stand].Count - AnzTreffer;
                int trefferZaehler = 0;
                ergebnis.SetValid(false);
                if (ergebnis.bild != null) ergebnis.bild.Dispose();
                ergebnis.bild = new Bitmap(StandZielscheiben[stand]);
                graphics = Graphics.FromImage(ergebnis.bild);
                ergebnis.fMaxX = 0.0f;
                ergebnis.fMaxY = 0.0f;
                foreach (SchussInfo info in trefferliste[stand])
                {
                    trefferZaehler++;
                    if (trefferZaehler > anzeigenAb)
                    {
                        Brush brush;
                        float textWidth;
                        float textHeight;
                        float schussPosLinks = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        float schussPosOben = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        float schussPosOben2 = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                        float AbstandVonMitteX, AbstandVonMitteY;
                        if (schussPosLinks < 0)
                            AbstandVonMitteX = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        else
                            AbstandVonMitteX = (info.xrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                        if (schussPosOben < 0)
                            AbstandVonMitteY = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        else
                            AbstandVonMitteY = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;

                        ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "";
                        if (Math.Abs(AbstandVonMitteX) > ergebnis.fMaxX) ergebnis.fMaxX = Math.Abs(AbstandVonMitteX);
                        if (Math.Abs(AbstandVonMitteY) > ergebnis.fMaxY) ergebnis.fMaxY = Math.Abs(AbstandVonMitteY);
                        if (info == trefferliste[stand].Last<SchussInfo>())
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Red));
                        }
                        else if (info.ring < 10)
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.LightGray));
                        }
                        else
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Green));
                        }
                        if (info.probe)
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Blue));
                            ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "Probe";
                        }

                        Rectangle rect = new Rectangle(
                            (int)(schussPosLinks + imgWidth / 2),
                            (int)(imgHeight / 2 - schussPosOben2),
                            (int)(schusslochDurchmesser),
                            (int)(schusslochDurchmesser));

                        graphics.FillEllipse(brush, rect);
                        graphics.DrawEllipse(new Pen(Brushes.LightGray, 1f), rect);
                        string text = info.schussnummer.ToString();
                        while (true)
                        {
                            textWidth = graphics.MeasureString(text, font).Width;
                            textHeight = graphics.MeasureString(text, font).Height;
                            if ((textHeight > (rect.Height * 0.8)) || (textWidth > (rect.Width * 0.8)))
                            {
                                break;
                            }
                            font = new Font("Arial", font.Size + 1f);
                        }
                        graphics.DrawString(text, font, Brushes.White, (float)((rect.X + (rect.Width / 2)) - (textWidth / 2f)), (float)((rect.Y + (rect.Height / 2)) - (textHeight / 2f)));
                    }

                    // So, jetzt kümmern wir uns mal um das Zoomen...
                    if (ergebnis.fMaxX > ergebnis.fMaxY)
                        ergebnis.maxAbstand = ergebnis.fMaxX;
                    else
                        ergebnis.maxAbstand = ergebnis.fMaxY;

                    if (ergebnis.maxAbstand < 200)
                        ergebnis.maxAbstand = 200;
                }
                graphics.Dispose();
                ergebnis.SetValid(true);
            }
        }

        private void ZeichneTrefferInZielscheibe(PictureBox pictureBox, Graphics g, int stand, List<SchussInfo>[] trefferliste, Bitmap[] zielscheiben, bool FillMatrix)
        {
            Pen pen = new Pen(Color.Red, 1f);
            Font font = new Font("Arial", 1f);
            float kaliber = 4.5f;
            float millimeterToPixel = 23.622f;
            float schusslochDurchmesser = kaliber * millimeterToPixel;
            float imgWidth = pictureBox.Image.Width;
            float pictureBoxWidth = pictureBox.Width;
            float zoomFactor = imgWidth / pictureBoxWidth;
            int stand1 = stand + 1;
            int iSumme = 0;
            int AnzTreffer = Properties.Settings.Default.AnzLetzteTreffer;
            int anzeigenAb = 0;
            if (trefferliste != null)
            {
                anzeigenAb = trefferliste[stand].Count - AnzTreffer;
                int trefferZaehler = 0;
                Bitmap scheibeBitmap = new Bitmap(zielscheiben[stand]);
                Graphics graphics = Graphics.FromImage(scheibeBitmap);
                float maxX = 0.0f, maxY = 0.0f;
                foreach (SchussInfo info in trefferliste[stand])
                {
                    trefferZaehler++;
                    //scheibeBitmap = new Bitmap(zielscheiben[stand]);
                    //graphics = Graphics.FromImage(scheibeBitmap);

                    if (trefferZaehler > anzeigenAb)
                    {
                        Brush brush;
                        float textWidth;
                        float textHeight;
                        float schussPosLinks = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        float schussPosOben = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        float schussPosOben2 = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                        float AbstandVonMitteX, AbstandVonMitteY;
                        if (schussPosLinks < 0)
                            AbstandVonMitteX = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        else
                            AbstandVonMitteX = (info.xrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                        if (schussPosOben < 0)
                            AbstandVonMitteY = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                        else
                            AbstandVonMitteY = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;

                        //UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1) + "SplitContainer"].Controls["lblProbe" + (stand + 1)].Text = "";
                        ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "";
                        if (Math.Abs(AbstandVonMitteX) > maxX) maxX = Math.Abs(AbstandVonMitteX);
                        if (Math.Abs(AbstandVonMitteY) > maxY) maxY = Math.Abs(AbstandVonMitteY);
                        if (info == trefferliste[stand].Last<SchussInfo>())
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Red));
                        }
                        else if (info.ring < 10)
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.LightGray));
                        }
                        else
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Green));
                        }
                        if (info.probe)
                        {
                            brush = new SolidBrush(Color.FromArgb(120, Color.Blue));
                            //UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1) + "SplitContainer"].Controls["lblProbe" + (stand + 1)].Text = "Probe";
                            ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "Probe";
                        }
                        //Rectangle rect = new Rectangle(
                        //    ((int)(schussPosLinks)) + (pictureBox.Image.Width / 2), 
                        //    ((int)(schussPosOben)) + (pictureBox.Image.Height / 2), 
                        //    (int)(schusslochDurchmesser), 
                        //    (int)(schusslochDurchmesser));

                        Rectangle r10, r9, r8, r7, r6, r5, r4, r3, r2, r1;

                        r10 = new Rectangle(
                            pictureBox.Image.Width / 2 - (int)(0.25 * millimeterToPixel),
                            pictureBox.Image.Height / 2 - (int)(0.25 * millimeterToPixel),
                            (int)(0.5 * millimeterToPixel),
                            (int)(0.5 * millimeterToPixel));
                        Pen ringPen = new Pen(Color.Blue);
                        graphics.DrawEllipse(ringPen, r10);

                        r9 = new Rectangle(
                            pictureBox.Image.Width / 2 - (int)(2.25 * millimeterToPixel),
                            pictureBox.Image.Height / 2 - (int)(2.25 * millimeterToPixel),
                            (int)(4.5 * millimeterToPixel),
                            (int)(4.5 * millimeterToPixel));
                        graphics.DrawEllipse(ringPen, r9);

                        r8 = new Rectangle(
                            pictureBox.Image.Width / 2 - (int)(5.25 * millimeterToPixel),
                            pictureBox.Image.Height / 2 - (int)(5.25 * millimeterToPixel),
                            (int)(10.5 * millimeterToPixel),
                            (int)(10.5 * millimeterToPixel));
                        graphics.DrawEllipse(ringPen, r8);

                        r7 = new Rectangle(
                            pictureBox.Image.Width / 2 - (int)(7.75 * millimeterToPixel),
                            pictureBox.Image.Height / 2 - (int)(7.75 * millimeterToPixel),
                            (int)(15.5 * millimeterToPixel),
                            (int)(15.5 * millimeterToPixel));
                        graphics.DrawEllipse(ringPen, r7);

                        r6 = new Rectangle(
                            pictureBox.Image.Width / 2 - (int)(10.25 * millimeterToPixel),
                            pictureBox.Image.Height / 2 - (int)(10.25 * millimeterToPixel),
                            (int)(20.5 * millimeterToPixel),
                            (int)(20.5 * millimeterToPixel));
                        graphics.DrawEllipse(ringPen, r10);

                        Rectangle rect = new Rectangle(
                            ((int)(schussPosLinks)) + (pictureBox.Image.Width / 2),
                            (pictureBox.Image.Height / 2) - ((int)(schussPosOben2)),
                            (int)(schusslochDurchmesser),
                            (int)(schusslochDurchmesser));

                        graphics.FillEllipse(brush, rect);
                        graphics.DrawEllipse(new Pen(Brushes.LightGray, 1f), rect);
                        string text = info.schussnummer.ToString();
                        while (true)
                        {
                            textWidth = graphics.MeasureString(text, font).Width;
                            textHeight = graphics.MeasureString(text, font).Height;
                            if ((textHeight > (rect.Height * 0.8)) || (textWidth > (rect.Width * 0.8)))
                            {
                                break;
                            }
                            font = new Font("Arial", font.Size + 1f);
                        }
                        graphics.DrawString(text, font, Brushes.White, (float)((rect.X + (rect.Width / 2)) - (textWidth / 2f)), (float)((rect.Y + (rect.Height / 2)) - (textHeight / 2f)));

                        //                    e.Graphics.FillEllipse(brush, rect);
                        //                    e.Graphics.DrawEllipse(new Pen(Brushes.LightGray, 1f), rect);
                        //                    string text = info.schussnummer.ToString();
                        //                    while (true)
                        //                    {
                        //                        textWidth = e.Graphics.MeasureString(text, font).Width;
                        //                        textHeight = e.Graphics.MeasureString(text, font).Height;
                        //                        if ((textHeight > (rect.Height * 0.8)) || (textWidth > (rect.Width * 0.8)))
                        //                        {
                        //                            break;
                        //                        }
                        //                        font = new Font("Arial", font.Size + 1f);
                        //                    }
                        //                    e.Graphics.DrawString(text, font, Brushes.White, (float)((rect.X + (rect.Width / 2)) - (textWidth / 2f)), (float)((rect.Y + (rect.Height / 2)) - (textHeight / 2f)));
                    }

                    // So, jetzt kümmern wir uns mal um das Zoomen...
                    float maxAbstand;
                    if (maxX > maxY)
                        maxAbstand = maxX;
                    else
                        maxAbstand = maxY;

                    if (maxAbstand < 200)
                        maxAbstand = 200;

                    //maxAbstand = 200;
                    //MessageBox.Show(pictureBox.Name);
                    //MessageBox.Show("Size: " + pictureBox.Image.Width + "," + pictureBox.Image.Height + ".");

                    // Berechne die kleinste Seitenlänge und mache das Bild quadratisch
                    int seitenlaenge;
                    if (pictureBox.Width < pictureBox.Height)
                        seitenlaenge = pictureBox.Width;
                    else
                        seitenlaenge = pictureBox.Height;

                    if (maxAbstand > pictureBox.Image.Width / 2)
                        maxAbstand = pictureBox.Image.Width / 2;
                    // MessageBox.Show("MaxAbstand: " + maxAbstand.ToString() + ", seitenlaenge: " + seitenlaenge + ", Bildgroesse: " + pictureBox.Image.Width + "x" + pictureBox.Image.Height);
                    g.DrawImage(
                        scheibeBitmap,
                        new Rectangle((pictureBox.Width / 2) - (seitenlaenge / 2), (pictureBox.Height / 2) - (seitenlaenge / 2), seitenlaenge, seitenlaenge),
                        new Rectangle(
                            (int)(zielscheiben[stand].Width / 2 - maxAbstand),
                            (int)(zielscheiben[stand].Height / 2 - maxAbstand),
                            (int)(2 * maxAbstand), (int)(2 * maxAbstand)),
                        GraphicsUnit.Pixel);

                    // e.Graphics.DrawImage(scheibeBitmap, pictureBox.ClientRectangle);

                    // mal schauen, ob man das darf...
                    // ansonsten muss ich das wieder rausnehmen und aber dann schauen, wieso der Speicher voll läuft
                    //graphics.Dispose();
                    //scheibeBitmap.Dispose();




                    if (FillMatrix)
                    {
                        int spalte = (info.schussnummer - 1) % 5;
                        int zeile = (info.schussnummer - 1) / 5;
                        string str2 = "txtSchuss" + stand1.ToString() + spalte.ToString() + zeile.ToString();

                        ((TableLayoutPanel)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["Stand" + stand1.ToString() + "SchussPanel"]).Controls[str2].Text = info.ring.ToString();
                        iSumme += info.ring;
                    }
                }
                graphics.Dispose();
                scheibeBitmap.Dispose();
            }
            if (FillMatrix)
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["txtSchussStand" + stand1.ToString()]).Text = iSumme.ToString();
        }

        private void ZeichneTrefferInZielscheibe2(PictureBox pictureBox, PrintPageEventArgs e, int stand, List<SchussInfo>[] trefferliste, Bitmap[] zielscheiben, bool FillMatrix, Rectangle ziel)
        {
            Pen pen = new Pen(Color.Red, 1f);
            Font font = new Font("Arial", 1f);
            float kaliber = 4.5f;
            float millimeterToPixel = 23.622f;
            float schusslochDurchmesser = kaliber * millimeterToPixel;
            float imgWidth = pictureBox.Image.Width;
            float pictureBoxWidth = pictureBox.Width;
            float zoomFactor = imgWidth / pictureBoxWidth;
            int stand1 = stand + 1;
            int iSumme = 0;
            int AnzTreffer = Properties.Settings.Default.AnzLetzteTreffer;
            int anzeigenAb = trefferliste[stand].Count - AnzTreffer;
            int trefferZaehler = 0;
            Bitmap scheibeBitmap = new Bitmap(zielscheiben[stand]);
            Graphics graphics = Graphics.FromImage(scheibeBitmap);
            float maxX = 0.0f, maxY = 0.0f;
            foreach (SchussInfo info in trefferliste[stand])
            {
                trefferZaehler++;
                //scheibeBitmap = new Bitmap(zielscheiben[stand]);
                //graphics = Graphics.FromImage(scheibeBitmap);

                if (trefferZaehler > anzeigenAb)
                {
                    Brush brush;
                    float textWidth;
                    float textHeight;
                    float schussPosLinks = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                    float schussPosOben = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                    float schussPosOben2 = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                    float AbstandVonMitteX, AbstandVonMitteY;
                    if (schussPosLinks < 0)
                        AbstandVonMitteX = (info.xrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                    else
                        AbstandVonMitteX = (info.xrahmeninmm + (kaliber / 2f)) * millimeterToPixel;
                    if (schussPosOben < 0)
                        AbstandVonMitteY = (info.yrahmeninmm - (kaliber / 2f)) * millimeterToPixel;
                    else
                        AbstandVonMitteY = (info.yrahmeninmm + (kaliber / 2f)) * millimeterToPixel;

                    //UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1) + "SplitContainer"].Controls["lblProbe" + (stand + 1)].Text = "";
                    ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "";
                    if (Math.Abs(AbstandVonMitteX) > maxX) maxX = Math.Abs(AbstandVonMitteX);
                    if (Math.Abs(AbstandVonMitteY) > maxY) maxY = Math.Abs(AbstandVonMitteY);
                    if (info == trefferliste[stand].Last<SchussInfo>())
                    {
                        brush = new SolidBrush(Color.FromArgb(120, Color.Red));
                    }
                    else if (info.ring < 10)
                    {
                        brush = new SolidBrush(Color.FromArgb(120, Color.LightGray));
                    }
                    else
                    {
                        brush = new SolidBrush(Color.FromArgb(120, Color.Green));
                    }
                    if (info.probe)
                    {
                        brush = new SolidBrush(Color.FromArgb(120, Color.Blue));
                        //UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1) + "SplitContainer"].Controls["lblProbe" + (stand + 1)].Text = "Probe";
                        ((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["lblProbe" + stand1.ToString()].Text = "Probe";
                    }
                    //Rectangle rect = new Rectangle(
                    //    ((int)(schussPosLinks)) + (pictureBox.Image.Width / 2), 
                    //    ((int)(schussPosOben)) + (pictureBox.Image.Height / 2), 
                    //    (int)(schusslochDurchmesser), 
                    //    (int)(schusslochDurchmesser));

                    Rectangle r10, r9, r8, r7, r6, r5, r4, r3, r2, r1;

                    r10 = new Rectangle(
                        pictureBox.Image.Width / 2 - (int)(0.25 * millimeterToPixel),
                        pictureBox.Image.Height / 2 - (int)(0.25 * millimeterToPixel),
                        (int)(0.5 * millimeterToPixel),
                        (int)(0.5 * millimeterToPixel));
                    Pen ringPen = new Pen(Color.Blue);
                    graphics.DrawEllipse(ringPen, r10);

                    r9 = new Rectangle(
                        pictureBox.Image.Width / 2 - (int)(2.25 * millimeterToPixel),
                        pictureBox.Image.Height / 2 - (int)(2.25 * millimeterToPixel),
                        (int)(4.5 * millimeterToPixel),
                        (int)(4.5 * millimeterToPixel));
                    graphics.DrawEllipse(ringPen, r9);

                    r8 = new Rectangle(
                        pictureBox.Image.Width / 2 - (int)(5.25 * millimeterToPixel),
                        pictureBox.Image.Height / 2 - (int)(5.25 * millimeterToPixel),
                        (int)(10.5 * millimeterToPixel),
                        (int)(10.5 * millimeterToPixel));
                    graphics.DrawEllipse(ringPen, r8);

                    r7 = new Rectangle(
                        pictureBox.Image.Width / 2 - (int)(7.75 * millimeterToPixel),
                        pictureBox.Image.Height / 2 - (int)(7.75 * millimeterToPixel),
                        (int)(15.5 * millimeterToPixel),
                        (int)(15.5 * millimeterToPixel));
                    graphics.DrawEllipse(ringPen, r7);

                    r6 = new Rectangle(
                        pictureBox.Image.Width / 2 - (int)(10.25 * millimeterToPixel),
                        pictureBox.Image.Height / 2 - (int)(10.25 * millimeterToPixel),
                        (int)(20.5 * millimeterToPixel),
                        (int)(20.5 * millimeterToPixel));
                    graphics.DrawEllipse(ringPen, r10);

                    Rectangle rect = new Rectangle(
                        ((int)(schussPosLinks)) + (pictureBox.Image.Width / 2),
                        (pictureBox.Image.Height / 2) - ((int)(schussPosOben2)),
                        (int)(schusslochDurchmesser),
                        (int)(schusslochDurchmesser));

                    graphics.FillEllipse(brush, rect);
                    graphics.DrawEllipse(new Pen(Brushes.LightGray, 1f), rect);
                    string text = info.schussnummer.ToString();
                    while (true)
                    {
                        textWidth = graphics.MeasureString(text, font).Width;
                        textHeight = graphics.MeasureString(text, font).Height;
                        if ((textHeight > (rect.Height * 0.8)) || (textWidth > (rect.Width * 0.8)))
                        {
                            break;
                        }
                        font = new Font("Arial", font.Size + 1f);
                    }
                    graphics.DrawString(text, font, Brushes.White, (float)((rect.X + (rect.Width / 2)) - (textWidth / 2f)), (float)((rect.Y + (rect.Height / 2)) - (textHeight / 2f)));

                    //                    e.Graphics.FillEllipse(brush, rect);
                    //                    e.Graphics.DrawEllipse(new Pen(Brushes.LightGray, 1f), rect);
                    //                    string text = info.schussnummer.ToString();
                    //                    while (true)
                    //                    {
                    //                        textWidth = e.Graphics.MeasureString(text, font).Width;
                    //                        textHeight = e.Graphics.MeasureString(text, font).Height;
                    //                        if ((textHeight > (rect.Height * 0.8)) || (textWidth > (rect.Width * 0.8)))
                    //                        {
                    //                            break;
                    //                        }
                    //                        font = new Font("Arial", font.Size + 1f);
                    //                    }
                    //                    e.Graphics.DrawString(text, font, Brushes.White, (float)((rect.X + (rect.Width / 2)) - (textWidth / 2f)), (float)((rect.Y + (rect.Height / 2)) - (textHeight / 2f)));
                }

                // So, jetzt kümmern wir uns mal um das Zoomen...
                float maxAbstand;
                if (maxX > maxY)
                    maxAbstand = maxX;
                else
                    maxAbstand = maxY;

                if (maxAbstand < 200)
                    maxAbstand = 200;

                //maxAbstand = 200;
                //MessageBox.Show(pictureBox.Name);
                //MessageBox.Show("Size: " + pictureBox.Image.Width + "," + pictureBox.Image.Height + ".");

                // Berechne die kleinste Seitenlänge und mache das Bild quadratisch
                int seitenlaenge;
                if (pictureBox.Width < pictureBox.Height)
                    seitenlaenge = pictureBox.Width;
                else
                    seitenlaenge = pictureBox.Height;

                if (maxAbstand > pictureBox.Image.Width / 2)
                    maxAbstand = pictureBox.Image.Width / 2;
                // MessageBox.Show("MaxAbstand: " + maxAbstand.ToString() + ", seitenlaenge: " + seitenlaenge + ", Bildgroesse: " + pictureBox.Image.Width + "x" + pictureBox.Image.Height);

                e.Graphics.DrawImage(
                    scheibeBitmap,
                    ziel,
                    //new Rectangle((pictureBox.Width / 2) - (seitenlaenge / 2), (pictureBox.Height / 2) - (seitenlaenge / 2), seitenlaenge, seitenlaenge),
                    new Rectangle(
                        (int)(zielscheiben[stand].Width / 2 - maxAbstand),
                        (int)(zielscheiben[stand].Height / 2 - maxAbstand),
                        (int)(2 * maxAbstand), (int)(2 * maxAbstand)),
                    GraphicsUnit.Pixel);

                // e.Graphics.DrawImage(scheibeBitmap, pictureBox.ClientRectangle);

                // mal schauen, ob man das darf...
                // ansonsten muss ich das wieder rausnehmen und aber dann schauen, wieso der Speicher voll läuft
                //graphics.Dispose();
                //scheibeBitmap.Dispose();




                if (FillMatrix)
                {
                    int spalte = (info.schussnummer - 1) % 5;
                    int zeile = (info.schussnummer - 1) / 5;
                    string str2 = "txtSchuss" + stand1.ToString() + spalte.ToString() + zeile.ToString();

                    ((TableLayoutPanel)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["Stand" + stand1.ToString() + "SchussPanel"]).Controls[str2].Text = info.ring.ToString();
                    iSumme += info.ring;
                }
            }
            graphics.Dispose();
            scheibeBitmap.Dispose();

            if (FillMatrix)
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + stand1.ToString() + "SplitContainer"]).Panel2.Controls["txtSchussStand" + stand1.ToString()]).Text = iSumme.ToString();
        }

        private string GenerateKoenigSQLStatement(string Jahrgangsklasse)
        {
            string ZeitFilter;
            string strAuflage = "";
            if (dtJahrBeginn == dtJahrEnde) // aktuelles Jahr?
            {
                ZeitFilter = " HAVING Datum > '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' ";
            }
            else
            {
                ZeitFilter = " HAVING (Datum >= '" + dtJahrBeginn.ToString("yyyy-MM-dd") + "' AND Datum < '" + dtJahrEnde.ToString("yyyy-MM-dd") + "') ";
            }

            string strJahrgangsKlasse;
            string grundgeruest_Frei_Auflage = @"set @row=0;
            set @d1a = 'LG';
            set @d2a = 'LP';
            select
                @row:= @row + 1 AS Rang,
                   Schuetze,
                   Datum,
                   Teiler,
                   Jahrgangsklasse,
                   Geschlecht,
                   Typ FROM(
                       SELECT
                           Schuetze,
                           Datum,
                           MIN(Teiler) AS Teiler,
                           Jahrgangsklasse,
                           Geschlecht,
                           Typ from(
                               select
                                   fullname AS Schuetze,
                                   schuetzenliste.id as ID,
                                   Jahrgangsklasse AS Jahrgangsklasse,
                                   Geschlecht,
                                   STR_TO_DATE(datum, '%a %M %d %Y') AS Datum,
                                   convert(ergebnis, unsigned integer) AS Teiler,
                                   @d1a AS Typ
                               from
                                   schiessbuch
                                       inner join schuetzenliste on schuetzenliste.id = schiessbuch.id
                                   where
                                       disziplin IN {3}
                                       AND
                                       concat('', ergebnis * 1) = ergebnis
                                       AND
                                       schiessjahrId = {0}
                                       AND
                                       {1}
                                    {2}
                               UNION
                               select
                                   fullname AS Schuetze,
                                   schuetzenliste.id as ID,
                                   Jahrgangsklasse AS Jahrgangsklasse,
                                   Geschlecht,
                                   STR_TO_DATE(datum, '%a %M %d %Y') AS Datum,
                                   convert(ROUND(ergebnis / 2.6, 0), unsigned integer) AS Teiler,
                                   @d2a AS Typ
                               from
                                   schiessbuch inner join schuetzenliste on schuetzenliste.id = schiessbuch.id
                               where
                                   disziplin IN {4}
                                   AND
                                   concat('', ergebnis * 1) = ergebnis
                                   and
                                   schiessjahrId = {0}
                                   AND
                                   {1}
                                {2}
                           ) T
                       group by id
                       order by Teiler ASC
                   ) T2";

            string grundgeruest_Jugend_Schueler = @"set @row=0;
            set @d1a = 'LG';
            set @d2a = 'LGA';
            select
                @row:= @row + 1 AS Rang,
                   Schuetze,
                   Datum,
                   Teiler,
                   Jahrgangsklasse,
                   Geschlecht,
                   Typ FROM(
                       SELECT
                           Schuetze,
                           Datum,
                           MIN(Teiler) AS Teiler,
                           Jahrgangsklasse,
                           Geschlecht,
                           Typ from(
                               select
                                   fullname AS Schuetze,
                                   schuetzenliste.id as ID,
                                   Jahrgangsklasse AS Jahrgangsklasse,
                                   Geschlecht,
                                   STR_TO_DATE(datum, '%a %M %d %Y') AS Datum,
                                   convert(ergebnis, unsigned integer) AS Teiler,
                                   @d1a AS Typ
                               from
                                   schiessbuch
                                       inner join schuetzenliste on schuetzenliste.id = schiessbuch.id
                                   where
                                       disziplin IN {3}
                                       AND
                                       concat('', ergebnis * 1) = ergebnis
                                       AND
                                       schiessjahrId = {0}
                                       AND
                                       {1}
                                    {2}
                           ) T
                       group by id
                       order by Teiler ASC
                   ) T2";
            string sqlString;
            switch (Jahrgangsklasse)
            {
                case "Schützenklasse":
                    strJahrgangsKlasse = "((Jahrgangsklasse = 'Schützenklasse') OR (Jahrgangsklasse = 'Seniorenklasse' AND Geschlecht = 'm'))";
                    sqlString = string.Format(grundgeruest_Frei_Auflage, aktuellesSchiessjahrID, strJahrgangsKlasse, ZeitFilter, "('LG Koenig', 'LG Koenig SK', 'LG Koenig DK', 'LG Koenig JUG')", "('LP Koenig', 'LP Koenig SK', 'LP Koenig DK', 'LP Koenig JUG')");
                    break;
                case "Damenklasse":
                    strJahrgangsKlasse = "((Jahrgangsklasse = 'Damenklasse') OR (Jahrgangsklasse = 'Seniorenklasse' AND Geschlecht = 'w'))";
                    sqlString = string.Format(grundgeruest_Frei_Auflage, aktuellesSchiessjahrID, strJahrgangsKlasse, ZeitFilter, "('LG Koenig', 'LG Koenig SK', 'LG Koenig DK', 'LG Koenig JUG')", "('LP Koenig', 'LP Koenig SK', 'LP Koenig DK', 'LP Koenig JUG')");
                    break;
                case "Schülerklasse":
                case "Jugendklasse":
                    strJahrgangsKlasse = "(Jahrgangsklasse = 'Jugendklasse' OR Jahrgangsklasse = 'Schülerklasse')";
                    sqlString = string.Format(grundgeruest_Jugend_Schueler, aktuellesSchiessjahrID, strJahrgangsKlasse, ZeitFilter, "('LG Koenig', 'LG Koenig SK', 'LG Koenig DK', 'LG Koenig JUG', 'LG Koenig Auflage', 'LG Koenig JUG Auflage')");
                    //sqlString = string.Format(grundgeruest_Frei_Auflage, aktuellesSchiessjahrID, strJahrgangsKlasse, ZeitFilter, "('LG Koenig', 'LG Koenig SK', 'LG Koenig DK', 'LG Koenig JUG')", "('LP Koenig', 'LP Koenig SK', 'LP Koenig DK', 'LP Koenig JUG')");
                    break;
                case "Auflage":
                    strJahrgangsKlasse = "Jahrgangsklasse = 'Seniorenklasse'"; strAuflage = " Auflage";
                    sqlString = string.Format(grundgeruest_Frei_Auflage, aktuellesSchiessjahrID, strJahrgangsKlasse, ZeitFilter, "('LG Koenig Auflage', 'LG Koenig SK Auflage', 'LG Koenig DK Auflage', 'LG Koenig JUG Auflage')", "('LP Koenig Auflage', 'LP Koenig SK Auflage', 'LP Koenig DK Auflage', 'LP Koenig JUG Auflage')");
                    break;
                default: strJahrgangsKlasse = "1=1"; sqlString = ""; break;
            }


            return sqlString;

        }

        
        /// <summary>
        /// Berechnung und Anzeige des Schützenkönigs
        /// </summary>
        private void UpdateKoenig()
        {
            KoenigSKGridView.Rows.Clear();
            KoenigDKGridView.Rows.Clear();
            KoenigJUGGridView.Rows.Clear();
            KoenigAuflageGridView.Rows.Clear();
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                //mysql.doMySqlReaderQuery("set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2", conn);
                //MySqlCommand cmd = new MySqlCommand("set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2", conn);
                //MySqlCommand cmd = new MySqlCommand("select * from schuetzen", conn);
                //MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                MySqlCommand cmd = new MySqlCommand("set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2", conn);
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                //MySqlDataReader reader = mysql.doMySqlReaderQuery("set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2");
                //mysql.closeMySqlReaderQuery(reader);
                reader.Close();



                // Für neue König-Ansicht
                // erstelle Zeitfilter
                string ZeitFilter;
                if (dtJahrBeginn == dtJahrEnde) // aktuelles Jahr?
                {
                    ZeitFilter = " HAVING Datum > '" + dtJahrBeginn.ToShortDateString() + "' ";
                }
                else
                {
                    ZeitFilter = " HAVING (Datum >= '" + dtJahrBeginn.ToShortDateString() + "' AND Datum < '" + dtJahrEnde.ToShortDateString() + "') ";
                }
                //cmd.CommandText = "set @row=0;select @row:=@row+1 AS Rang, Schuetze, Teiler, Typ FROM (SELECT Schuetze, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ergebnis, unsigned integer) AS Teiler, 'LG' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LG Koenig' AND concat('',ergebnis * 1) = ergebnis UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, 'LP' AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='LP Koenig' AND concat('',ergebnis * 1) = ergebnis) T GROUP BY ID ORDER BY Teiler ASC ) T2";
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig SK';set @d1a='LG';set @d2='LP Koenig SK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                //reader = mysql.doMySqlReaderQuery("set @row=0;set @d1='LG Koenig';set @d1a='LG';set @d2='LP Koenig';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig';set @d1a='LG';set @d2='LP Koenig';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                cmd.CommandText = GenerateKoenigSQLStatement("Schützenklasse");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig';set @d1a='LG';set @d2='LP Koenig';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                int row = 0;
                while (reader.Read())
                {
                    if (KoenigSKGridView.InvokeRequired)
                    {
                        KoenigSKGridView.BeginInvoke(
                            new MethodInvoker(
                                delegate () { row = FillSchuetzenklasseDGV(reader); }));
                    }
                    else
                    {
                        row = FillSchuetzenklasseDGV(reader);
                    }
                    
                }
                reader.Close();
                //mysql.closeMySqlReaderQuery(reader);
                //reader = mysql.doMySqlReaderQuery("set @row=0;set @d1='LG Koenig DK';set @d1a='LG';set @d2='LP Koenig DK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig DK';set @d1a='LG';set @d2='LP Koenig DK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                cmd.CommandText = GenerateKoenigSQLStatement("Damenklasse");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig DK';set @d1a='LG';set @d2='LP Koenig DK';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    if (KoenigSKGridView.InvokeRequired)
                    {
                        KoenigSKGridView.BeginInvoke(
                            new MethodInvoker(
                                delegate () { row = FillDamenklasseDGV(reader); }));
                    }
                    else
                    {
                        row = FillDamenklasseDGV(reader);
                    }
                }
                reader.Close();
                //mysql.closeMySqlReaderQuery(reader);
                //reader = mysql.doMySqlReaderQuery("set @row=0;set @d1='LG Koenig JUG';set @d1a='LG';set @d2='LP Koenig JUG';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig JUG';set @d1a='LG';set @d2='LP Koenig JUG';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                cmd.CommandText = GenerateKoenigSQLStatement("Jugendklasse");
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    if (KoenigSKGridView.InvokeRequired)
                    {
                        KoenigSKGridView.BeginInvoke(
                            new MethodInvoker(
                                delegate () { row = FillJugendklasseDGV(reader); }));
                    }
                    else
                    {
                        row = FillJugendklasseDGV(reader);
                    }
                }
                reader.Close();
                //mysql.closeMySqlReaderQuery(reader);
                //reader = mysql.doMySqlReaderQuery("set @row=0;set @d1='LG Koenig Auflage';set @d1a='LG';set @d2='LP Koenig Auflage';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2");
                //cmd.CommandText = "set @row=0;set @d1='LG Koenig Auflage';set @d1a='LG';set @d2='LP Koenig Auflage';set @d2a='LP';select @row:=@row+1 AS Rang, Schuetze, Datum, Teiler, Typ FROM (SELECT Schuetze, Datum, MIN(Teiler) AS Teiler, Typ from (select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ergebnis, unsigned integer) AS Teiler, @d1a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d1 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " UNION select CONCAT(name, ', ', vorname) AS Schuetze, schuetzen.id as ID, STR_TO_DATE(datum, '%a %M %d %Y') AS Datum, convert(ROUND (ergebnis / 2.6, 0), unsigned integer) AS Teiler, @d2a AS Typ from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin=@d2 AND concat('',ergebnis * 1) = ergebnis " + ZeitFilter + " ) T group by id order by Teiler ASC) T2";
                cmd.CommandText = GenerateKoenigSQLStatement("Auflage");
                reader = cmd.ExecuteReader();
                row = 0;
                while (reader.Read())
                {
                    if (KoenigSKGridView.InvokeRequired)
                    {
                        KoenigSKGridView.BeginInvoke(
                            new MethodInvoker(
                                delegate () { row = FillAuflageDGV(reader); }));
                    }
                    else
                    {
                        row = FillAuflageDGV(reader);
                    }
                }
                reader.Close();
                //mysql.closeMySqlReaderQuery(reader);






                //reader.Close();
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

        private int FillAuflageDGV(MySqlDataReader reader)
        {
            int row = KoenigAuflageGridView.Rows.Add();
            KoenigAuflageGridView.Rows[row].Cells["PositionAuflage"].Value = reader["Rang"].ToString();
            KoenigAuflageGridView.Rows[row].Cells["FullnameAuflage"].Value = reader["Schuetze"].ToString();
            KoenigAuflageGridView.Rows[row].Cells["DatumAuflage"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
            KoenigAuflageGridView.Rows[row].Cells["TeilerAuflage"].Value = reader["Teiler"].ToString();
            KoenigAuflageGridView.Rows[row].Cells["TypAuflage"].Value = reader["Typ"].ToString();
            row++;
            return row;
        }

        private int FillJugendklasseDGV(MySqlDataReader reader)
        {
            int row = KoenigJUGGridView.Rows.Add();
            KoenigJUGGridView.Rows[row].Cells["PositionJUG"].Value = reader["Rang"].ToString();
            KoenigJUGGridView.Rows[row].Cells["FullnameJUG"].Value = reader["Schuetze"].ToString();
            KoenigJUGGridView.Rows[row].Cells["DatumJUG"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
            KoenigJUGGridView.Rows[row].Cells["TeilerJUG"].Value = reader["Teiler"].ToString();
            KoenigJUGGridView.Rows[row].Cells["TypJUG"].Value = reader["Typ"].ToString();
            row++;
            return row;
        }

        private int FillDamenklasseDGV(MySqlDataReader reader)
        {
            int row = KoenigDKGridView.Rows.Add();
            KoenigDKGridView.Rows[row].Cells["PositionDK"].Value = reader["Rang"].ToString();
            KoenigDKGridView.Rows[row].Cells["FullnameDK"].Value = reader["Schuetze"].ToString();
            KoenigDKGridView.Rows[row].Cells["DatumDK"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
            KoenigDKGridView.Rows[row].Cells["TeilerDK"].Value = reader["Teiler"].ToString();
            KoenigDKGridView.Rows[row].Cells["TypDK"].Value = reader["Typ"].ToString();
            row++;
            return row;
        }

        private int FillSchuetzenklasseDGV(MySqlDataReader reader)
        {
            int row = KoenigSKGridView.Rows.Add();
            KoenigSKGridView.Rows[row].Cells["Position"].Value = reader["Rang"].ToString();
            KoenigSKGridView.Rows[row].Cells["Fullname"].Value = reader["Schuetze"].ToString();
            KoenigSKGridView.Rows[row].Cells["Datum"].Value = DateTime.Parse(reader["Datum"].ToString()).ToShortDateString();
            KoenigSKGridView.Rows[row].Cells["Teiler"].Value = reader["Teiler"].ToString();
            KoenigSKGridView.Rows[row].Cells["Typ"].Value = reader["Typ"].ToString();
            row++;
            return row;
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

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ManuellNachtragen manuell = new ManuellNachtragen();
            manuell.id = idTextBox.Text;
            manuell.connStr = connStr;
            manuell.ShowDialog();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            GauligaAuswertung gauliga = new GauligaAuswertung();
            gauliga.ShowDialog();
        }

        /// <summary>
        /// Wird aufgerufen wenn im tabControl im Hauptdialog ein neuer Tab ausgewählt wird
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // zuerst mal werden alle periodisch arbeitenden Aktualisierungen abgeschaltet.
            // Sollte nämlich eine davon bis jetzt gelaufen sein (sollte also der entsprechende Tab aktiv gewesen sein),
            // so ist das nach dem Wechsel des Tabs nicht mehr der Fall und die Aktualisierung ist nicht mehr notwendig.
            // Sollte die Aktualisierung nicht aktiv gewesen sein, schadet eine Beendigung der sowieso schon beendeten
            // Aktualisierung nichts.
            this.stopUebersicht();
            this.stopEinzelScheibe();
            tmBildUpdateTimer.Stop();
            if (tabControl1.SelectedTab.Text.Equals("König"))
                UpdateKoenig();
            if (tabControl1.SelectedTab.Text.Equals("Tagesauswertung"))
                ErstelleAuswertung();
            if (tabControl1.SelectedTab.Text.Equals("Übersicht"))
                startUebersicht();
            if (tabControl1.SelectedTab.Text.Equals("Gemeindemeisterschaft"))
                ErstelleAuswertungGemeindemeisterschaft();
            if (!tabControl1.SelectedTab.Name.Equals("tabEinzelscheibe"))
                tabControl1.TabPages.RemoveByKey("tabEinzelscheibe");
            else
                startEinzelScheibe();
        }

        private void startEinzelScheibe()
        {
            this.generateEinzelScheibe = true;
        }

        private void stopEinzelScheibe()
        {
            this.generateEinzelScheibe = false;
        }

        private void stopUebersicht()
        {
            this.generateOverview = false;
        }

        private void startUebersicht()
        {
            tmBildUpdateTimer.Start();
            foreach (Ergebnisbild ereignisbild in ergebnisbilder)
            {
                ereignisbild.bIsChanged = true; // Nach dem Umstellen auf den Übersicht-Tab sollen alle Bilder nochmal neu gezeichnet werden - zur Sicherheit ;-)
            }
            this.generateOverview = true;
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
                vereinTextBox.Visible = false;
                vereinComboBox.Visible = true;
                bindingNavigatorAddNewItem.Enabled = true;
                bindingNavigatorDeleteItem.Enabled = true;
                geschlechtTextBox.ReadOnly = false;
                geburtsdatumTextBox.Visible = false;
                GeburtstagDateTimePicker.Visible = true;
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
                geschlechtTextBox.ReadOnly = true;
                geburtsdatumTextBox.Visible = true;
                geburtsdatumTextBox.ReadOnly = true;
                GeburtstagDateTimePicker.Visible = false;
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            InsertOrUpdateDatabaseWithNewSchuetze();
            //bindingNavigatorAddNewItem.Enabled = true;
            //saveToolStripButton1.Enabled = true;
            //bearbeitungsmodusToolStripMenuItem.Checked = false;
            //SetEnableDisableEditControls(false);
            schuetzenlisteTableAdapter.Fill(this.siusclubDataSet.schuetzenliste);
            bindingNavigatorAddNewItem.Enabled = true;
        }

        private void InsertOrUpdateDatabaseWithNewSchuetze()
        {
            string str;
            MySqlCommand command;
            MySqlConnection connection = new MySqlConnection(connStr);
            if (Int16.Parse(this.idTextBox.Text) < 0)
            {
                if ((((this.nameTextBox.Text.Length != 0) && (this.vornameTextBox.Text.Length != 0)) && (this.vereinComboBox.Text.Length != 0)) && (this.geschlechtTextBox.Text.Length > 0))
                {
                    str = "INSERT INTO schuetzen (name, vorname, email, verein) VALUES (@name, @vorname, @email, @verein); select last_insert_id()";
                    connection.Open();
                    command = new MySqlCommand(str, connection);
                    command.Parameters.Add("@name", MySqlDbType.VarChar).Value = this.nameTextBox.Text;
                    command.Parameters.Add("@vorname", MySqlDbType.VarChar).Value = this.vornameTextBox.Text;
                    command.Parameters.Add("@email", MySqlDbType.VarChar).Value = this.emailTextBox.Text;
                    command.Parameters.Add("@verein", MySqlDbType.VarChar).Value = this.vereinComboBox.Text;
                    int result = 0;
                    if (int.TryParse(command.ExecuteScalar().ToString(), out result))
                    {
                        str = "INSERT INTO schuetzendetails (idschuetzen, geburtsdatum, geschlecht) VALUES (@idschuetzen, @geburtsdatum, @geschlecht)";
                        command.CommandText = str;
                        command.Parameters.Add("@idschuetzen", MySqlDbType.Int32).Value = result;
                        command.Parameters.Add("@geburtsdatum", MySqlDbType.Date).Value = this.GeburtstagDateTimePicker.Value.ToString("yyyy-MM-dd");
                        command.Parameters.Add("@geschlecht", MySqlDbType.VarChar).Value = this.geschlechtTextBox.Text;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Eintragen eines neuen Sch\x00fctzen.");
                    }
                }
                else
                {
                    MessageBox.Show("Nicht alle notwendigen Angaben wurden in der Maske gemacht.");
                }
            }
            else if ((((this.nameTextBox.Text.Length != 0) && (this.vornameTextBox.Text.Length != 0)) && (this.vereinComboBox.Text.Length != 0)) && (this.geschlechtTextBox.Text.Length > 0))
            {
                str = "UPDATE schuetzen SET name=@name,vorname=@vorname,email=@email,verein=@verein WHERE id=@id";
                connection.Open();
                command = new MySqlCommand(str, connection);
                command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.idTextBox.Text;
                command.Parameters.Add("@name", MySqlDbType.VarChar).Value = this.nameTextBox.Text;
                command.Parameters.Add("@vorname", MySqlDbType.VarChar).Value = this.vornameTextBox.Text;
                command.Parameters.Add("@email", MySqlDbType.VarChar).Value = this.emailTextBox.Text;
                command.Parameters.Add("@verein", MySqlDbType.VarChar).Value = this.vereinComboBox.Text;
                command.ExecuteNonQuery();
                str = "INSERT INTO schuetzendetails (idschuetzen, geburtsdatum, geschlecht) VALUES (@idschuetzen, @geburtsdatum, @geschlecht) ON DUPLICATE KEY UPDATE idschuetzen=@idschuetzen,geburtsdatum=@geburtsdatum,geschlecht=@geschlecht";
                command.CommandText = str;
                command.Parameters.Add("@idschuetzen", MySqlDbType.Int32).Value = this.idTextBox.Text;
                command.Parameters.Add("@geburtsdatum", MySqlDbType.Date).Value = this.GeburtstagDateTimePicker.Value.ToString("yyyy-MM-dd");
                command.Parameters.Add("@geschlecht", MySqlDbType.VarChar).Value = this.geschlechtTextBox.Text;
                command.ExecuteNonQuery();
            }
            else
            {
                MessageBox.Show("Nicht alle notwendigen Angaben wurden in der Maske gemacht.");
            }
        }

        private void nameTextBox_Leave(object sender, EventArgs e)
        {
        }

        private void bindingNavigatorDeleteItem1_Click(object sender, EventArgs e)
        {
            if (siusclubDataSet.HasChanges())
                {
                    saveToolStripButton1.Enabled = true;
                    bindingNavigatorAddNewItem.Enabled = false;
                }
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
            bindingNavigatorAddNewItem.Enabled = false;
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)bearbeitungsmodusToolStripMenuItem).Checked == true)
            {
                saveToolStripButton1.Enabled = true;
                bindingNavigatorAddNewItem.Enabled = false;
            }
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
            MySqlConnection conn = new MySqlConnection(connStr);
            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            try
            {
                conn.Open();
                string filterDateStr = dateTimePicker1.Value.Year + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Day;
                MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT DISZIPLIN, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch HAVING Date='" + filterDateStr + "'", conn);
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
                //MySqlDataReader reader = mysql.doMySqlReaderQuery("SELECT DISTINCT DISZIPLIN, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch HAVING Date='" + filterDateStr + "'");
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
                //mysql.closeMySqlReaderQuery(reader);

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
                //mysql.doMySqlReaderQuery("SELECT DISTINCT schuetzen.id as SID, name, vorname, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id WHERE status='beendet' OR status='manuell' HAVING Date='" + filterDateStr + "' ORDER BY name, vorname");
                cmd.CommandText = "SELECT DISTINCT schuetzen.id as SID, name, vorname, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id WHERE status='beendet' OR status='manuell' HAVING Date='" + filterDateStr + "' ORDER BY name, vorname";
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

                    MySqlConnection conn2 = new MySqlConnection(connStr);
                    conn2.Open();
                    MySqlDataReader reader2;
                    for (int j = 0; j < disziplinen; j++)
                    {

                        //MessageBox.Show(Schiessabend.Columns[j + 3].Name);
                        string cmdstr = "select MAX(CONVERT(ergebnis, UNSIGNED INTEGER)) AS ergebnis FROM (SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + Schiessabend.Columns[j + 3].Name + "' AND id='" + reader["SID"] + "' AND (status='beendet' OR status='manuell') HAVING Date='" + filterDateStr + "') T";
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
                //mysql.closeMySqlReaderQuery(reader);
                reader.Close();
                reader.Dispose();
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
                MessageBox.Show(mysqle.StackTrace);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ErstelleAuswertung();
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

        private void btnTagesAuswertungListeDrucken_Click(object sender, EventArgs e)
        {
            if (pd == null)
            {
                pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPageListe);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();

            ConnectToDatabaseForTagesAuswertungListe();


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

        private void ConnectToDatabaseForTagesAuswertungListe()
        {
            string filterDateStr = dateTimePicker1.Value.Year + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Day;
            TagesAuswertungListeConnection = new MySqlConnection(connStr);
            TagesAuswertungListeConnection.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT `name`, vorname, concat(name, ', ', vorname) as fullname, disziplin, ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id WHERE status='beendet' HAVING Date='" + filterDateStr + "'  order by fullname", TagesAuswertungListeConnection);
            TagesAuswertungListeDataReader = cmd.ExecuteReader();
            TagesAuswertungConnectionIsActive = true;
        }

        private void pd_PrintPageListe(object sender, PrintPageEventArgs ev)
        {
            if (!TagesAuswertungConnectionIsActive) ConnectToDatabaseForTagesAuswertungListe();

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
                }
                else
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
            TagesAuswertungConnectionIsActive = false;
        }

        private void splitContainer1_Resize(object sender, EventArgs e)
        {
            splitContainerKoenig1.SplitterDistance = splitContainerKoenig1.Width / 2;
        }

        private void splitContainer2_Resize(object sender, EventArgs e)
        {
            splitContainerKoenig2.SplitterDistance = splitContainerKoenig2.Height / 2;
        }

        private void splitContainer3_Resize(object sender, EventArgs e)
        {
            splitContainerKoenig3.SplitterDistance = splitContainerKoenig3.Height / 2;
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
            MySqlConnection conn = new MySqlConnection(connStr);
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


        #region Alte Königsberechnungen
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
        #endregion

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
            Stand1SchussPanel.Height = (int)(Stand1SchussPanel.Width * (8 / 5));
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

            int disziplinen = 0;
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
                        if (Schiessabend[3 + i, currentLinesPrinted].Value.ToString().Length != 0)
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

        private void schießabendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Druckansicht da = new Druckansicht();
            //da.ShowDialog();
        }

        private void neuesSchießjahrBeginnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NeuesSchiessjahr neuSj = new NeuesSchiessjahr();
            neuSj.ShowDialog();
            AktualisiereSchiessjahrMenu();
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

        private void cleanSchussTable(int stand)
        {
            int spalte;
            string strSpalte;
            int zeile;
            string strZeile;
            for (int i = 1; i <= 40; i++)
            {
                int num2 = stand + 1;
                spalte = (i - 1) % 5;
                strSpalte = spalte.ToString();
                zeile = (i - 1) / 5;
                strZeile = zeile.ToString();
                string str = "txtSchuss" + num2.ToString() + strSpalte + strZeile;

                ((TableLayoutPanel)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + num2.ToString() + "SplitContainer"]).Panel2.Controls["Stand" + num2.ToString() + "SchussPanel"]).Controls[str].Text = "";
            }
        }

        private void GeburtstagDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (this.bearbeitungsmodusToolStripMenuItem.Checked)
            {
                saveToolStripButton1.Enabled = true;
                bindingNavigatorAddNewItem.Enabled = false;
            }
        }

        private void standXZielscheibe_Paint(int stand, object sender, PaintEventArgs e)
        {
            PictureBox pb;
            pb = (PictureBox)Controls["tabControl1"]
                .Controls["tabStandUebersicht"]
                .Controls["UebersichtTableLayoutPanel"]
                .Controls["Stand" + (stand + 1).ToString() + "SplitContainer"]
                .Controls[0]
                .Controls["stand" + (stand + 1).ToString() + "Zielscheibe"];

            // Berechne die kleinste Seitenlänge und mache das Bild quadratisch
            int seitenlaenge;
            if (pb.Width < pb.Height)
                seitenlaenge = (int)pb.Width;
            else
                seitenlaenge = (int)pb.Height;

            if (ergebnisbilder[stand].maxAbstand > pb.Image.Width / 2)
                ergebnisbilder[stand].maxAbstand = pb.Image.Width / 2;

            if (ergebnisbilder[stand].IsValid())
            {
                e.Graphics.DrawImage(
                    ergebnisbilder[stand].bild,
                    new Rectangle((pb.Width / 2) - (seitenlaenge / 2), (pb.Height / 2) - (seitenlaenge / 2), seitenlaenge, seitenlaenge),
                    new Rectangle(
                        (int)(StandZielscheiben[stand].Width / 2 - ergebnisbilder[stand].maxAbstand),
                        (int)(StandZielscheiben[stand].Height / 2 - ergebnisbilder[stand].maxAbstand),
                        (int)(2 * ergebnisbilder[stand].maxAbstand), (int)(2 * ergebnisbilder[stand].maxAbstand)),
                    GraphicsUnit.Pixel);
                ergebnisbilder[stand].bIsChanged = false;
                //e.Graphics.Dispose();
            }

            if (sSchusszahlVeraendert[stand].veraendert)
            {
                this.cleanSchussTable(stand);
                // Fülle die Tabelle aller Schüsse auf
                int iSumme = 0;
                foreach (SchussInfo info in aktuelleTreffer[stand])
                {
                    int spalte = (info.schussnummer - 1) % 5;
                    int zeile = (info.schussnummer - 1) / 5;
                    string str2 = "txtSchuss" + (stand + 1).ToString() + spalte.ToString() + zeile.ToString();

                    ((TableLayoutPanel)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1).ToString() + "SplitContainer"]).Panel2.Controls["Stand" + (stand + 1).ToString() + "SchussPanel"]).Controls[str2].Text = info.ring.ToString();
                    iSumme += info.ring;
                }
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + (stand + 1).ToString() + "SplitContainer"]).Panel2.Controls["txtSchussStand" + (stand + 1).ToString()]).Text = iSumme.ToString();
                sSchusszahlVeraendert[stand].veraendert = false; // bestätige, dass die Veränderung berücksichtigt wurde und verhindere ein Neuzeichnen bis zur nächsten Veränderung
            }
        }

        private void stand1Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(0, sender, e);
        }

        private void stand2Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(1, sender, e);
        }

        private void stand3Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(2, sender, e);
        }

        private void stand4Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(3, sender, e);
        }

        private void stand5Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(4, sender, e);
        }

        private void stand6Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            standXZielscheibe_Paint(5, sender, e);
        }

        private void SetTextAcrossThread(Control control, string text)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(
                    new MethodInvoker(
                        delegate () { SetTextAcrossThread(control, text); }));
            } else
            {
                control.Text = text;
            }
        }

        private void SetImageAcrossThread(PictureBox control, Bitmap image)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(
                    new MethodInvoker(
                        delegate () { SetImageAcrossThread(control, image); }));
            }
            else
            {
                control.Image = image;
            }
        }

        private void setzeZielscheibeInUebersicht(int stand)
        {
            string strStand = (stand + 1).ToString();
            Bitmap bScheibe = schiessbuch.Properties.Resources.Luftgewehr;
            try
            {
                string strZielscheibeInXML = aktuelleTreffer[stand][0].strZielscheibe; // ich lese die Zielscheibe einfach aus dem ersten schuss aus und hoffe, dass diese dann auch bei allen anderen schüssen die selbe ist. Falls nicht, wird sowieso eine Fehlermeldung ausgegeben.
                if (strZielscheibeInXML.Equals(strZielscheibeLuftgewehr) || strZielscheibeInXML.Equals(strZielscheibeLuftgewehrBlattl) || strZielscheibeInXML.Equals(strZielscheibeLuftgewehrBlattlRot))
                {
                    bScheibe = schiessbuch.Properties.Resources.Luftgewehr;
                    StandZielscheiben[stand] = Properties.Resources.Luftgewehr;
                }
                if (strZielscheibeInXML.Equals(strZielscheibeLuftpistole) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleBlattl) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleBlattlRot) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleRot))
                {
                    bScheibe = schiessbuch.Properties.Resources.Luftpistole;
                    StandZielscheiben[stand] = Properties.Resources.Luftpistole;
                }
            }
            catch (ArgumentOutOfRangeException)
            { // Exception soll einfach ignoriert werden 
            }
            SetImageAcrossThread(((PictureBox)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel1.Controls["Stand" + strStand + "Zielscheibe"]), bScheibe);
            //((PictureBox)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel1.Controls["Stand" + strStand + "Zielscheibe"]).Image = bScheibe;

            // Disziplin setzen
            try
            {
                SetTextAcrossThread(((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtDisziplinStand" + strStand]), aktuelleTreffer[stand][0].disziplin);
            }
            catch (ArgumentOutOfRangeException)
            {
                SetTextAcrossThread(((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtDisziplinStand" + strStand]), "keine");
                //((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtDisziplinStand" + strStand]).Text = "keine";
            }

            // Schütze setzen
            MySqlConnection conn = new MySqlConnection(connStr);
            try {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM schuetzenliste WHERE id='" + aktuelleTreffer[stand][0].schuetze.ToString() + "' AND SchiessjahrID='" + aktuellesSchiessjahrID.ToString() + "'", conn);
                
                MySqlDataReader reader= cmd.ExecuteReader();
                if (reader.Read())
                {
                    SetTextAcrossThread(((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]), reader["fullname"].ToString());
                    // ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]).Text = reader["fullname"].ToString();
                } else
                {
                    SetTextAcrossThread(((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]), "Gastschütze");
                    // ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]).Text = "Gastschütze";
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                SetTextAcrossThread(((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]), "kein Schütze");
                // ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]).Text = "kein Schütze";
            }
            catch (MySqlException e)
            {
                MessageBox.Show("Fehler beim Lesen von Daten aus der Datenbank.");
                MessageBox.Show("Fehler: " + e.Message);
                MessageBox.Show("Stack:" + e.StackTrace);
                MessageBox.Show("Connection string: " + connStr);
            }
            conn.Clone();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);

        }

        /*private void stand2Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            this.cleanSchussTable(1);
            //ZeichneTrefferInZielscheibe(stand2Zielscheibe, e, 1, aktuelleTreffer, StandZielscheiben, true);
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            BackgroundWorkerZeichneTrefferArguments bwArguments = new BackgroundWorkerZeichneTrefferArguments();
            bwArguments.pictureBox = this.stand2Zielscheibe;
            bwArguments.g = e.Graphics;
            bwArguments.stand = 1;
            bwArguments.trefferliste = aktuelleTreffer;
            bwArguments.zielscheiben = StandZielscheiben;
            bwArguments.fillMatrix = true;
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(bwArguments);
            }
        }

        private void stand3Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            this.cleanSchussTable(2);
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            BackgroundWorkerZeichneTrefferArguments bwArguments = new BackgroundWorkerZeichneTrefferArguments();
            bwArguments.pictureBox = this.stand3Zielscheibe;
            bwArguments.g = e.Graphics;
            bwArguments.stand = 2;
            bwArguments.trefferliste = aktuelleTreffer;
            bwArguments.zielscheiben = StandZielscheiben;
            bwArguments.fillMatrix = true;
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(bwArguments);
            }
        }

        private void stand4Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            this.cleanSchussTable(3);
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            BackgroundWorkerZeichneTrefferArguments bwArguments = new BackgroundWorkerZeichneTrefferArguments();
            bwArguments.pictureBox = this.stand4Zielscheibe;
            bwArguments.g = e.Graphics;
            bwArguments.stand = 3;
            bwArguments.trefferliste = aktuelleTreffer;
            bwArguments.zielscheiben = StandZielscheiben;
            bwArguments.fillMatrix = true;
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(bwArguments);
            }
        }

        private void stand5Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            this.cleanSchussTable(4);
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            BackgroundWorkerZeichneTrefferArguments bwArguments = new BackgroundWorkerZeichneTrefferArguments();
            bwArguments.pictureBox = this.stand5Zielscheibe;
            bwArguments.g = e.Graphics;
            bwArguments.stand = 4;
            bwArguments.trefferliste = aktuelleTreffer;
            bwArguments.zielscheiben = StandZielscheiben;
            bwArguments.fillMatrix = true;
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(bwArguments);
            }
        }

        private void stand6Zielscheibe_Paint(object sender, PaintEventArgs e)
        {
            this.cleanSchussTable(5);
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            BackgroundWorkerZeichneTrefferArguments bwArguments = new BackgroundWorkerZeichneTrefferArguments();
            bwArguments.pictureBox = this.stand6Zielscheibe;
            bwArguments.g = e.Graphics;
            bwArguments.stand = 5;
            bwArguments.trefferliste = aktuelleTreffer;
            bwArguments.zielscheiben = StandZielscheiben;
            bwArguments.fillMatrix = true;
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(bwArguments);
            }
        }
*/
        private void geschlechtTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.bearbeitungsmodusToolStripMenuItem.Checked)
            {
                saveToolStripButton1.Enabled = true;
                bindingNavigatorAddNewItem.Enabled = false;
            }
        }

        private void setzeZielscheibeInEinzelansicht(int stand)
        {
            stand--; // Ich brauche die 0-basierte Standnummer
            string strStand = (stand + 1).ToString();
            Bitmap bScheibe = schiessbuch.Properties.Resources.Luftgewehr;
            try
            {
                string strZielscheibeInXML = aktuelleTreffer[stand][0].strZielscheibe; // ich lese die Zielscheibe einfach aus dem ersten schuss aus und hoffe, dass diese dann auch bei allen anderen schüssen die selbe ist. Falls nicht, wird sowieso eine Fehlermeldung ausgegeben.
                if (strZielscheibeInXML.Equals(strZielscheibeLuftgewehr) || strZielscheibeInXML.Equals(strZielscheibeLuftgewehrBlattl) || strZielscheibeInXML.Equals(strZielscheibeLuftgewehrBlattlRot))
                    bScheibe = schiessbuch.Properties.Resources.Luftgewehr;
                if (strZielscheibeInXML.Equals(strZielscheibeLuftpistole) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleBlattl) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleBlattlRot) || strZielscheibeInXML.Equals(strZielscheibeLuftpistoleRot))
                    bScheibe = schiessbuch.Properties.Resources.Luftpistole;
            }
            catch (ArgumentOutOfRangeException)
            { // Exception soll einfach ignoriert werden 
            }
            if (!pictureBoxEinzelScheibe.Image.Equals(bScheibe)) pictureBoxEinzelScheibe.Image = bScheibe;
            //((PictureBox)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel1.Controls["Stand" + strStand + "Zielscheibe"]).Image = bScheibe;

            // Disziplin setzen
            /*
            try
            {
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtDisziplinStand" + strStand]).Text = aktuelleTreffer[stand][0].disziplin;
            }
            catch (ArgumentOutOfRangeException)
            {
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtDisziplinStand" + strStand]).Text = "keine";
            }

            // Schütze setzen
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM schuetzenliste WHERE id='" + aktuelleTreffer[stand][0].schuetze.ToString() + "' AND SchiessjahrID='" + aktuellesSchiessjahrID.ToString() + "'", conn);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]).Text = reader["fullname"].ToString();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                ((Label)((SplitContainer)this.UebersichtTableLayoutPanel.Controls["Stand" + strStand + "SplitContainer"]).Panel2.Controls["txtSchuetzeStand" + strStand]).Text = "kein Schütze";
            }
            conn.Clone();
            conn.Dispose();
            MySqlConnection.ClearPool(conn);
            */
        }

        private void stand1Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(1, sender, e);
        }

        private void stand2Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(2, sender, e);
        }

        private void stand3Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(3, sender, e);
        }

        private void stand4Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(4, sender, e);
        }

        private void stand5Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(5, sender, e);
        }

        private void stand6Zielscheibe_DoubleClick(object sender, EventArgs e)
        {
            standXZielscheibe_DoubleClick(6, sender, e);
        }

        private void standXZielscheibe_DoubleClick(int stand, object sender, EventArgs e)
        {
            int num = this.tabControl1.TabPages.IndexOfKey("tabStandUebersicht");
            this.tabControl1.TabPages.Insert(num + 1, this.EinzelScheibe);
            this.tabControl1.SelectTab("tabEinzelscheibe");
            standFuerEinzelScheibe = stand;
            
        }

        private void pictureBox3_Resize(object sender, EventArgs e)
        {
            if (tabEinzelscheibe.ClientRectangle.Width > tabEinzelscheibe.ClientRectangle.Height) {
                // Das Fenster ist breiter als hoch
                // Dann die Breite der PictureBox auf die Höhe beschränken, so dass es quadratisch wird.
                pictureBoxEinzelScheibe.Height = tabEinzelscheibe.ClientRectangle.Height;
                pictureBoxEinzelScheibe.Width = pictureBoxEinzelScheibe.Height;
            } else
            {
                // Das Fenster ist höher als breit. Ich kann mir zwar nicht vorstellen, dass das mal vorkommen wird, aber wer weiss :-)
                pictureBoxEinzelScheibe.Width = tabEinzelscheibe.ClientRectangle.Width;
                pictureBoxEinzelScheibe.Height = pictureBoxEinzelScheibe.Width;
            }
        }

        private void Schiessbuch_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoUpdates.Checked = false;
            tmBildUpdateTimer.Stop();

            DialogResult res = MessageBox.Show("Soll eine Sicherung der Datenbank erstellt werden?", "Sicherung erstellen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                if (Properties.Settings.Default.BackupFileName.Length > 0 && Properties.Settings.Default.BackupDirectory.Length > 0)
                {
                    Process externalProcess = new Process();
                    externalProcess.StartInfo.FileName = Properties.Settings.Default.BackupFileName;
                    externalProcess.StartInfo.Arguments = "--add-drop-database --add-drop-table --add-drop-trigger --add-locks --complete-insert --create-options --extended-insert --single-transaction --dump-date -u siusclub --host=" + backupDestination + " --password=\"siusclub\" siusclub -r \"" + Properties.Settings.Default.BackupDirectory + "\\backup-" + DateTime.Now.ToShortDateString() + ".sql\"";
                    externalProcess.StartInfo.UseShellExecute = false;
                    externalProcess.StartInfo.RedirectStandardOutput = true;
                    externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    externalProcess.StartInfo.CreateNoWindow = true;
                    externalProcess.Start();
                    string output = externalProcess.StandardOutput.ReadToEnd();
                    File.WriteAllText(Properties.Settings.Default.BackupDirectory + "\\backup-" + DateTime.Now.ToShortDateString() + ".sqltxt", output);
                    //MessageBox.Show(output);
                    externalProcess.WaitForExit();
                    MessageBox.Show("Backup erstellt.");
                }
                else
                {
                    MessageBox.Show("Es wurden keine Einstellungen für die Sicherungsverzeichnisse gefunden. Bitte Verzeichnisse festlegen.");
                }
                e.Cancel = false;
            }
            if (res == DialogResult.No) e.Cancel = false;
            if (res == DialogResult.Cancel) e.Cancel = true;

        }

        private void einstellungenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Einstellungen einstellungenDlg = new Einstellungen();
            einstellungenDlg.tbTimerInterval.Text = Properties.Settings.Default.TimerInterval.ToString();
            einstellungenDlg.tbDatabaseRefresh.Text = Properties.Settings.Default.DatabaseInterval.ToString();
            einstellungenDlg.tbAnzLetzteTreffer.Text = Properties.Settings.Default.AnzLetzteTreffer.ToString();
            einstellungenDlg.tbDBServer.Text = Properties.Settings.Default.MySQLServer;
            DialogResult result = einstellungenDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                float fValue=10.0f;
                CultureInfo enus = new CultureInfo("en-US");
                float.TryParse(einstellungenDlg.tbTimerInterval.Text, 
                    NumberStyles.AllowDecimalPoint 
                    | NumberStyles.Float 
                    | NumberStyles.Integer 
                    | NumberStyles.Number, 
                    enus, 
                    out fValue);
                Properties.Settings.Default.TimerInterval = fValue;
                RefreshTimer.Interval = (int)(fValue * 1000);

                int iValue = 1;
                int.TryParse(einstellungenDlg.tbDatabaseRefresh.Text,
                    NumberStyles.Integer
                    | NumberStyles.Number,
                    enus,
                    out iValue);
                Properties.Settings.Default.DatabaseInterval = iValue;

                iValue = 1;
                int.TryParse(einstellungenDlg.tbAnzLetzteTreffer.Text,
                    NumberStyles.Integer
                    | NumberStyles.Number,
                    enus,
                    out iValue);
                Properties.Settings.Default.AnzLetzteTreffer = iValue;
                Properties.Settings.Default.MySQLServer = einstellungenDlg.tbDBServer.Text;
                Properties.Settings.Default.Save();
                einstellungenDlg.Close();
                einstellungenDlg.Dispose();
            }

        }

        private void pictureBoxEinzelScheibe_Paint(object sender, PaintEventArgs e)
        {
            // this.cleanSchussTable(0); nur benötigt, wenn eine Tabelle mit allen Schüssen angezeigt werden soll
            // setze die richtige Zielscheibe ein
            Graphics g = e.Graphics;
            ZeichneTrefferInZielscheibe(pictureBoxEinzelScheibe, g, standFuerEinzelScheibe - 1, aktuelleTreffer, StandZielscheiben, true);
            g.Dispose();
        }

        private void Schiessabend_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // stelle fest, ob eine Zelle in der Spalte "Kasse" verlassen wurde. Nur dann soll nämlich ein Update in der Datenbank durchgeführt werden.
            if (Schiessabend.Columns[e.ColumnIndex].HeaderText.Equals("Kasse"))
            {
                try
                {
                    Schiessabend.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    string strDatum = dateTimePicker1.Value.ToString("yyyy-MM-dd"); // Datum, an dem bezahlt wurde (das Datum, für das die Tagesansicht erzeugt wird)
                    int iSchuetze = int.Parse(Schiessabend["ID", e.RowIndex].Value.ToString()); // der Schütze, der bezahlt hat
                    MySqlConnection conn = new MySqlConnection(connStr);
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
                catch (FormatException)
                {
                    MessageBox.Show("Es darf nur ein Betrag eingegeben werden. Soll eine Zahlung gelöscht werden, dann 0 eintragen.", "Falsches Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        EinzelAuswertungDaten einzelauswertung;

        /// <summary>
        /// Wenn "Auswerten" im Schießbuch bei einer Serie ausgewählt wird
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void auswertenEntry_Click(object sender, EventArgs e)
        {
            // Auswertung einer Serie
            // Dazu brauchen wir ein PrintDocument, weil die Auswertung ja (nur) gedruckt werden soll
            PrintDocument pdSerienAuswertung = new PrintDocument();
            pdSerienAuswertung.PrintPage += new PrintPageEventHandler(PdSerienAuswertung_PrintPage);
            printFont = new Font("Arial", 10);

            // Die Mausposition wird durch das Event CellMouseEnter bei Hovern über jede Zelle neu festgelegt und in der Variable MousePosition gespeichert
            //mouseLocation.ColumnIndex;

            // xx Name:
            // xx Vorname:
            // Disziplin:
            // Ergebnis:
            // xx Verein:
            // Datum:
            // Uhrzeit:
            // Stand:
            string strName = nameTextBox.Text;
            string strVorname = vornameTextBox.Text;
            string strVerein = vereinTextBox.Text;
            string strDisziplin = schiessbuchDataGridView[0, mouseLocation.RowIndex].Value.ToString();
            string strStand = schiessbuchDataGridView[1, mouseLocation.RowIndex].Value.ToString();
            string strErgebnis = schiessbuchDataGridView[3, mouseLocation.RowIndex].Value.ToString();
            string strSession = schiessbuchDataGridView[2, mouseLocation.RowIndex].Value.ToString();
            string strDatum = ((DateTime)schiessbuchDataGridView[5, mouseLocation.RowIndex].Value).ToShortDateString();
            string strUhrzeit = ((DateTime)schiessbuchDataGridView[6, mouseLocation.RowIndex].Value).ToShortTimeString();

            string strTest = "Name:      " + strName + "\n";
            strTest += "Vorname:   " + strVorname + "\n";
            strTest += "Verein:    " + strVerein + "\n";
            strTest += "Disziplin: " + strDisziplin + "\n";
            strTest += "Stand:     " + strStand + "\n";
            strTest += "Ergebnis:  " + strErgebnis + "\n";
            strTest += "Session:   " + strSession + "\n";
            strTest += "Datum:     " + strDatum + "\n";
            strTest += "Uhrzeit:   " + strUhrzeit + "\n";
            MessageBox.Show(strTest);

            int anzSchuss = 0;
            einzelauswertung = new EinzelAuswertungDaten();
            MySqlConnection conn = new MySqlConnection(connStr);
            if (strDisziplin.Equals("LG 20 Schuss") || strDisziplin.Equals("LG 40 Schuss"))
            {
                if (strDisziplin.Equals("LG 20 Schuss")) anzSchuss = 20;
                if (strDisziplin.Equals("LG 40 Schuss")) anzSchuss = 40;
                //anzSchuss = 20;

                conn.Open();
                // Auslesen der Ring- und der Zehntelwertung
                MySqlCommand cmd = new MySqlCommand("select sum(ring) AS Ringwertung, round(sum(zehntel),1) AS Zehntelwertung from treffer where session='" + strSession + "' and schritt=1;", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    einzelauswertung.iErgebnisRing = Int16.Parse(reader["Ringwertung"].ToString());
                    einzelauswertung.fErgebnisZehntel = float.Parse(reader["Zehntelwertung"].ToString());
                }
                reader.Close();
                einzelauswertung.strDatum = strDatum;
                einzelauswertung.strDisziplin = strDisziplin;
                einzelauswertung.strName = strName;
                einzelauswertung.strUhrzeit = strUhrzeit;
                einzelauswertung.strVerein = strVerein;
                einzelauswertung.strVorname = strVorname;
                einzelauswertung.iStandNummer = Int16.Parse(strStand);

                cmd.CommandText = @"SELECT 10 AS Ring, COUNT(ring) AS Anzahl from treffer where session='" + strSession + @"' and ring=10 and schritt = 1 UNION ALL 
SELECT 9, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 9 and schritt = 1 UNION ALL
SELECT 8, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 8 and schritt = 1 UNION ALL
SELECT 7, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 7 and schritt = 1 UNION ALL
SELECT 6, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 6 and schritt = 1 UNION ALL
SELECT 5, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 5 and schritt = 1 UNION ALL
SELECT 4, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 4 and schritt = 1 UNION ALL
SELECT 3, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 3 and schritt = 1 UNION ALL
SELECT 2, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 2 and schritt = 1 UNION ALL
SELECT 1, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 1 and schritt = 1 UNION ALL
SELECT 0, COUNT(ring) from treffer where session='" + strSession + @"' and ring = 0 and schritt = 1 UNION ALL
SELECT 11, COUNT(ring) from treffer where session='" + strSession + "' and schritt = 1 and zehntel>= 10.2;";
                einzelauswertung.schussverteilung = new EinzelAuswertungDaten.Schussverteilung();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    switch (Int16.Parse(reader["Ring"].ToString()))
                    {
                        case 10: einzelauswertung.schussverteilung.iAnzZehner = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 9: einzelauswertung.schussverteilung.iAnzNeuner = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 8: einzelauswertung.schussverteilung.iAnzAchter = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 7: einzelauswertung.schussverteilung.iAnzSiebener = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 6: einzelauswertung.schussverteilung.iAnzSechser = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 5: einzelauswertung.schussverteilung.iAnzFuenfer = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 4: einzelauswertung.schussverteilung.iAnzVierer = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 3: einzelauswertung.schussverteilung.iAnzDreier = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 2: einzelauswertung.schussverteilung.iAnzZweier = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 1: einzelauswertung.schussverteilung.iAnzEinser = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 0: einzelauswertung.schussverteilung.iAnzNuller = Int16.Parse(reader["Anzahl"].ToString()); break;
                        case 11: einzelauswertung.schussverteilung.iInnenzehner = Int16.Parse(reader["Anzahl"].ToString()); break;
                    }
                }
                reader.Close();

                einzelauswertung.beste = new EinzelAuswertungDaten.SchussWert[3];
                cmd.CommandText = "SELECT schussnummer, teiler from treffer where session='" + strSession + "' and schritt=1 order by teiler asc limit 3;";
                reader = cmd.ExecuteReader();
                int iIndex = 0;
                while (reader.Read())
                {
                    einzelauswertung.beste[iIndex] = new EinzelAuswertungDaten.SchussWert();
                    einzelauswertung.beste[iIndex].iSchussNummer = Int16.Parse(reader["schussnummer"].ToString());
                    einzelauswertung.beste[iIndex].iSchussWert = Int16.Parse(reader["teiler"].ToString());
                    iIndex++;
                }
                reader.Close();

                einzelauswertung.schlechteste = new EinzelAuswertungDaten.SchussWert[3];
                cmd.CommandText = "SELECT schussnummer, teiler from treffer where session='" + strSession + "' and schritt=1 order by teiler desc limit 3;";
                reader = cmd.ExecuteReader();
                iIndex = 0;
                while (reader.Read())
                {
                    einzelauswertung.schlechteste[iIndex] = new EinzelAuswertungDaten.SchussWert();
                    einzelauswertung.schlechteste[iIndex].iSchussNummer = Int16.Parse(reader["schussnummer"].ToString());
                    einzelauswertung.schlechteste[iIndex].iSchussWert = Int16.Parse(reader["teiler"].ToString());
                    iIndex++;
                }
                reader.Close();

                cmd.CommandText = "SELECT ROUND(AVG(xrahmeninmm), 2) AS TrefferlageHoriz, ROUND(AVG(yrahmeninmm), 2) AS TrefferlageVert, ROUND(AVG(radiusziel), 2) AS AbstandDurchschnitt from treffer where session='" + strSession + "' and schritt = 1; ";
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    einzelauswertung.fTrefferlage_r = float.Parse(reader["AbstandDurchschnitt"].ToString());
                    einzelauswertung.fTrefferlage_x = float.Parse(reader["TrefferlageHoriz"].ToString());
                    einzelauswertung.fTrefferlage_y = float.Parse(reader["TrefferlageVert"].ToString());
                }
                reader.Close();

                cmd.CommandText = "SELECT ROUND(SQRT(SUM(xrahmeninmm * xrahmeninmm) / COUNT(xrahmeninmm) - AVG(xrahmeninmm) * AVG(xrahmeninmm)), 2) AS xStreuungSTABW, ROUND(SQRT(SUM(yrahmeninmm * yrahmeninmm) / COUNT(yrahmeninmm) - AVG(yrahmeninmm) * AVG(yrahmeninmm)), 2) AS yStreuungSTABW, ROUND(SQRT(SUM(radiusziel * radiusziel) / COUNT(radiusziel) - AVG(radiusziel) * AVG(radiusziel)), 2) AS rStreuungSTABW, ROUND(SUM(xrahmeninmm * xrahmeninmm) / COUNT(xrahmeninmm) - AVG(xrahmeninmm) * AVG(xrahmeninmm), 2) AS xStreuungVar, ROUND(SUM(yrahmeninmm * yrahmeninmm) / COUNT(yrahmeninmm) - AVG(yrahmeninmm) * AVG(yrahmeninmm), 2) AS yStreuungVar, ROUND(SUM(radiusziel * radiusziel) / COUNT(radiusziel) - AVG(radiusziel) * AVG(radiusziel), 2) AS rStreuungVar from treffer where session='" + strSession + "' and schritt = 1; ";
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    einzelauswertung.fVarianz_r = float.Parse(reader["rStreuungVar"].ToString());
                    einzelauswertung.fVarianz_x = float.Parse(reader["xStreuungVar"].ToString());
                    einzelauswertung.fVarianz_y = float.Parse(reader["yStreuungVar"].ToString());
                    einzelauswertung.fStabw_r = float.Parse(reader["rStreuungSTABW"].ToString());
                    einzelauswertung.fStabw_x = float.Parse(reader["xStreuungSTABW"].ToString());
                    einzelauswertung.fStabw_y = float.Parse(reader["yStreuungSTABW"].ToString());
                }
                reader.Close();
                einzelauswertung.serien = new List<EinzelAuswertungDaten.SerienAuswertung>();
                einzelauswertung.alleSchuss = new List<EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie>();
                for (int i=0; i < anzSchuss / 10; i++) // Ermittle die Wertungen für die einzelnen Serien
                {
                    EinzelAuswertungDaten.SerienAuswertung serie = new EinzelAuswertungDaten.SerienAuswertung();
                    serie.iSerienNr = i + 1;
                    cmd.CommandText = "select sum(ring)AS Serie FROM(SELECT * from treffer where session='" + strSession + "' and schritt = 1 limit " + i * 10 + ", 10) T;";
                    serie.iSerienSumme = Int16.Parse(cmd.ExecuteScalar().ToString());
                    cmd.CommandText = "select ring, zehntel, winkelmassrahmen, schussnummer, xrahmeninmm, yrahmeninmm FROM treffer where session='" + strSession + "' and schritt = 1 limit " + i * 10 + ", 10;";
                    reader = cmd.ExecuteReader();
                    serie.treffer = new List<EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie>();
                    while (reader.Read())
                    {
                        EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie trf = new EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie();
                        trf.iSchussNummer = Int16.Parse(reader["schussnummer"].ToString());
                        trf.fWinkel = float.Parse(reader["winkelmassrahmen"].ToString());
                        trf.fWertung = float.Parse(reader["zehntel"].ToString());
                        trf.iRing = Int16.Parse(reader["ring"].ToString());
                        trf.xrahmeninmm = float.Parse(reader["xrahmeninmm"].ToString());
                        trf.yrahmeninmm = float.Parse(reader["yrahmeninmm"].ToString());
                        if (trf.fWertung > 10.2)
                            trf.bInnenZehner = true;
                        else
                            trf.bInnenZehner = false;
                        serie.treffer.Add(trf);
                        einzelauswertung.alleSchuss.Add(trf);
                    }
                    reader.Close();

                    // TODO: Hier muss noch ein Bitmap eingefügt werden, in das alle Schüsse eingetragen werden.

                    serie.beste = new EinzelAuswertungDaten.SchussWert[3];
                    cmd.CommandText = "select teiler, schussnummer FROM (SELECT teiler, schussnummer FROM treffer where session='" + strSession + "' and schritt=1 limit " + i * 10 + ", 10) T order by teiler asc limit 3;";
                    reader = cmd.ExecuteReader();
                    iIndex = 0;
                    while (reader.Read())
                    {
                        serie.beste[iIndex] = new EinzelAuswertungDaten.SchussWert();
                        serie.beste[iIndex].iSchussNummer = Int16.Parse(reader["schussnummer"].ToString());
                        serie.beste[iIndex].iSchussWert = Int16.Parse(reader["teiler"].ToString());
                        iIndex++;
                    }
                    reader.Close();

                    cmd.CommandText = "SELECT ROUND(AVG(xrahmeninmm), 2) AS TrefferlageHoriz, ROUND(AVG(yrahmeninmm), 2) AS TrefferlageVert, ROUND(AVG(radiusziel), 2) AS AbstandDurchschnitt from (select * from treffer where session='" + strSession + "' and schritt = 1 limit " + i * 10 + ", 10) T; ";
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        serie.fTrefferlage_r = float.Parse(reader["AbstandDurchschnitt"].ToString());
                        serie.fTrefferlage_x = float.Parse(reader["TrefferlageHoriz"].ToString());
                        serie.fTrefferlage_y = float.Parse(reader["TrefferlageVert"].ToString());
                    }
                    reader.Close();

                    cmd.CommandText = "SELECT ROUND(SQRT(SUM(xrahmeninmm * xrahmeninmm) / COUNT(xrahmeninmm) - AVG(xrahmeninmm) * AVG(xrahmeninmm)), 2) AS xStreuungSTABW, ROUND(SQRT(SUM(yrahmeninmm * yrahmeninmm) / COUNT(yrahmeninmm) - AVG(yrahmeninmm) * AVG(yrahmeninmm)), 2) AS yStreuungSTABW, ROUND(SQRT(SUM(radiusziel * radiusziel) / COUNT(radiusziel) - AVG(radiusziel) * AVG(radiusziel)), 2) AS rStreuungSTABW, ROUND(SUM(xrahmeninmm * xrahmeninmm) / COUNT(xrahmeninmm) - AVG(xrahmeninmm) * AVG(xrahmeninmm), 2) AS xStreuungVar, ROUND(SUM(yrahmeninmm * yrahmeninmm) / COUNT(yrahmeninmm) - AVG(yrahmeninmm) * AVG(yrahmeninmm), 2) AS yStreuungVar, ROUND(SUM(radiusziel * radiusziel) / COUNT(radiusziel) - AVG(radiusziel) * AVG(radiusziel), 2) AS rStreuungVar from (select * from treffer where session='" + strSession + "' and schritt = 1 limit " + i * 10 + ", 10) T; ";
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        serie.fVarianz_r = float.Parse(reader["rStreuungVar"].ToString());
                        serie.fVarianz_x = float.Parse(reader["xStreuungVar"].ToString());
                        serie.fVarianz_y = float.Parse(reader["yStreuungVar"].ToString());
                        serie.fStabw_r = float.Parse(reader["rStreuungSTABW"].ToString());
                        serie.fStabw_x = float.Parse(reader["xStreuungSTABW"].ToString());
                        serie.fStabw_y = float.Parse(reader["yStreuungSTABW"].ToString());
                    }
                    reader.Close();
                    einzelauswertung.serien.Add(serie);
                }
            }
            conn.Close();
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();
            ppdlg.Document = pdSerienAuswertung;
            ppdlg.ShowDialog();
        }

        private void PdSerienAuswertung_PrintPage(object sender, PrintPageEventArgs ev)
        {
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
            str = String.Format("Einzelauswertung", dateTimePicker1.Value.ToShortDateString());
            strl = (int)ev.Graphics.MeasureString(str, headFont2).Width;
            headHeight = (int)ev.Graphics.MeasureString(str, headFont2).Height;
            ev.Graphics.DrawString(str, headFont2, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);

            topMargin += headHeight;

            ev.Graphics.DrawString("Name: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            ev.Graphics.DrawString(einzelauswertung.strName + ", " + einzelauswertung.strVorname, printFont, Brushes.Black, ev.MarginBounds.Left + 50, topMargin);

            ev.Graphics.DrawString("Datum / Zeit: ", printFont, Brushes.Black, ev.PageBounds.Width / 2 + 100, topMargin);
            ev.Graphics.DrawString(einzelauswertung.strDatum + ", " + einzelauswertung.strUhrzeit, printFont, Brushes.Black, ev.PageBounds.Width / 2 + 200, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Verein: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            ev.Graphics.DrawString(einzelauswertung.strVerein, printFont, Brushes.Black, ev.MarginBounds.Left + 50, topMargin);

            ev.Graphics.DrawString("Disziplin: ", printFont, Brushes.Black, ev.PageBounds.Width / 2 + 100, topMargin);
            ev.Graphics.DrawString(einzelauswertung.strDisziplin, printFont, Brushes.Black, ev.PageBounds.Width / 2 + 200, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Stand: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            ev.Graphics.DrawString("Stand " + einzelauswertung.iStandNummer.ToString(), printFont, Brushes.Black, ev.MarginBounds.Left + 50, topMargin);

            topMargin += 25;
            Pen pen = new Pen(Brushes.Black, 3.0f);
            ev.Graphics.DrawLine(pen, new Point(ev.MarginBounds.X, (int)topMargin), new Point(ev.MarginBounds.X + ev.MarginBounds.Width, (int)topMargin));
            topMargin += 10;

            int iRectangleLinks = ev.MarginBounds.Right - 200;
            int iRectangleOben = (int)topMargin;
            Bitmap[] b = new Bitmap[1];
            b[0] = Properties.Resources.Luftgewehr;
            PictureBox pb = new PictureBox();
            pb.Image = b[0];
            List<SchussInfo>[] trefferliste = new List<SchussInfo>[1];
            trefferliste[0] = new List<SchussInfo>();
            foreach (EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie treffer in einzelauswertung.alleSchuss)
            {
                SchussInfo si = new SchussInfo(treffer.xrahmeninmm, treffer.yrahmeninmm, treffer.iRing, treffer.iSchussNummer, "", 0, "", false);
                trefferliste[0].Add(si);
            }

            ZeichneTrefferInZielscheibe2(pb, ev, 0, trefferliste, b, false, new Rectangle(new Point(iRectangleLinks, iRectangleOben), new Size(200, 200)));
            //ev.Graphics.DrawImage(pb.Image, new Rectangle(new Point(iRectangleLinks, iRectangleOben), new Size(200, 200)));
            //ev.Graphics.DrawImage(pb.Image, new Point(iRectangleLinks, iRectangleOben));
            //ev.Graphics.DrawRectangle(pen, new Rectangle(new Point(iRectangleLinks, iRectangleOben), new Size(200, 200)));

            ev.Graphics.DrawString("Ergebnis: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            string tmpStr = String.Format("{0} ({1:0.0})", einzelauswertung.iErgebnisRing, einzelauswertung.fErgebnisZehntel);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Serien: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = "";
            foreach (EinzelAuswertungDaten.SerienAuswertung serie in einzelauswertung.serien)
                tmpStr += string.Format("{0}: {1}   ", serie.iSerienNr, serie.iSerienSumme);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Zähler: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0}  {1}  {2}  {3}  {4}  {5}  {6}  {7}  {8}  {9}  ({10})", 
                einzelauswertung.schussverteilung.iAnzZehner, 
                einzelauswertung.schussverteilung.iAnzNeuner, 
                einzelauswertung.schussverteilung.iAnzAchter, 
                einzelauswertung.schussverteilung.iAnzSiebener, 
                einzelauswertung.schussverteilung.iAnzSechser, 
                einzelauswertung.schussverteilung.iAnzFuenfer, 
                einzelauswertung.schussverteilung.iAnzVierer, 
                einzelauswertung.schussverteilung.iAnzDreier, 
                einzelauswertung.schussverteilung.iAnzZweier, 
                einzelauswertung.schussverteilung.iAnzEinser, 
                einzelauswertung.schussverteilung.iAnzNuller);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Innenzehner: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            ev.Graphics.DrawString(einzelauswertung.schussverteilung.iInnenzehner.ToString(), printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("weiteste: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0} ({1}.), {2} ({3}.), {4} ({5}.)",
                einzelauswertung.schlechteste[0].iSchussWert, einzelauswertung.schlechteste[0].iSchussNummer,
                einzelauswertung.schlechteste[1].iSchussWert, einzelauswertung.schlechteste[1].iSchussNummer,
                einzelauswertung.schlechteste[2].iSchussWert, einzelauswertung.schlechteste[2].iSchussNummer);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("beste Teiler: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0} ({1}.), {2} ({3}.), {4} ({5}.)",
                einzelauswertung.beste[0].iSchussWert, einzelauswertung.beste[0].iSchussNummer,
                einzelauswertung.beste[1].iSchussWert, einzelauswertung.beste[1].iSchussNummer,
                einzelauswertung.beste[2].iSchussWert, einzelauswertung.beste[2].iSchussNummer);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Trefferlage: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0} mm {1}, {2} mm {3}, Abst: {4}",
                Math.Abs(einzelauswertung.fTrefferlage_x).ToString(), (einzelauswertung.fTrefferlage_x > 0 ? "rechts" : "links"),
                Math.Abs(einzelauswertung.fTrefferlage_y).ToString(), (einzelauswertung.fTrefferlage_y > 0 ? "hoch" : "tief"),
                einzelauswertung.fTrefferlage_r);
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Standardabw.: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0} mm hor., {1} mm vert., {2} mm Abst.",
                einzelauswertung.fStabw_x.ToString(), einzelauswertung.fStabw_y.ToString(), einzelauswertung.fStabw_r.ToString());
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            topMargin += 15;
            ev.Graphics.DrawString("Varianz: ", printFont, Brushes.Black, ev.MarginBounds.Left, topMargin);
            tmpStr = String.Format("{0} mm² hor., {1} mm² vert., {2} mm² Abst.",
                einzelauswertung.fVarianz_x.ToString(), einzelauswertung.fVarianz_y.ToString(), einzelauswertung.fVarianz_r.ToString());
            ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, ev.MarginBounds.Left + 100, topMargin);

            iRectangleLinks = ev.MarginBounds.Left;
            iRectangleOben = iRectangleOben + 200;
            /**************************************************************************/
            foreach (EinzelAuswertungDaten.SerienAuswertung serie in einzelauswertung.serien)
            {
                //Bitmap[] b = new Bitmap[1];
                b[0] = Properties.Resources.Luftgewehr;
                pb = new PictureBox();
                pb.Image = b[0];
                //List<SchussInfo>[] trefferliste = new List<SchussInfo>[1];
                trefferliste[0].Clear();
                //trefferliste[0] = new List<SchussInfo>();
                foreach (EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie treffer in serie.treffer)
                {
                    SchussInfo si = new SchussInfo(treffer.xrahmeninmm, treffer.yrahmeninmm, treffer.iRing, treffer.iSchussNummer, "", 0, "", false);
                    trefferliste[0].Add(si);
                }

                ZeichneTrefferInZielscheibe2(pb, ev, 0, trefferliste, b, false, new Rectangle(new Point(iRectangleLinks, iRectangleOben), new Size(150, 150)));

                string strSerie = string.Format("Serie {0}:", serie.iSerienNr);
                topMargin = iRectangleOben;
                int iSerienBeschreibungLinks = iRectangleLinks + 150 + 20;
                ev.Graphics.DrawString(strSerie, printFont, Brushes.Black, iSerienBeschreibungLinks, iRectangleOben);
                topMargin += 20;
                int iZaehler = 0;
                int iAnfang = iSerienBeschreibungLinks;
                foreach (EinzelAuswertungDaten.SerienAuswertung.TrefferInSerie trf in serie.treffer)
                {
                    iZaehler++;
                    string strWertung = trf.fWertung.ToString();
                    float fWertungWidth = ev.Graphics.MeasureString(strWertung, printFont).Width;
                    // strWertung += string.Format(", {0}°", trf.fWinkel);
                    ev.Graphics.DrawString(strWertung, printFont, Brushes.Black, iAnfang, topMargin);
                    if (!trf.bInnenZehner)
                    {
                        Graphics g = Graphics.FromImage(Properties.Resources.Pfeil);
                        /* Bei der Berechnung des Winkels werde ich nicht ganz schlau
                           bei positiven x- und positiven x-Werten stimmt der Winkel
                           bei negativen x- und negativen y-Werten stimmt der Winkel in der Datenbank auch
                           Unterscheiden sich aber die Winkel der x- und der y-Werte, ist der Winkel in der Datenbank falsch.
                           Der richtige Winkel kann dann durch 360-(Winkel) berechnet werden.
                           */
                        float fRichtigerWinkel;
                        if ((trf.xrahmeninmm * trf.yrahmeninmm) < 0) // wenn also x und y unterschiedliche Vorzeichen haben
                            fRichtigerWinkel = 360.0f - trf.fWinkel;
                        else
                            fRichtigerWinkel = trf.fWinkel;
                        g.RotateTransform(fRichtigerWinkel);
                        ev.Graphics.DrawImage(RotateImage(Properties.Resources.Pfeil3, new Point(Properties.Resources.Pfeil3.Width / 2, Properties.Resources.Pfeil3.Height / 2), fRichtigerWinkel), iAnfang + fWertungWidth, topMargin);
                    }
                    if (iZaehler % 5 != 0)
                    {
                        iAnfang += 50;
                    } else
                    {
                        topMargin += 20;
                        iAnfang = iSerienBeschreibungLinks;
                    }

                }

                topMargin += 15;
                ev.Graphics.DrawString("beste Teiler: ", printFont, Brushes.Black, iAnfang, topMargin);
                tmpStr = String.Format("{0} ({1}.), {2} ({3}.), {4} ({5}.)",
                    serie.beste[0].iSchussWert, serie.beste[0].iSchussNummer,
                    serie.beste[1].iSchussWert, serie.beste[1].iSchussNummer,
                    serie.beste[2].iSchussWert, serie.beste[2].iSchussNummer);
                ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, iAnfang + 100, topMargin);

                topMargin += 15;
                ev.Graphics.DrawString("Trefferlage: ", printFont, Brushes.Black, iAnfang, topMargin);
                tmpStr = String.Format("{0} mm {1}, {2} mm {3}, Abst: {4}",
                    Math.Abs(serie.fTrefferlage_x).ToString(), (serie.fTrefferlage_x > 0 ? "rechts" : "links"),
                    Math.Abs(serie.fTrefferlage_y).ToString(), (serie.fTrefferlage_y > 0 ? "hoch" : "tief"),
                    serie.fTrefferlage_r);
                ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, iAnfang + 100, topMargin);

                topMargin += 15;
                ev.Graphics.DrawString("Standardabw.: ", printFont, Brushes.Black, iAnfang, topMargin);
                tmpStr = String.Format("{0} mm hor., {1} mm vert., {2} mm Abst.",
                    serie.fStabw_x.ToString(), serie.fStabw_y.ToString(), serie.fStabw_r.ToString());
                ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, iAnfang + 100, topMargin);

                topMargin += 15;
                ev.Graphics.DrawString("Varianz: ", printFont, Brushes.Black, iAnfang, topMargin);
                tmpStr = String.Format("{0} mm² hor., {1} mm² vert., {2} mm² Abst.",
                    serie.fVarianz_x.ToString(), serie.fVarianz_y.ToString(), serie.fVarianz_r.ToString());
                ev.Graphics.DrawString(tmpStr, printFont, Brushes.Black, iAnfang + 100, topMargin);


                iRectangleOben += 160;
//                MessageBox.Show(g.MeasureString("100", printFont).Height.ToString());

            }



            /**************************************************************************/

            // Links oben Namen hinschreiben
            ev.HasMorePages = false;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new Image containing the same image only rotated
        /// </summary>
        /// <param name=""image"">The <see cref=""System.Drawing.Image"/"> to rotate
        /// <param name=""offset"">The position to rotate from.
        /// <param name=""angle"">The amount to rotate the image, clockwise, in degrees
        /// <returns>A new <see cref=""System.Drawing.Bitmap"/"> of the same size rotated.</see>
        /// <exception cref=""System.ArgumentNullException"">Thrown if <see cref=""image"/"> 
        /// is null.</see>
        public static Bitmap RotateImage(Image image, PointF offset, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            schiessbuchDataGridView.Sort(this.dataGridViewTextBoxColumn8, System.ComponentModel.ListSortDirection.Descending);
        }

        private void ErstelleAuswertungGemeindemeisterschaft()
        {
            this.SuspendLayout();
            MySqlConnection connGMM = new MySqlConnection(connStr);
            connGMM.Open();
            MySqlCommand cmdGMM0 = new MySqlCommand("SET autocommit=1;", connGMM);
            cmdGMM0.ExecuteNonQuery();
            MySqlCommand cmdGMM = new MySqlCommand("call siusclub.fillGemeindemeisterschaftTable();", connGMM);
            cmdGMM.CommandTimeout = 100;
            cmdGMM.ExecuteNonQuery();
            connGMM.Close();
            connGMM.Dispose();
            MySqlConnection.ClearAllPools();
            //            uebersichtgemeindemeisterschaftTableAdapter.Fill(gemeindemeisterschaft.uebersichtgemeindemeisterschaft);
            this.uebersichtgemeindemeisterschaftTableAdapter3.Fill(this.vereinsheimSiusclubDataSet2.uebersichtgemeindemeisterschaft);
            gmmDGV.Invalidate();
            this.ResumeLayout();
        }

        private void cbVereineFiltern_CheckedChanged(object sender, EventArgs e)
        {
            if (cbVereineFiltern.Checked)
            {
                comboVereineFiltern.Visible = true;
                comboVereineFiltern.Enabled = true;
            } else
            {
                comboVereineFiltern.Enabled = false;
                comboVereineFiltern.Visible = false;
            }
            FilterGemeindemeisterschaft();
        }

        private void FilterGemeindemeisterschaft()
        {
            CultureInfo deutschesDatum = new CultureInfo("de-DE");
            //MessageBox.Show(comboDatumFiltern.Text + " " + comboVereineFiltern.Text);
            if (cbDatumFiltern.Checked && (!cbVereineFiltern.Checked))
                if (comboDatumFiltern.Text != "") uebersichtgemeindemeisterschaftBindingSource4.Filter = "Datum='" + comboDatumFiltern.Text + "'";
            if (cbVereineFiltern.Checked && (!cbDatumFiltern.Checked))
                if (comboVereineFiltern.Text != "") uebersichtgemeindemeisterschaftBindingSource4.Filter = "Verein='" + comboVereineFiltern.Text + "'";
            if (cbVereineFiltern.Checked && cbDatumFiltern.Checked)
                if ((comboDatumFiltern.Text != "") && (comboVereineFiltern.Text != ""))  uebersichtgemeindemeisterschaftBindingSource4.Filter = "Verein='" + comboVereineFiltern.Text + "' AND " + String.Format("Datum='{0:yyyy-MM-dd}'", DateTime.Parse(comboDatumFiltern.Text, deutschesDatum));
            if ((!cbVereineFiltern.Checked) && (!cbDatumFiltern.Checked))
                uebersichtgemeindemeisterschaftBindingSource4.Filter = "";
        }

        private void cbDatumFiltern_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDatumFiltern.Checked)
            {
                comboDatumFiltern.Visible = true;
                comboDatumFiltern.Enabled = true;
            } else
            {
                comboDatumFiltern.Enabled = false;
                comboDatumFiltern.Visible = false;
            }
            FilterGemeindemeisterschaft();
        }

        private void comboVereineFiltern_SelectedValueChanged(object sender, EventArgs e)
        {
            FilterGemeindemeisterschaft();
        }

        private void comboDatumFiltern_SelectedValueChanged(object sender, EventArgs e)
        {
            FilterGemeindemeisterschaft();
        }

        private void comboDatumFiltern_TextChanged(object sender, EventArgs e)
        {
            FilterGemeindemeisterschaft();
        }

        private void comboVereineFiltern_TextChanged(object sender, EventArgs e)
        {
            FilterGemeindemeisterschaft();
        }

        PrintDocument gmmAuswertung;
        int gmmLinesCount;

        private void btnGmmDrucken2_Click_1(object sender, EventArgs e)
        {
        }

        int gmmPageNo;
        private List<SchussInfo>[] testLGTreffer;

        private void pd_gmmPrintPage(object sender, PrintPageEventArgs ev)
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
            str = String.Format("Auswertung Gemeindemeisterschaft", dateTimePicker1.Value.ToShortDateString());
            strl = (int)ev.Graphics.MeasureString(str, headFont2).Width;
            headHeight = (int)ev.Graphics.MeasureString(str, headFont2).Height;
            ev.Graphics.DrawString(str, headFont2, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, topMargin);
            topMargin += headHeight;

            int disc = 16; // 16 Disziplinen mit LP Auflage, ansonsten nur 12
            int beginDisziplinen = 400;

            // berechne maximale Textlänge der Disziplinen
            int maxLen = 0;
            for (int i = 0; i < disc; i++)
            {
                float l = ev.Graphics.MeasureString(gmmDGV.Columns[i + 5].HeaderText.Replace(" L", "\nL"), printFont).Width;
                if ((int)l > maxLen)
                    maxLen = (int)l;
            }

            yPos = topMargin + maxLen;
            ev.Graphics.DrawString("Name", printFont, Brushes.Black, leftMargin, yPos-20, new StringFormat());
            //ev.Graphics.DrawString("Name", printFont, Brushes.Black, leftMargin + 35, yPos-10, new StringFormat());

            for (int i = 0; i < disc; i++)
            {
                ev.Graphics.TranslateTransform(leftMargin + beginDisziplinen + i * 40, yPos);
                ev.Graphics.RotateTransform(-90);
                ev.Graphics.DrawString(gmmDGV.Columns[i + 5].HeaderText.Replace(" L", "\nL"),
                    printFont,
                    Brushes.Black,
                    0,
                    0,
                    new StringFormat());
                ev.Graphics.RotateTransform(90);
                ev.Graphics.TranslateTransform(-(leftMargin + beginDisziplinen + i * 40), -yPos);
            }

            // calculate the number of lines per page
            // linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
            float tmpHoehe; // aktuell noch verfügbare Höhe ermitteln
            tmpHoehe = ev.PageBounds.Height - yPos - ev.PageSettings.Margins.Bottom;
            //linesPerPage = tmpHoehe / printFont.GetHeight(ev.Graphics);
            linesPerPage = tmpHoehe / 20; // Pro Zeile springen wir um 20 nach unten!
            linesPerPage--; // um ein bisschen Abstand zum Datum in der Fußzeile zu haben


            while (count < linesPerPage && currentLinesPrinted < gmmLinesCount)
            {
                if (gmmDGV[0, currentLinesPrinted].Value != null)
                {
                    //string idstr = gmmDGV["ID", currentLinesPrinted].Value.ToString();
                    //yPos = topMargin + maxLen + ((count + 1) * printFont.GetHeight(ev.Graphics));
                    //ev.Graphics.DrawString(idstr, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                    ev.Graphics.DrawString(
                        gmmDGV[1, currentLinesPrinted].Value.ToString() + ", " + gmmDGV[2, currentLinesPrinted].Value.ToString(),
                        printFont,
                        Brushes.Black,
                        leftMargin,
                        yPos,
                        new StringFormat());
                    ev.Graphics.DrawString(
                        gmmDGV[0, currentLinesPrinted].Value.ToString(), 
                        printFont, 
                        Brushes.Black, 
                        leftMargin + 200, 
                        yPos, 
                        new StringFormat());
                    //int disziplinen = 16; // 16 Disziplinen
                    for (int i = 0; i < disc; i++)
                    {
                        ev.Graphics.DrawString(gmmDGV[5 + i, currentLinesPrinted].Value.ToString(),
                            printFont,
                            Brushes.Black,
                            leftMargin + beginDisziplinen + i * 40,
                            yPos,
                            new StringFormat());
                    }
                    count++;
                    currentLinesPrinted++;
                    yPos += 20;
                }
            }

            string pageStr = "Seite " + gmmPageNo;
            ev.Graphics.DrawString(
                pageStr,
                printFont,
                Brushes.Black,
                ev.PageBounds.Width - ev.PageSettings.Margins.Right - ev.Graphics.MeasureString(pageStr, printFont).Width,
                ev.PageBounds.Height - ev.PageSettings.Margins.Bottom);

            CultureInfo provider = new CultureInfo("de-DE");
            pageStr = DateTime.Now.ToString(provider);
            ev.Graphics.DrawString(
                pageStr,
                printFont,
                Brushes.Black,
                ev.PageSettings.Margins.Left,
                ev.PageBounds.Height - ev.PageSettings.Margins.Bottom);

            ev.Graphics.DrawImage(
                Properties.Resources.edelweiss, 
                new Rectangle(
                    ev.PageSettings.Margins.Left, 
                    ev.PageSettings.Margins.Top, 
                    150, 
                    150), 
                new Rectangle(
                    0,
                    0,
                    Properties.Resources.edelweiss.Width, 
                    Properties.Resources.edelweiss.Height), 
                GraphicsUnit.Pixel);

            if (currentLinesPrinted < gmmDGV.Rows.Count)
            {
                ev.HasMorePages = true;
                gmmPageNo++;
            }
            else
            {
                ev.HasMorePages = false;
                currentLinesPrinted = 0;
            }
        }

        private void btnGmmDruck_Click(object sender, EventArgs e)
        {
            gmmPageNo = 1;
            if (gmmAuswertung == null)
            {
                gmmAuswertung = new PrintDocument();
                PageSettings ps = new PageSettings();
                ps = gmmAuswertung.DefaultPageSettings;
                ps.Landscape = true;
                ps.Margins.Left = 50;
                ps.Margins.Right = 50;
                ps.Margins.Top = 50;
                ps.Margins.Bottom = 50;
                gmmAuswertung.DefaultPageSettings = ps;
                gmmAuswertung.PrintPage += new PrintPageEventHandler(pd_gmmPrintPage);
            }
            printFont = new Font("Arial", 10);
            gmmLinesCount = gmmDGV.Rows.Count;
            PrintPreviewDialog ppdlg = new PrintPreviewDialog();
            ppdlg.Document = gmmAuswertung;
            ppdlg.ShowDialog();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ErstelleAuswertungGemeindemeisterschaft();
        }

        private void comboDatumFiltern_DropDown(object sender, EventArgs e)
        {
            datumlisteTableAdapter.ClearBeforeFill = true;
            datumlisteTableAdapter.Fill(gemeindemeisterschaft.datumliste);
        }

        private void comboVereineFiltern_DropDown(object sender, EventArgs e)
        {
            vereinslisteTableAdapter.ClearBeforeFill = true;
            vereinslisteTableAdapter.Fill(gemeindemeisterschaft.vereinsliste);
        }

        private void TestPictureLG_Paint(object sender, PaintEventArgs e)
        {
            if (testLGTreffer == null)
                testLGTreffer = new List<SchussInfo>[1];
            if (testLGTreffer[0] == null)
                testLGTreffer[0] = new List<SchussInfo>();
            if (testLGTreffer[0].Count > 0)
            {
                Graphics g = e.Graphics;
                //ZeichneTrefferInZielscheibe(TestPictureLG, g, 0, testLGTreffer, StandZielscheiben, false);
                g.Dispose();
            }
            
        }
        
        int iTestLGSchussNummer = 0;
        private void button6_Click(object sender, EventArgs e)
        {
            SchussInfo si;
            float alpha, r;
            switch (iTestLGSchussNummer)
            {
                case 0: alpha = 75; r = 4.6634f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
                case 1: alpha = 0; r = 0.0f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
                case 2: alpha = 20; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
                case 3: alpha = 40; r = 4.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;

                /* case 28: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            case 29: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            case 30: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            case 31: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            case 32: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            case 33: alpha = 30; r = 2.5f; si = new SchussInfo((float)(r * Math.Cos(2 * Math.PI / 360 * alpha)), (float)(r * Math.Sin(2 * Math.PI / 360 * alpha)), 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
           */
                default: si = new SchussInfo(0, 0, 10, iTestLGSchussNummer++, strZielscheibeLuftgewehr, -1, "Test", false); break;
            }

            if (iTestLGSchussNummer == 18) testLGTreffer[0].Clear();
            testLGTreffer[0].Add(si);
            //TestPictureLG.Invalidate();
        }

        private void schiessbuchDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            MessageBox.Show("DataError-Ereignis aufgetreten");
            MessageBox.Show("Error happened " + anError.Context.ToString());

            if (anError.Context == DataGridViewDataErrorContexts.Commit)
            {
                MessageBox.Show("Commit error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                MessageBox.Show("Cell change");
            }
            if (anError.Context == DataGridViewDataErrorContexts.Parsing)
            {
                MessageBox.Show("parsing error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
            {
                MessageBox.Show("leave control error");
            }

            if ((anError.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[anError.RowIndex].ErrorText = "an error";
                view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                anError.ThrowException = false;
            }
        }

        private void Schiessbuch_ResizeBegin(object sender, EventArgs e)
        {
            SuspendLayout();
        }

        private void Schiessbuch_ResizeEnd(object sender, EventArgs e)
        {
            ResumeLayout();
        }

        private void Schiessbuch_Resize(object sender, EventArgs e)
        {
            
        }

        private void UebersichtTableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {
            SuspendLayout();
            base.OnPaint(e);
            ResumeLayout();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        public static string connStr = "";

        private void detectMySQLServer()
        {
            MySqlConnection _conn;
            string strMySQLServer = Properties.Settings.Default.MySQLServer;
            connStr = string.Format("server={0};user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true;Connection Timeout=10;", strMySQLServer);
            _conn = new MySqlConnection(connStr);
            try
            {
                _conn.Open();
                _conn.Close();
            }
            catch (MySqlException mysqle)
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
            connStr = string.Format("server={0};user id=siusclub;password=siusclub;database=siusclub;persistsecurityinfo=True;Allow User Variables=true;Connection Timeout=10;", strMySQLServer);
            _conn.ConnectionString = connStr;
            try
            {
                _conn.Open();
                _conn.Close();
            }
            catch (MySqlException)
            {
                MessageBox.Show("Eine Exception ist aufgetreten beim Öffnen der Datenbank!");
                Application.Exit();
            }
        }

        private void Schiessbuch_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            lblProbe1.ForeColor = Color.Blue; lblProbe1.Text = "";
            lblProbe2.ForeColor = Color.Blue; lblProbe2.Text = "";
            lblProbe3.ForeColor = Color.Blue; lblProbe3.Text = "";
            lblProbe4.ForeColor = Color.Blue; lblProbe4.Text = "";
            lblProbe5.ForeColor = Color.Blue; lblProbe5.Text = "";
            lblProbe6.ForeColor = Color.Blue; lblProbe6.Text = "";

            //MySqlConnectionWrapper mysql = MySqlConnectionWrapper.Instance;
            detectMySQLServer();

            // Zum Aktualisieren der Ergebnisse aller gerade schießenden Schützen wird ein Timer verwendet, der alle x Sekunden
            // die neusten Daten holt. Dieser Wert kann über einen Einstellungsdialog in der Anwendung verändert werden.
            // Dieser Wert wird in den Properties gespeichert. Hier wird dieser Wert gelesen und der Timer auf diesen Wert voreingestellt.
            RefreshTimer.Interval = (int)(Properties.Settings.Default.TimerInterval * 1000);

            StandZielscheiben = new Bitmap[6];
            for (int i = 0; i < 6; StandZielscheiben[i++] = Properties.Resources.Luftgewehr) ;

            // Ergebnisbilder instantiieren, um später die Abbildungen für die Übersicht zeichnen zu können
            ergebnisbilder = new Ergebnisbild[6];
            for (int i = 0; i < 6; ergebnisbilder[i++] = new Ergebnisbild()) ;

            // Hilfsstruktur, um das Zeichnen der Schussliste auf ein Minimum zu reduzieren
            sSchusszahlVeraendert = new SchusszahlInfo[6];
            for (int i = 0; i < 6; i++)
            {
                sSchusszahlVeraendert[i] = new SchusszahlInfo();
                sSchusszahlVeraendert[i].veraendert = false;
            }

            // Initialisiere den Timer, der die Bilder der Schießscheiben aktuell hält
            tmBildUpdateTimer = new System.Windows.Forms.Timer();
            tmBildUpdateTimer.Interval = 2000; // alle zwei Sekunden weden die Bilder aktualisiert
            tmBildUpdateTimer.Tick += TmBildUpdateTimer_Tick;


            // Die Zugriffe aller TableAdapters sollen auf die richtige Datenbank erfolgen. Deshalb wird hier der Connection String entsprechend gesetzt.
            schiessbuchTableAdapter.Connection.ConnectionString = connStr;
            schuetzenTableAdapter.Connection.ConnectionString = connStr;
            trefferTableAdapter.Connection.ConnectionString = connStr;
            vereineTableAdapter.Connection.ConnectionString = connStr;
            schuetzenlisteTableAdapter.Connection.ConnectionString = connStr;
            // uebersichtgemeindemeisterschaftTableAdapter.Connection.ConnectionString = connStr;
            vereinslisteTableAdapter.Connection.ConnectionString = connStr;
            datumlisteTableAdapter.Connection.ConnectionString = connStr;
            uebersichtgemeindemeisterschaftTableAdapter3.Connection.ConnectionString = connStr;
            vereinslisteTableAdapter.Connection.ConnectionString = connStr;
            uebersichtgemeindemeisterschaftTableAdapter3.Connection.ConnectionString = connStr;

            // Jetzt wird versucht, Werte aus der Datenbank zu lesen
            int numAllRead = 0; // Das dient zur Überprüfung, ob Daten aus der Datenbank kommen
            this.uebersichtgemeindemeisterschaftTableAdapter3.Fill(this.vereinsheimSiusclubDataSet2.uebersichtgemeindemeisterschaft);
            this.datumlisteTableAdapter.Fill(this.gemeindemeisterschaft.datumliste);
            this.vereinslisteTableAdapter.Fill(this.gemeindemeisterschaft.vereinsliste);

            // alle in der Datenbank abgespeicherten Vereine werden eingelesen
            numAllRead += this.vereineTableAdapter.Fill(this.siusclubDataSet1.Vereine);
            // alle in der Datenbank abgespeicherten Treffer werden eingelesen
            numAllRead += this.trefferTableAdapter.Fill(this.siusclubDataSet.treffer);
            // außerdem wird der TableAdapter für die Tabelle schuetzenliste gefüllt
            this.schuetzenlisteTableAdapter.Fill(this.siusclubDataSet.schuetzenliste);
            if (numAllRead == 0)
            {
                // sollte nichts gelesen worden sein, wird eine Warnung ausgegeben, das Programm läuft aber weiter.
                MessageBox.Show("Keine Datensätze aus der Datenbank gelesen. Möglicherweise keine Datenbankverbindung.");
            }

            // Hier werden standardmäßig die periodischen Aktualisierungen der Schießergebnisse aktiviert
            DoUpdates.Checked = true;

            // Die Anzahl an Übungsschießen/Wettkämpfen wird in eine Variable gelesen
            // Diese Variable wird später dazu verwenden, herauszufinden, ob in der Zwischenzeit 
            // ein neuer Schütze an den Stand gegangen ist bzw. ein bereits schießender Schütze eine weitere Disziplin schießt.
            ereignisse_count = GetEreignisseCount();

            // Hier wird die Anzahl an Schüssen in eine Variable gelesen.
            // Mithilfe dieser Variable soll ermittelt werden, ob seit dem letzten Aktualisieren ein Schuss abgegeben wurde
            // ist dem nicht so, kann man sich das Auslesen der Schüsse sparen. Ein erneutes Auslesen wird keine neuen Erkenntnisse geben...
            treffer_count = GetTrefferCount();

            // Dieser Bereich sollte überarbeitet werden.
            // Leider muss die Methode Aktualisiere SchiessjahrMenu() zweimal aufgerufen werden.
            AktualisiereSchiessjahrMenu();
            // Die Liste der Schützen wird aktualisiert. Je nach ausgewähltem Schießjahr kann ein Schütze noch oder schon
            // nicht mehr zur Jugendklasse zählen
            UpdateSchuetzenListe();

            //AktualisiereSchiessjahrMenu();

            // Die Termine, zu denen für den Jahrespokal geschossen werden darf, werden in der Datenbank hinterlegt.
            // Hier soll geprüft werden, ob heute ein solcher Termin ist. Wenn ja, wird eine Meldung angezeigt.
            PruefeJahrespokalAbend();

            // Alle Termine für den Jahrespokal werden aus der Datenbank gelesen und im Hauptformular angezeigt
            FillWanderPokalTermine();

            // Für die Auswertung des Königsschießens soll das Hauptformular in vier gleiche Teile geteilt werden.
            // Das wird mittels SplitContainern bewerkstelligt.
            splitContainerKoenig1.SplitterDistance = splitContainerKoenig1.Width / 2;
            splitContainerKoenig2.SplitterDistance = splitContainerKoenig2.Height / 2;
            splitContainerKoenig3.SplitterDistance = splitContainerKoenig3.Height / 2;

            // TODO: Was wird hier genau gemacht?
            // Diese Methoden werden zur Berechnung des Schützenkönigs verwendet.
            // Sie werden aber nicht mehr gebraucht.
            ResizeAllKoenigGridViews();

            // Hier werden die Ergebnisse aus dem Königsschießen ausgewertet und angezeigt.
            //UpdateKoenig(); // Das wird auch beim Aktualisieren durch das Setzen des Schießjahres ausgeführt, deshalb soll es hier nicht gemacht werden.

            // Dieser Teil könnte wahrscheinlich verschoben werden in die Methode, die aufgerufen wird, wenn der Tab "Übersicht" angeklickt wird.
            for (int stand = 1; stand <= 6; stand++)
                for (int schuss = 0; schuss < 40; schuss++)
                    setSchussValue(stand, schuss, schuss.ToString());

            // Die Struktur "aktuelleTreffer" wird initialisiert
            // Sie wird später gebraucht, wenn über den REST Webservice die Daten der aktuell schießenden Schützen ausgelesen werden
            // Diese Daten werden dann in "aktuelleTreffer" geschrieben und aus dieser Struktur werden die Daten aus dem Übersicht-Tab befüllt.
            aktuelleTreffer = new List<SchussInfo>[6];
            for (int i = 0; i < 6; i++)
            {
                aktuelleTreffer[i] = new List<SchussInfo>();
            }

            // Hier wird ein Handle auf den Tab "tabEinzelscheibe" geholt...
            EinzelScheibe = tabControl1.TabPages["tabEinzelscheibe"];
            // und dieser Tab dann aus dem TabControl entfernt. Er wird wieder eingehängt, wenn jemand in der Übersicht auf eine
            // Scheibe mit der Maus doppelklickt.
            this.tabControl1.TabPages.RemoveByKey("tabEinzelscheibe");

            //schiessbuchDataGridView.Sort(dataGridViewTextBoxColumn8, System.ComponentModel.ListSortDirection.Ascending);
            //schiessbuchDataGridView.Invalidate();
            schuetzenlisteschiessbuchBindingSource.Sort = "dt DESC";
        }
        bool bInTmBildUpdateTimer = false;
        private void TmBildUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!bInTmBildUpdateTimer)
            {
                bInTmBildUpdateTimer = true;
                PictureBox pb;
                string strControlName;
                for (int i = 0; i < 6; i++)
                {
                    strControlName = "stand" + (i + 1).ToString() + "Zielscheibe";

                    pb = (PictureBox)Controls["tabControl1"].Controls["tabStandUebersicht"].Controls["UebersichtTableLayoutPanel"].Controls["Stand" + (i + 1).ToString() + "SplitContainer"].Controls[0].Controls["stand" + (i + 1).ToString() + "Zielscheibe"];
                    CreateGraphicsForAnzeige(ergebnisbilder[i], pb.Width, pb.Height, pb.Image.Width, pb.Image.Height, i, aktuelleTreffer);
                }
                bInTmBildUpdateTimer = false;
            }
        }

        private void schiessbuchDataGridView_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            RefreshTimer.Enabled = false;
        }

        private void schiessbuchDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DoUpdates_CheckedChanged(null, null);
        }
    }
}
