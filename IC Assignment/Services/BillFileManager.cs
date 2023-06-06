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

        public BillFileManager(string file) : this()
        {
            xmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(BillDataset));
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
                    Bills = ((BillDataset)serializer.Deserialize(reader)).Bills;
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

            // now need to write to the RPT file
            string filename = $"BillFile-{today.ToString("MMddyyyy")}.rpt";
            string filePath = Path.Combine(directoryPath, filename);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(report.ToString());
                }
            }
            catch
            {
                return false;
            }

            report.Clear();

            var serviceAddress = new StringBuilder();

            // writing and clearing just in case there's a large amount of bills,
            // because there is a set limit for SringBuilder in memory. Int32.Max
            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                foreach (var bill in Bills)
                {
                    DateTime notificationTwo = bill.DueDt.AddDays(-3);
                    string notifTwoString = notificationTwo.ToString("MM/dd/yyyy");

                    string billDt = bill.BillDt.ToString("MM/dd/yyyy");
                    string dueDt = bill.DueDt.ToString("MM/dd/yyyy");

                    // wouldn't want to be creating a new StringBuilder every loop
                    // but concatenating or Clear() wouldn't be much better.
                    // i think the service address is just the combined mailing address
                    // but it's probably supposed to be something different
                    serviceAddress.Append(bill.AddressInfo.MailingAddress1).Append(", ");
                    if (!string.IsNullOrEmpty(bill.AddressInfo.MailingAddress2))
                        serviceAddress.Append(bill.AddressInfo.MailingAddress2).Append(", ");
                    serviceAddress.Append(bill.AddressInfo.City).Append(", ")
                        .Append(bill.AddressInfo.State).Append(", ")
                        .Append(bill.AddressInfo.Zip);

                    // unknown input size, append better than concatenate due to GC memory
                    // still doesn't make this appealing to look at though
                    // and definitely not a begin fan of it. would still need something like
                    // this but probably iterating through a list of keys if I had gone with a
                    // dictionary route
                    report.Append("AA~CT") // customer info
                        .Append($"|BB~{bill.AccountNo}")
                        .Append($"|VV~{bill.CustomerName}")
                        .Append($"|CC~{bill.AddressInfo.MailingAddress1}")
                        .Append($"|DD~{bill.AddressInfo.MailingAddress2}")
                        .Append($"|EE~{bill.AddressInfo.City}")
                        .Append($"|FF~{bill.AddressInfo.State}")
                        .Append($"|GG~{bill.AddressInfo.Zip}\n")
                        .Append($"HH~IH|II~R") // bill invoice
                        .Append($"|JJ~{JJ}")
                        .Append($"|KK~{bill.InvoiceNo}")
                        .Append($"|LL~{billDt}")
                        .Append($"|MM~{dueDt}")
                        .Append($"|NN~{bill.Bill.BillAmount}")
                        .Append($"|OO~{notifOneString}")
                        .Append($"|PP~{notifTwoString}")
                        .Append($"|QQ~{bill.Bill.BalanceDue}")
                        .Append($"|RR~{todayString}")
                        .Append($"|SS~{serviceAddress.ToString()}\n");

                    try
                    {
                        writer.Write(report.ToString());
                    }
                    catch (Exception e)
                    {   // would need better error handling here
                        // this shouldn't really ever be the case
                        return false;
                    }

                    report.Clear();
                    serviceAddress.Clear();
                }
            }

            return true;
        }

        public List<BillHeader> ListBills() => Bills;
        public void ClearBills() => Bills.Clear();
    }
}
