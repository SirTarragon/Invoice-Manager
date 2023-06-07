/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/05/2023
 * */

using IC_Assignment.Models;
using System.Xml;
using System.Xml.Serialization;

namespace IC_Assignment.Services
{
    public class BillFileManager
    {
        private XmlDocument xmlDoc;
        private XmlSerializer xmlSerializer;
        private BillRptSerializer rptSerializer;
        public List<BillHeader> Bills { get; private set; }

        public BillFileManager()
        {
            xmlDoc = new XmlDocument();
            xmlSerializer = new XmlSerializer(typeof(BillDataset));
            rptSerializer = new BillRptSerializer();
        }

        public BillFileManager(string file) : this()
        {
            ImportXMLData(file);
        }

        public bool ImportXMLData(string file)
        {
            try
            {
                xmlDoc.Load(file);
                Console.WriteLine("Loaded file");

                // could overcomplicate and use a list/array of dictionaries
                // with a list of keys, etc, etc, and parse it myself but it's just
                // easier to manage this as a class and utilize the deserialize functionality
                using (var reader = new XmlNodeReader(xmlDoc))
                {
                    Console.WriteLine("Retrieving Data");
                    Bills = ((BillDataset)xmlSerializer.Deserialize(reader)).Bills;
                    Console.WriteLine("Retrieved Data");
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ClearBills();
                return false;
            }
        }

        public bool ExportToRPT() => ExportToRPT(Directory.GetCurrentDirectory());

        public bool ExportToRPT(string directoryPath)
        {
            // should never be the case, but just in case...
            if (Bills == null || Bills.Count == 0) { return false; }

            return rptSerializer.Serialize(Bills, directoryPath);
        }

        public List<BillHeader> ListBills() => Bills;
        public void ClearBills() => Bills.Clear();
    }
}
