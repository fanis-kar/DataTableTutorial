using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataTableTutorial.Models
{
    public class DTResponse
    {
        public string draw { get; set; }
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public List<DataTableTutorial> data { get; set; }
    }
}
