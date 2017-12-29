using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            sqloperations = new SQLoperations(this, textBox1);
        }

        DataTable masterTable = new DataTable();
        List<dataStructure> inspectionData = new List<dataStructure>();
        Dictionary<string, string> lotModelDictionary = new Dictionary<string, string>();
        Dictionary<string, string> lotToOrderedQty = new Dictionary<string, string>();
        private SQLoperations sqloperations;

        private void Form1_Load(object sender, EventArgs e)
        {
            masterTable = SQLoperations.DownloadFromSQL();
            textBox1.Text += "SQL table: " + masterTable.Rows.Count + " rows" + Environment.NewLine;
            comboBox1.Items.AddRange(CreateOperatorsList(masterTable).ToArray());

            inspectionData = dataLoader.LoadData(masterTable);
            lotModelDictionary = SQLoperations.LotList()[0];
            lotToOrderedQty = SQLoperations.LotList()[1];

            comboBoxModel.Items.AddRange(lotModelDictionary.Select(m => m.Value.Replace("LLFML","")).Distinct().OrderBy(o=>o).ToArray());

            dateTimePickerPrzyczynyOdpaduOd.Value = DateTime.Now.AddDays(-30);
            dateTimePickerWasteLevelBegin.Value = DateTime.Now.AddDays(-30);
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Wszyscy");

            dataGridViewDuplikaty.DataSource=  SzukajDuplikatow();
            dataGridViewDuplikaty.Sort(dataGridViewDuplikaty.Columns[0], ListSortDirection.Descending);
            ColumnsAutoSize(dataGridViewDuplikaty, DataGridViewAutoSizeColumnMode.AllCells);

            dataGridViewPomylkiIlosc.DataSource = PomylkiIlosci();
            ColumnsAutoSize(dataGridViewPomylkiIlosc, DataGridViewAutoSizeColumnMode.AllCells);

            dataGridView2.DataSource = MoreThan50();
            ColumnsAutoSize(dataGridView2, DataGridViewAutoSizeColumnMode.AllCells);
            dataGridView2.Sort(dataGridView2.Columns["NG"], ListSortDirection.Descending);

            PropertyInfo[] properties = typeof(dataStructure).GetProperties();
            HashSet<string> uniqueWaste = new HashSet<string>();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.StartsWith("Ng") || property.Name.StartsWith("Scrap"))
                {
                    uniqueWaste.Add(property.Name.Replace("Ng", "").Replace("Scrap", ""));
                }
                
            }
            comboBox2.Items.AddRange(uniqueWaste.ToArray());
            comboBox3.Items.AddRange(modelFamilyList(inspectionData, lotModelDictionary));

        }

        private string[] modelFamilyList(List<dataStructure> inputData,  Dictionary<string, string> lotModelDictionary)
        {

            HashSet<string> uniquemodels = new HashSet<string>();
            foreach (var item in inputData)
            {
                if (lotModelDictionary.ContainsKey(item.NumerZlecenia))
                uniquemodels.Add(lotModelDictionary[item.NumerZlecenia].Substring(0,6));
            }

            return uniquemodels.ToList().OrderBy(o => o).ToArray();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            chart1.Width = this.Width - panel1.Width;
            dataGridView1.Height = panel1.Height - comboBox1.Height;
            chartPrzyczynyOdpaduScrap.Height = tabPage2.Height / 2;
            dataGridViewDuplikaty.Width = this.Width / 3;
            dataGridViewPomylkiIlosc.Width = this.Width / 3;
            chartReasonLevel.Height = tabPage6.Height / 2;
            chartModelLevel.Height = tabPage7.Height / 2;

            label1.Location = new Point(dataGridViewDuplikaty.Location.X + 100,20);
            label2.Location = new Point(dataGridViewPomylkiIlosc.Location.X + 100,20);
            label3.Location = new Point(dataGridView2.Location.X + 100, 20);
        }

        private DataTable MoreThan50()
        {
            DataTable result = new DataTable();
            result.Columns.Add("Data");
            result.Columns.Add("Operator");
            result.Columns.Add("Model");
            result.Columns.Add("LOT");
            result.Columns.Add("NG", typeof (int));

            foreach (var record in inspectionData)
            {
                if (record.AllNg > 15) 
                {
                    result.Rows.Add(record.RealDateTime, record.Oper, lotModelDictionary[record.NumerZlecenia],record.NumerZlecenia, record.AllNg);
                }
            }

            return result;

        }

        private void ColumnsAutoSize(DataGridView grid, DataGridViewAutoSizeColumnMode mode)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = mode;
            }
        }

        private DataTable PomylkiIlosci()
        {
            DataTable result = new DataTable();
            result.Columns.Add("Numer zlecenia");
            result.Columns.Add("Operator");
            result.Columns.Add("Data");
            
            result.Columns.Add("NG");
            result.Columns.Add("Wszystkie");
            result.Columns.Add("Zlecone");
            result.Columns.Add("Różnica");

            foreach (var record in inspectionData)
            {

                string orderedQty = "";
                lotToOrderedQty.TryGetValue(record.NumerZlecenia, out orderedQty);
                int orderedQtyInt = 0;
                int allQty = 0;
                int.TryParse(orderedQty, out orderedQtyInt);
                int.TryParse(record.AllQty.ToString(), out allQty);



                if ((allQty>0 & orderedQtyInt>0) & (allQty>orderedQtyInt))
                {
                    result.Rows.Add(record.NumerZlecenia, record.Oper, record.RealDateTime, record.AllNg, record.AllQty, orderedQty,(allQty-orderedQtyInt).ToString() );
                }

            }


            return result;
        }

        private DataTable SzukajDuplikatow()
        {
            DataTable result = new DataTable();
            result.Columns.Add("Numer zlecenia");
            result.Columns.Add("Operator");
            result.Columns.Add("Data");
            result.Columns.Add("Dobrych");
            result.Columns.Add("NG");

            var duplicateKeys = inspectionData.GroupBy(x => x.NumerZlecenia)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key).ToList();

            foreach (var record in inspectionData)
            {
                if (duplicateKeys.Contains(record.NumerZlecenia))
                {
                    result.Rows.Add(record.NumerZlecenia, record.Oper, record.RealDateTime,record.GoodQty,(record.AllNg).ToString());
                }
            }

            return result;
        }

        private List<string> CreateOperatorsList(DataTable inputTable)
        {
            HashSet<string> result = new HashSet<string>();
            result.Add("Wszyscy");
            foreach (DataRow row in inputTable.Rows)
            {
                result.Add(row["Operator"].ToString());
            }

            return result.OrderBy(o=>o).ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           dataGridView1.DataSource = Charting.DrawCapaChart(chart1, inspectionData, comboBox1.Text, lotModelDictionary);
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
        
        private void dateTimePickerPrzyczynyOdpaduOd_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource= Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerPrzyczynyOdpaduDo_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerWasteLevelBegin_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel);
            ColumnsAutoSize(dataGridViewWasteLevel, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);

        }

        private void radioButtonDaily_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel);
            foreach (DataGridViewColumn col in dataGridViewWasteLevel.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void comboBoxModel_TextChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel);
        }

        private void dateTimePickerWasteLevelEnd_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel);
            ColumnsAutoSize(dataGridViewWasteLevel, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            DataTable gridSource = new DataTable();
            gridSource.Columns.Add("Pole");
            gridSource.Columns.Add("Wartość");

            foreach (var record in inspectionData)
            {
                if (record.NumerZlecenia==textBox2.Text)
                {
                    gridSource.Rows.Add("Model", lotModelDictionary[record.NumerZlecenia]);
                    gridSource.Rows.Add("Ordered Qty", lotToOrderedQty [record.NumerZlecenia]);

                    PropertyInfo[] properties = typeof(dataStructure).GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        string name = property.Name;
                        string value = property.GetValue(record, null).ToString();
                        gridSource.Rows.Add(name, value);
                    }
                    break;
                }
            }
            dataGridView3.DataSource = gridSource;
            ColumnsAutoSize(dataGridView3, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawScrapPerReason(chartReasonLevel, chartReasonPareto, inspectionData, comboBox2.Text, lotModelDictionary);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWastePerModel(chartModelLevel, chartModelReasons, inspectionData, lotModelDictionary, comboBox3.Text);
        }
    }
}
