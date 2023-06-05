using System.Xml.Serialization;

namespace IC_Assignment.Models
{
    [XmlRoot(ElementName = "Address_Information")]
    public class AddressInfoClass
    {
        [XmlElement(ElementName = "Mailing_Address_1")]
        public string MailingAddress1 { get; set; }
        [XmlElement(ElementName = "Mailing_Address_2")]
        public string MailingAddress2 { get; set; }
        [XmlElement(ElementName = "City")]
        public string City { get; set; }
        [XmlElement(ElementName = "State")]
        public string State { get; set; }
        [XmlElement(ElementName = "Zip")]
        public uint Zip { get; set; }
    }
}
