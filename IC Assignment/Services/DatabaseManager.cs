using System.Data.OleDb;
using System.Runtime.Versioning;

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

    [SupportedOSPlatform("windows")]
    public class DatabaseManager
    {
        private string connectionString;
        private int BillIDCounter, CustomerIDCounter;

        public DatabaseManager()
        {
            this.connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=""D:\Personal Projects\IC Assignment\!Provided Files\Billing.mdb"";";
            if (LoadConnection(this.connectionString))
                Console.WriteLine("No issue with connection.");
        }

        public DatabaseManager(string connectionString)
        {
            if(LoadConnection(connectionString))
            {
                this.connectionString = connectionString;
                Console.WriteLine("No issue with connection.");
            }
        }

        public bool LoadConnection(string s)
        {
            Console.WriteLine(s);
            try
            {
                // had issues with ODBC, a lot better time with OleDB though
                using (var connection = new OleDbConnection(s))
                {
                    connection.Open();

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
                                BillIDCounter = 1;
                                CustomerIDCounter = 1;

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

            using (StreamReader reader = new StreamReader(filename))
            {
                string check = reader.ReadLine(); // should be the initial line
                                                  // don't need to do much with it currently

                // basic initial check for formatting, won't hold up if there are changes elsewhere
                if (!(check[0] == '1' && check[1] == '~' && check[2] == 'F' && check[3] == 'R'))
                    return false;

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
                        return false;

                    int customerID = CustomerIDCounter;
                    int checkAccNumCount = 0;
                    int keyLength = 3;

                    try
                    {
                        // search for active customer information based on account number
                        using (var connection = new OleDbConnection(connectionString))
                        {
                            // we know where the delimiters are going to be for each string,
                            // so just better to substring instead of adding array memory overhang
                            // could easily be done with a .Split("~")[1] though
                            string checkAccountNumber = customerInfo[1].Substring(keyLength);
                            connection.Open();

                            // Check if the account number exists in the Customers table
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "SELECT COUNT(*) FROM Customer WHERE AccountNumber = ?";
                                _ = command.Parameters.AddWithValue("AccountNumberParam", checkAccountNumber);
                                checkAccNumCount = Convert.ToInt32(command.ExecuteScalar());
                            }

                            // it exists, get the ID
                            if (checkAccNumCount > 0)
                            {
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = "SELECT ID FROM Customer WHERE AccountNumber = ?";
                                    _ = command.Parameters.AddWithValue("AccountNumberParam", checkAccountNumber);
                                    customerID = Convert.ToInt32(command.ExecuteScalar());
                                }
                            }
                            else
                            {   // time to insert into the table the new customer information
                                string name = customerInfo[2].Substring(keyLength);
                                string mailAddr1 = customerInfo[3].Substring(keyLength);
                                string mailAddr2 = customerInfo[4].Substring(keyLength);
                                string city = customerInfo[5].Substring(keyLength);
                                string state = customerInfo[6].Substring(keyLength);
                                string zip = customerInfo[7].Substring(keyLength);

                                InsertCustomerTable(customerID, connection, checkAccountNumber, name,
                                    mailAddr1, mailAddr2, city, state, zip);
                            }
                        }
                    }
                    catch(Exception e) 
                    {
                        Console.WriteLine(e);
                        return false;
                    }

                    try
                    {
                        using (var connection = new OleDbConnection(connectionString))
                        {
                            connection.Open();

                            string formatGUID = billingInfo[2].Substring(keyLength);
                            string invoiceNumber = billingInfo[3].Substring(3);
                            string billDate = billingInfo[4].Substring(keyLength);
                            string dueDate = billingInfo[5].Substring(keyLength);
                            string billAmount = billingInfo[6].Substring(keyLength);
                            string firstNotifDate = billingInfo[7].Substring(keyLength);
                            string secondNotifDate = billingInfo[8].Substring(keyLength);
                            string balanceDue = billingInfo[9].Substring(keyLength);
                            string dateAdded = billingInfo[10].Substring(keyLength);
                            string serviceAddress = billingInfo[11].Substring(keyLength);

                            InsertBillsTable(customerID, connection, formatGUID, invoiceNumber, 
                                billDate, dueDate, billAmount, firstNotifDate, secondNotifDate, 
                                balanceDue, dateAdded, serviceAddress);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                        return false;
                    }
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
            string checkAccountNumber, string name, string mailAddr1, string mailAddr2, string city, string state, string zip)
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
                _ = command.Parameters.AddWithValue("DateAddedParam", DateTime.Today);

                _ = command.ExecuteNonQuery();

                CustomerIDCounter++;
            }
        }

        public bool ExportAsCSV(string dirName)
        {
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

            string customerID, customerName, accountNumber, customerAddress, customerCity, customerState, customerZip, dateAdded;
            string billsID, billDate, billNumber, accountBalance, dueDate, billAmount, formatGUID;

            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    using (var command = new OleDbCommand(query, connection))
                    {
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            using (StreamWriter writer = new StreamWriter("output.csv"))
                            {
                                // Write header line
                                writer.WriteLine("Customer.ID,Customer.CustomerName,Customer.AccountNumber,Customer.CustomerAddress,Customer.CustomerCity,Customer.CustomerState,Customer.CustomerZip,Bills.ID,Bills.BillDate,Bills.BillNumber,Bills.AccountBalance,Bills.DueDate,Bills.BillAmount,Bills.FormatGUID,Customer.DateAdded");

                                // Write data lines
                                while (reader.Read())
                                {
                                    writer.WriteLine($"{reader["ID"]},{reader["CustomerName"]},{reader["AccountNumber"]},{reader["CustomerAddress"]},{reader["CustomerCity"]},{reader["CustomerState"]},{reader["CustomerZip"]},{reader["BillID"]},{reader["BillDate"]},{reader["BillNumber"]},{reader["AccountBalance"]},{reader["DueDate"]},{reader["BillAmount"]},{reader["FormatGUID"]},{reader["DateAdded"]}");
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
