/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/05/2023
 * */

using System.Xml.Serialization;

namespace IC_Assignment.Models
{
    // this should allow me to get the serializer in Services/BillFileParser
    // to get the data in a way that i would want it
    // there's probably a better way of doing this
    [XmlRoot(ElementName = "BILL_HEADER_Dataset")]
    public class BillDataset
    {
        [XmlElement(ElementName = "BILL_HEADER")]
        public List<BillHeader> Bills { get; set; } // important thing from it
    }
}
