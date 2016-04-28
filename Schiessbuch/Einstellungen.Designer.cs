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
            this.groupEinstellungenAktualisierung.SuspendLayout();
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
            // Einstellungen
            // 
            this.AcceptButton = this.btnOKEinstellungen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnEinstellungenAbbrechen;
            this.ClientSize = new System.Drawing.Size(747, 336);
            this.ControlBox = false;
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
            this.groupEinstellungenAktualisierung.ResumeLayout(false);
            this.groupEinstellungenAktualisierung.PerformLayout();
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
    }
}