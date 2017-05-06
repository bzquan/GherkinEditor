using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Printing;

namespace Gherkin.Model
{
    /// <summary>
    /// Represents Print Settings
    /// </summary>
    class PrintSettings
    {
        public PrintSettings()
        {
            this.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            this.PrintTicket = this.PrintQueue.DefaultPrintTicket;
            this.PageSettings = new PageSettings();
            this.PageSettings.Margins = new Margins(left: mmToinch(12), right: mmToinch(12), top: mmToinch(15), bottom: mmToinch(15));  // unit is hundredths of an inch
        }

        public PageSettings PageSettings { get; set; }
        public PrintQueue PrintQueue { get; set; }
        public PrintTicket PrintTicket { get; set; }
        public String DocumentTitle { get; set; }

        // Millimeter to hundredths of an inch
        private int mmToinch(int mm) => (int)(mm / 0.254);
    }
}
