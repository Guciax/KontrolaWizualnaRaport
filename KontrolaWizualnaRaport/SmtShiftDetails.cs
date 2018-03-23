using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public partial class SmtShiftDetails : Form
    {
        private readonly DataTable dtSource;

        public SmtShiftDetails(DataTable dtSource)
        {
            InitializeComponent();
            this.dtSource = dtSource;
        }

        private void SmtShiftDetails_Load(object sender, EventArgs e)
        {
            dataGridViewShiftDetails.DataSource = dtSource;

            Dictionary<string, double> qtyPerModel = new Dictionary<string, double>();
            double totalQty=0;
            foreach (DataRow row in dtSource.Rows)
            {
                string model = row["model"].ToString();
                if (!qtyPerModel.ContainsKey(model))
                {
                    qtyPerModel.Add(model, 0);
                }

                double qty = double.Parse(row["IloscWykonana"].ToString());

                qtyPerModel[model] += qty;
                totalQty += qty;
            }

            dataGridViewSummary.Columns.Add("Model", "Model");
            dataGridViewSummary.Columns.Add("Ilosc", "Ilosc");
            foreach (var modelEntry in qtyPerModel)
            {
                dataGridViewSummary.Rows.Add(modelEntry.Key, modelEntry.Value);
            }
            dataGridViewSummary.Rows.Add("Razem", totalQty);
            SMTOperations.autoSizeGridColumns(dataGridViewSummary);
            SMTOperations.autoSizeGridColumns(dataGridViewShiftDetails);
            
        }

        private void dataGridViewShiftDetails_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }
    }
}
