using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denovo
{
    public class InvoiceDocument
    {
        private string reference, description1, description2, description3, amount1, amount2, amount3;

        public InvoiceDocument() 
        {
            Reference = string.Empty;
            Description1 = string.Empty;
            Description2 = string.Empty;
            Description3 = string.Empty;

            Amount1 = string.Empty;
            Amount2 = string.Empty;
            Amount3 = string.Empty;
        }

        public string Reference { get => reference; set => reference = value; }
        public string Description1 { get => description1; set => description1 = value; }
        public string Description2 { get => description2; set => description2 = value; }
        public string Description3 { get => description3; set => description3 = value; }
        public string Amount1 { get => amount1; set => amount1 = value; }
        public string Amount2 { get => amount2; set => amount2 = value; }
        public string Amount3 { get => amount3; set => amount3 = value; }
    }
}
