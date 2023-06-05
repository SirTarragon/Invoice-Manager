using System.Data.Odbc;

namespace IC_Assignment.Services
{

    /* Database schema
    CREATE TABLE [Bills]
    (
                [ID]                    Long Integer, // this is just an int?
                [BillDate]              DateTime,
                [BillNumber]            Text (255),
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

    public class DatabaseManager
    {
        private int BillIDCounter, CustomerIDCounter;

        public DatabaseManager(string connectionString)
        {
            if(LoadConnection(connectionString))
            {
                Console.WriteLine("No issue with connection.");
            }
        }

        public bool LoadConnection(string s)
        {
            try
            {
                using (var connection = new OdbcConnection(s))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT MAX(Bills.ID), MAX(Customer.ID) FROM Bills, Customer";
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                BillIDCounter = reader.GetInt32(0);
                                CustomerIDCounter = reader.GetInt32(1);

                                return true;
                            }
                            else
                            {
                                BillIDCounter = 0;
                                CustomerIDCounter = 0;

                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Issue with connection or updating IDCounters for DBManager");
                return false;
            }
        }

        public void UpdateFromRPT(string filename)
        {
            if (!filename.Contains(".rpt")) return;

        }

        public void ExportAsCSV(string dirName)
        {

        }
    }
}
