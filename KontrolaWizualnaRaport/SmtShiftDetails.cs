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
        private readonly string description;

        public SmtShiftDetails(DataTable dtSource, string description)
        {
            InitializeComponent();
            this.dtSource = dtSource;
            this.description = description;
        }

        private void SmtShiftDetails_Load(object sender, EventArgs e)
        {
            dataGridViewShiftDetails.DataSource = dtSource;
            if (dtSource.Columns.Count > 2 & dtSource.Columns.Contains("model"))
            {
                Dictionary<string, double> qtyPerModel = new Dictionary<string, double>();
                double totalQty = 0;
                foreach (DataRow row in dtSource.Rows)
                {
                    string model = row["model"].ToString();
                    if (!qtyPerModel.ContainsKey(model))
                    {
                        qtyPerModel.Add(model, 0);
                    }

                    double qty = double.Parse(row["Ilosc"].ToString());

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
                dataGridViewSummary.Sort(dataGridViewSummary.Columns["Ilosc"], ListSortDirection.Descending);

                SMTOperations.autoSizeGridColumns(dataGridViewSummary);
            }
            else
            {
                double sum = 0;
                foreach (DataRow row in dtSource.Rows)
                {
                    sum += double.Parse(row["Ilosc"].ToString());
                }
                dtSource.Rows.Add("Razem", sum);
            }

            SMTOperations.autoSizeGridColumns(dataGridViewShiftDetails);
            label1.Text = description;
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
