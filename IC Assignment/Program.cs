using IC_Assignment.Services;
using System;

internal class Program
{
    private static string connectionString = @"C:\Billing.mdb";

    private static BillFileManager fileManager;
    private static DatabaseManager dbManager;

    private static void Main(string[] args)
    {
        fileManager = new BillFileManager();
        dbManager = new DatabaseManager(connectionString);

        while (true)
        {
            Console.WriteLine("Look at these commands and select one, please use its associated number:");
            Console.WriteLine("1: Load BillFile XML");
            Console.WriteLine("2: Load RPT file to database");
            Console.WriteLine("3: Export database to CSV file");
            Console.WriteLine("0: Exit");
            string input = Console.ReadLine();

            if(input == "1")
            {
                Console.WriteLine("Prompting user to load XML file.");
                LoadXMLFile();
            }
            else if(input == "2")
            {
                Console.WriteLine("Prompting user to load RPT file.");
                LoadRPTFile();
            }
            else if(input == "3")
            {
                Console.WriteLine("Prompting user for export directory.");
                ExportDBtoCSV();
            }
            else if (input == "0")
            {
                Console.WriteLine("Exiting the program . . .");
                break;
            }
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
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
                break;
            }

            if (File.Exists(input) && Path.GetExtension(input).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Processing XML file: {input}");

                if (fileManager.Load(input))
                {
                    SaveRPT();
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
            Console.WriteLine("Enter the path for the RPT file, otherwise press 'Enter' for default:");
            string input = Console.ReadLine();

            if(Directory.Exists(input))
            {
                Console.WriteLine("Saving...");

                if(fileManager.Save(input))
                {
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
        
    }

    private static void ExportDBtoCSV()
    {

    }
}