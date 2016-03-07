namespace schiessbuch
{
    partial class ManuellNachtragen
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
            this.datum = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.disziplin = new System.Windows.Forms.ComboBox();
            this.ergebnis = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nachtragen = new System.Windows.Forms.Button();
            this.abbrechen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // datum
            // 
            this.datum.Location = new System.Drawing.Point(94, 12);
            this.datum.Name = "datum";
            this.datum.Size = new System.Drawing.Size(200, 20);
            this.datum.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Schiessen am:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Disziplin:";
            // 
            // disziplin
            // 
            this.disziplin.FormattingEnabled = true;
            this.disziplin.Items.AddRange(new object[] {
            "LG 20 Schuss",
            "LG 30 Schuss",
            "LG 40 Schuss",
            "LP 20 Schuss",
            "LG 20 Schuss Auflage",
            "LG 30 Schuss Auflage",
            "LG 40 Schuss Auflage"});
            this.disziplin.Location = new System.Drawing.Point(94, 38);
            this.disziplin.Name = "disziplin";
            this.disziplin.Size = new System.Drawing.Size(200, 21);
            this.disziplin.TabIndex = 4;
            // 
            // ergebnis
            // 
            this.ergebnis.Location = new System.Drawing.Point(94, 66);
            this.ergebnis.Name = "ergebnis";
            this.ergebnis.Size = new System.Drawing.Size(46, 20);
            this.ergebnis.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ergebnis:";
            // 
            // nachtragen
            // 
            this.nachtragen.Location = new System.Drawing.Point(15, 92);
            this.nachtragen.Name = "nachtragen";
            this.nachtragen.Size = new System.Drawing.Size(75, 23);
            this.nachtragen.TabIndex = 7;
            this.nachtragen.Text = "Nachtragen";
            this.nachtragen.UseVisualStyleBackColor = true;
            this.nachtragen.Click += new System.EventHandler(this.nachtragen_Click);
            // 
            // abbrechen
            // 
            this.abbrechen.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.abbrechen.Location = new System.Drawing.Point(219, 92);
            this.abbrechen.Name = "abbrechen";
            this.abbrechen.Size = new System.Drawing.Size(75, 23);
            this.abbrechen.TabIndex = 8;
            this.abbrechen.Text = "Abbrechen";
            this.abbrechen.UseVisualStyleBackColor = true;
            // 
            // ManuellNachtragen
            // 
            this.AcceptButton = this.nachtragen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.abbrechen;
            this.ClientSize = new System.Drawing.Size(309, 126);
            this.Controls.Add(this.abbrechen);
            this.Controls.Add(this.nachtragen);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ergebnis);
            this.Controls.Add(this.disziplin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.datum);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManuellNachtragen";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Manuell Nachtragen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker datum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox disziplin;
        private System.Windows.Forms.TextBox ergebnis;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button nachtragen;
        private System.Windows.Forms.Button abbrechen;
    }
}