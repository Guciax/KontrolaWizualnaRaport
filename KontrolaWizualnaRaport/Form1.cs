using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
            ColumnsAutoSize(dataGridViewDuplikaty, DataGridViewAutoSizeColumnMode.AllCells);
            dataGridViewDuplikaty.Sort(dataGridViewDuplikaty.Columns[0], ListSortDirection.Descending);
            ColumnsAutoSize(dataGridViewDuplikaty, DataGridViewAutoSizeColumnMode.AllCells);

            dataGridViewPomylkiIlosc.DataSource = PomylkiIlosci();
            ColumnsAutoSize(dataGridViewPomylkiIlosc, DataGridViewAutoSizeColumnMode.AllCells);

            dataGridViewPowyzej50.DataSource = MoreThan50();
            ColumnsAutoSize(dataGridViewPowyzej50, DataGridViewAutoSizeColumnMode.AllCells);
            dataGridViewPowyzej50.Sort(dataGridViewPowyzej50.Columns["Ile"], ListSortDirection.Descending);

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

        private void Form1_Resize(object sender, EventArgs e)
        {
            //tab wydajnosc
            chartEfficiency.Width = this.Width - panel1.Width;
            dataGridViewEffciency.Height = panel1.Height - comboBox1.Height;

            //tab przyczyny odpadu
            chartPrzyczynyOdpaduScrap.Height = tabPage2.Height / 2;

            //tab bledy
            panel11.Width = this.Width / 2;
            dataGridViewDuplikaty.Width = panel11.Width / 2;
            dataGridViewPomylkiIlosc.Width = panel12.Width / 2;
            label1.Location = new Point(dataGridViewDuplikaty.Location.X + 100, 20);
            label2.Location = new Point(dataGridViewPomylkiIlosc.Location.X + panel12.Location.X + 100, 20);
            panel14.Location = new Point(dataGridViewPowyzej50.Location.X + 80, 10);

            //tab analiza po przyczynie
            chartReasonLevel.Height = tabPage6.Height / 2;
            chartReasonPareto.Width = tabPage6.Width / 2;

            //tab analiza po modelu
            chartModelLevel.Height = tabPage7.Height / 2;
            chartModelReasonsNg.Width = panel13.Width / 2;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0://tab wydajnosc
                    chartEfficiency.Width = this.Width - panel1.Width;
                    dataGridViewEffciency.Height = panel1.Height - comboBox1.Height;
                    break;
                case 1://tab przyczyny odpadu
                    chartPrzyczynyOdpaduScrap.Height = tabPage2.Height / 2;
                    break;
                case 3://tab bledy
                    panel11.Width = this.Width / 2;
                    dataGridViewDuplikaty.Width = panel11.Width / 2;
                    dataGridViewPomylkiIlosc.Width = panel12.Width / 2;
                    label1.Location = new Point(dataGridViewDuplikaty.Location.X + 100, 20);
                    label2.Location = new Point(dataGridViewPomylkiIlosc.Location.X + panel12.Location.X+ 100, 20);
                    panel14.Location = new Point(dataGridViewPowyzej50.Location.X + 100, 20);
                    break;
                case 5://tab analiza po przyczynie
                    chartReasonLevel.Height = tabPage6.Height / 2;
                    chartReasonPareto.Width = tabPage6.Width / 2;
                    break;
                case 6://tab analiza po modelu
                    chartModelLevel.Height = tabPage7.Height / 2;
                    chartModelReasonsNg.Width = panel13.Width / 2;
                    break;
            }
            
        }

        private DataTable LotWrongNumber(List<dataStructure> inputData)
        {
            DataTable result = new DataTable();
            result.Columns.Add("LOT");
            result.Columns.Add("Operator");
            result.Columns.Add("Data");

            foreach (var record in inputData)
            {
                if (lotModelDictionary.ContainsKey(record.NumerZlecenia)) continue;
                result.Rows.Add(record.NumerZlecenia, record.Oper, record.RealDateTime);
            }

            return result;
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

        

        private DataTable MoreThan50()
        {
            DataTable result = new DataTable();
            result.Columns.Add("Data");
            result.Columns.Add("Operator");
            result.Columns.Add("Model");
            result.Columns.Add("LOT");
            result.Columns.Add("Typ");
            result.Columns.Add("Ile", typeof (int));
            decimal ngThreshold = numericUpDown1.Value;
            decimal scrapThreshold = numericUpDown2.Value;

            foreach (var record in inspectionData)
            {
                if (record.AllNg >= ngThreshold) 
                {
                    result.Rows.Add(record.RealDateTime, record.Oper, lotModelDictionary[record.NumerZlecenia],record.NumerZlecenia,"NG", record.AllNg);
                }
                if (record.AllScrap >= scrapThreshold) 
                {
                    result.Rows.Add(record.RealDateTime, record.Oper, lotModelDictionary[record.NumerZlecenia], record.NumerZlecenia,"SCRAP", record.AllScrap);
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
           dataGridViewEffciency.DataSource = Charting.DrawCapaChart(chartEfficiency, inspectionData, comboBox1.Text, lotModelDictionary);
            foreach (DataGridViewColumn col in dataGridViewEffciency.Columns)
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
            Charting.DrawWasteLevelPerReason(chartReasonLevel, chartReasonPareto,"all", chartReasonsParetoPercentage, inspectionData, comboBox2.Text, lotModelDictionary);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBox3.Text);
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBox3.Text);
        }


        string currentReasonOnChart = "";
        private void chartModelReasons_MouseMove(object sender, MouseEventArgs e)
        {
            

            var results = chartModelReasonsNg.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartModelReasonsNg.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBox3.Text);
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerModel(chartModelLevel, p.AxisLabel, inspectionData, lotModelDictionary, comboBox3.Text);
                    currentReasonOnChart = p.AxisLabel;
                    Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void chartModelReasonsScrap_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartModelReasonsScrap.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartModelReasonsScrap.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBox3.Text);
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerModel(chartModelLevel, p.AxisLabel, inspectionData, lotModelDictionary, comboBox3.Text);
                    currentReasonOnChart = p.AxisLabel;
                    Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewPowyzej50.DataSource = MoreThan50();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewPowyzej50.DataSource = MoreThan50();
        }

        private void chartReasonPareto_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartReasonPareto.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartReasonPareto.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerReason(chartReasonLevel, chartReasonPareto, "all", chartReasonsParetoPercentage, inspectionData, comboBox2.Text, lotModelDictionary);
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerReason(chartReasonLevel, chartReasonPareto, p.AxisLabel, chartReasonsParetoPercentage, inspectionData, comboBox2.Text, lotModelDictionary);
                    currentReasonOnChart = p.AxisLabel;
                    //Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }
    }
    }
