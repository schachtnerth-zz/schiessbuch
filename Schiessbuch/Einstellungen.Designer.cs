namespace schiessbuch
{
    partial class Einstellungen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Einstellungen));
            this.groupEinstellungenAktualisierung = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbDatabaseRefresh = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTimerInterval = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOKEinstellungen = new System.Windows.Forms.Button();
            this.btnEinstellungenAbbrechen = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbAnzLetzteTreffer = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.grpDisziplinen = new System.Windows.Forms.GroupBox();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnSetzeInaktiv = new System.Windows.Forms.Button();
            this.btnSetzeAktiv = new System.Windows.Forms.Button();
            this.lbAktiveDisziplinen = new System.Windows.Forms.ListBox();
            this.lbVerfuegbareDisziplinen = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupEinstellungenAktualisierung.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpDisziplinen.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupEinstellungenAktualisierung
            // 
            this.groupEinstellungenAktualisierung.Controls.Add(this.label5);
            this.groupEinstellungenAktualisierung.Controls.Add(this.tbDatabaseRefresh);
            this.groupEinstellungenAktualisierung.Controls.Add(this.label6);
            this.groupEinstellungenAktualisierung.Controls.Add(this.label4);
            this.groupEinstellungenAktualisierung.Controls.Add(this.label3);
            this.groupEinstellungenAktualisierung.Controls.Add(this.tbTimerInterval);
            this.groupEinstellungenAktualisierung.Controls.Add(this.label2);
            this.groupEinstellungenAktualisierung.Controls.Add(this.label1);
            this.groupEinstellungenAktualisierung.Location = new System.Drawing.Point(13, 13);
            this.groupEinstellungenAktualisierung.Name = "groupEinstellungenAktualisierung";
            this.groupEinstellungenAktualisierung.Size = new System.Drawing.Size(251, 200);
            this.groupEinstellungenAktualisierung.TabIndex = 0;
            this.groupEinstellungenAktualisierung.TabStop = false;
            this.groupEinstellungenAktualisierung.Text = "Aktualisierung";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(131, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "-te Mal abfragen";
            // 
            // tbDatabaseRefresh
            // 
            this.tbDatabaseRefresh.Location = new System.Drawing.Point(97, 170);
            this.tbDatabaseRefresh.Name = "tbDatabaseRefresh";
            this.tbDatabaseRefresh.Size = new System.Drawing.Size(33, 20);
            this.tbDatabaseRefresh.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 173);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Datenbank jedes";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(7, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(234, 92);
            this.label4.TabIndex = 4;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Sekunden";
            // 
            // tbTimerInterval
            // 
            this.tbTimerInterval.Location = new System.Drawing.Point(97, 52);
            this.tbTimerInterval.Name = "tbTimerInterval";
            this.tbTimerInterval.Size = new System.Drawing.Size(33, 20);
            this.tbTimerInterval.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Werte laden alle";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(234, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Dieser Wert gibt an, wie oft die aktuellen Werte vom Schießstand-PC abgerufen wer" +
    "den sollen.";
            // 
            // btnOKEinstellungen
            // 
            this.btnOKEinstellungen.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOKEinstellungen.Location = new System.Drawing.Point(13, 301);
            this.btnOKEinstellungen.Name = "btnOKEinstellungen";
            this.btnOKEinstellungen.Size = new System.Drawing.Size(75, 23);
            this.btnOKEinstellungen.TabIndex = 1;
            this.btnOKEinstellungen.Text = "OK";
            this.btnOKEinstellungen.UseVisualStyleBackColor = true;
            // 
            // btnEinstellungenAbbrechen
            // 
            this.btnEinstellungenAbbrechen.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnEinstellungenAbbrechen.Location = new System.Drawing.Point(660, 301);
            this.btnEinstellungenAbbrechen.Name = "btnEinstellungenAbbrechen";
            this.btnEinstellungenAbbrechen.Size = new System.Drawing.Size(75, 23);
            this.btnEinstellungenAbbrechen.TabIndex = 2;
            this.btnEinstellungenAbbrechen.Text = "Abbrechen";
            this.btnEinstellungenAbbrechen.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.tbAnzLetzteTreffer);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Location = new System.Drawing.Point(13, 219);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(251, 81);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Schussanzeige";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(109, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(84, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Treffer anzeigen";
            // 
            // tbAnzLetzteTreffer
            // 
            this.tbAnzLetzteTreffer.Location = new System.Drawing.Point(70, 52);
            this.tbAnzLetzteTreffer.Name = "tbAnzLetzteTreffer";
            this.tbAnzLetzteTreffer.Size = new System.Drawing.Size(33, 20);
            this.tbAnzLetzteTreffer.TabIndex = 2;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 55);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 13);
            this.label11.TabIndex = 1;
            this.label11.Text = "Die letzten";
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.Gray;
            this.label12.Location = new System.Drawing.Point(7, 20);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(234, 28);
            this.label12.TabIndex = 0;
            this.label12.Text = "Dieser Wert gibt an, die wie viel letzten Schüsse angezeigt werden sollen.";
            // 
            // grpDisziplinen
            // 
            this.grpDisziplinen.Controls.Add(this.label8);
            this.grpDisziplinen.Controls.Add(this.label7);
            this.grpDisziplinen.Controls.Add(this.button1);
            this.grpDisziplinen.Controls.Add(this.btnDown);
            this.grpDisziplinen.Controls.Add(this.btnUp);
            this.grpDisziplinen.Controls.Add(this.btnSetzeInaktiv);
            this.grpDisziplinen.Controls.Add(this.btnSetzeAktiv);
            this.grpDisziplinen.Controls.Add(this.lbAktiveDisziplinen);
            this.grpDisziplinen.Controls.Add(this.lbVerfuegbareDisziplinen);
            this.grpDisziplinen.Location = new System.Drawing.Point(270, 13);
            this.grpDisziplinen.Name = "grpDisziplinen";
            this.grpDisziplinen.Size = new System.Drawing.Size(465, 287);
            this.grpDisziplinen.TabIndex = 4;
            this.grpDisziplinen.TabStop = false;
            this.grpDisziplinen.Text = "Disziplinen verwalten";
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(426, 136);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(33, 23);
            this.btnDown.TabIndex = 5;
            this.btnDown.Text = "ab";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(426, 107);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(33, 23);
            this.btnUp.TabIndex = 4;
            this.btnUp.Text = "auf";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnSetzeInaktiv
            // 
            this.btnSetzeInaktiv.Location = new System.Drawing.Point(217, 136);
            this.btnSetzeInaktiv.Name = "btnSetzeInaktiv";
            this.btnSetzeInaktiv.Size = new System.Drawing.Size(33, 23);
            this.btnSetzeInaktiv.TabIndex = 3;
            this.btnSetzeInaktiv.Text = "<-";
            this.btnSetzeInaktiv.UseVisualStyleBackColor = true;
            // 
            // btnSetzeAktiv
            // 
            this.btnSetzeAktiv.Location = new System.Drawing.Point(217, 107);
            this.btnSetzeAktiv.Name = "btnSetzeAktiv";
            this.btnSetzeAktiv.Size = new System.Drawing.Size(33, 23);
            this.btnSetzeAktiv.TabIndex = 2;
            this.btnSetzeAktiv.Text = "->";
            this.btnSetzeAktiv.UseVisualStyleBackColor = true;
            // 
            // lbAktiveDisziplinen
            // 
            this.lbAktiveDisziplinen.FormattingEnabled = true;
            this.lbAktiveDisziplinen.Location = new System.Drawing.Point(256, 33);
            this.lbAktiveDisziplinen.Name = "lbAktiveDisziplinen";
            this.lbAktiveDisziplinen.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAktiveDisziplinen.Size = new System.Drawing.Size(164, 212);
            this.lbAktiveDisziplinen.TabIndex = 1;
            this.lbAktiveDisziplinen.SelectedIndexChanged += new System.EventHandler(this.lbAktiveDisziplinen_SelectedIndexChanged);
            this.lbAktiveDisziplinen.DoubleClick += new System.EventHandler(this.lbAktiveDisziplinen_DoubleClick);
            // 
            // lbVerfuegbareDisziplinen
            // 
            this.lbVerfuegbareDisziplinen.FormattingEnabled = true;
            this.lbVerfuegbareDisziplinen.Location = new System.Drawing.Point(7, 33);
            this.lbVerfuegbareDisziplinen.Name = "lbVerfuegbareDisziplinen";
            this.lbVerfuegbareDisziplinen.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbVerfuegbareDisziplinen.Size = new System.Drawing.Size(204, 212);
            this.lbVerfuegbareDisziplinen.TabIndex = 0;
            this.lbVerfuegbareDisziplinen.SelectedIndexChanged += new System.EventHandler(this.lbVerfuegbareDisziplinen_SelectedIndexChanged);
            this.lbVerfuegbareDisziplinen.DoubleClick += new System.EventHandler(this.lbVerfuegbareDisziplinen_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(292, 258);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Disziplinen speichern";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Verfügbare Disziplinen";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(253, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Aktive Disziplinen";
            // 
            // Einstellungen
            // 
            this.AcceptButton = this.btnOKEinstellungen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnEinstellungenAbbrechen;
            this.ClientSize = new System.Drawing.Size(747, 336);
            this.ControlBox = false;
            this.Controls.Add(this.grpDisziplinen);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnEinstellungenAbbrechen);
            this.Controls.Add(this.btnOKEinstellungen);
            this.Controls.Add(this.groupEinstellungenAktualisierung);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Einstellungen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Einstellungen";
            this.Load += new System.EventHandler(this.Einstellungen_Load);
            this.groupEinstellungenAktualisierung.ResumeLayout(false);
            this.groupEinstellungenAktualisierung.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpDisziplinen.ResumeLayout(false);
            this.grpDisziplinen.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupEinstellungenAktualisierung;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOKEinstellungen;
        private System.Windows.Forms.Button btnEinstellungenAbbrechen;
        public System.Windows.Forms.TextBox tbDatabaseRefresh;
        public System.Windows.Forms.TextBox tbTimerInterval;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox tbAnzLetzteTreffer;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox grpDisziplinen;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnSetzeInaktiv;
        private System.Windows.Forms.Button btnSetzeAktiv;
        private System.Windows.Forms.ListBox lbAktiveDisziplinen;
        private System.Windows.Forms.ListBox lbVerfuegbareDisziplinen;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
    }
}