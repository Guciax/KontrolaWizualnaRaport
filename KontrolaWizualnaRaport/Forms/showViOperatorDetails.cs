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
    public partial class showViOperatorDetails : Form
    {
        private readonly DataTable sourceTable;

        public showViOperatorDetails(DataTable sourceTable)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
        }

        private void showViOperatorDetails_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = sourceTable;

            Color rowCol = Color.LightBlue;
            
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = rowCol;
                }

                
                if (row.Cells["Data"].Value != null)
                {
                    if (row.Cells["Data"].Value.ToString() != "")
                    {
                        if (rowCol == Color.LightBlue)
                        {
                            rowCol = Color.White;
                        }
                        else
                        {
                            rowCol = Color.LightBlue;
                        }
                    }
                }

                
            }

            SMTOperations.autoSizeGridColumns(dataGridView1);
        }
    }
}
