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
    public partial class SimpleDetailsDT : Form
    {
        private readonly DataTable sourceTable;
        private readonly string title;
        private readonly int colIndexForColors;

        public SimpleDetailsDT(DataTable sourceTable, string title, int colIndexForColors)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
            this.title = title;
            this.colIndexForColors = colIndexForColors;
        }

        private void SimpleDetailsDT_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = sourceTable;
            label1.Text = title;
            SMTOperations.autoSizeGridColumns(dataGridView1);
            MakeInterlacedColors();
        }

        private void MakeInterlacedColors()
        {
            Color rowColor = Color.LightBlue;
            if (colIndexForColors>=0)
            {
                for (int r = 1; r < dataGridView1.Rows.Count; r++) 
                {
                    if (dataGridView1.Rows[r].Cells[colIndexForColors].Value.ToString() != dataGridView1.Rows[r - 1].Cells[colIndexForColors].Value.ToString())
                    {
                        if (rowColor == Color.LightBlue)
                        {
                            rowColor = Color.White;
                        }
                        else
                        {
                            rowColor = Color.LightBlue;
                        }
                    }

                    foreach (DataGridViewCell cell in dataGridView1.Rows[r].Cells)
                    {
                        cell.Style.BackColor = rowColor;

                    }
                    
                    
                }
            }
        }
    }
}
