using System.Collections.Generic;

namespace LTW.Areas.Admin.Model
{
    public class KhuyenMaiReport
    {
        public int TotalPromotions { get; set; }
        public int ActivePromotions { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal ActualRevenue { get; set; }

        public List<string> ChartLabels { get; set; }
        public List<decimal> ChartData { get; set; }

        public List<string> PieLabels { get; set; }
        public List<int> PieData { get; set; }

        public List<PromotionDetailReport> PromotionDetails { get; set; }

        public KhuyenMaiReport()
        {
            ChartLabels = new List<string>();
            ChartData = new List<decimal>();
            PieLabels = new List<string>();
            PieData = new List<int>();
            PromotionDetails = new List<PromotionDetailReport>();
        }
    }
}
