using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    class ChangeOverTools
    {
        public static SortedDictionary<DateTime, Dictionary<int, Dictionary<string, HashSet<string>>>> BuildDateShiftLineDictionary(DataTable smtRectods)
        {
            SortedDictionary<DateTime, Dictionary<int, Dictionary<string, HashSet<string>>>> result = new SortedDictionary<DateTime, Dictionary<int, Dictionary<string, HashSet<string>>>>();

            foreach (DataRow row in smtRectods.Rows)
            {
                DateTime endDate = DateTime.Parse(row["DataCzasKoniec"].ToString());
                string line = row["LiniaSMT"].ToString();
                dateShiftNo endDateShiftInfo = whatDayShiftIsit(endDate);
                int shiftNumer = endDateShiftInfo.shift;

                if (!result.ContainsKey(endDateShiftInfo.fixedDate.Date))
                {
                    result.Add(endDateShiftInfo.fixedDate.Date, new Dictionary<int, Dictionary<string, HashSet<string>>>());
                }
                if (!result[endDateShiftInfo.fixedDate.Date].ContainsKey(shiftNumer))
                {
                    result[endDateShiftInfo.fixedDate.Date].Add(shiftNumer, new Dictionary<string, HashSet<string>>());
                }
                if (!result[endDateShiftInfo.fixedDate.Date][shiftNumer].ContainsKey(line))
                {
                    result[endDateShiftInfo.fixedDate.Date][shiftNumer].Add(line, new HashSet<string>());
                }

                string model = row["Model"].ToString();
                result[endDateShiftInfo.fixedDate.Date][shiftNumer][line].Add(model);
            }
            return result;
        }

        public static void BuildSmtChangeOverGrid(SortedDictionary<DateTime, Dictionary<int, Dictionary<string, HashSet<string>>>> smtModels, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            grid.Columns.Add("Dzien", "Dzien");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("SMT1", "SMT1");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");
            System.Drawing.Color rowColor = System.Drawing.Color.LightBlue;

            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (var dayEntry in smtModels)
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
                    grid.Rows.Add(dayEntry.Key.Date.ToString("dd-MM-yyyy"),shiftEntry.Key);
                    foreach (var lineEntry in shiftEntry.Value)
                    {
                        grid.Rows[grid.Rows.Count - 1].Cells[lineEntry.Key].Value = lineEntry.Value.Count;
                    }

                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
