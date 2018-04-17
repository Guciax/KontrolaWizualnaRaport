using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport
{
    public partial class Drukuj : Form
    {
        private  Chart chart;
        Chart chart2 = new Chart();

        public Drukuj(Chart chart)
        {
            InitializeComponent();
            this.chart = chart;
            this.Visible = false;
        }

        private void Drukuj_Load(object sender, EventArgs e)
        {
            System.IO.MemoryStream myStream = new System.IO.MemoryStream();
            
            chart.Serializer.Save(myStream);
            chart2.Serializer.Load(myStream);

            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;

            chart2.Width = pd.DefaultPageSettings.PaperSize.Height;
            chart2.Height = pd.DefaultPageSettings.PaperSize.Width-50;

            pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);

            PrintDialog printdlg = new PrintDialog();
            PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();

            // preview the assigned document or you can create a different previewButton for it
            printPrvDlg.Document = pd;
            printPrvDlg.ShowDialog(); // this shows the preview and then show the Printer Dlg below

            printdlg.Document = pd;

            if (printdlg.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);

            PrintDialog printdlg = new PrintDialog();
            PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();

            // preview the assigned document or you can create a different previewButton for it
            printPrvDlg.Document = pd;
            printPrvDlg.ShowDialog(); // this shows the preview and then show the Printer Dlg below

            printdlg.Document = pd;

            if (printdlg.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Single yPos = 0;
            Single leftMargin = e.MarginBounds.Left;
            Single topMargin = e.MarginBounds.Top;
            Image img = Form1.chartToBitmap(chart2);

            using (Font printFont = new Font("Arial", 20.0f))
            {
                e.Graphics.DrawImage(img, new Point(5,55));
                e.Graphics.DrawRectangle(new Pen(Color.Black), new Rectangle(5, 5, e.PageBounds.Width - e.MarginBounds.Left - e.MarginBounds.Right, 10));
                e.Graphics.DrawString("Header", printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
            }

        }
    }
}
