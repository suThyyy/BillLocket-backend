using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Domain.Entities
{
    public class ShareBill
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public long TotalAmountMinor { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
