using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport
{


    class Charting
    {
        public static DataTable DrawCapaChart(Chart chart, List<dataStructure> inputData, string oper, Dictionary<string, string> modelDictionary)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();

            Series serColumn = new Series();
            serColumn.IsVisibleInLegend = false;
            serColumn.IsValueShownAsLabel = false;
            serColumn.Color = System.Drawing.Color.Blue;
            serColumn.BorderColor = System.Drawing.Color.Black;

            ChartArea area = new ChartArea();
            area.AxisX.IsLabelAutoFit = true;
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 10);
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;
            area.AxisY.MajorGrid.LineWidth = 1;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MinorGrid.Interval = 1000;
            area.AxisY.MajorGrid.Interval = 5000;
            area.AxisX.LabelStyle.Interval = 1;
            area.AxisY.LabelStyle.Interval = 5000;

            Dictionary<DateTime, int> qtyPerDayForAll = new Dictionary<DateTime, int>();
            Dictionary<DateTime, Dictionary<string, int>> qtyPerDayPerModel = new Dictionary<DateTime, Dictionary<string, int>>();
            DataTable gridTable = new DataTable();
            gridTable.Columns.Add("Data");
            gridTable.Columns.Add("Ilość");

            if (oper == "Wszyscy")
            {
                Dictionary<DateTime, string> labelDict = new Dictionary<DateTime, string>();

                foreach (var item in inputData)
                {
                    if (item.Oper == oper || oper == "Wszyscy")
                    {
                        string model = "???";
                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                            model = modelDictionary[item.NumerZlecenia];

                        if (!qtyPerDayForAll.ContainsKey(item.FixedDateTime.Date))
                        {
                            qtyPerDayForAll.Add(item.FixedDateTime.Date, 0);
                            labelDict.Add(item.FixedDateTime.Date, model);
                        }
                        qtyPerDayForAll[item.FixedDateTime.Date] += item.AllQty;
                        if (!labelDict[item.FixedDateTime.Date].Contains(model))
                            labelDict[item.FixedDateTime.Date] += Environment.NewLine + model;
                    }
                }
                serColumn.ChartType = SeriesChartType.Column;

                foreach (var key in qtyPerDayForAll)
                {
                    serColumn.Points.AddXY(key.Key, key.Value);
                    gridTable.Rows.Add(key.Key.Date.ToShortDateString(), key.Value);
                }

                chart.Series.Add(serColumn);
            }
            else
            {
                HashSet<string> uniqueModels = new HashSet<string>();
                HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
                Dictionary<string, Dictionary<DateTime, int>> dictFirstModelThenDate = new Dictionary<string, Dictionary<DateTime, int>>();

                foreach (var item in inputData)
                {
                    if (item.Oper == oper)
                    {
                        if (!qtyPerDayPerModel.ContainsKey(item.FixedDateTime.Date))
                        {
                            qtyPerDayPerModel.Add(item.FixedDateTime.Date, new Dictionary<string, int>());
                        }
                        string model = "??";

                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                            model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");

                        uniqueModels.Add(model);
                        uniqueDates.Add(item.FixedDateTime.Date);

                        if (!qtyPerDayPerModel[item.FixedDateTime.Date].ContainsKey(model))
                            qtyPerDayPerModel[item.FixedDateTime.Date].Add(model, 0);

                        qtyPerDayPerModel[item.FixedDateTime.Date][model] += item.AllQty;
                    }
                }
                serColumn.ChartType = SeriesChartType.StackedColumn;
                Dictionary<DateTime, int> qtyPerDayPerOperator = new Dictionary<DateTime, int>();
                foreach (var model in uniqueModels)
                {

                    dictFirstModelThenDate.Add(model, new Dictionary<DateTime, int>());
                    foreach (var date in uniqueDates)
                    {
                        dictFirstModelThenDate[model].Add(date, 0);

                        if (!qtyPerDayPerOperator.ContainsKey(date))
                            qtyPerDayPerOperator.Add(date, 0);
                    }
                }


                foreach (var item in inputData)
                {
                    if (item.Oper == oper)
                    {
                        string model = "??";
                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                            model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");

                        dictFirstModelThenDate[model][item.FixedDateTime.Date] += item.AllQty;
                        qtyPerDayPerOperator[item.FixedDateTime.Date] += item.AllQty;
                    }
                }

                foreach (var keyEntry in qtyPerDayPerOperator)
                {
                    gridTable.Rows.Add(keyEntry.Key, keyEntry.Value);
                }



                foreach (var model in dictFirstModelThenDate)
                {
                    chart.Series.Add(new Series(model.Key));
                    chart.Series[model.Key].ChartType = SeriesChartType.StackedColumn;
                    chart.Series[model.Key].IsValueShownAsLabel = true;
                    chart.Series[model.Key].ToolTip = model.Key;

                    foreach (var date in model.Value)
                    {
                        chart.Series[model.Key].Points.AddXY(date.Key, date.Value);

                    }

                    foreach (var point in chart.Series[model.Key].Points)
                    {
                        if (point.YValues[0] == 0) point.IsValueShownAsLabel = false;
                    }

                }
                area.AxisY.LabelStyle.Interval = 500;
                area.AxisY.MinorGrid.Interval = 100;
                area.AxisY.MajorGrid.Interval = 500;
            }

            chart.ChartAreas.Add(area);


            //chart.Legends[0].DockedToChartArea = chart.ChartAreas[0].Name;
            //chart.Legends[0].TableStyle = LegendTableStyle.Auto;
            chart.Legends.Clear();



            return gridTable;
        }

        private class WasteStruc
        {
            public string name;
            public int qty;
        }


        private static WasteStruc CreateWasteStruc(string name)
        {
            WasteStruc result = new WasteStruc();
            result.name = name;
            result.qty = 0;
            return result;
        }

        private static int FindIndexOfWaste(string name, List<WasteStruc> searchList)
        {
            int result = 0;
            for (int i = 0; i < searchList.Count; i++)
            {
                if (searchList[i].name == name)
                {
                    return i;

                }
            }
            return result;
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {

            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            int year = (time.Year - 2000) * 1000;
            return year + CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DataTable DrawWasteLevel(bool weekly, Chart chartWasteLevel, List<dataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary, ComboBox comboModel)
        {
            DataTable result = new DataTable();
            Dictionary<string, double> ngLevel = new Dictionary<string, double>();
            Dictionary<string, double> scrapLevel = new Dictionary<string, double>();
            Dictionary<string, double> allLevel = new Dictionary<string, double>();

            Dictionary<string, Dictionary<string, double>> ngLevelPerModel = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> scrapLevelPerModel = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> allLevelPerModel = new Dictionary<string, Dictionary<string, double>>();

            Dictionary<string, double> totalProdQuantity = new Dictionary<string, double>();

            string dictionaryKey = "";

            result.Columns.Add("Data");
            result.Columns.Add("Ilość");
            result.Columns.Add("Produkcja");
            result.Columns.Add("%");

            foreach (var item in inputData)
            {
                if (item.FixedDateTime >= dateBegin & item.FixedDateTime <= dateEnd)
                {
                    string model = "";
                    if (!modelDictionary.TryGetValue(item.NumerZlecenia, out model)) model = "???";


                    if (weekly)
                    {
                        dictionaryKey = GetIso8601WeekOfYear(item.FixedDateTime).ToString();
                    }
                    else
                    {
                        dictionaryKey = item.FixedDateTime.Date.ToShortDateString();
                    }

                    if (!ngLevel.ContainsKey(dictionaryKey))
                    {
                        ngLevel.Add(dictionaryKey, 0);
                        scrapLevel.Add(dictionaryKey, 0);
                        allLevel.Add(dictionaryKey, 0);

                        ngLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());
                        scrapLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());
                        allLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());

                        totalProdQuantity.Add(dictionaryKey, 0);
                    }

                    if (!ngLevelPerModel[dictionaryKey].ContainsKey(model))
                    {
                        ngLevelPerModel[dictionaryKey].Add(model, 0);
                        scrapLevelPerModel[dictionaryKey].Add(model, 0);
                        allLevelPerModel[dictionaryKey].Add(model, 0);
                    }

                    if (model.Replace("LLFML", "") == comboModel.Text || comboModel.Text == "")
                    {
                        ngLevel[dictionaryKey] += item.AllNg;
                        scrapLevel[dictionaryKey] += item.AllScrap;
                        allLevel[dictionaryKey] += item.AllQty;

                        ngLevelPerModel[dictionaryKey][model] += item.AllNg;
                        scrapLevelPerModel[dictionaryKey][model] += item.AllScrap;
                        allLevelPerModel[dictionaryKey][model] += item.AllQty;

                        totalProdQuantity[dictionaryKey] += item.AllQty;
                    }
                }
            }



            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Line;
            ngSeries.BorderWidth = 3;
            ngSeries.Name = "NG [%]";

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Line;
            scrapSeries.BorderWidth = 3;
            scrapSeries.Name = "SCRAP [%]";

            Series productionLevel = new Series();
            productionLevel.Name = "productionLevel";
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";

            chartWasteLevel.Series.Clear();
            chartWasteLevel.ChartAreas.Clear();

            DataTable tempScrapTable = result.Clone();
            result.Rows.Add("NG:");
            foreach (var keyEntry in ngLevel)
            {
                double ng = ((double)keyEntry.Value / (double)allLevel[keyEntry.Key]) * 100;
                double scrap = ((double)scrapLevel[keyEntry.Key] / (double)allLevel[keyEntry.Key]) * 100;

                List<string> wastePerModelToolTip = new List<string>();
                foreach (var model in ngLevelPerModel[keyEntry.Key])
                {
                    wastePerModelToolTip.Add(Math.Round((model.Value / allLevelPerModel[keyEntry.Key][model.Key]) * 100, 1).ToString() + "% - " + model.Value+@"/"+ allLevelPerModel[keyEntry.Key][model.Key]+" - " + model.Key);
                }
                wastePerModelToolTip = wastePerModelToolTip.OrderByDescending(o => o).ToList();

                string ngtoolTip = "";
                foreach (var item in wastePerModelToolTip)
                {
                    ngtoolTip += item + Environment.NewLine;
                }
                DataPoint ngPoint = new DataPoint();
                ngPoint.SetValueXY(keyEntry.Key, ng);
                ngPoint.ToolTip = ngtoolTip;

                //ngSeries.Points.AddXY(keyEntry.Key, ng);
                ngSeries.Points.Add(ngPoint);
                scrapSeries.Points.AddXY(keyEntry.Key, scrap);

                result.Rows.Add(keyEntry.Key, keyEntry.Value, (double)allLevel[keyEntry.Key] , Math.Round(ng, 2) + "%");
                tempScrapTable.Rows.Add(keyEntry.Key, scrapLevel[keyEntry.Key], (double)allLevel[keyEntry.Key] , Math.Round(scrap, 2) + "%");

                productionLevel.Points.AddXY(keyEntry.Key, totalProdQuantity[keyEntry.Key]);
            }
            result.Rows.Add();
            result.Rows.Add("SCRAP:");

            foreach (DataRow r in tempScrapTable.Rows)
            {
                result.Rows.Add(r[0].ToString(), r[1].ToString(), r[2].ToString(), r[3].ToString());
            }
            chartWasteLevel.Series.Add(productionLevel);
            chartWasteLevel.ChartAreas.Add(ar);
            chartWasteLevel.Series.Add(ngSeries);
            chartWasteLevel.Series.Add(scrapSeries);

            chartWasteLevel.Legends[0].DockedToChartArea = chartWasteLevel.ChartAreas[0].Name;


            foreach (var point in chartWasteLevel.Series[2].Points)
            {
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 10;
            }
            foreach (var point in chartWasteLevel.Series[1].Points)
            {
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 10;
            }



            return result;
        }

        public static DataTable DrawWasteReasonsCHart(Chart ngChart, Chart scrapChart, List<dataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Nazwa");
            result.Columns.Add("Ilość");

            List<WasteStruc> wasteList = new List<WasteStruc>();
            Dictionary<string, Dictionary<string, double>> wasteNgPerModel = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> wasteScrapPerModel = new Dictionary<string, Dictionary<string, double>>();

            wasteList.Add(CreateWasteStruc("goodQty"));
            wasteList.Add(CreateWasteStruc("allQty"));

            wasteList.Add(CreateWasteStruc("NgBrakLutowia"));
            wasteList.Add(CreateWasteStruc("NgBrakDiodyLed"));
            wasteList.Add(CreateWasteStruc("NgBrakResConn"));
            wasteList.Add(CreateWasteStruc("NgPrzesuniecieLed"));
            wasteList.Add(CreateWasteStruc("NgPrzesuniecieResConn"));
            wasteList.Add(CreateWasteStruc("NgZabrudzenieLed"));
            wasteList.Add(CreateWasteStruc("NgUszkodzenieMechaniczneLed"));
            wasteList.Add(CreateWasteStruc("NgUszkodzenieConn"));
            wasteList.Add(CreateWasteStruc("NgWadaFabrycznaDiody"));
            wasteList.Add(CreateWasteStruc("NgUszkodzonePcb"));
            wasteList.Add(CreateWasteStruc("NgWadaNaklejki"));
            wasteList.Add(CreateWasteStruc("NgSpalonyConn"));
            wasteList.Add(CreateWasteStruc("NgInne"));

            wasteList.Add(CreateWasteStruc("ScrapBrakLutowia"));
            wasteList.Add(CreateWasteStruc("ScrapBrakDiodyLed"));
            wasteList.Add(CreateWasteStruc("ScrapBrakResConn"));
            wasteList.Add(CreateWasteStruc("ScrapPrzesuniecieLed"));
            wasteList.Add(CreateWasteStruc("ScrapPrzesuniecieResConn"));
            wasteList.Add(CreateWasteStruc("ScrapZabrudzenieLed"));
            wasteList.Add(CreateWasteStruc("ScrapUszkodzenieMechaniczneLed"));
            wasteList.Add(CreateWasteStruc("ScrapUszkodzenieConn"));
            wasteList.Add(CreateWasteStruc("ScrapWadaFabrycznaDiody"));
            wasteList.Add(CreateWasteStruc("ScrapUszkodzonePcb"));
            wasteList.Add(CreateWasteStruc("ScrapWadaNaklejki"));
            wasteList.Add(CreateWasteStruc("ScrapSpalonyConn"));
            wasteList.Add(CreateWasteStruc("ScrapInne"));

            wasteList.Add(CreateWasteStruc("NgTestElektryczny"));

            foreach (var item in wasteList)
            {
                if (item.name.Contains("Ng"))
                {
                    wasteNgPerModel.Add(item.name, new Dictionary<string, double>());
                }

                if (item.name.Contains("Scrap"))
                {
                    wasteScrapPerModel.Add(item.name, new Dictionary<string, double>());
                }
            }

            foreach (var dataRecord in inputData)
            {
                if (dataRecord.FixedDateTime.Date >= dateBegin.Date & dataRecord.FixedDateTime.Date <= dateEnd.Date)
                {
                    string model = "???";
                    modelDictionary.TryGetValue(dataRecord.NumerZlecenia, out model);
                    if (model == null)
                    {
                        model = "???";
                    }
                    else
                    {
                        model = model.Replace("LLFML", "");
                    }

                    wasteList[FindIndexOfWaste("GoodQty", wasteList)].qty += dataRecord.GoodQty; ;
                    wasteList[FindIndexOfWaste("allQty", wasteList)].qty += dataRecord.AllQty;

                    Dictionary<string, int> wasteInRecord = new Dictionary<string, int>();


                    wasteInRecord.Add("NgBrakLutowia", dataRecord.NgBrakLutowia);
                    wasteInRecord.Add("NgBrakDiodyLed", dataRecord.NgBrakDiodyLed);
                    wasteInRecord.Add("NgBrakResConn", dataRecord.NgBrakResConn);
                    wasteInRecord.Add("NgPrzesuniecieLed", dataRecord.NgPrzesuniecieLed);
                    wasteInRecord.Add("NgPrzesuniecieResConn", dataRecord.NgPrzesuniecieResConn);
                    wasteInRecord.Add("NgZabrudzenieLed", dataRecord.NgZabrudzenieLed);
                    wasteInRecord.Add("NgUszkodzenieMechaniczneLed", dataRecord.NgUszkodzenieMechaniczneLed);
                    wasteInRecord.Add("NgUszkodzenieConn", dataRecord.NgUszkodzenieConn);
                    wasteInRecord.Add("NgWadaFabrycznaDiody", dataRecord.NgWadaFabrycznaDiody);
                    wasteInRecord.Add("NgUszkodzonePcb", dataRecord.NgUszkodzonePcb);
                    wasteInRecord.Add("NgWadaNaklejki", dataRecord.NgWadaNaklejki);
                    wasteInRecord.Add("NgSpalonyConn", dataRecord.NgSpalonyConn);
                    wasteInRecord.Add("NgInne", dataRecord.NgInne);
                    wasteInRecord.Add("NgTestElektryczny", dataRecord.NgTestElektryczny);

                    wasteInRecord.Add("ScrapBrakLutowia", dataRecord.ScrapBrakLutowia);
                    wasteInRecord.Add("ScrapBrakDiodyLed", dataRecord.ScrapBrakDiodyLed);
                    wasteInRecord.Add("ScrapBrakResConn", dataRecord.ScrapBrakResConn);
                    wasteInRecord.Add("ScrapPrzesuniecieLed", dataRecord.ScrapPrzesuniecieLed);
                    wasteInRecord.Add("ScrapPrzesuniecieResConn", dataRecord.ScrapPrzesuniecieResConn);
                    wasteInRecord.Add("ScrapZabrudzenieLed", dataRecord.ScrapZabrudzenieLed);
                    wasteInRecord.Add("ScrapUszkodzenieMechaniczneLed", dataRecord.ScrapUszkodzenieMechaniczneLed);
                    wasteInRecord.Add("ScrapUszkodzenieConn", dataRecord.ScrapUszkodzenieConn);
                    wasteInRecord.Add("ScrapWadaFabrycznaDiody", dataRecord.ScrapWadaFabrycznaDiody);
                    wasteInRecord.Add("ScrapUszkodzonePcb", dataRecord.ScrapUszkodzonePcb);
                    wasteInRecord.Add("ScrapWadaNaklejki", dataRecord.ScrapWadaNaklejki);
                    wasteInRecord.Add("ScrapSpalonyConn", dataRecord.ScrapSpalonyConn);
                    wasteInRecord.Add("ScrapInne", dataRecord.ScrapInne);

                    foreach (var item in wasteInRecord)
                    {
                        if (item.Value > 0)
                        {
                            if (item.Key.Contains("Ng"))
                            {
                                if (!wasteNgPerModel[item.Key].ContainsKey(model))
                                {
                                    wasteNgPerModel[item.Key].Add(model, 0);
                                }
                                wasteNgPerModel[item.Key][model] += item.Value;
                            }
                            if (item.Key.Contains("Scrap"))
                            {
                                if (!wasteScrapPerModel[item.Key].ContainsKey(model))
                                {
                                    wasteScrapPerModel[item.Key].Add(model, 0);
                                }
                                wasteScrapPerModel[item.Key][model] += item.Value;
                            }
                        }
                    }


                    wasteList[FindIndexOfWaste("NgBrakLutowia", wasteList)].qty += wasteInRecord["NgBrakLutowia"];
                    wasteList[FindIndexOfWaste("NgBrakDiodyLed", wasteList)].qty += wasteInRecord["NgBrakDiodyLed"];
                    wasteList[FindIndexOfWaste("NgBrakResConn", wasteList)].qty += wasteInRecord["NgBrakResConn"];
                    wasteList[FindIndexOfWaste("NgPrzesuniecieLed", wasteList)].qty += wasteInRecord["NgPrzesuniecieLed"];
                    wasteList[FindIndexOfWaste("NgPrzesuniecieResConn", wasteList)].qty += wasteInRecord["NgPrzesuniecieResConn"];
                    wasteList[FindIndexOfWaste("NgZabrudzenieLed", wasteList)].qty += wasteInRecord["NgZabrudzenieLed"];
                    wasteList[FindIndexOfWaste("NgUszkodzenieMechaniczneLed", wasteList)].qty += wasteInRecord["NgUszkodzenieMechaniczneLed"];
                    wasteList[FindIndexOfWaste("NgUszkodzenieConn", wasteList)].qty += wasteInRecord["NgUszkodzenieConn"];
                    wasteList[FindIndexOfWaste("NgWadaFabrycznaDiody", wasteList)].qty += wasteInRecord["NgWadaFabrycznaDiody"];
                    wasteList[FindIndexOfWaste("NgUszkodzonePcb", wasteList)].qty += wasteInRecord["NgUszkodzonePcb"];
                    wasteList[FindIndexOfWaste("NgWadaNaklejki", wasteList)].qty += wasteInRecord["NgWadaNaklejki"];
                    wasteList[FindIndexOfWaste("NgSpalonyConn", wasteList)].qty += wasteInRecord["NgSpalonyConn"];
                    wasteList[FindIndexOfWaste("NgInne", wasteList)].qty += wasteInRecord["NgInne"];

                    wasteList[FindIndexOfWaste("ScrapBrakLutowia", wasteList)].qty += wasteInRecord["ScrapBrakLutowia"];
                    wasteList[FindIndexOfWaste("ScrapBrakDiodyLed", wasteList)].qty += wasteInRecord["ScrapBrakDiodyLed"];
                    wasteList[FindIndexOfWaste("ScrapBrakResConn", wasteList)].qty += wasteInRecord["ScrapBrakResConn"];
                    wasteList[FindIndexOfWaste("ScrapPrzesuniecieLed", wasteList)].qty += wasteInRecord["ScrapPrzesuniecieLed"];
                    wasteList[FindIndexOfWaste("ScrapPrzesuniecieResConn", wasteList)].qty += wasteInRecord["ScrapPrzesuniecieResConn"];
                    wasteList[FindIndexOfWaste("ScrapZabrudzenieLed", wasteList)].qty += wasteInRecord["ScrapZabrudzenieLed"];
                    wasteList[FindIndexOfWaste("ScrapUszkodzenieMechaniczneLed", wasteList)].qty += wasteInRecord["ScrapUszkodzenieMechaniczneLed"];
                    wasteList[FindIndexOfWaste("ScrapUszkodzenieConn", wasteList)].qty += wasteInRecord["ScrapUszkodzenieConn"];
                    wasteList[FindIndexOfWaste("ScrapWadaFabrycznaDiody", wasteList)].qty += wasteInRecord["ScrapWadaFabrycznaDiody"];
                    wasteList[FindIndexOfWaste("ScrapUszkodzonePcb", wasteList)].qty += wasteInRecord["ScrapUszkodzonePcb"];
                    wasteList[FindIndexOfWaste("ScrapWadaNaklejki", wasteList)].qty += wasteInRecord["ScrapWadaNaklejki"];
                    wasteList[FindIndexOfWaste("ScrapSpalonyConn", wasteList)].qty += wasteInRecord["ScrapSpalonyConn"];
                    wasteList[FindIndexOfWaste("ScrapInne", wasteList)].qty += wasteInRecord["ScrapInne"];

                    wasteList[FindIndexOfWaste("NgTestElektryczny", wasteList)].qty += wasteInRecord["NgTestElektryczny"];
                }
            }

            wasteList = wasteList.OrderByDescending(o => o.qty).ToList();


            ngChart.Series.Clear();
            ngChart.ChartAreas.Clear();

            scrapChart.Series.Clear();
            scrapChart.ChartAreas.Clear();

            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Column;

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Column;


            ChartArea ngArea = new ChartArea();
            ngArea.AxisX.LabelStyle.Interval = 1;
            ngArea.AxisX.IsLabelAutoFit = true;
            ngArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            ngArea.AxisX.LabelStyle.Font = new System.Drawing.Font(ngArea.AxisX.LabelStyle.Font.Name, 10f);

            ChartArea scrapArea = new ChartArea();
            scrapArea.AxisX.LabelStyle.Interval = 1;
            scrapArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            scrapArea.AxisX.LabelStyle.Font = new System.Drawing.Font(scrapArea.AxisX.LabelStyle.Font.Name, 10f);

            DataTable scrapTempTable = result.Clone();
            result.Rows.Add("NG", "");
            foreach (var wasteEntry in wasteList)
            {
                if (wasteEntry.name.Substring(0, 2) == "Ng")
                {
                    Dictionary<string, string> label = new Dictionary<string, string>();
                    foreach (var wasteName in wasteNgPerModel)
                    {
                        label.Add(wasteName.Key, string.Join(Environment.NewLine, wasteName.Value.OrderByDescending(q => q.Value).Select(sel => (sel.Key + " " + sel.Value + "szt.")).ToArray()));
                    }

                    DataPoint ngPoint = new DataPoint();
                    ngPoint.SetValueXY(wasteEntry.name, wasteEntry.qty);
                    ngPoint.ToolTip = label[wasteEntry.name];
                    ngSeries.Points.Add(ngPoint);
                    result.Rows.Add(wasteEntry.name, wasteEntry.qty);
                }

                if (wasteEntry.name.Substring(0, 2) == "Sc")
                {
                    Dictionary<string, string> label = new Dictionary<string, string>();
                    foreach (var wasteName in wasteScrapPerModel)
                    {
                        label.Add(wasteName.Key, string.Join(Environment.NewLine, wasteName.Value.OrderByDescending(q => q.Value).Select(sel => (sel.Key + " " + sel.Value + "szt.")).ToArray()));
                    }

                    DataPoint scrapPoint = new DataPoint();
                    scrapPoint.SetValueXY(wasteEntry.name, wasteEntry.qty);
                    scrapPoint.ToolTip = label[wasteEntry.name];
                    scrapSeries.Points.Add(scrapPoint);
                    scrapTempTable.Rows.Add(wasteEntry.name, wasteEntry.qty);
                }
            }

            result.Rows.Add();
            result.Rows.Add("SCRAP:", "");
            foreach (DataRow row in scrapTempTable.Rows)
            {
                result.Rows.Add(row[0].ToString(), row[1].ToString());
            }

            ngChart.Series.Add(ngSeries);
            ngChart.ChartAreas.Add(ngArea);
            ngChart.Legends.Clear();

            scrapChart.Series.Add(scrapSeries);
            scrapChart.ChartAreas.Add(scrapArea);
            scrapChart.Legends.Clear();



            return result;
        }

        public static void DrawScrapPerReason(Chart targetChart, Chart paretoChart, List<dataStructure> inputData, string reason, Dictionary<string, string> modelDictionary)
        {
            DataTable result = new DataTable();
            Dictionary<DateTime, Dictionary<string, double>> wasteInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();
            Dictionary<DateTime, Dictionary<string, double>> scrapInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();
            Dictionary<DateTime, Dictionary<string, double>> totalInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();
            Dictionary<string, double> modelPareto = new Dictionary<string, double>();

            foreach (var record in inputData)
            {
                if (!wasteInDayPerModel.ContainsKey(record.FixedDateTime.Date))
                {
                    wasteInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                    totalInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                    scrapInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                }
                string model = "???";
                modelDictionary.TryGetValue(record.NumerZlecenia, out model);
                if (model == null)
                    model = "???";
                else
                    model = model.Replace("LLFML", "");

                if (!wasteInDayPerModel[record.FixedDateTime.Date].ContainsKey(model))
                {
                    wasteInDayPerModel[record.FixedDateTime.Date].Add(model, 0);
                    totalInDayPerModel[record.FixedDateTime.Date].Add(model, 0);
                    scrapInDayPerModel[record.FixedDateTime.Date].Add(model, 0);
                }

                var typ = typeof(dataStructure);
                string reasonNg = "Ng" + reason;
                string reasonScrap = "Scrap" + reason;

                foreach (var type in typ.GetProperties())
                {
                    if (type.Name== reasonNg)
                    {
                        double value = double.Parse(type.GetValue(record).ToString());
                        wasteInDayPerModel[record.FixedDateTime.Date][model] += value;
                        if (!modelPareto.ContainsKey(model)) modelPareto.Add(model, 0);
                        modelPareto[model] += value;
                    }

                    if (type.Name == reasonScrap)
                    {
                        
                        double value = double.Parse(type.GetValue(record).ToString());
                        scrapInDayPerModel[record.FixedDateTime.Date][model] += value;
                        //Debug.WriteLine(record.FixedDateTime.ToShortDateString() + " " + type.Name + " " + value);
                    }

                }
                totalInDayPerModel[record.FixedDateTime.Date][model] += record.AllQty;



            }

            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Line;
            ngSeries.BorderWidth = 3;
            ngSeries.Name = "NG [%]";

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Line;
            scrapSeries.BorderWidth = 3;
            scrapSeries.Name = "SCRAP [%]";
            
            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisX.MajorGrid.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisX.IntervalType = DateTimeIntervalType.Days;
            ar.AxisY.LabelStyle.Format = "{0.00} %";

            foreach (var dateEntry in wasteInDayPerModel)
            {
                double totalNg = wasteInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);
                double totalTotal = totalInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);
                double totalScrap = scrapInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);

                DataPoint ngPoint = new DataPoint();
                ngPoint.MarkerStyle = MarkerStyle.Circle;
                ngPoint.MarkerSize = 10;
                ngPoint.SetValueXY(dateEntry.Key, (totalNg / totalTotal) * 100);
                List<string> ngToolTip = new List<string>();
                foreach (var modelEntry in wasteInDayPerModel[dateEntry.Key])
                {
                    ngToolTip.Add(modelEntry.Value + @"/" + totalInDayPerModel[dateEntry.Key][modelEntry.Key] + "szt. - " + modelEntry.Key);
                }
                ngToolTip = ngToolTip.OrderByDescending(o => o).ToList(); ;
                ngPoint.ToolTip = string.Join(Environment.NewLine, ngToolTip.ToArray());
                ngSeries.Points.Add(ngPoint);

                DataPoint scrapPoint = new DataPoint();
                scrapPoint.MarkerStyle = MarkerStyle.Circle;
                scrapPoint.MarkerSize = 10;
                scrapPoint.SetValueXY(dateEntry.Key, (totalScrap / totalTotal) * 100);

                List<string> scrapToolTip = new List<string>();
                foreach (var modelEntry in scrapInDayPerModel[dateEntry.Key])
                {
                    scrapToolTip.Add(modelEntry.Value+ @"/" + totalInDayPerModel[dateEntry.Key][modelEntry.Key] + "szt. - " + modelEntry.Key);
                }
                scrapToolTip = scrapToolTip.OrderByDescending(o => o).ToList(); ;
                scrapPoint.ToolTip = string.Join(Environment.NewLine, scrapToolTip.ToArray());
                scrapSeries.Points.Add(scrapPoint);
            }



            // var dictNg = wasteInDayPerModel.Select(item => new { Key = item.Value.Keys, wartosc = item.Value.Values }).ToDictionary(item => item, item=> item.wartosc);
            // var dictScrap = scrapInDayPerModel.SelectMany(sel => sel.Value).ToDictionary(p => p.Key, p => p.Value);

            targetChart.Series.Clear();
            targetChart.ChartAreas.Clear();

            targetChart.Series.Add(ngSeries);
            targetChart.Series.Add(scrapSeries);
            targetChart.ChartAreas.Add(ar);
            targetChart.Legends[0].DockedToChartArea = targetChart.ChartAreas[0].Name;
            //targetChart.Legends[0].BackColor = System.Drawing.Color.Transparent;
            targetChart.Legends[0].Position = new ElementPosition(0, 0, targetChart.Legends[0].Position.Width, targetChart.Legends[0].Position.Height);

            //modelPareto
            List<Tuple<double, string>> paretoList = new List<Tuple<double, string>>();
            
            foreach (var keyentry in modelPareto)
            {
                paretoList.Add(new Tuple<double, string>(keyentry.Value, keyentry.Key));

            }

            paretoList = paretoList.OrderByDescending(o => o.Item1).ToList();

            paretoChart.Series.Clear();
            paretoChart.ChartAreas.Clear();
            paretoChart.Legends.Clear();

            Series seriesParetoNg = new Series();
            seriesParetoNg.ChartType = SeriesChartType.Column;

            ChartArea areaPareto = new ChartArea();
            areaPareto.AxisX.LabelStyle.Interval = 1;


            foreach (var item in paretoList)
            {
                if (item.Item1>0)
                seriesParetoNg.Points.AddXY(item.Item2, item.Item1);
            }

            paretoChart.ChartAreas.Add(areaPareto);
            paretoChart.Series.Add(seriesParetoNg);
        }

        public static void DrawWastePerModel (Chart chartLevel, Chart chartReasons, List<dataStructure> inputData, Dictionary<string, string> modelDictionary, string selectedModel)
        {
            Dictionary<DateTime, double> wastePerDay = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> totalPerDay = new Dictionary<DateTime, double>();
            Dictionary<string, double> wasteReasons = new Dictionary<string, double>();
            double total = 0;

            foreach (var record in inputData)
            {
                string model = "";
                if (!modelDictionary.TryGetValue(record.NumerZlecenia, out model)) continue;
                
                if (model.Contains(selectedModel))
                {
                    if (!wastePerDay.ContainsKey(record.FixedDateTime.Date))
                    {
                        wastePerDay.Add(record.FixedDateTime.Date, 0);
                        totalPerDay.Add(record.FixedDateTime.Date, 0);
                    }
                    wastePerDay[record.FixedDateTime.Date] += record.AllNg;
                    totalPerDay[record.FixedDateTime.Date] += record.AllQty;

                    var typ = typeof(dataStructure);

                    foreach (var type in typ.GetProperties())
                    {
                        if(type.Name.StartsWith("Ng"))
                        {
                            if (!wasteReasons.ContainsKey(type.Name))
                            {
                                wasteReasons.Add(type.Name, 0);
                            }
                            wasteReasons[type.Name] += double.Parse(type.GetValue(record).ToString());
                        }
                    }
                }
            }

            List<Tuple<double, string>> reasonsList = new List<Tuple<double, string>>();
            foreach (var reasonEntry in wasteReasons)
            {
                reasonsList.Add(new Tuple<double, string>(reasonEntry.Value, reasonEntry.Key));
            }
            reasonsList=reasonsList.OrderByDescending(o => o.Item1).ToList();

            chartLevel.Series.Clear();
            chartLevel.Legends.Clear();
            chartLevel.ChartAreas.Clear();
            chartReasons.Series.Clear();
            chartReasons.Legends.Clear();
            chartReasons.ChartAreas.Clear();


            Series seriesLevel = new Series();
            seriesLevel.ChartType = SeriesChartType.Line;
            seriesLevel.BorderWidth = 3;
            
            ChartArea arLevel = new ChartArea();
            arLevel.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arLevel.AxisX.IntervalType = DateTimeIntervalType.Days;

            arLevel.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arLevel.AxisY.LabelStyle.Format = "{0.00} %";

            foreach (var dayEntry in wastePerDay)
            {
                DataPoint p = new DataPoint();
                p.MarkerStyle = MarkerStyle.Circle;
                p.MarkerSize = 10;
                p.SetValueXY(dayEntry.Key, dayEntry.Value / totalPerDay[dayEntry.Key]*100);
                seriesLevel.Points.Add(p);
            }
            chartLevel.ChartAreas.Add(arLevel);
            chartLevel.Series.Add(seriesLevel);

            Series seriesReasosn = new Series();
            seriesReasosn.ChartType = SeriesChartType.Column;

            ChartArea arReasons = new ChartArea();
            arReasons.AxisX.LabelStyle.Interval = 1;
            arReasons.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arReasons.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

            foreach (var item in reasonsList)
            {
                seriesReasosn.Points.AddXY(item.Item2, item.Item1);
            }
            chartReasons.ChartAreas.Add(arReasons);
            chartReasons.Series.Add(seriesReasosn);
        }
        
    }
}
