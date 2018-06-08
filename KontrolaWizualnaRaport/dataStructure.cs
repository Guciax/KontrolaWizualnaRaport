using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    public class WasteDataStructure
    {
        public WasteDataStructure(int id, DateTime fixedDateTime, DateTime realDateTime, int shiftNumber, string oper, int goodQty,int allQty,int allNg, int allScrap,string numerZlecenia, string model,Dictionary<string, int> wastePerReason, string smtLine)
        {
            sqlId = id;
            FixedDateTime = fixedDateTime;
            RealDateTime = realDateTime;
            ShiftNumber = shiftNumber;
            Oper = oper;
            GoodQty = goodQty;
            AllQty = allQty;
            AllNg = allNg;
            AllScrap = allScrap;
            NumerZlecenia = numerZlecenia;
            Model = model;
            WastePerReason = wastePerReason;
            SmtLine = smtLine;
        }

        public int sqlId { get; }
        public DateTime FixedDateTime { get; }
        public DateTime RealDateTime { get; }
        public int ShiftNumber { get; }
        public string Oper { get; }
        public int GoodQty { get; }
        public int AllQty { get; }
        public int AllNg { get; }
        public int AllScrap { get; }
        public string NumerZlecenia { get; }
        public string Model { get; }
        public Dictionary<string, int> WastePerReason { get; }

        public int NgTestElektryczny { get; }
        public string SmtLine { get; set; }
        public string Customer { get; }
    }
}
