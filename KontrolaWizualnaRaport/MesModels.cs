﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    class MesModels
    {
        public MesModels( int ledSumQty, int ledAQty, int ledBQty, string type)
        {

            LedSumQty = ledSumQty;
            LedAQty = ledAQty;
            LedBQty = ledBQty;
            Type = type;
        }


        public int LedSumQty { get; }
        public int LedAQty { get; }
        public int LedBQty { get; }
        public string Type { get; }
    }
}
