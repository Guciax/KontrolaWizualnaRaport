namespace KontrolaWizualnaRaport
{
    partial class SmtShiftDetails
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewSummary = new System.Windows.Forms.DataGridView();
            this.dataGridViewShiftDetails = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSummary)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShiftDetails)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1079, 61);
            this.panel1.TabIndex = 0;
            // 
            // dataGridViewSummary
            // 
            this.dataGridViewSummary.AllowUserToAddRows = false;
            this.dataGridViewSummary.AllowUserToDeleteRows = false;
            this.dataGridViewSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSummary.Dock = System.Windows.Forms.DockStyle.Right;
            this.dataGridViewSummary.Location = new System.Drawing.Point(871, 61);
            this.dataGridViewSummary.Name = "dataGridViewSummary";
            this.dataGridViewSummary.Size = new System.Drawing.Size(208, 660);
            this.dataGridViewSummary.TabIndex = 1;
            // 
            // dataGridViewShiftDetails
            // 
            this.dataGridViewShiftDetails.AllowUserToAddRows = false;
            this.dataGridViewShiftDetails.AllowUserToDeleteRows = false;
            this.dataGridViewShiftDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewShiftDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewShiftDetails.Location = new System.Drawing.Point(0, 61);
            this.dataGridViewShiftDetails.Name = "dataGridViewShiftDetails";
            this.dataGridViewShiftDetails.Size = new System.Drawing.Size(871, 660);
            this.dataGridViewShiftDetails.TabIndex = 2;
            this.dataGridViewShiftDetails.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dataGridViewShiftDetails_RowPostPaint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(25, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nazwa";
            // 
            // SmtShiftDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1079, 721);
            this.Controls.Add(this.dataGridViewShiftDetails);
            this.Controls.Add(this.dataGridViewSummary);
            this.Controls.Add(this.panel1);
            this.Name = "SmtShiftDetails";
            this.Text = "SmtShiftDetails";
            this.Load += new System.EventHandler(this.SmtShiftDetails_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSummary)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShiftDetails)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridViewSummary;
        private System.Windows.Forms.DataGridView dataGridViewShiftDetails;
        private System.Windows.Forms.Label label1;
    }
}