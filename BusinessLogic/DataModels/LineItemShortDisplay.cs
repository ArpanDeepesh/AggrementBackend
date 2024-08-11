using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class LineItemShortDisplay
    {
        public long LineItemId { get; set; }
        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal? Rate { get; set; }

        public DateTime ItemCompletionDate { get; set; }

        public string LineItemStatus { get; set; }
        public List<RemarksDisplay> Remarks {get;set;}
    }
}
