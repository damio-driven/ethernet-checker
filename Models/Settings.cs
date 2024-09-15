using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthernetChecker.Models
{
    internal class Settings
    {
        public string AdapterName { get; set; } = null!;
        public int RecurringSeconds { get; set; }
    }
}
