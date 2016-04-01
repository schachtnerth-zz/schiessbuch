namespace schiessbuch
{
    partial class NeuesSchiessjahr
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
            this.label1 = new System.Windows.Forms.Label();
            this.SchiessjahrTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SchiessjahrBeginn = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(258, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bezeichnung für neues Schießjahr (z. B. 2016/2017):";
            // 
            // SchiessjahrTextbox
            // 
            this.SchiessjahrTextbox.Location = new System.Drawing.Point(15, 25);
            this.SchiessjahrTextbox.Name = "SchiessjahrTextbox";
            this.SchiessjahrTextbox.Size = new System.Drawing.Size(200, 20);
            this.SchiessjahrTextbox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Beginn:";
            // 
            // SchiessjahrBeginn
            // 
            this.SchiessjahrBeginn.Location = new System.Drawing.Point(15, 64);
            this.SchiessjahrBeginn.Name = "SchiessjahrBeginn";
            this.SchiessjahrBeginn.Size = new System.Drawing.Size(200, 20);
            this.SchiessjahrBeginn.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 115);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "&Anlegen";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(207, 115);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Abbrechen";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // NeuesSchiessjahr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 161);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SchiessjahrBeginn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SchiessjahrTextbox);
            this.Controls.Add(this.label1);
            this.Name = "NeuesSchiessjahr";
            this.Text = "Neues Schießjahr beginnen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SchiessjahrTextbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker SchiessjahrBeginn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}