namespace KontrolaWizualnaRaport
{
    partial class Drukuj
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
            this.comboBoxPrintersList = new System.Windows.Forms.ComboBox();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxPrintersList
            // 
            this.comboBoxPrintersList.FormattingEnabled = true;
            this.comboBoxPrintersList.Location = new System.Drawing.Point(12, 39);
            this.comboBoxPrintersList.Name = "comboBoxPrintersList";
            this.comboBoxPrintersList.Size = new System.Drawing.Size(271, 21);
            this.comboBoxPrintersList.TabIndex = 0;
            // 
            // buttonPrint
            // 
            this.buttonPrint.Location = new System.Drawing.Point(208, 115);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(75, 23);
            this.buttonPrint.TabIndex = 1;
            this.buttonPrint.Text = "Drukuj";
            this.buttonPrint.UseVisualStyleBackColor = true;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Drukarka:";
            // 
            // Drukuj
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 168);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.comboBoxPrintersList);
            this.Name = "Drukuj";
            this.Text = "Drukuj";
            this.Load += new System.EventHandler(this.Drukuj_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxPrintersList;
        private System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.Label label1;
    }
}