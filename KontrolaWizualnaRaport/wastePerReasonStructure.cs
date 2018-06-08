using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    public class WastePerReasonStructure
    {
        public WastePerReasonStructure(List<WasteDataStructure> lots, int quantity)
        {
            Lots = lots;
            Quantity = quantity;
        }

        public List<WasteDataStructure> Lots { get; set; }
        public int Quantity { get; set; }
    }
}
