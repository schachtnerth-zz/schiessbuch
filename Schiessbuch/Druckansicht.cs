using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using MySql.Data.MySqlClient;

namespace schiessbuch
{
    public partial class Druckansicht : Form
    {
        public Druckansicht()
        {
            InitializeComponent();
        }

        private void Druckansicht_Load(object sender, EventArgs e)
        {
            //PrintDocument pd = new PrintDocument();
            //pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            //pd.Print();
            //Properties.Settings.Default.siusclubConnectionString = "server=192.168.178.202;user id=siusclub;password=siusclub;persistsecurityinfo=True;data" +
            //"base=siusclub;Allow User Variables=true";

        }

        private Font printFont;
        private int linesCount;
        private int currentLinesPrinted;

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
            int maxLen=0;
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
                    string idstr=Schiessabend["ID", currentLinesPrinted].Value.ToString();
                    yPos = topMargin + maxLen + ((count +1) * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(idstr, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                    ev.Graphics.DrawString(
                        Schiessabend["name",currentLinesPrinted].Value.ToString() + ", " + Schiessabend["vorname", currentLinesPrinted].Value.ToString(),
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
            int i=0;
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

        PrintDocument pd;

        private void button1_Click(object sender, EventArgs e)
        {
            if (pd== null) { 
                pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            }
            printFont = new Font("Arial", 10);
            linesCount = Schiessabend.Rows.Count;
            //pd = new PrintDocument();
            if (printPreviewControl1.Document != null)
                printPreviewControl1.Document.Dispose();
            
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
            pdlg.Dispose();
            //pdlg.ShowDialog();
        }
    }
}
