using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing.Printing;

namespace schiessbuch
{
    public partial class GauligaAuswertung : Form
    {
        public GauligaAuswertung()
        {
            InitializeComponent();
        }
        private Font printFont;
        //private int linesCount;
        //private int currentLinesPrinted;

        private float mm2pt(float mm) { return mm / 0.254f; }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            
            //ev.Graphics.Clear(Color.White);
            float linesPerPage = 0;
            //float yPos = 0;
            //int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;

            // calculate the number of lines per page
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

            string str = "Gauliga - Rundenwettkampf";
            Font headFont = new Font("Arial", 20f, FontStyle.Bold | FontStyle.Underline);
            Font headFont2 = new Font("Arial", 18, FontStyle.Bold | FontStyle.Underline);
            int strl = (int)ev.Graphics.MeasureString(str, headFont).Width;
            int headHeight = (int)ev.Graphics.MeasureString(str, headFont).Height;
            ev.Graphics.DrawString(str, headFont, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, mm2pt(17));
            topMargin += headHeight;
            str = String.Format("Ergebnisliste");
            strl = (int)ev.Graphics.MeasureString(str, headFont2).Width;
            headHeight = (int)ev.Graphics.MeasureString(str, headFont2).Height;
            ev.Graphics.DrawString(str, headFont2, Brushes.Black, ev.PageBounds.Width / 2 - strl / 2, mm2pt(27));
            topMargin += headHeight;
            Font arial12 = new System.Drawing.Font("Arial", 12);
            ev.Graphics.PageUnit = GraphicsUnit.Millimeter;
            //float f = ev.PageBounds.Width * 0.254f;
            //MessageBox.Show(f.ToString());
            //ev.Graphics.DrawRectangle(new Pen(Brushes.Black), new Rectangle(
            //ev.Graphics.DrawString("Heimmannschaft: Schützengesellschaft Edelweiß Eltheim e. V.", new Font("Arial", 10), Brushes.Black, mm2pt(20), mm2pt(97));
            ev.Graphics.DrawString("Heimmannschaft: Schützengesellschaft Edelweiß Eltheim e. V.", arial12, Brushes.Black, 9, 97);
            int heim_links = 8;
            int heim_oben = 106;

            DrawTable(ev, arial12, heim_links, heim_oben);
            int gast_links = 8;
            int gast_oben = 186;
            DrawTable(ev, arial12, gast_links, gast_oben);

            
            ev.Graphics.DrawString("Gastmannschaft: _____________________________________________________", new Font("Arial", 12), Brushes.Black, 9, 177);

            ev.Graphics.DrawString("Unterschrift: _________________________________________________________", arial12, Brushes.Black, 10, 260);
            ev.Graphics.DrawString("Heim                                                                                                           Gast", new Font("Arial", 7), Brushes.Black, 48, 265);

            // Anschrift:
            string strName = "Heinz Breu";
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, 28, 56);
            int height = (int)ev.Graphics.MeasureString(strName, arial12).Height;
            strName = "Hauptstr. 15";
            int yPos2 = 56 + height;
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, 28, yPos2);
            height = (int)ev.Graphics.MeasureString(strName, arial12).Height;
            yPos2 += (height * 2);
            strName = "93080 Pentling";
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, 28, yPos2);

            // Wettkampfdetails
            ev.Graphics.DrawString("Gruppe:", arial12, Brushes.Black, 109, 43);
            ev.Graphics.DrawString("Waffenart:", arial12, Brushes.Black, 109, 53);
            ev.Graphics.DrawString("Runde Nr.:", arial12, Brushes.Black, 109, 63);
            ev.Graphics.DrawString("Ort:", arial12, Brushes.Black, 109, 73);
            ev.Graphics.DrawString("Datum:", arial12, Brushes.Black, 109, 83);

            Pen p = new Pen(Brushes.Black, 0.5f);
            ev.Graphics.DrawLine(p, 133, 48, 190, 48);
            ev.Graphics.DrawLine(p, 133, 58, 190, 58);
            ev.Graphics.DrawLine(p, 133, 68, 190, 68);
            ev.Graphics.DrawLine(p, 133, 78, 190, 78);
            ev.Graphics.DrawLine(p, 133, 88, 190, 88);

            // Fill values
            Font schrift = new Font("Segoe Script", 16);
            ev.Graphics.DrawString("LG - Luftgewehr", schrift, Brushes.Blue, 135, 50);
            ev.Graphics.DrawString("Eltheim", schrift, Brushes.Blue, 135, 70);
            ev.Graphics.DrawString(dateTimePicker1.Value.ToShortDateString(), schrift, Brushes.Blue, 135, 80);
            
            //MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.siusclubConnectionStringTEST);
            conn.Open();
            string anzVereineSQL = String.Format("select distinct verein, STR_TO_DATE(datum, '%a %M %d %Y') AS Date from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='Gauliga' having YEAR(Date)={0} and MONTH(Date)={1} and DAY(Date)={2}", dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day);
            MySqlCommand cmd = new MySqlCommand(anzVereineSQL, conn);
            string gastverein="";
            string heimverein="";
            int countVereine = 0;
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                countVereine++; // ein Verein wurde eingelesen
                if (countVereine > 2)
                {
                    MessageBox.Show("Mehr als zwei verschiedene Vereinsnamen gefunden!");
                    return;
                }
                if (reader["verein"].ToString().Contains("Eltheim"))
                    if (heimverein != "")
                    {
                        MessageBox.Show("Heimatverein mehrfach vorhanden. Möglcherweise unterschiedliche Schreibweisen?");
                        return;
                    }
                    else
                    {
                        heimverein = reader["verein"].ToString();
                    }
                else
                    if (gastverein != "")
                    {
                        MessageBox.Show("Entweder mehrere Gastvereine gefunden oder unterschiedliche Schreibweisen!");
                        return;
                    }
                    else 
                    { 
                        gastverein = reader["verein"].ToString(); 
                    }
            }
            if (heimverein == "" || gastverein == "")
            {
                MessageBox.Show("keine zwei Vereine gefunden für Gauligakampf an diesem Tag");
                return;
            }
            MessageBox.Show("Heimverein: " + heimverein + ", Gastverein: " + gastverein);
            ev.Graphics.DrawString(gastverein, schrift, Brushes.Blue, 50, 174);
            reader.Close();

            // jetzt lese alle Schützen der Heimmannschaft und prüfe, ob es mehr als 5 sind.
            string strHeimschuetzen = string.Format("SELECT CONCAT(name, ', ', vorname) as fullname, ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='Gauliga' and verein='{3}' and status='beendet' having YEAR(Date)={0} and MONTH(Date)={1} and DAY(Date)={2}", dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, heimverein);
            //reader.Dispose();
            cmd.CommandText = strHeimschuetzen;
            reader = cmd.ExecuteReader();
            int countSchuetzen=0;
            string[] strHeimSchuetzen;
            strHeimSchuetzen = new string[5];
            while (reader.Read())
            {
                countSchuetzen++;
                if (countSchuetzen > 5) { MessageBox.Show("Mehr als 5 Schützen angetreten."); return; }
                strHeimSchuetzen[countSchuetzen - 1] = reader["fullname"].ToString();
                ev.Graphics.DrawString(strHeimSchuetzen[countSchuetzen - 1], schrift, Brushes.Blue, 27, heim_oben + 8 + (countSchuetzen - 1) * 9);
                string strErgebnis = reader["ergebnis"].ToString();
                float ergWidth = ev.Graphics.MeasureString(strErgebnis, schrift).Width;
                ev.Graphics.DrawString(strErgebnis, schrift, Brushes.Blue, 181 - ergWidth, heim_oben + 8 + (countSchuetzen - 1) * 9);
            }
            reader.Close();
            reader.Dispose();
            string strHeimSumme = string.Format("SELECT SUM(ergebnis) AS Summe, STR_TO_DATE(datum, '%a %M %d %Y') AS Date from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='Gauliga' and verein='{3}' and status='beendet' group by Date having YEAR(Date)={0} and MONTH(Date)={1} and DAY(Date)={2}", dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, heimverein);
            cmd.CommandText = strHeimSumme;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string strSumme = reader["Summe"].ToString();
                float sumWidth = ev.Graphics.MeasureString(strSumme, schrift).Width;
                ev.Graphics.DrawString(strSumme, schrift, Brushes.Blue, 181 - sumWidth, heim_oben + 8 + 5 * 9);
            }

            // jetzt lese alle Schützen der Gastmannschaft und prüfe, ob es mehr als 5 sind.
            string strGastschuetzen = string.Format("SELECT CONCAT(name, ', ', vorname) as fullname, ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='Gauliga' and verein='{3}' and status='beendet' having YEAR(Date)={0} and MONTH(Date)={1} and DAY(Date)={2}", dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, gastverein);
            reader.Dispose();
            cmd.CommandText = strGastschuetzen;
            reader = cmd.ExecuteReader();
            countSchuetzen = 0;
            while (reader.Read())
            {
                countSchuetzen++;
                if (countSchuetzen > 5) { MessageBox.Show("Gast: Mehr als 5 Schützen angetreten."); return; }
                ev.Graphics.DrawString(reader["fullname"].ToString(), schrift, Brushes.Blue, 27, gast_oben + 8 + (countSchuetzen - 1) * 9);
                string strErgebnis = reader["ergebnis"].ToString();
                float ergWidth = ev.Graphics.MeasureString(strErgebnis, schrift).Width;
                ev.Graphics.DrawString(strErgebnis, schrift, Brushes.Blue, 181 - ergWidth, gast_oben + 8 + (countSchuetzen - 1) * 9);
            }
            reader.Close();
            reader.Dispose();
            string strGastSumme = string.Format("SELECT SUM(ergebnis) AS Summe, STR_TO_DATE(datum, '%a %M %d %Y') AS Date from schiessbuch inner join schuetzen on schuetzen.id=schiessbuch.id where disziplin='Gauliga' and verein='{3}' and status='beendet' group by Date having YEAR(Date)={0} and MONTH(Date)={1} and DAY(Date)={2}", dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, gastverein);
            cmd.CommandText = strGastSumme;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string strSumme = reader["Summe"].ToString();
                float sumWidth = ev.Graphics.MeasureString(strSumme, schrift).Width;
                ev.Graphics.DrawString(strSumme, schrift, Brushes.Blue, 181 - sumWidth, gast_oben + 8 + 5 * 9);
            }



            // Fusszeile:
            Font arial10 = new Font("Arial", 10, FontStyle.Bold);
            ev.Graphics.DrawString("Ergebnisliste an:", arial10, Brushes.Black, 10, 272);
            string strFusszeile = "Heinz Breu Hauptstr. 15 - 93080 Pentling";
            float wdthFusszeile = ev.Graphics.MeasureString(strFusszeile, arial10).Height;
            ev.Graphics.DrawString(strFusszeile, arial10, Brushes.Black, 48, 272);
            ev.Graphics.DrawString("Mobil 0151 6244 28 39 E-mail: rwkl@ksv-donaugau.de", arial10, Brushes.Black, 48, 272 + wdthFusszeile);
            ev.Graphics.DrawString("Fax 032 21 - 119 112 5", arial10, Brushes.Black, 48, 272 + 2 * wdthFusszeile);
        }

        private void DrawTable(PrintPageEventArgs ev, Font arial12, int heim_links, int heim_oben)
        {
            int heim_breite = 174;
            int heim_hoehe = 52;
            int heim_kopf_hoehe = 7;
            int heim_zeile_hoehe = 9;
            int Nr_breite = 8;
            int S_pos = 14;
            int E_pos = 19;
            int Name_pos = 111;
            int Pass_pos = 152;
            Pen p = new Pen(Brushes.Black, .5f);
            ev.Graphics.DrawRectangle(p, new Rectangle(heim_links, heim_oben, heim_breite, heim_hoehe));
            ev.Graphics.DrawLine(p, heim_links, heim_oben + heim_kopf_hoehe, heim_links + heim_breite, heim_oben + heim_kopf_hoehe);
            for (int i = 0; i < 4; i++)
            {
                ev.Graphics.DrawLine(p, heim_links, heim_oben + heim_kopf_hoehe + (i + 1) * heim_zeile_hoehe, heim_links + heim_breite, heim_oben + heim_kopf_hoehe + (i + 1) * heim_zeile_hoehe);
            }
            ev.Graphics.DrawLine(p, heim_links + Nr_breite, heim_oben, heim_links + Nr_breite, heim_oben + heim_hoehe);
            ev.Graphics.DrawLine(p, heim_links + S_pos, heim_oben, heim_links + S_pos, heim_oben + heim_hoehe);
            ev.Graphics.DrawLine(p, heim_links + E_pos, heim_oben, heim_links + E_pos, heim_oben + heim_hoehe);
            ev.Graphics.DrawLine(p, heim_links + Name_pos, heim_oben, heim_links + Name_pos, heim_oben + heim_hoehe);
            ev.Graphics.DrawLine(p, heim_links + Pass_pos, heim_oben, heim_links + Pass_pos, heim_oben + heim_hoehe);
            ev.Graphics.DrawString("Nr", arial12, Brushes.Black, 9, heim_oben + 1);
            ev.Graphics.DrawString("S", arial12, Brushes.Black, 17, heim_oben + 1);
            ev.Graphics.DrawString("E", arial12, Brushes.Black, 22, heim_oben + 1);
            string strName = "Name";
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, heim_links + 19 + ((111 - 19) / 2) - (int)ev.Graphics.MeasureString(strName, arial12).Width / 2, heim_oben + 1);
            strName = "Schützenpass-Nr.";
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, heim_links + 111 + ((152 - 111) / 2) - (int)ev.Graphics.MeasureString(strName, arial12).Width / 2, heim_oben + 1);
            strName = "Ringe";
            ev.Graphics.DrawString(strName, arial12, Brushes.Black, heim_links + 152 + ((heim_breite - 152) / 2) - (int)ev.Graphics.MeasureString(strName, arial12).Width / 2, heim_oben + 1);
            Pen fett = new Pen(Brushes.Black, 1);
            ev.Graphics.DrawRectangle(fett, heim_links + heim_breite - (heim_breite - 152), heim_oben + heim_hoehe, heim_breite - 152, 12);

            ev.Graphics.DrawString("S = Stammschütze, E = Ersatzschütze, zutreffendes ankreuzen", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, 9, heim_oben+55);
            ev.Graphics.DrawString("Gesamtergebnis", new Font("Arial", 14), Brushes.Black, 122, heim_oben + 55);

            ev.Graphics.DrawString("1", arial12, Brushes.Black, 10, heim_oben + 1 + 8);
            ev.Graphics.DrawString("2", arial12, Brushes.Black, 10, heim_oben + 1 + 8 + 9 * 1);
            ev.Graphics.DrawString("3", arial12, Brushes.Black, 10, heim_oben + 1 + 8 + 9 * 2);
            ev.Graphics.DrawString("4", arial12, Brushes.Black, 10, heim_oben + 1 + 8 + 9 * 3);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
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
            //MessageBox.Show(Properties.Settings.Default.siusclubConnectionString);
            MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
            conn.Open();
            string filterDateStr = dateTimePicker1.Value.Year + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Day;
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT DISZIPLIN, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch HAVING Date='" + filterDateStr + "'", conn);
            MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);
            //DataGridViewColumn[] cols = new DataGridViewColumnCollection();
            int i=0;
            while (reader.Read())
            {
                DataGridViewColumn col = new DataGridViewColumn();
                col.Name = reader["disziplin"].ToString();
                col.CellTemplate = cell;
                Schiessabend.Columns.Add(col);
                i++;
            }
            reader.Close();
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
                for (int j = 0; j < disziplinen; j++)
                {
                    MySqlConnection conn2 = new MySqlConnection(Properties.Settings.Default.siusclubConnectionString);
                    conn2.Open();
                    //MessageBox.Show(Schiessabend.Columns[j + 3].Name);
                    string cmdstr = "SELECT ergebnis, STR_TO_DATE(datum, '%a %M %d %Y') AS Date FROM schiessbuch WHERE disziplin='" + Schiessabend.Columns[j + 3].Name + "' AND id='" + reader["SID"] + "' HAVING Date='" + filterDateStr + "'";
                    MySqlCommand cmd2 = new MySqlCommand(cmdstr, conn2);
                    MySqlDataReader reader2 = cmd2.ExecuteReader();
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
                    conn2.Close();
                    Schiessabend[j + 3, newRow].Value = result;

                }
                
            }

            conn.Close();
        }

        PrintDocument pd;

        private void button1_Click(object sender, EventArgs e)
        {
            if (pd != null)
                pd.Dispose();
            else
                pd = new PrintDocument();
            printFont = new Font("Arial", 10);
            //linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            printPreviewControl1.Document = pd;
            button2.Enabled = true;
            //PrintPreviewDialog pvd = new PrintPreviewDialog();
            //pvd.Document = pd;
            //pvd.ShowDialog();
            //pd.Print();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //pd.Print();
            PrintDialog pdlg = new PrintDialog();
            pdlg.Document=pd;
            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                pd.Print();
            //pdlg.ShowDialog();
        }
    }
}
