namespace schiessbuch
{
    partial class GauligaAuswahlDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.GauligaDGVHeimverein = new System.Windows.Forms.DataGridView();
            this.schuetzeName2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ergebnisHeim = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DatumHeim = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HeimWertung = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SE_Heim = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.idschuetzeHeim = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HeimSession = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gauligaOKBtn = new System.Windows.Forms.Button();
            this.GauligaDGVGastverein = new System.Windows.Forms.DataGridView();
            this.schuetzeNameGast = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ergebnisGast = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DatumGast = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GastWertung = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SE_Gast = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.idschuetzeGast = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GastSession = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblHeimVerein = new System.Windows.Forms.Label();
            this.lblGastVerein = new System.Windows.Forms.Label();
            this.gauligaCancelBtn = new System.Windows.Forms.Button();
            this.labelHeimVerein = new System.Windows.Forms.Label();
            this.labelGastVerein = new System.Windows.Forms.Label();
            this.tbGruppe = new System.Windows.Forms.TextBox();
            this.lblGruppe = new System.Windows.Forms.Label();
            this.tbRunde = new System.Windows.Forms.TextBox();
            this.lblRunde = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.GauligaDGVHeimverein)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GauligaDGVGastverein)).BeginInit();
            this.SuspendLayout();
            // 
            // GauligaDGVHeimverein
            // 
            this.GauligaDGVHeimverein.AllowUserToAddRows = false;
            this.GauligaDGVHeimverein.AllowUserToDeleteRows = false;
            this.GauligaDGVHeimverein.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GauligaDGVHeimverein.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.schuetzeName2,
            this.ergebnisHeim,
            this.DatumHeim,
            this.HeimWertung,
            this.SE_Heim,
            this.idschuetzeHeim,
            this.HeimSession});
            this.GauligaDGVHeimverein.Location = new System.Drawing.Point(12, 29);
            this.GauligaDGVHeimverein.Name = "GauligaDGVHeimverein";
            this.GauligaDGVHeimverein.RowHeadersVisible = false;
            this.GauligaDGVHeimverein.Size = new System.Drawing.Size(483, 166);
            this.GauligaDGVHeimverein.TabIndex = 0;
            this.GauligaDGVHeimverein.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GauligaDGVHeimverein_CellMouseUp);
            this.GauligaDGVHeimverein.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.GauligaDGVHeimverein_CellValueChanged);
            // 
            // schuetzeName2
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.schuetzeName2.DefaultCellStyle = dataGridViewCellStyle7;
            this.schuetzeName2.HeaderText = "Name";
            this.schuetzeName2.Name = "schuetzeName2";
            this.schuetzeName2.Width = 200;
            // 
            // ergebnisHeim
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ergebnisHeim.DefaultCellStyle = dataGridViewCellStyle8;
            this.ergebnisHeim.HeaderText = "Ergebnis";
            this.ergebnisHeim.Name = "ergebnisHeim";
            this.ergebnisHeim.Width = 50;
            // 
            // DatumHeim
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.DatumHeim.DefaultCellStyle = dataGridViewCellStyle9;
            this.DatumHeim.HeaderText = "Datum";
            this.DatumHeim.Name = "DatumHeim";
            this.DatumHeim.Width = 80;
            // 
            // HeimWertung
            // 
            this.HeimWertung.FalseValue = "F";
            this.HeimWertung.HeaderText = "Wertung";
            this.HeimWertung.Name = "HeimWertung";
            this.HeimWertung.TrueValue = "T";
            this.HeimWertung.Width = 50;
            // 
            // SE_Heim
            // 
            this.SE_Heim.HeaderText = "Stammschütze";
            this.SE_Heim.Items.AddRange(new object[] {
            "Stammschütze",
            "Ersatzschütze"});
            this.SE_Heim.Name = "SE_Heim";
            // 
            // idschuetzeHeim
            // 
            this.idschuetzeHeim.HeaderText = "ID";
            this.idschuetzeHeim.Name = "idschuetzeHeim";
            this.idschuetzeHeim.Visible = false;
            // 
            // HeimSession
            // 
            this.HeimSession.HeaderText = "session";
            this.HeimSession.Name = "HeimSession";
            this.HeimSession.Visible = false;
            // 
            // gauligaOKBtn
            // 
            this.gauligaOKBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gauligaOKBtn.Location = new System.Drawing.Point(421, 474);
            this.gauligaOKBtn.Name = "gauligaOKBtn";
            this.gauligaOKBtn.Size = new System.Drawing.Size(75, 23);
            this.gauligaOKBtn.TabIndex = 1;
            this.gauligaOKBtn.Text = "OK";
            this.gauligaOKBtn.UseVisualStyleBackColor = true;
            this.gauligaOKBtn.Click += new System.EventHandler(this.gauligaOKBtn_Click);
            // 
            // GauligaDGVGastverein
            // 
            this.GauligaDGVGastverein.AllowUserToAddRows = false;
            this.GauligaDGVGastverein.AllowUserToDeleteRows = false;
            this.GauligaDGVGastverein.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GauligaDGVGastverein.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.schuetzeNameGast,
            this.ergebnisGast,
            this.DatumGast,
            this.GastWertung,
            this.SE_Gast,
            this.idschuetzeGast,
            this.GastSession});
            this.GauligaDGVGastverein.Location = new System.Drawing.Point(12, 235);
            this.GauligaDGVGastverein.Name = "GauligaDGVGastverein";
            this.GauligaDGVGastverein.RowHeadersVisible = false;
            this.GauligaDGVGastverein.Size = new System.Drawing.Size(483, 166);
            this.GauligaDGVGastverein.TabIndex = 2;
            this.GauligaDGVGastverein.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GauligaDGVGastverein_CellMouseUp);
            this.GauligaDGVGastverein.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.GauligaDGVGastverein_CellValueChanged);
            // 
            // schuetzeNameGast
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.schuetzeNameGast.DefaultCellStyle = dataGridViewCellStyle10;
            this.schuetzeNameGast.HeaderText = "Name";
            this.schuetzeNameGast.Name = "schuetzeNameGast";
            this.schuetzeNameGast.Width = 200;
            // 
            // ergebnisGast
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.ergebnisGast.DefaultCellStyle = dataGridViewCellStyle11;
            this.ergebnisGast.HeaderText = "Ergebnis";
            this.ergebnisGast.Name = "ergebnisGast";
            this.ergebnisGast.Width = 50;
            // 
            // DatumGast
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.DatumGast.DefaultCellStyle = dataGridViewCellStyle12;
            this.DatumGast.HeaderText = "Datum";
            this.DatumGast.Name = "DatumGast";
            this.DatumGast.Width = 80;
            // 
            // GastWertung
            // 
            this.GastWertung.FalseValue = "F";
            this.GastWertung.HeaderText = "Wertung";
            this.GastWertung.Name = "GastWertung";
            this.GastWertung.TrueValue = "T";
            this.GastWertung.Width = 50;
            // 
            // SE_Gast
            // 
            this.SE_Gast.HeaderText = "Stammschütze";
            this.SE_Gast.Items.AddRange(new object[] {
            "Stammschütze",
            "Ersatzschütze"});
            this.SE_Gast.Name = "SE_Gast";
            // 
            // idschuetzeGast
            // 
            this.idschuetzeGast.HeaderText = "ID";
            this.idschuetzeGast.Name = "idschuetzeGast";
            this.idschuetzeGast.Visible = false;
            // 
            // GastSession
            // 
            this.GastSession.HeaderText = "session";
            this.GastSession.Name = "GastSession";
            this.GastSession.Visible = false;
            // 
            // lblHeimVerein
            // 
            this.lblHeimVerein.AutoSize = true;
            this.lblHeimVerein.Location = new System.Drawing.Point(9, 13);
            this.lblHeimVerein.Name = "lblHeimVerein";
            this.lblHeimVerein.Size = new System.Drawing.Size(63, 13);
            this.lblHeimVerein.TabIndex = 3;
            this.lblHeimVerein.Text = "Heimverein:";
            // 
            // lblGastVerein
            // 
            this.lblGastVerein.AutoSize = true;
            this.lblGastVerein.Location = new System.Drawing.Point(9, 219);
            this.lblGastVerein.Name = "lblGastVerein";
            this.lblGastVerein.Size = new System.Drawing.Size(61, 13);
            this.lblGastVerein.TabIndex = 4;
            this.lblGastVerein.Text = "Gastverein:";
            // 
            // gauligaCancelBtn
            // 
            this.gauligaCancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gauligaCancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.gauligaCancelBtn.Location = new System.Drawing.Point(12, 474);
            this.gauligaCancelBtn.Name = "gauligaCancelBtn";
            this.gauligaCancelBtn.Size = new System.Drawing.Size(75, 23);
            this.gauligaCancelBtn.TabIndex = 5;
            this.gauligaCancelBtn.Text = "Abbrechen";
            this.gauligaCancelBtn.UseVisualStyleBackColor = true;
            this.gauligaCancelBtn.Click += new System.EventHandler(this.gauligaCancelBtn_Click);
            // 
            // labelHeimVerein
            // 
            this.labelHeimVerein.AutoSize = true;
            this.labelHeimVerein.Location = new System.Drawing.Point(78, 13);
            this.labelHeimVerein.Name = "labelHeimVerein";
            this.labelHeimVerein.Size = new System.Drawing.Size(60, 13);
            this.labelHeimVerein.TabIndex = 6;
            this.labelHeimVerein.Text = "Heimverein";
            // 
            // labelGastVerein
            // 
            this.labelGastVerein.AutoSize = true;
            this.labelGastVerein.Location = new System.Drawing.Point(78, 219);
            this.labelGastVerein.Name = "labelGastVerein";
            this.labelGastVerein.Size = new System.Drawing.Size(58, 13);
            this.labelGastVerein.TabIndex = 7;
            this.labelGastVerein.Text = "Gastverein";
            // 
            // tbGruppe
            // 
            this.tbGruppe.Location = new System.Drawing.Point(74, 407);
            this.tbGruppe.Name = "tbGruppe";
            this.tbGruppe.Size = new System.Drawing.Size(100, 20);
            this.tbGruppe.TabIndex = 8;
            // 
            // lblGruppe
            // 
            this.lblGruppe.AutoSize = true;
            this.lblGruppe.Location = new System.Drawing.Point(12, 410);
            this.lblGruppe.Name = "lblGruppe";
            this.lblGruppe.Size = new System.Drawing.Size(45, 13);
            this.lblGruppe.TabIndex = 9;
            this.lblGruppe.Text = "Gruppe:";
            // 
            // tbRunde
            // 
            this.tbRunde.Location = new System.Drawing.Point(74, 434);
            this.tbRunde.Name = "tbRunde";
            this.tbRunde.Size = new System.Drawing.Size(100, 20);
            this.tbRunde.TabIndex = 10;
            // 
            // lblRunde
            // 
            this.lblRunde.AutoSize = true;
            this.lblRunde.Location = new System.Drawing.Point(12, 437);
            this.lblRunde.Name = "lblRunde";
            this.lblRunde.Size = new System.Drawing.Size(56, 13);
            this.lblRunde.TabIndex = 11;
            this.lblRunde.Text = "Runde Nr.";
            // 
            // GauligaAuswahlDlg
            // 
            this.AcceptButton = this.gauligaOKBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.gauligaCancelBtn;
            this.ClientSize = new System.Drawing.Size(511, 509);
            this.Controls.Add(this.lblRunde);
            this.Controls.Add(this.tbRunde);
            this.Controls.Add(this.lblGruppe);
            this.Controls.Add(this.tbGruppe);
            this.Controls.Add(this.labelGastVerein);
            this.Controls.Add(this.labelHeimVerein);
            this.Controls.Add(this.gauligaCancelBtn);
            this.Controls.Add(this.lblGastVerein);
            this.Controls.Add(this.lblHeimVerein);
            this.Controls.Add(this.GauligaDGVGastverein);
            this.Controls.Add(this.gauligaOKBtn);
            this.Controls.Add(this.GauligaDGVHeimverein);
            this.Name = "GauligaAuswahlDlg";
            this.Text = "Schützenauswahl";
            this.Load += new System.EventHandler(this.GauligaAuswahl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.GauligaDGVHeimverein)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GauligaDGVGastverein)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        //private System.Windows.Forms.DataGridViewTextBoxColumn schuetzeName;
        public System.Windows.Forms.DataGridView GauligaDGVHeimverein;
        private System.Windows.Forms.Button gauligaOKBtn;
        public System.Windows.Forms.DataGridView GauligaDGVGastverein;
        public System.Windows.Forms.Label lblGastVerein;
        public System.Windows.Forms.Label lblHeimVerein;
        private System.Windows.Forms.DataGridViewTextBoxColumn schuetzeName2;
        private System.Windows.Forms.DataGridViewTextBoxColumn ergebnisHeim;
        private System.Windows.Forms.DataGridViewTextBoxColumn DatumHeim;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HeimWertung;
        private System.Windows.Forms.DataGridViewComboBoxColumn SE_Heim;
        private System.Windows.Forms.DataGridViewTextBoxColumn idschuetzeHeim;
        private System.Windows.Forms.DataGridViewTextBoxColumn HeimSession;
        private System.Windows.Forms.Button gauligaCancelBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn schuetzeNameGast;
        private System.Windows.Forms.DataGridViewTextBoxColumn ergebnisGast;
        private System.Windows.Forms.DataGridViewTextBoxColumn DatumGast;
        private System.Windows.Forms.DataGridViewCheckBoxColumn GastWertung;
        private System.Windows.Forms.DataGridViewComboBoxColumn SE_Gast;
        private System.Windows.Forms.DataGridViewTextBoxColumn idschuetzeGast;
        private System.Windows.Forms.DataGridViewTextBoxColumn GastSession;
        public System.Windows.Forms.Label labelHeimVerein;
        public System.Windows.Forms.Label labelGastVerein;
        public System.Windows.Forms.Label lblGruppe;
        public System.Windows.Forms.Label lblRunde;
        public System.Windows.Forms.TextBox tbGruppe;
        public System.Windows.Forms.TextBox tbRunde;
    }
}