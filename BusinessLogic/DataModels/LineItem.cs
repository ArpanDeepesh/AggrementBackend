using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class LineItem
    {
        public long Id { get; set; }

        public long? PurchaseOrderId { get; set; }

        public string LiTitle { get; set; } = null!;

        public string? LiDescription { get; set; }

        public decimal LiQuantity { get; set; }

        public decimal? LiRate { get; set; }

        public int LiItemCompletionInDays { get; set; }

        public sbyte? LineItemStatus { get; set; }

    }
}
