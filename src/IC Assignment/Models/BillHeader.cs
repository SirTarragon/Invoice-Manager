/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/05/2023
 * */

using IC_Assignment.Helpers;
using System.Xml.Serialization;

namespace IC_Assignment.Models
{
    [XmlRoot(ElementName = "BILL_HEADER")]
    public class BillHeader
    {
        [XmlElement(ElementName = "Invoice_No")]
        public string InvoiceNo { get; set; }   // most of the billfile.xml case could be numbers
                                                // but one of them is effectively a string, don't know if it's
                                                // supposed to actually be an unaccepted case
        [XmlElement(ElementName = "Account_No")]
        public string AccountNo { get; set; }
        [XmlElement(ElementName = "Customer_Name")]
        public string CustomerName { get; set; }
        [XmlElement(ElementName = "Cycle_Cd")]
        public uint CycleCd { get; set; }
        [XmlElement(ElementName = "Bill_Dt")]
        public string billDt { get; set; }
        public DateTime BillDt
        {
            get
            {
                return DateTime.ParseExact(StringDateFixer.FixStringDate(billDt), "MMM-dd-yyyy", null);
            }
        }
        [XmlElement(ElementName = "Due_Dt")]
        public string dueDt { get; set; }
        public DateTime DueDt
        {
            get
            {
                return DateTime.ParseExact(StringDateFixer.FixStringDate(dueDt), "MMM-dd-yyyy", null);
            }
        }
        [XmlElement(ElementName = "Bill")]
        public BillClass Bill { get; set; }
        [XmlElement(ElementName = "Address_Information")]
        public AddressInfoClass AddressInfo { get; set; }

        [XmlElement(ElementName = "Account_Class")]
        public string AccountClass { get; set; }
    }
}
