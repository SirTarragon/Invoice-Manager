using IC_Assignment.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IC_Assignment.Services
{
    public class BillFileManager
    {
        private XmlDocument xmlDoc;
        private XmlSerializer serializer;
        public List<BillHeader> Bills { get; private set; }

        public BillFileManager()
        {
            xmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(BillDataset));
        }

        public BillFileManager(string file)
        {
            xmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(BillDataset));
            if (!Load(file))
                Console.WriteLine("ERROR IN BILLFILEMANAGER: Issue with loading XML file." +
                    " Check if it's correct format or the file selected is indeed a XML file.");
        }

        public bool Load(string file)
        {
            try
            {
                xmlDoc.Load(file);
                Console.WriteLine("Loaded file");

                using (var reader = new XmlNodeReader(xmlDoc))
                {
                    Console.WriteLine("Retrieving Data");
                    Bills = ((BillDataset)serializer.Deserialize(reader)).Bills;
                    Console.WriteLine("Retrieved Data");
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred.");
                Console.WriteLine(e);
                return false;
            }
        }

        public bool Save() => Save(Directory.GetCurrentDirectory());

        public bool Save(string directoryPath)
        {
            if (Bills == null || Bills.Count == 0) { return false; }

            // assigned values from document needed in RPT file
            string Key2 = "8203ACC7-2094-43CC-8F7A-B8F19AA9BDA2";
            string JJ = "8E2FEA69-5D77-4D0F-898E-DFA25677D19E";

            // go through and sum the billamount
            decimal totalBillAmount = Bills.Sum(billHeader => billHeader.Bill.BillAmount);

            DateTime today = DateTime.Today;
            string todayString = today.ToString("MM/dd/yyyy"); // need to enforce format

            DateTime notificationOne = today.AddDays(5);
            string notifOneString = notificationOne.ToString("MM/dd/yyyy");

            // create builder and initial line for RPT file
            StringBuilder report = new StringBuilder(
                $"1~FR|2~{Key2}|3~Sample UT file|4~{todayString}|5~{Bills.Count}|6~{totalBillAmount}\n");

            foreach (var bill in Bills)
            {
                DateTime notificationTwo = bill.DueDt.AddDays(-3);
                string notifTwoString = notificationTwo.ToString("MM/dd/yyyy");

                string billDt = bill.BillDt.ToString("MM/dd/yyyy");
                string dueDt = bill.DueDt.ToString("MM/dd/yyyy");

                string serviceAddress = string.Empty;

                // unknown input size, append better than concatenate due to GC memory
                // still doesn't make this appealing to look at though
                report.Append("AA~CT") // customer info
                    .Append($"|BB~{bill.AccountNo}")
                    .Append($"|VV~{bill.CustomerName}")
                    .Append($"|CC~{bill.AddressInfo.MailingAddress1}")
                    .Append($"|DD~{bill.AddressInfo.MailingAddress2}")
                    .Append($"|EE~{bill.AddressInfo.City}")
                    .Append($"|FF~{bill.AddressInfo.State}")
                    .Append($"|GG~{bill.AddressInfo.Zip}\n")
                    .Append($"HH~IH|II~R") // bills
                    .Append($"|JJ~{JJ}")
                    .Append($"|KK~{bill.InvoiceNo}")
                    .Append($"|LL~{billDt}")
                    .Append($"|MM~{dueDt}")
                    .Append($"|NN~{bill.Bill.BillAmount}")
                    .Append($"|OO~{notifOneString}")
                    .Append($"|PP~{notifTwoString}")
                    .Append($"|QQ~{bill.Bill.BalanceDue}")
                    .Append($"|RR~{todayString}")
                    .Append($"|SS~{serviceAddress}\n");
            }

            // now need to write to the RPT file
            string filename = $"BillFile-{today.ToString("MMddyyyy")}.rpt";

            try
            {
                string filePath = Path.Combine(directoryPath, filename);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(report.ToString());
                }

                Console.WriteLine($"Wrote to {filePath}");
            }
            catch
            {
                return false;
            }

            return true;
        }

        public List<BillHeader> ListBills() => Bills;
    }
}
