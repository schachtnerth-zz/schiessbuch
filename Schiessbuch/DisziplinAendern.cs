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
    public partial class DisziplinAendern : Form
    {
        public DisziplinAendern()
        {
            InitializeComponent();
        }

        private void DisziplinAendern_Load(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(Schiessbuch.connStr);
            string sql = "SELECT DISTINCT disziplin FROM schiessbuch";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            cbDisziplinen.DataSource = dt;
            cbDisziplinen.DisplayMember = "disziplin";
            cbDisziplinen.ValueMember = "disziplin";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
