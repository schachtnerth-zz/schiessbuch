using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace schiessbuch
{
    public partial class Einstellungen : Form
    {
        public Einstellungen()
        {
            InitializeComponent();
        }

        XDocument konfigurationDoc;
        private readonly string konfiguration= @"C:\Users\Thomas\Documents\Backup Datenbanken\konfiguration.xml";
        private readonly string disziplinen= @"C:\Users\Thomas\Documents\Backup Datenbanken\disziplinen.xml";
//        private readonly string konfiguration = @"\\siusclub\siusclub\konfiguration.xml";
//        private readonly string disziplinen = @"\\siusclub\siusclub\disziplinen.xml";

        private void Einstellungen_Load(object sender, EventArgs e)
        {
            konfigurationDoc = XDocument.Load(konfiguration);
            XDocument disziplinenDoc = XDocument.Load(disziplinen);
            foreach (XElement element in konfigurationDoc.Element("konfiguration").Elements())
            {
                if (element.Name.LocalName.Equals("disziplinenverzeichnis"))
                {
                    // alle disziplinen durchgehen und in liste eintragen
                    foreach (XElement disziplin in element.Elements())
                    {
                        string dname = "";
                        string dId = "";
                        foreach (XElement detailInfo in disziplin.Elements())
                        {
                            if (detailInfo.Name.LocalName.Equals("name"))
                            {
                                dname = detailInfo.Value;
                            }
                            if (detailInfo.Name.LocalName.Equals("id"))
                            {
                                dId = detailInfo.Value;
                            }
                            if (dname.Length > 0 && dId.Length > 0)
                            {
                                disziplin disz = new disziplin(dId, dname, disziplin);
                                //MessageBox.Show(disz.ToString());
                                lbAktiveDisziplinen.Items.Add(disz);
                                dname = "";
                                dId = "";
                            }
                        }
                    }
                }
            }
            foreach (XElement element in disziplinenDoc.Root.Elements())
            {
                if (element.Name.LocalName.Equals("disziplinenverzeichnis"))
                {
                    // alle disziplinen durchgehen und in liste eintragen
                    foreach (XElement disziplin in element.Elements())
                    {
                        string dname = "";
                        string dId = "";
                        foreach (XElement detailInfo in disziplin.Elements())
                        {
                            if (detailInfo.Name.LocalName.Equals("name")) {
                                dname = detailInfo.Value;
                            }
                            if (detailInfo.Name.LocalName.Equals("id"))
                            {
                                dId = detailInfo.Value;
                            }
                            
                            if (dname.Length > 0 && dId.Length > 0)
                            {
                                bool aktiv = false;
                                disziplin disz = new disziplin(dId, dname, disziplin);
                                // prüfe, ob diese disziplin vielleicht schon unter den aktiven ist. dann zeig sie nicht mehr an
                                foreach (disziplin aktiveDisziplin in lbAktiveDisziplinen.Items)
                                {
                                    if (aktiveDisziplin.Equals(disz)) aktiv = true;
                                }
                                //MessageBox.Show(disz.ToString());
                                if (!aktiv) lbVerfuegbareDisziplinen.Items.Add(disz);
                                dname = "";
                                dId = "";
                            }
                        }
                    }
                }
            }
        }

        private void lbAktiveDisziplinen_DoubleClick(object sender, EventArgs e)
        {
            SetzeDisziplinInaktiv(lbAktiveDisziplinen.SelectedIndex);
        }

        private void SetzeDisziplinInaktiv(int index)
        {
            lbVerfuegbareDisziplinen.Items.Add(lbAktiveDisziplinen.Items[index]);
            lbAktiveDisziplinen.Items.RemoveAt(index);
        }

        private void lbVerfuegbareDisziplinen_DoubleClick(object sender, EventArgs e)
        {
            SetzeDisziplinAktiv(lbVerfuegbareDisziplinen.SelectedIndex);
        }

        private void SetzeDisziplinAktiv(int selectedIndex)
        {
            lbAktiveDisziplinen.Items.Add(lbVerfuegbareDisziplinen.Items[selectedIndex]);
            lbVerfuegbareDisziplinen.Items.RemoveAt(selectedIndex);
        }

        private void lbVerfuegbareDisziplinen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbVerfuegbareDisziplinen.SelectedIndices.Count > 0)
                btnSetzeAktiv.Enabled = true;
            else
                btnSetzeAktiv.Enabled = false;
        }

        private void lbAktiveDisziplinen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbAktiveDisziplinen.SelectedIndices.Count > 0)
                btnSetzeInaktiv.Enabled = true;
            else
                btnSetzeInaktiv.Enabled = false;
            if (lbAktiveDisziplinen.SelectedIndices.Count == 1)
            {
                if (lbAktiveDisziplinen.SelectedIndex == 0)
                    btnUp.Enabled = false;
                else
                    btnUp.Enabled = true;
                if (lbAktiveDisziplinen.SelectedIndex == lbAktiveDisziplinen.Items.Count - 1)
                    btnDown.Enabled = false;
                else
                    btnDown.Enabled = true;
            } else
            {
                btnUp.Enabled = false;
                btnDown.Enabled = false;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveItem(1);
        }



        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            // Checking selected item
            if (lbAktiveDisziplinen.SelectedItem == null || lbAktiveDisziplinen.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = lbAktiveDisziplinen.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= lbAktiveDisziplinen.Items.Count)
                return; // Index out of range - nothing to do

            object selected = lbAktiveDisziplinen.SelectedItem;

            // Removing removable element
            lbAktiveDisziplinen.Items.Remove(selected);
            // Insert it in new position
            lbAktiveDisziplinen.Items.Insert(newIndex, selected);
            // Restore selection
            lbAktiveDisziplinen.SetSelected(newIndex, true);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveItem(-1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            konfigurationDoc = XDocument.Load(konfiguration);
            foreach (XElement element in konfigurationDoc.Root.Elements())
            {
                if (element.Name.LocalName.Equals("disziplinenverzeichnis"))
                {
                    element.RemoveAll();
                    foreach (disziplin disz in lbAktiveDisziplinen.Items)
                    {
                        element.Add(disz.xmlElement);
                    }
                }
            }
            konfigurationDoc.Save(konfiguration);
        }

        private void btnOKEinstellungen_Click(object sender, EventArgs e)
        {

        }
    }
}
