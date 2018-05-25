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
    public partial class WasteReasonDetails : Form
    {
        private readonly DataTable sourceTable;
        private readonly string title;

        public WasteReasonDetails(DataTable sourceTable, string title)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
            this.title = title;
        }

        private void WasteReasonDetails_Load(object sender, EventArgs e)
        {
            DataTable dtCloned = sourceTable.Clone();
            dtCloned.Columns["Ilość"].DataType = typeof(Int32);
            Dictionary<string, Int32> qtyPerModel = new Dictionary<string, int>();
            Dictionary<string, Int32> qtyPerLine = new Dictionary<string, int>();
            foreach (DataRow row in sourceTable.Rows)
            {
                dtCloned.ImportRow(row);

                string model = row["Model"].ToString();
                string line = row["Linia"].ToString();

                if (!qtyPerLine.ContainsKey(line)) qtyPerLine.Add(line, 0);
                if (!qtyPerModel.ContainsKey(model)) qtyPerModel.Add(model, 0);

                Int32 qty = Int32.Parse(row["Ilość"].ToString());
                qtyPerLine[line] += qty;
                qtyPerModel[model] += qty;
            }
            DataView dv = dtCloned.DefaultView;
            dv.Sort = "Ilość desc";
            dataGridView1.DataSource = dv.ToTable();

            
            label1.Text = title;

            DataTable modelSource = new DataTable();
            modelSource.Columns.Add("Model");
            modelSource.Columns.Add("Ilość", typeof (Int32));

            DataTable lineSource = new DataTable();
            lineSource.Columns.Add("Linia");
            lineSource.Columns.Add("Ilość", typeof(Int32));

            foreach (var modelEntry in qtyPerModel)
            {
                modelSource.Rows.Add(modelEntry.Key, modelEntry.Value);
            }

            foreach (var lineEntry in qtyPerLine)
            {
                lineSource.Rows.Add(lineEntry.Key, lineEntry.Value);
            }

            dataGridViewModel.DataSource = modelSource;
            dataGridViewLine.DataSource = lineSource;

            SMTOperations.autoSizeGridColumns(dataGridView1);
            SMTOperations.autoSizeGridColumns(dataGridViewLine);
            SMTOperations.autoSizeGridColumns(dataGridViewModel);

            dataGridViewLine.Sort(this.dataGridViewLine.Columns["Ilość"], ListSortDirection.Descending);
            dataGridViewModel.Sort(this.dataGridViewModel.Columns["Ilość"], ListSortDirection.Descending);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
