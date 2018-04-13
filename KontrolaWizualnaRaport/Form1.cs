using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        Dictionary<string, string> lotToSmtLine = new Dictionary<string, string>();
        List<excelOperations.order12NC> mstOrders = new List<excelOperations.order12NC>();
        private SQLoperations sqloperations;

        private void Form1_Load(object sender, EventArgs e)
        {
            mstOrders = excelOperations.loadExcel();
            masterTable = SQLoperations.DownloadFromSQL(45);
            textBox1.Text += "SQL table: " + masterTable.Rows.Count + " rows" + Environment.NewLine;
            comboBox1.Items.AddRange(CreateOperatorsList(masterTable).ToArray());

            inspectionData = dataLoader.LoadData(masterTable);
            lotModelDictionary = SQLoperations.LotList()[0];
            lotToOrderedQty = SQLoperations.LotList()[1];
            lotToSmtLine = SQLoperations.lotToSmtLine(30);

            string[] smtLines = lotToSmtLine.Select(l => l.Value).Distinct().OrderBy(o => o).ToArray();

            comboBoxPrzyczynySmtLine.Items.Add("Wszystkie");
            comboBoxPrzyczynySmtLine.Text = "Wszystkie";
            comboBoxPrzyczynySmtLine.Items.AddRange(smtLines);

            comboBoxPoziomOdpaduSmtLine.Items.Add("Wszystkie");
            comboBoxPoziomOdpaduSmtLine.Text = "Wszystkie";
            comboBoxPoziomOdpaduSmtLine.Items.AddRange(smtLines);

            comboBoxReasonSmtLine.Items.Add("Wszystkie");
            comboBoxReasonSmtLine.Text = "Wszystkie";
            comboBoxReasonSmtLine.Items.AddRange(smtLines);

            
            comboBoxModel.Items.AddRange(lotModelDictionary.Select(m => m.Value.Replace("LLFML","")).Distinct().OrderBy(o=>o).ToArray());

            dateTimePickerPrzyczynyOdpaduOd.Value = DateTime.Now.AddDays(-30);
            dateTimePickerWasteLevelBegin.Value = DateTime.Now.AddDays(-30);
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Wszyscy");

            dataGridViewDuplikaty.DataSource=  SzukajDuplikatow();
            ColumnsAutoSize(dataGridViewDuplikaty, DataGridViewAutoSizeColumnMode.AllCells);
            dataGridViewDuplikaty.Sort(dataGridViewDuplikaty.Columns[0], ListSortDirection.Descending);
            ColumnsAutoSize(dataGridViewDuplikaty, DataGridViewAutoSizeColumnMode.AllCells);

            dataGridViewPomylkiIlosc.DataSource = PomylkiIlosci();
            ColumnsAutoSize(dataGridViewPomylkiIlosc, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);

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

            comboBoxReasonAnalyses.Items.AddRange(uniqueWaste.ToArray());
            comboBox3.Items.AddRange(modelFamilyList(inspectionData, lotModelDictionary));
            comboBox4.Items.AddRange(uniqueModelsList(inspectionData, lotModelDictionary));

            dataGridView2.DataSource = UnknownOrderNumberTable();
            SMTOperations.shiftSummaryDataSource(SMTOperations.sortTableByDayAndShift(SQLoperations.GetSmtRecordsFromDbQuantityOnly(30)), dataGridView1);

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
            label6.Location = new Point( panel12.Location.X + 10, 20);
            panel14.Location = new Point(dataGridViewPowyzej50.Location.X + 80, 10);

            //tab analiza po przyczynie
            chartReasonLevel.Height = tabPage6.Height / 2;
            chartReasonPareto.Width = tabPage6.Width / 2;

            //tab analiza po modelu
            chartModelLevel.Height = tabPage7.Height / 2;
            chartModelReasonsNg.Width = panel13.Width / 2;
        }

        private DataTable UnknownOrderNumberTable()
        {
            DataTable result = new DataTable();
            result.Columns.Add("Data");
            result.Columns.Add("Operator");
            result.Columns.Add("Nr zlecenia");

            foreach (var record in inspectionData)
            {
                string model = "";
                if (lotModelDictionary.TryGetValue(record.NumerZlecenia, out model)) continue;
                result.Rows.Add(record.RealDateTime, record.Oper, record.NumerZlecenia);
            }
            return result;
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
                    label6.Location = new Point(panel12.Location.X + 50, 20);
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
        
        private string[] uniqueModelsList(List<dataStructure> inputData, Dictionary<string, string> lotModelDictionary)
        {
            HashSet<string> uniquemodels = new HashSet<string>();
            foreach (var item in inputData)
            {
                if (lotModelDictionary.ContainsKey(item.NumerZlecenia))
                    uniquemodels.Add(lotModelDictionary[item.NumerZlecenia]);
            }

            return uniquemodels.OrderBy(o=>o).ToArray();
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
                if (lotModelDictionary.ContainsKey(record.NumerZlecenia))
                {
                    if (record.AllNg >= ngThreshold)
                    {
                        result.Rows.Add(record.RealDateTime, record.Oper, lotModelDictionary[record.NumerZlecenia], record.NumerZlecenia, "NG", record.AllNg);
                    }
                    if (record.AllScrap >= scrapThreshold)
                    {
                        result.Rows.Add(record.RealDateTime, record.Oper, lotModelDictionary[record.NumerZlecenia], record.NumerZlecenia, "SCRAP", record.AllScrap);
                    }
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
           dataGridViewEffciency.DataSource = Charting.DrawCapaChart(chartEfficiency, inspectionData, comboBox1.Text, lotModelDictionary, radioButtonCapaLGI.Checked, mstOrders);
            foreach (DataGridViewColumn col in dataGridViewEffciency.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
        
        private void dateTimePickerPrzyczynyOdpaduOd_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource= Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, comboBoxPrzyczynySmtLine.Text,lotToSmtLine);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerPrzyczynyOdpaduDo_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, comboBoxPrzyczynySmtLine.Text,lotToSmtLine);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerWasteLevelBegin_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text,lotToSmtLine);
            ColumnsAutoSize(dataGridViewWasteLevel, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);

        }

        private void copyChartToClipboard(Chart chrt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chrt.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }

        private void radioButtonDaily_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text, lotToSmtLine);
            foreach (DataGridViewColumn col in dataGridViewWasteLevel.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void comboBoxModel_TextChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text, lotToSmtLine);
        }

        private void dateTimePickerWasteLevelEnd_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text, lotToSmtLine);
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
                    string model = "nieznany";
                    lotModelDictionary.TryGetValue(record.NumerZlecenia, out model);
                    //gridSource.Rows.Add("Model", lotModelDictionary[record.NumerZlecenia]);
                    string orderQty = "nieznane";
                    lotToOrderedQty.TryGetValue(record.NumerZlecenia, out orderQty);
                    //gridSource.Rows.Add("Ordered Qty", lotToOrderedQty[record.NumerZlecenia]);

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
            Charting.DrawWasteLevelPerReason(chartReasonLevel, "all",  inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text,lotToSmtLine);
            Charting.DrawWasteParetoPerReason(chartReasonPareto, chartReasonsParetoPercentage, inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBox3.Text);
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBox3.Text);
            comboBox4.Text = "";
        }


        string currentReasonOnChart = "";
        private void chartModelReasons_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartModelReasonsNg.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);
            string model = comboBox3.Text + comboBox4.Text;
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

                        
                        Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, model);
                        currentReasonOnChart = "all";
                    }
                    continue;
                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerModel(chartModelLevel, p.AxisLabel, inspectionData, lotModelDictionary, model);
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

                        Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerReason(chartReasonLevel, p.AxisLabel, inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
                    currentReasonOnChart = p.AxisLabel;
                    //Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void chartReasonsParetoPercentage_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartReasonsParetoPercentage.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartReasonsParetoPercentage.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerReason(chartReasonLevel, p.AxisLabel, inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
                    currentReasonOnChart = p.AxisLabel;
                    //Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBox4.Text);
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBox4.Text);
            comboBox3.Text = "";
        }

        private void chartPrzyczynyOdpaduNg_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                copyChartToClipboard(chartPrzyczynyOdpaduNg);
            }
        }

        private void chartWasteLevel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                copyChartToClipboard(chartWasteLevel);
            }
        }

        private void chartEfficiency_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                copyChartToClipboard(chartEfficiency);
            }
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            foreach (var line in richTextBox1.Lines)
            {
                ListViewItem itm = new ListViewItem();
                itm.Name = line.Trim();
                itm.Text = line.Trim();
                if (lotToSmtLine.ContainsKey(line))
                    itm.ForeColor = Color.Red;

                listView1.Items.Add(itm);
            }
        }



        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                textBox2.Text = listView1.Items[listView1.SelectedItems[0].Index].Name;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            DataTable dt = (DataTable)cell.Tag;
            if (dt!=null)
            {
                SmtShiftDetails detailsForm = new SmtShiftDetails(dt);
                detailsForm.Show();
            }
        }

        private void comboBoxSmtLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, comboBoxPrzyczynySmtLine.Text, lotToSmtLine);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void comboBoxPoziomOdpaduSmtLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text, lotToSmtLine);
            ColumnsAutoSize(dataGridViewWasteLevel, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);
        }

        private void comboBoxReasonSmtLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
            Charting.DrawWasteParetoPerReason(chartReasonPareto, chartReasonsParetoPercentage, inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
        }

        private void radioButtonCapaLGI_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewEffciency.DataSource = Charting.DrawCapaChart(chartEfficiency, inspectionData, comboBox1.Text, lotModelDictionary, radioButtonCapaLGI.Checked, mstOrders);
            foreach (DataGridViewColumn col in dataGridViewEffciency.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
    }
    }
