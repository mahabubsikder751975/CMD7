namespace Cloud7CMS.Models
{
    public class LiveDataModel
    {
        public DateTime ReportDate { get; set; }
        public int ReportHour { get; set; }
        public int ReportMinute { get; set; }
        public int ReportSecond { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalTraffic { get; set; }


    }
}
