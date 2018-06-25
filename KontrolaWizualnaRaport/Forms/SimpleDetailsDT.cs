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

        public SimpleDetailsDT(DataTable sourceTable, string title)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
            this.title = title;
        }

        private void SimpleDetailsDT_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = sourceTable;
            label1.Text = title;
            SMTOperations.autoSizeGridColumns(dataGridView1);
        }
    }
}
