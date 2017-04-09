namespace schiessbuch
{
    partial class Vereinsmeisterübersicht
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
            this.VereinsmeisterDGV = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.VereinsmeisterDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // VereinsmeisterDGV
            // 
            this.VereinsmeisterDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.VereinsmeisterDGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VereinsmeisterDGV.Location = new System.Drawing.Point(0, 0);
            this.VereinsmeisterDGV.Name = "VereinsmeisterDGV";
            this.VereinsmeisterDGV.Size = new System.Drawing.Size(284, 261);
            this.VereinsmeisterDGV.TabIndex = 0;
            // 
            // Vereinsmeisterübersicht
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.VereinsmeisterDGV);
            this.Name = "Vereinsmeisterübersicht";
            this.Text = "Vereinsmeisterübersicht";
            ((System.ComponentModel.ISupportInitialize)(this.VereinsmeisterDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView VereinsmeisterDGV;
    }
}