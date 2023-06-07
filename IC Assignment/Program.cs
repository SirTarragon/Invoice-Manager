/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/05/2023
 * */

using IC_Assignment.Services;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")] // due to DatabaseManager
internal class Program
{
    private static BillFileManager fileManager;
    private static DatabaseManager dbManager;

    private static void Main(string[] args)
    {
        // sometimes the application will completely freeze up
        Console.WriteLine("If you still can see this after 10 seconds, restart the application.");

        fileManager = new BillFileManager();
        Console.WriteLine("Initialized BillFileManager...");
        dbManager = new DatabaseManager();  // this can hang, possibly due to the thread for opening the connection
                                        // not sure how to fix beyond telling user to reopen the application
                                        // doesn't seem to be a repeat issue elsewhere after testing, so not truly sure
        Console.WriteLine("Initialized DatabaseManager...");

        Console.Clear();

        while (true)
        {
            MenuPrompt();
            string input = Console.ReadLine();

            if(input == "1")
            {
                Console.Clear();
                Console.WriteLine("Prompting user to load XML file.");
                LoadXMLFile();
            }
            else if(input == "2")
            {
                Console.Clear();
                Console.WriteLine("Prompting user to load RPT file.");
                LoadRPTFile();
            }
            else if(input == "3")
            {
                Console.Clear();
                Console.WriteLine("Prompting user for export directory.");
                ExportDBtoCSV();
            }
            else if (input == "0")
            {
                Console.Clear();
                Console.WriteLine("Exiting the program . . .");
                break;
            }
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void MenuPrompt()
    {
        Console.WriteLine("Look at these commands and select one, please use its associated number:");
        Console.WriteLine("1: Load BillFile XML");
        Console.WriteLine("2: Load RPT file to database");
        Console.WriteLine("3: Export database to CSV file");
        Console.WriteLine("0: Exit");
    }

    private static void LoadXMLFile()
    {
        while (true)
        {
            Console.WriteLine("Enter the filename of the XML file (including the path)");
            Console.WriteLine("Otherwise, type 'Back' to return to menu:");
            string input = Console.ReadLine();

            if (input.ToLower() == "back")
            {
                Console.Clear();
                break;
            }

            if (File.Exists(input) && Path.GetExtension(input).Equals(".xml"))
            {
                Console.WriteLine($"Processing XML file... Please wait...");

                if (fileManager.ImportXMLData(input))
                {
                    SaveRPT(); // call local function to handle input for RPT file location
                    // would clear, but want to see any errors
                    break;
                }
            }
            else
            {
                Console.WriteLine("Invalid filename or file format. Please try again.");
            }
        }
    }

    private static void SaveRPT()
    {
        while (true)
        {
            Console.WriteLine("Enter the path for the RPT file:");
            string input = Console.ReadLine();

            if(Directory.Exists(input))
            {
                Console.WriteLine("Saving...");

                if(fileManager.ExportToRPT(input))
                {
                    fileManager.ClearBills(); // shouldn't be needing to save the data
                            // again if successfully saved
                    break;
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist. Please try again.");
            }
        }
    }

    private static void LoadRPTFile()
    {
        while (true)
        {
            Console.WriteLine("Enter the path for the RPT file, otherwise type 'Back' to return to menu:");
            string input = Console.ReadLine();

            if (input.ToLower() == "back")
            {
                Console.Clear();
                break;
            }

            if (File.Exists(input) && Path.GetExtension(input).Equals(".rpt"))
            {
                Console.WriteLine($"Processing RPT file... Please wait...");

                if (dbManager.UpdateFromRPT(input))
                {
                    break;
                }
            }
            else
            {
                Console.WriteLine("Invalid filename or file format. Please try again.");
            }
        }
    }

    private static void ExportDBtoCSV()
    {
        while (true)
        {
            Console.WriteLine("Enter the path for the BillingReport file, otherwise type 'Back' to return to menu:");
            string input = Console.ReadLine();

            if (input.ToLower() == "back")
            {
                Console.Clear();
                break;
            }

            if (Directory.Exists(input))
            {
                Console.WriteLine("Exporting...");

                if(dbManager.ExportAsCSV(input))
                {
                    break;
                }
            }
        }
    }
}