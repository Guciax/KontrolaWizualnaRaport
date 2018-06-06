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
    public partial class LedWasteDetails : Form
    {
        private readonly DataTable sourceTable;
        private readonly string title;

        public LedWasteDetails(DataTable sourceTable, string title)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
            this.title = title;
        }

        private void LedWasteDetails_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            double usedA = 0;
            double usedB = 0;
            double droppedA = 0;
            double droppedB = 0;

            for (int i=1;i< sourceTable.Rows.Count;i++)
            {
                usedA += double.Parse(sourceTable.Rows[i]["Mont.A"].ToString());
                usedB += double.Parse(sourceTable.Rows[i]["Mont.B"].ToString());
                droppedA += double.Parse(sourceTable.Rows[i]["Odp_A"].ToString());
                droppedB += double.Parse(sourceTable.Rows[i]["Odp_B"].ToString());
            }

            labelTitle.Text = sourceTable.Rows[0][0].ToString() + Environment.NewLine;
            labelTitle.Text += "odpad A=" + Math.Round(droppedA / usedA * 100, 2) + "% odpad B=" + Math.Round(droppedB / usedB * 100, 2) + "%";

            sourceTable.Rows.RemoveAt(0);
            Charting.DrawLedWasteForDetailView(sourceTable, chartLedWasteDetails);
            dataGridView1.DataSource = sourceTable;
            sourceTable.Rows.Add("Total", "", "","", usedA, droppedA, usedB, droppedB);
            
            SMTOperations.autoSizeGridColumns(dataGridView1);
        }
    }
}
