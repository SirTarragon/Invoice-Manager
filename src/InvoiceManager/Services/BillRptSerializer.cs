/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/07/2023
 * */

using CustomExtensions;
using InvoiceManager.Models;
using System.Text;

namespace InvoiceManager.Services
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
                            .Append("HH~IH|II~R") // bill invoice
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
                string readRpt;
                while ((readRpt = reader.ReadLines(2)) != null)
                {
                    // likely doesn't help runtime, but, it's easier to understand
                    // what's going on by doing this
                    string[] billData = readRpt.Split('|');

                    // prevents overreading or hopefully possible inaccurate formatting
                    if (billData.Length < 19) break;

                    try
                    {
                        var parsedData = ExtractData(billData);
                        output.Add(parsedData);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return output;
        }

        private static BillRptData ExtractData(string[] arrPairs, int keyLength = 3)
        {
            var data = new BillRptData();

            for (int i = 0; i < arrPairs.Length; i++)
            {
                // check for valid key and assign data based on it
                string key = $"{arrPairs[i][0]}{arrPairs[i][1]}{arrPairs[i][2]}";
                switch (key)
                {
                    case "AA~":
                        break;
                    case "BB~":
                        data.AccountNumber = arrPairs[i].Substring(keyLength);
                        break;
                    case "VV~":
                        data.CustomerName = arrPairs[i].Substring(keyLength);
                        break;
                    case "CC~":
                        data.MailAddress1 = arrPairs[i].Substring(keyLength);
                        break;
                    case "DD~":
                        data.MailAddress2 = arrPairs[i].Substring(keyLength);
                        break;
                    case "EE~":
                        data.City = arrPairs[i].Substring(keyLength);
                        break;
                    case "FF~":
                        data.State = arrPairs[i].Substring(keyLength);
                        break;
                    case "GG~":
                        data.Zip = arrPairs[i].Substring(keyLength);
                        break;
                    case "HH~":
                        break;
                    case "II~":
                        break;
                    case "JJ~":
                        data.FormatGUID = arrPairs[i].Substring(keyLength);
                        break;
                    case "KK~":
                        data.InvoiceNumber = arrPairs[i].Substring(keyLength);
                        break;
                    case "LL~":
                        try
                        { data.BillDt = DateTime.ParseExact(arrPairs[i].Substring(keyLength), "MM/dd/yyyy", null); }
                        catch
                        { throw new Exception("Check all LL keys; invalid formatting. Expecting 'MM/dd/yyyy' format."); }
                        break;
                    case "MM~":
                        try
                        { data.DueDt = DateTime.ParseExact(arrPairs[i].Substring(keyLength), "MM/dd/yyyy", null); }
                        catch
                        { throw new Exception("Check all MM keys; invalid formatting. Expecting 'MM/dd/yyyy' format."); }
                        break;
                    case "NN~":
                        data.BillAmount = arrPairs[i].Substring(keyLength);
                        break;
                    case "OO~":
                        try
                        { data.NotifOne = DateTime.ParseExact(arrPairs[i].Substring(keyLength), "MM/dd/yyyy", null); }
                        catch
                        { throw new Exception("Check all OO keys; invalid formatting. Expecting 'MM/dd/yyyy' format."); }
                        break;
                    case "PP~":
                        try
                        { data.NotifTwo = DateTime.ParseExact(arrPairs[i].Substring(keyLength), "MM/dd/yyyy", null); }
                        catch
                        { throw new Exception("Check all PP keys; invalid formatting. Expecting 'MM/dd/yyyy' format."); }
                        break;
                    case "QQ~":
                        data.BalanceDue = arrPairs[i].Substring(keyLength);
                        break;
                    case "RR~":
                        try
                        { data.DateAdded = DateTime.ParseExact(arrPairs[i].Substring(keyLength), "MM/dd/yyyy", null); }
                        catch
                        { throw new Exception("Check all RR keys; invalid formatting. Expecting 'MM/dd/yyyy' format."); }
                        break;
                    case "SS~":
                        data.ServiceAddress = arrPairs[i].Substring(keyLength);
                        break;
                    default:
                        throw new Exception($"Invalid Key: {key}");
                }
            }

            return data;
        }
    }
}
