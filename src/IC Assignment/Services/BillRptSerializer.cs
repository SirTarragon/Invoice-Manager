/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/07/2023
 * */

using IC_Assignment.Models;
using System.Text;

namespace IC_Assignment.Services
{
    // have to make my own, not sure how to do it properly
    // with inherit
    public class BillRptSerializer
    {
        public BillRptSerializer() { }

        public bool Serialize(List<BillHeader> bills, string directoryPath)
        {
            if (bills is null || bills.Count == 0)
            {
                throw new ArgumentNullException(nameof(bills));
            }
            if (string.IsNullOrEmpty(directoryPath) || directoryPath.Length == 0)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            // assigned values from document needed in RPT file
            string Key2 = "8203ACC7-2094-43CC-8F7A-B8F19AA9BDA2";
            string JJ = "8E2FEA69-5D77-4D0F-898E-DFA25677D19E";

            // go through and sum the billamount
            decimal totalBillAmount = bills.Sum(billHeader => billHeader.Bill.BillAmount);

            DateTime today = DateTime.Today;
            string todayString = today.ToString("MM/dd/yyyy"); // need to enforce format

            // create builder and initial line for RPT file
            var report = new StringBuilder(
            $"1~FR|2~{Key2}|3~Sample UT file|4~{todayString}|5~{bills.Count}|6~{totalBillAmount}\n");

            // now need to write to the RPT file
            string filename = $"BillFile-{today.ToString("MMddyyyy")}.rpt";
            string filePath = Path.Combine(directoryPath, filename);

            try
            {
                if (!File.Exists(filePath)) // prevent duplicate writing of header line
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.Write(report.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            report.Clear();

            var serviceAddress = new StringBuilder();

            // writing and clearing just in case there's a large amount of bills,
            // because there is a set limit for SringBuilder in memory. Int32.Max
            try
            {
                using (var writer = new StreamWriter(filePath, append: true))
                {
                    foreach (var bill in bills)
                    {
                        DateTime notificationOne = today.AddDays(5);
                        string notifOneString = notificationOne.ToString("MM/dd/yyyy");

                        DateTime notificationTwo = bill.DueDt.AddDays(-3);
                        string notifTwoString = notificationTwo.ToString("MM/dd/yyyy");

                        string billDt = bill.BillDt.ToString("MM/dd/yyyy");
                        string dueDt = bill.DueDt.ToString("MM/dd/yyyy");

                        // wouldn't want to be creating a new StringBuilder every loop
                        // but concatenating or Clear() wouldn't be much better.
                        // i think the service address is just the combined mailing address
                        // but not 100% that this is the wanted formatting
                        // 1234 Address, Apartment 1, City, State, 12345
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
                            .Append($"|QQ~{bill.Bill.BalanceDue}")  // my guess the balance due is the account balance, probably wrong
                            .Append($"|RR~{todayString}")
                            .Append($"|SS~{serviceAddress.ToString()}\n");

                        writer.Write(report.ToString());

                        report.Clear();
                        serviceAddress.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public List<BillRptData> Deserialize(string filename)
        {   // should be faster for the computer to deserialize then insert into database
            // than doing both at once
            List<BillRptData> output = new List<BillRptData>();
            if (!filename.Contains(".rpt")) throw new Exception("Invalid filetype ending.");
            else if (string.IsNullOrEmpty(filename)) throw new Exception("Error, filename is empty.");

            using (var reader = new StreamReader(filename))
            {
                string check = reader.ReadLine(); // should be the initial line
                                                  // don't need to do much with it currently

                // basic initial check for formatting, won't hold up if there are changes elsewhere
                if (string.IsNullOrEmpty(check) ||
                    !(check[0] == '1' && check[1] == '~' && check[2] == 'F' && check[3] == 'R'))
                    throw new Exception("File does not meet expected format"); // should be null

                // a lot of assumptions that the file hasn't been manually changed out of format
                string customer, bill;  // contains data from the given line
                while ((customer = reader.ReadLine()) != null && (bill = reader.ReadLine()) != null)
                {
                    // likely doesn't help runtime, but, it's easier to understand
                    // what's going on by doing this
                    string[] customerInfo = customer.Split('|');
                    string[] billingInfo = bill.Split('|');

                    // prevents overreading or hopefully possible inaccurate formatting
                    if (customerInfo.Length < 8 || billingInfo.Length < 12)
                        break;

                    int keyLength = 3;

                    var data = new BillRptData();

                    // customer
                    data.AccountNumber = customerInfo[1].Substring(keyLength);
                    data.CustomerName = customerInfo[2].Substring(keyLength);
                    data.MailAddress1 = customerInfo[3].Substring(keyLength);
                    data.MailAddress2 = customerInfo[4].Substring(keyLength);
                    data.City = customerInfo[5].Substring(keyLength);
                    data.State = customerInfo[6].Substring(keyLength);
                    data.Zip = customerInfo[7].Substring(keyLength);

                    // billing
                    data.FormatGUID = billingInfo[2].Substring(keyLength);
                    data.InvoiceNumber = billingInfo[3].Substring(keyLength);

                    // time to parse date and make sure data is expected
                    // if not, throw exception with custom message to help with identifying issue
                    try
                    {
                        data.BillDt = DateTime.ParseExact(
                            billingInfo[4].Substring(keyLength), "MM/dd/yyyy", null);
                    }
                    catch
                    {
                        throw new Exception("Check all LL keys; invalid formatting. Expecting 'MM/dd/yyyy' format.");
                    }
                    try
                    {
                        data.DueDt = DateTime.ParseExact(
                            billingInfo[5].Substring(keyLength), "MM/dd/yyyy", null);
                    }
                    catch
                    {
                        throw new Exception("Check all MM keys; invalid formatting. Expecting 'MM/dd/yyyy' format.");
                    }

                    data.BillAmount = billingInfo[6].Substring(keyLength);

                    try
                    {
                        data.NotifOne = DateTime.ParseExact(
                            billingInfo[7].Substring(keyLength), "MM/dd/yyyy", null);
                    }
                    catch
                    {
                        throw new Exception("Check all OO keys; invalid formatting. Expecting 'MM/dd/yyyy' format.");
                    }
                    try
                    {
                        data.NotifTwo = DateTime.ParseExact(
                            billingInfo[8].Substring(keyLength), "MM/dd/yyyy", null);
                    }
                    catch
                    {
                        throw new Exception("Check all PP keys; invalid formatting. Expecting 'MM/dd/yyyy' format.");
                    }

                    data.BalanceDue = billingInfo[9].Substring(keyLength);

                    try
                    {
                        data.DateAdded = DateTime.ParseExact(
                            billingInfo[10].Substring(keyLength), "MM/dd/yyyy", null);
                    }
                    catch
                    {
                        throw new Exception("Check all RR keys; invalid formatting. Expecting 'MM/dd/yyyy' format.");
                    }

                    data.ServiceAddress = billingInfo[11].Substring(keyLength);

                    output.Add(data);

                }
            }

            return output;
        }
    }
}
