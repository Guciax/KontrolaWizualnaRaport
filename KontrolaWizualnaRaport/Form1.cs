using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            sqloperations = new SQLoperations(this, textBox1);
        }

        DataTable masterVITable = new DataTable();
        List<dataStructure> inspectionData = new List<dataStructure>();
        Dictionary<string, string> lotModelDictionary = new Dictionary<string, string>();
        Dictionary<string, string> lotToOrderedQty = new Dictionary<string, string>();
        Dictionary<string, string> lotToSmtLine = new Dictionary<string, string>();
        List<excelOperations.order12NC> mstOrders = new List<excelOperations.order12NC>();
        private SQLoperations sqloperations;
        Dictionary<string, Dictionary<string, List<durationQuantity>>> smtModelLineQuantity = new Dictionary<string, Dictionary<string, List<durationQuantity>>>();
        DataTable lotTable = new DataTable();

        private void Form1_Load(object sender, EventArgs e)
        {
            lotTable = SQLoperations.lotTable();


        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl2.SelectedTab.Text)
            {
                case "SMT":
                    {
                        if (smtModelLineQuantity.Count < 1)
                        {
                            DataTable smtRecords = SQLoperations.GetSmtRecordsFromDbQuantityOnly(90);
                            SMTOperations.shiftSummaryDataSource(SMTOperations.sortTableByDayAndShift(smtRecords, "DataCzasKoniec"), dataGridViewSmtProduction);

                            smtModelLineQuantity = SMTOperations.smtQtyPerModelPerLine(smtRecords);
                            comboBoxSmtModels.Items.AddRange(smtModelLineQuantity.Select(m => m.Key).OrderBy(m => m).ToArray());
                        }
                            break;
                    }
                case "KITTING":
                    {
                        if (dataGridViewKitting.Rows.Count == 0)
                        {
                            KittingOperations.FillGrid(lotTable, dataGridViewKitting);
                        }
                        break;
                    }
                case "SPLITTING":
                    {
                        if (dataGridViewSplitting.Rows.Count == 0)
                        {
                            SplittingOperations.FillGrid(lotTable, dataGridViewSplitting);
                        }
                        break;
                    }
                case "KONTROLA WZROKOWA":
                    {
                        if (inspectionData.Count < 1)
                        {
                            mstOrders = excelOperations.loadExcel();
                            if (masterVITable.Rows.Count < 1)
                            {
                                masterVITable = SQLoperations.DownloadVisInspFromSQL(45);
                            }
                            //textBox1.Text += "SQL table: " + masterVITable.Rows.Count + " rows" + Environment.NewLine;
                            comboBox1.Items.AddRange(CreateOperatorsList(masterVITable).ToArray());

                            inspectionData = dataLoader.LoadData(masterVITable);
                            Dictionary<string, string>[] lotList = VIOperations.lotArray(lotTable);
                            lotModelDictionary = lotList[0];
                            lotToOrderedQty = lotList[1];
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

                            comboBoxModel.Items.AddRange(lotModelDictionary.Select(m => m.Value.Replace("LLFML", "")).Distinct().OrderBy(o => o).ToArray());

                            dateTimePickerPrzyczynyOdpaduOd.Value = DateTime.Now.AddDays(-30);
                            dateTimePickerWasteLevelBegin.Value = DateTime.Now.AddDays(-30);
                            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("Wszyscy");

                            dataGridViewDuplikaty.DataSource = SzukajDuplikatow();
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
                        }
                        break;
                        
                    }
            }

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
            label6.Location = new Point(panel12.Location.X + 10, 20);
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
                    label2.Location = new Point(dataGridViewPomylkiIlosc.Location.X + panel12.Location.X + 100, 20);
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

            return uniquemodels.OrderBy(o => o).ToArray();
        }

        private string[] modelFamilyList(List<dataStructure> inputData, Dictionary<string, string> lotModelDictionary)
        {

            HashSet<string> uniquemodels = new HashSet<string>();
            foreach (var item in inputData)
            {
                if (lotModelDictionary.ContainsKey(item.NumerZlecenia))
                    uniquemodels.Add(lotModelDictionary[item.NumerZlecenia].Substring(0, 6));
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
            result.Columns.Add("Ile", typeof(int));
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



                if ((allQty > 0 & orderedQtyInt > 0) & (allQty > orderedQtyInt))
                {
                    result.Rows.Add(record.NumerZlecenia, record.Oper, record.RealDateTime, record.AllNg, record.AllQty, orderedQty, (allQty - orderedQtyInt).ToString());
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
                    result.Rows.Add(record.NumerZlecenia, record.Oper, record.RealDateTime, record.GoodQty, (record.AllNg).ToString());
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

            return result.OrderBy(o => o).ToList();
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
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, comboBoxPrzyczynySmtLine.Text, lotToSmtLine);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerPrzyczynyOdpaduDo_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, comboBoxPrzyczynySmtLine.Text, lotToSmtLine);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerWasteLevelBegin_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(radioButtonWeekly.Checked, chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value, dateTimePickerWasteLevelEnd.Value, lotModelDictionary, comboBoxModel, comboBoxPoziomOdpaduSmtLine.Text, lotToSmtLine);
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
                if (record.NumerZlecenia == textBox2.Text)
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
            Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, comboBoxReasonAnalyses.Text, lotModelDictionary, comboBoxReasonSmtLine.Text, lotToSmtLine);
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



        private DataGridViewCell[] GetCellsFromOneDay(DataGridViewCell dateCell, DataGridView grid)
        {
            List<DataGridViewCell> cellsList = new List<DataGridViewCell>();
            int rowIndex = dateCell.RowIndex;
            int upLimit = rowIndex;
            int bottomLimit = rowIndex;

            Color cellColor = dateCell.Style.BackColor;
            if (TryGetCellColor(rowIndex + 1, grid) == cellColor)
            {
                upLimit++;
                if (TryGetCellColor(rowIndex + 2, grid) == cellColor)
                {
                    upLimit++;
                }
            }

            if (TryGetCellColor(rowIndex - 1, grid) == cellColor)
            {
                bottomLimit--;
                if (TryGetCellColor(rowIndex - 2, grid) == cellColor)
                {
                    bottomLimit--;
                }
            }

            for (int r = bottomLimit; r <= upLimit; r++) 
            {
                foreach (DataGridViewCell cell in grid.Rows[r].Cells)
                {
                    if (cell.Tag!=null)
                    {
                        cellsList.Add(cell);
                    }
                }
            }

            return cellsList.ToArray();
        }

        private Color TryGetCellColor(int row, DataGridView grid)
        {
            Color result = Color.Black;
            try
            {
                result = grid.Rows[row].Cells[0].Style.BackColor;
            }
            catch(Exception exc) { }

            return result;
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

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            SumOfSelectedCells(dataGridViewSmtProduction, label1SmtSelectedSum);
        }

        private void SumOfSelectedCells(DataGridView grid, Label lbl)
        {
            Int32 sum = 0;
            foreach (DataGridViewCell cell in grid.SelectedCells)
            {
                Int32 cellValue = 0;
                if (cell.Tag!=null)
                {
                    Int32.TryParse(cell.Value.ToString(), out cellValue);
                }
                sum += cellValue;
            }
            lbl.Text = "Suma zaznaczonych: " + sum;
        }


        public static Bitmap chartToBitmap(Chart chrt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chrt.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                return bm;
            }
        }
        private void contextMenuStripPrint_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Drukuj")
            {
                //Drukuj printForm = new Drukuj(chartPrzyczynyOdpaduNg);
                //printForm.ShowDialog();

                System.IO.MemoryStream myStream = new System.IO.MemoryStream();
                Chart chartCopy = new Chart();
                chartPrzyczynyOdpaduNg.Serializer.Save(myStream);
                chartCopy.Serializer.Load(myStream);

                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.Landscape = true;

                chartCopy.Width = pd.DefaultPageSettings.PaperSize.Height;
                chartCopy.Height = pd.DefaultPageSettings.PaperSize.Width - 50;

                chartCopy.Tag = "Przyczyny odpadu okres: " + dateTimePickerPrzyczynyOdpaduOd.Value.ToShortDateString() + " - " + dateTimePickerPrzyczynyOdpaduDo.Value.ToShortDateString();
                pd.PrintPage += (sender2, args) => printing_PrintPage(chartCopy, args);

                PrintDialog printdlg = new PrintDialog();
                PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();


                printPrvDlg.Document = pd;
                printPrvDlg.ShowDialog(); 

                printdlg.Document = pd;

                if (printdlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }

            }
        }

        private void printing_PrintPage(object sender, PrintPageEventArgs e)
        {
            Chart chart = sender as Chart;
            Single yPos = 0;
            Single leftMargin = e.MarginBounds.Left;
            Single topMargin = e.MarginBounds.Top;
            Image img = Form1.chartToBitmap(chart);
            int textYPos = 20;
            int w = e.PageBounds.Width;
            int h = e.PageBounds.Height;
            int rect1locX = 70;
            int rect2locX = 70;
            string title = chart.Tag.ToString();

            using (Font printFont = new Font("Arial", 20.0f))
            {
                e.Graphics.DrawImage(img, new Point(5, 55));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, 70, 70));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, w-100, 70));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, w-10, 70));
                e.Graphics.DrawString("MST", printFont, Brushes.Black, 6, textYPos, new StringFormat());
                e.Graphics.DrawString(title, printFont, Brushes.Black, 100, textYPos, new StringFormat());
            }

        }

        private void contextMenuStripPrintPoziomOdpadu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Drukuj")
            {
                //Drukuj printForm = new Drukuj(chartPrzyczynyOdpaduNg);
                //printForm.ShowDialog();
                System.IO.MemoryStream myStream = new System.IO.MemoryStream();
                Chart chartCopy = new Chart();
                chartWasteLevel.Serializer.Save(myStream);
                chartCopy.Serializer.Load(myStream);
                chartCopy.Series.RemoveAt(2);
                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.Landscape = true;

                chartCopy.Width = pd.DefaultPageSettings.PaperSize.Height;
                chartCopy.Height = pd.DefaultPageSettings.PaperSize.Width - 50;

                chartCopy.Tag = "Tygodniowy poziom odpadu " + dateTimePickerWasteLevelBegin.Value.ToShortDateString() + " - " + dateTimePickerWasteLevelEnd.Value.ToShortDateString();
                pd.PrintPage += (sender2, args) => printing_PrintPage(chartCopy, args);

                PrintDialog printdlg = new PrintDialog();
                PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();


                printPrvDlg.Document = pd;
                printPrvDlg.ShowDialog();

                printdlg.Document = pd;

                if (printdlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }

            }
        }

        private void comboBoxSmtModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewSmtModelStats.DataSource = SMTOperations.MakeTableForModel(smtModelLineQuantity, comboBoxSmtModels.Text);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewSmtProduction, "SMT");
        }

        private void dataGridViewKitting_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewKitting, "Kitting");
        }

        private void showJobDetails(DataGridViewCellEventArgs e, DataGridView grid, string station)
        {
            DataGridViewCell cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.Value != null)
            {
                if (cell.Value.ToString() != "0")
                {
                    if (cell.ColumnIndex > 0)
                    {
                        DataTable dt = (DataTable)cell.Tag;
                        if (dt.Columns.Contains("NC12_wyrobu"))
                            dt.Columns["NC12_wyrobu"].ColumnName = "model";
                        if (dt.Columns.Contains("Ilosc_wyrobu_zlecona"))
                            dt.Columns["Ilosc_wyrobu_zlecona"].ColumnName = "Ilosc";
                        if (dt.Columns.Contains("IloscWykonana"))
                            dt.Columns["IloscWykonana"].ColumnName = "Ilosc";

                        string description = "";
                        if (cell.OwningColumn.Name.Contains("SMT"))
                        {
                            description = cell.OwningColumn.Name + " " + dataGridViewSmtProduction.Rows[e.RowIndex].Cells[0].Value.ToString() + " Zm." + dataGridViewSmtProduction.Rows[e.RowIndex].Cells[1].Value.ToString();
                        }
                        else
                        {
                            description = station + " " + grid.Rows[e.RowIndex].Cells[0].Value.ToString() + " Zm." + grid.Rows[e.RowIndex].Cells[1].Value.ToString();
                        }
                        SmtShiftDetails detailsForm = new SmtShiftDetails(dt, description);
                        detailsForm.Show();
                    }
                    else
                    {
                        DataGridViewCell[] dayCells = GetCellsFromOneDay(cell, grid);
                        Dictionary<string, double> quantityPerModel = new Dictionary<string, double>();
                        DataTable combinedTable = new DataTable();
                        foreach (var c in dayCells)
                        {
                            DataTable table = (DataTable)c.Tag;
                            if (table.Columns.Contains("NC12_wyrobu"))
                                table.Columns["NC12_wyrobu"].ColumnName = "model";
                            if (table.Columns.Contains("Ilosc_wyrobu_zlecona"))
                                table.Columns["Ilosc_wyrobu_zlecona"].ColumnName = "Ilosc";
                            if (table.Columns.Contains("IloscWykonana"))
                                table.Columns["IloscWykonana"].ColumnName = "Ilosc";
                            if (combinedTable.Columns.Count == 0)
                            {
                                combinedTable = table.Clone();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                combinedTable.Rows.Add(row.ItemArray);
                            }
                        }

                        string description = "";
                        description = station + " " + grid.Rows[e.RowIndex].Cells[0].Value.ToString();

                        SmtShiftDetails detailsForm = new SmtShiftDetails(combinedTable, description);
                        detailsForm.Show();
                    }
                }
            }
        }

        private void dataGridViewSplitting_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewSplitting, "Splitting");
        }

        private void dataGridViewKitting_SelectionChanged(object sender, EventArgs e)
        {
            SumOfSelectedCells(dataGridViewKitting, labelKittingSelectedSum);
        }

        private void dataGridViewSplitting_SelectionChanged(object sender, EventArgs e)
        {
            SumOfSelectedCells(dataGridViewSplitting, labelSplittingSelectedSum);
        }
    }
}
