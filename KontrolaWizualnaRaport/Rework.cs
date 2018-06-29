using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    public class Rework
    {
        public struct PcbReworkData
        {
            public string id;
            public List<ReworkOperationDetails> operations;
        }

        public struct ReworkOperationDetails
        {
            public DateTime fixedDate;
            public DateTime realDate;
            public string operation;
            public string model;
            public string compRef;
            public string oper;
            public bool result;
        }

        public static Dictionary<DateTime, Dictionary<int,Dictionary<string, PcbReworkData>>> SortSqlTableBydateShiftPcb(DataTable sqlTable)
        {
            Dictionary<DateTime, Dictionary<int, Dictionary<string, PcbReworkData>>> result = new Dictionary<DateTime, Dictionary<int, Dictionary<string, PcbReworkData>>>();

            foreach (DataRow row in sqlTable.Rows)
            {
                string pcbId = row["SerialNo"].ToString();
                DateTime realDate = DateTime.Parse(row["Data"].ToString());
                DateTime fixedDate = dataLoader.FixedShiftDate(realDate);
                string operation = row["OpisNaprawy"].ToString();
                string compRef = row["NaprawianyKomponent"].ToString();
                string oper = row["Operator"].ToString();
                string model = row["Model"].ToString();
                bool repairResult = true;

                if (row["WynikNaprawy"].ToString() != "OK") { repairResult = false; }

                if (!result.ContainsKey(fixedDate.Date))
                {
                    result.Add(fixedDate.Date, new Dictionary<int, Dictionary<string, PcbReworkData>>());
                }
                dateShiftNo shiftInfo = SMTOperations.whatDayShiftIsit(realDate);
                if (!result[fixedDate.Date].ContainsKey(shiftInfo.shift))
                {
                    result[fixedDate.Date].Add(shiftInfo.shift, new Dictionary<string, PcbReworkData>());
                }

                


                PcbReworkData reworkItem = new PcbReworkData();
                reworkItem.operations = new List<ReworkOperationDetails>();
                reworkItem.id = pcbId;
                ReworkOperationDetails newOperation = new ReworkOperationDetails();

                newOperation.fixedDate = fixedDate;
                newOperation.realDate = realDate;
                newOperation.operation = operation;
                newOperation.model = model;
                newOperation.compRef = compRef;
                newOperation.oper = oper;
                newOperation.result = repairResult;
                reworkItem.operations.Add(newOperation);
                if (!result[fixedDate.Date][shiftInfo.shift].ContainsKey(pcbId))
                {
                    result[fixedDate.Date][shiftInfo.shift].Add(pcbId, reworkItem);
                }
                else
                {
                    result[fixedDate.Date][shiftInfo.shift][pcbId].operations.Add(newOperation);
                }
            }

            return result;
        }

  
        public static void FillOutGridDailyProdReport(DataGridView grid, Dictionary<DateTime, Dictionary<int, Dictionary<string, PcbReworkData>>> reworkDataByDate)
        {
            grid.Columns.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("PCB", "PCB");
            grid.Columns.Add("Operacji", "Operacji");



            Color rowColor = Color.White;

            foreach (var dateEntry in reworkDataByDate)
            {
                if (rowColor==Color.White)
                {
                    rowColor = Color.LightBlue;
                }
                else
                {
                    rowColor = Color.White;
                }
                foreach (var shiftEntry in dateEntry.Value)
                {
                    DataTable shiftTagTable = new DataTable();

                    shiftTagTable.Columns.Add("#pcb");
                    shiftTagTable.Columns.Add("#operacja");
                    shiftTagTable.Columns.Add("Data");
                    shiftTagTable.Columns.Add("Model");
                    shiftTagTable.Columns.Add("serialNo");
                    shiftTagTable.Columns.Add("Operator");
                    shiftTagTable.Columns.Add("Naprawa");
                    shiftTagTable.Columns.Add("Komponent");
                    shiftTagTable.Columns.Add("Wynik");
                    
                    int operationCount = 0;
                    int pcbCount = 0;
                    

                    foreach (var pcbId in shiftEntry.Value)
                    {
                        pcbCount++;
                        foreach (var operation in pcbId.Value.operations)
                        {
                            operationCount++;
                            string wynik = "OK";
                            if (!operation.result) wynik = "NG";
                            shiftTagTable.Rows.Add(pcbCount, operationCount, operation.fixedDate, operation.model, pcbId.Key, operation.oper, operation.operation, operation.compRef, wynik);
                        }
                    }

                    grid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), dateTools.GetIso8601WeekOfYear(dateEntry.Key), shiftEntry.Key, shiftEntry.Value.Count, operationCount);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.OwningColumn.HeaderText.ToLower()!="data" & cell.OwningColumn.HeaderText.ToLower() != "tydz" & cell.OwningColumn.HeaderText.ToLower() != "zmiana")
                        {
                            cell.Tag = shiftTagTable;
                        }
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
        }
    }
}
