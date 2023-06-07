/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/06/2023
 * */

using IC_Assignment.Models;
using System.Data.OleDb;
using System.Runtime.Versioning;
using System.Text;

namespace IC_Assignment.Services
{

    /* Database schema
            CREATE TABLE [Bills]
            (
                        [ID]                    Long Integer, // this is just an int?
                        [BillDate]              DateTime,
                        [BillNumber]            Text (255),     // okay so meant to be a string, good
                        [BillAmount]            Currency,
                        [FormatGUID]            Memo/Hyperlink (255),
                        [AccountBalance]        Currency,
                        [DueDate]               DateTime,
                        [ServiceAddress]        Text (255),
                        [FirstEmailDate]        DateTime,
                        [SecondEmailDate]       DateTime,
                        [DateAdded]             DateTime NOT NULL,
                        [CustomerID]            Long Integer
            );

            CREATE TABLE [Customer]
            (
                        [ID]                    Long Integer,
                        [CustomerName]          Text (255),
                        [AccountNumber]         Text (255),     
                        [CustomerAddress]       Text (255),
                        [CustomerCity]          Text (255),
                        [CustomerState]         Text (255),
                        [CustomerZip]           Text (255),
                        [DateAdded]             DateTime NOT NULL
            );
    */

    [SupportedOSPlatform("windows")] // cuts down on warnings, mainly due to OleDb
    public class DatabaseManager
    {
        private const string defProvider = "Microsoft.ACE.OLEDB.12.0";
        private const string defDBLoc = @"D:\Personal Projects\IC Assignment\!Provided Files\Billing.mdb";
        private string connectionString;
        private int BillIDCounter, CustomerIDCounter;
        private BillRptSerializer rptSerializer;

        // default string needs to be changed depending on expected default location
        public DatabaseManager(string connectionString = 
            $@"Provider={defProvider};Data Source=""{defDBLoc}"";")
        {
            rptSerializer = new BillRptSerializer();
            if(LoadConnection(connectionString))
            {
                Console.WriteLine("No issue with database connection.");
            }
        }

        public bool LoadConnection(string s)
        {
            try
            {
                // most of my time today was trying to figure out issue with ODBC
                // swapped to OleDb and had no further issues
                using (var connection = new OleDbConnection(s))
                {
                    connection.Open();
                    // if it succeeded, then save the string
                    this.connectionString = s;

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(Bills.ID) AS MaxBillsID, MAX(Customer.ID) AS MaxCustomerID FROM Bills, Customer";
                        using (var reader = command.ExecuteReader())
                        {
                            // need to initialize the service counters to distribute IDs
                            if (reader.HasRows)
                            {
                                reader.Read();

                                BillIDCounter = reader.IsDBNull(reader.GetOrdinal("MaxBillsID")) ? 1 : 
                                                reader.GetInt32(reader.GetOrdinal("MaxBillsID")) + 1;
                                CustomerIDCounter = reader.IsDBNull(reader.GetOrdinal("MaxCustomerID")) ? 1 :
                                                    reader.GetInt32(reader.GetOrdinal("MaxCustomerID")) + 1;

                                return true;
                            }
                            else
                            {
                                BillIDCounter = -1;
                                CustomerIDCounter = -1;

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool UpdateFromRPT(string filename)
        {
            if (!filename.Contains(".rpt")) return false;
            if (BillIDCounter == -1) throw new Exception("Attempt to update failed due to failure to initially connect to database.");

            var listData = new List<BillRptData>();

            // deserialize the data, mainly to make this function cleaner
            // and to separate the functionality
            try
            {
                listData = rptSerializer.Deserialize(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine (e);
                return false;
            }

            // there's probably a better way of doing this
            foreach(var data in listData)
            {
                int customerID = CustomerIDCounter;
                try
                {
                    // search for active customer information based on account number
                    using (var connection = new OleDbConnection(connectionString))
                    {
                        int checkAccNumCount = 0;
                        connection.Open();

                        // Check if the account number exists in the Customers table
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT COUNT(*) FROM Customer WHERE AccountNumber = ?";
                            _ = command.Parameters.AddWithValue("AccountNumberParam", data.AccountNumber);
                            checkAccNumCount = Convert.ToInt32(command.ExecuteScalar());
                        }

                        if (checkAccNumCount > 1)
                        { // for some reason there's a duplicate accountNumber
                            throw new Exception($"There's a duplicate Account Number in the database! Inform IT about {data.AccountNumber}!");
                        }
                        else if (checkAccNumCount == 1)
                        { // it exists, get the ID
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "SELECT ID FROM Customer WHERE AccountNumber = ?";
                                _ = command.Parameters.AddWithValue("AccountNumberParam", data.AccountNumber);
                                customerID = Convert.ToInt32(command.ExecuteScalar());
                            }
                        }
                        else
                        {   // time to insert into the table the new customer information
                            InsertCustomerTable(customerID, connection, data.AccountNumber, data.CustomerName,
                                data.MailAddress1, data.MailAddress2, data.City, data.State, data.Zip, data.DateAdded.ToString("MM/dd/yyyy"));

                            Console.WriteLine("Submitted new customer data to database...");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }

                try
                {
                    using (var connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();

                        // not sure if duplicate invoice numbers are a thing/allowed, but it should be a simple change
                        // if it isn't meant to be

                        InsertBillsTable(customerID, connection, data.FormatGUID, data.InvoiceNumber,
                            data.BillDt.ToString("MM/dd/yyyy"), data.DueDt.ToString("MM/dd/yyyy"), data.BillAmount, 
                            data.NotifOne.ToString("MM/dd/yyyy"), data.NotifTwo.ToString("MM/dd/yyyy"),
                            data.BalanceDue, data.DateAdded.ToString("MM/dd/yyyy"), data.ServiceAddress);

                        Console.WriteLine("Submitted new billing data to database...");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        private void InsertBillsTable(int customerID, OleDbConnection connection, 
            string formatGUID, string invoiceNumber, string billDate, string dueDate, string billAmount, string firstNotifDate,
            string secondNotifDate, string balanceDue, string dateAdded, string serviceAddress)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO Bills (ID, BillDate, BillNumber, BillAmount, 
                FormatGUID, AccountBalance, DueDate, ServiceAddress, FirstEmailDate, SecondEmailDate, 
                DateAdded, CustomerID) VALUES (?,?,?,?,?,?,?,?,?,?,?,?)";

                _ = command.Parameters.AddWithValue("IDParam", BillIDCounter++);
                _ = command.Parameters.AddWithValue("BillDateParam", billDate);
                _ = command.Parameters.AddWithValue("BillNumberParam", invoiceNumber);
                _ = command.Parameters.AddWithValue("BillAmountParam", billAmount);
                _ = command.Parameters.AddWithValue("FormatGUIDParam", formatGUID);
                _ = command.Parameters.AddWithValue("AccountBalanceParam", balanceDue);
                _ = command.Parameters.AddWithValue("DueDateParam", dueDate);
                _ = command.Parameters.AddWithValue("ServiceAddressParam", serviceAddress);
                _ = command.Parameters.AddWithValue("FirstEmailDateParam", firstNotifDate);
                _ = command.Parameters.AddWithValue("SecondEmailDateParam", secondNotifDate);
                _ = command.Parameters.AddWithValue("DateAddedParam", dateAdded);
                _ = command.Parameters.AddWithValue("CustomerIDParam", customerID);

                _ = command.ExecuteNonQuery();
            }
        }

        private void InsertCustomerTable(int customerID, OleDbConnection connection, 
            string checkAccountNumber, string name, string mailAddr1, string mailAddr2, string city, string state, string zip, string dateAdded)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO Customer (ID, CustomerName, AccountNumber, CustomerAddress, 
                    CustomerCity, CustomerState, CustomerZip, DateAdded) VALUES (?,?,?,?,?,?,?,?)";
                _ = command.Parameters.AddWithValue("IDParam", customerID);
                _ = command.Parameters.AddWithValue("CustomerNameParam", name);
                _ = command.Parameters.AddWithValue("AccountNumberParam", checkAccountNumber);
                string customerAddr = mailAddr1;
                if (!string.IsNullOrEmpty(mailAddr2))
                {
                    customerAddr += ", " + mailAddr2;
                }
                _ = command.Parameters.AddWithValue("CustomerAddressParam", customerAddr);
                _ = command.Parameters.AddWithValue("CustomerCityParam", city);
                _ = command.Parameters.AddWithValue("CustomerStateParam", state);
                _ = command.Parameters.AddWithValue("CustomerZipParam", zip);
                _ = command.Parameters.AddWithValue("DateAddedParam", dateAdded);

                _ = command.ExecuteNonQuery();

                CustomerIDCounter++;
            }
        }

        public bool ExportAsCSV(string dirName)
        {
            if (BillIDCounter == -1) throw new Exception("Attempt to export failed due to failure to initially connect to database.");

            // not sure if it just wants to be ordered by Customer ID
            // or if it wants the Bills information done like a coalesce.
            // tried it as a coalesce but it's not an available functionality with this db
            string query = @"
            SELECT 
                Customer.ID,
                Customer.CustomerName,
                Customer.AccountNumber,
                Customer.CustomerAddress,
                Customer.CustomerCity,
                Customer.CustomerState,
                Customer.CustomerZip,
                Bills.ID AS BillID,
                Bills.BillDate,
                Bills.BillNumber,
                Bills.AccountBalance,
                Bills.DueDate,
                Bills.BillAmount,
                Bills.FormatGUID,
                Customer.DateAdded
            FROM 
                Customer
            LEFT JOIN 
                Bills ON Customer.ID = Bills.CustomerID";

            string header = "Customer.ID,Customer.CustomerName,Customer.AccountNumber,Customer.CustomerAddress,Customer.CustomerCity," +
                "Customer.CustomerState,Customer.CustomerZip,Bills.ID,Bills.BillDate,Bills.BillNumber,Bills.AccountBalance,Bills.DueDate," +
                "Bills.BillAmount,Bills.FormatGUID,Customer.DateAdded";

            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;

                        using (var reader = command.ExecuteReader())
                        {
                            // billing report could simply be without the added MMddyyyy, but felt
                            // like it could do with having that, especially if you potentially want to keep that as another record
                            string path = Path.Combine(dirName, $"BillingReport-{DateTime.Today.ToString("MMddyyyy")}.txt");

                            // though we don't want to append to it with more data if it already exists
                            if(File.Exists(path)) File.Delete(path);

                            using (var writer = new StreamWriter(path, true))
                            {
                                // Write header line
                                writer.WriteLine(header);

                                var builder = new StringBuilder();

                                // Write data lines
                                while (reader.Read())
                                {
                                    builder.Append(reader["ID"]).Append(",")
                                        .Append($@"""{reader["CustomerName"]}""").Append(",")
                                        .Append($@"""{reader["AccountNumber"]}""").Append(",")
                                        .Append($@"""{reader["CustomerAddress"]}""").Append(",")
                                        .Append($@"""{reader["CustomerCity"]}""").Append(",")
                                        .Append($@"""{reader["CustomerState"]}""").Append(",")
                                        .Append(reader["CustomerZip"]).Append(",")
                                        .Append(reader["BillID"]).Append(",")
                                        .Append($@"""{reader["BillDate"]}""").Append(",")
                                        .Append($@"""{reader["BillNumber"]}""").Append(",")
                                        .Append(reader["AccountBalance"]).Append(",")
                                        .Append($@"""{reader["DueDate"]}""").Append(",")
                                        .Append(reader["BillAmount"]).Append(",")
                                        .Append($@"""{reader["FormatGUID"]}""").Append(",")
                                        .Append($@"""{reader["DateAdded"]}""");
                                    writer.WriteLine(builder.ToString());
                                    builder.Clear();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
