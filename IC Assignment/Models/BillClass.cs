using IC_Assignment.Helpers;
using System.Xml.Serialization;

namespace IC_Assignment.Models
{
    [XmlRoot(ElementName = "Bill")]
    public class BillClass
    {
        [XmlElement(ElementName = "Bill_Amount")]
        public decimal BillAmount { get; set; }
        [XmlElement(ElementName = "Balance_Due")]
        public decimal BalanceDue { get; set; }
        [XmlElement(ElementName = "Bill_Run_Dt")]
        private string billRunDt { get; set; }
        public DateTime BillRunDt
        { 
            get
            {
                return DateTime.ParseExact(StringDateFixer.FixStringDate(billRunDt), "MMM-dd-yyyy", null);
            }
        }
        [XmlElement(ElementName = "Bill_Run_Seq")]
        public uint BillRunSeq { get; set; }
        [XmlElement(ElementName = "Bill_Run_Tm")]
        public uint BillRunTm { get; set; }
        [XmlElement(ElementName = "Bill_Tp")]
        public string BillTp { get; set; }
    }
}
