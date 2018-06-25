using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class KittingOperations
    {
        public static void FillGridWorkReport(DataTable lotTable, DataGridView grid)
        {
            DataTable dt = lotTable.Copy();
            dt.Columns.Remove("LiniaProdukcyjna");
            SortedDictionary<DateTime, SortedDictionary<int, DataTable>> tablesPerDayPerShift = SMTOperations.sortTableByDayAndShift(dt, "DataCzasWydruku");
            

            System.Drawing.Color rowColor = System.Drawing.Color.LightBlue;

            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in tablesPerDayPerShift)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }

                foreach (var shiftEntry in dayEntry.Value)
                {
                    double qty = 0;
                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        qty += double.Parse(row["Ilosc_wyrobu_zlecona"].ToString());
                    }

                    grid.Rows.Add(dayEntry.Key.ToShortDateString(), shiftEntry.Key.ToString(), qty);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.OwningColumn.HeaderText == "Ilość")
                        {
                            cell.Tag = shiftEntry.Value;
                        }
                    }
                }
            }
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }

        public static void FillGridReadyLots(DataGridView grid, DataTable lotTable, DataTable smtRecords)
        {
            Dictionary<string, Int32> qtyModulesPerModel = new Dictionary<string, int>();
            Dictionary<string, Int32> qtyLotsPerModel = new Dictionary<string, int>();
            Dictionary<string, DataTable> tagPerModel = new Dictionary<string, DataTable>();
            DataTable tagTemplate = new DataTable();
            tagTemplate.Columns.Add("Data_Poczatku_Zlecenia");
            tagTemplate.Columns.Add("DataCzasWydruku");
            tagTemplate.Columns.Add("LOT");
            tagTemplate.Columns.Add("NC12_wyrobu");
            tagTemplate.Columns.Add("Ilosc_wyrobu_zlecona");
            DateTime oldestLot = DateTime.Now;
            List<string> smtLots = new List<string>();
            foreach (DataRow row in smtRecords.Rows)
            {
                smtLots.Add(row["NrZlecenia"].ToString());
                DateTime date = DateTime.Parse(row["DataCzasKoniec"].ToString());
                if (date<oldestLot)
                {
                    oldestLot = date;
                }
            }


            foreach (DataRow row in lotTable.Rows)
            {
                string lot = row["Nr_Zlecenia_Produkcyjnego"].ToString();
                string endDate = row["Data_Konca_Zlecenia"].ToString();
                string startDate = row["Data_Poczatku_Zlecenia"].ToString();
                if (endDate != "") continue;
                string dateString = row["DataCzasWydruku"].ToString();
                if (dateString == "") continue;
                DateTime lotDate = DateTime.Parse(dateString);
                if (lotDate < oldestLot) continue;

                if (!smtLots.Contains(lot))
                {
                    string model = row["NC12_wyrobu"].ToString();
                    if (!qtyModulesPerModel.ContainsKey(model))
                    {
                        qtyModulesPerModel.Add(model, 0);
                        qtyLotsPerModel.Add(model, 0);
                        tagPerModel.Add(model, tagTemplate.Clone());
                    }
                    string qtyString = row["Ilosc_wyrobu_zlecona"].ToString();
                    qtyModulesPerModel[model] += Int32.Parse(qtyString);
                    qtyLotsPerModel[model]++;
                    tagPerModel[model].Rows.Add(startDate, dateString, lot, model, qtyString);
                }
            }

            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Ilosc KITow", "Ilosc KITow");
            grid.Columns.Add("Ilosc wyrobow", "Ilosc wyrobow");

            foreach (var modelEntry in qtyLotsPerModel)
            {
                grid.Rows.Add(modelEntry.Key, modelEntry.Value, qtyModulesPerModel[modelEntry.Key]);
                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                {
                    cell.Tag = tagPerModel[modelEntry.Key];
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
        }
    }
}
