namespace schiessbuch
{
    partial class AlternativerMySQLServer
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
            this.tbAlternativeServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOKAltServer = new System.Windows.Forms.Button();
            this.btnCancelAltServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbAlternativeServer
            // 
            this.tbAlternativeServer.Location = new System.Drawing.Point(16, 29);
            this.tbAlternativeServer.Name = "tbAlternativeServer";
            this.tbAlternativeServer.Size = new System.Drawing.Size(221, 20);
            this.tbAlternativeServer.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server der SIUSCLUB-Datenbank angeben:";
            // 
            // btnOKAltServer
            // 
            this.btnOKAltServer.Location = new System.Drawing.Point(16, 55);
            this.btnOKAltServer.Name = "btnOKAltServer";
            this.btnOKAltServer.Size = new System.Drawing.Size(75, 23);
            this.btnOKAltServer.TabIndex = 2;
            this.btnOKAltServer.Text = "OK";
            this.btnOKAltServer.UseVisualStyleBackColor = true;
            this.btnOKAltServer.Click += new System.EventHandler(this.btnOKAltServer_Click);
            // 
            // btnCancelAltServer
            // 
            this.btnCancelAltServer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelAltServer.Location = new System.Drawing.Point(162, 55);
            this.btnCancelAltServer.Name = "btnCancelAltServer";
            this.btnCancelAltServer.Size = new System.Drawing.Size(75, 23);
            this.btnCancelAltServer.TabIndex = 3;
            this.btnCancelAltServer.Text = "Abbrechen";
            this.btnCancelAltServer.UseVisualStyleBackColor = true;
            this.btnCancelAltServer.Click += new System.EventHandler(this.btnCancelAltServer_Click);
            // 
            // AlternativerMySQLServer
            // 
            this.AcceptButton = this.btnOKAltServer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelAltServer;
            this.ClientSize = new System.Drawing.Size(249, 90);
            this.Controls.Add(this.btnCancelAltServer);
            this.Controls.Add(this.btnOKAltServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbAlternativeServer);
            this.Name = "AlternativerMySQLServer";
            this.Text = "Server angeben";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOKAltServer;
        private System.Windows.Forms.Button btnCancelAltServer;
        public System.Windows.Forms.TextBox tbAlternativeServer;
    }
}