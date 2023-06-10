# Invoice Manager

This was a small, 3-day project that was done as a part of a technical assessment for a job interview. The goal was to import an XML document, parse the data and export it as a custom formatted file. From there, the program needed to be able to import the custom formatted file and write to a database with the data. The program also needed to be able to export the data from the database as a CSV-like file.

## Technical Assessment Prompt

1. Create a console application in C# that reads the contents of file BillFile.xml, parses its contents and writes out a file in the format specified in document BillsOutput.txt. Please name the export file BillFile-mmddyyyy.rpt. <br> <br>The values for the fields in square brackes should either be populated from the file, a constant variable referenced in the table below, or be the result of a calculation from values in the file. Fields not existing in the file specification should be omitted from the output file.

The file follows the following kvp format:

`FieldID~FieldValue|`

| FieldID | Value/Reference |
| ---- | ---- |
| 2 | 8203ACC7-2094-43CC-8F7A-B8F19AA9BDA2 |
| 5 | Count of IH records |
| 6 | SUM of BILL_AMOUNT values |
| JJ | 8E2FEA69-5D77-4D0F-898E-DFA25677D19E |
| OO | 5 days after the current date |
| PP | 3 days before the Due Date (MM) |

    *All dates should be in the format MM/DD/YYYY

    *Number fields do not require commas

    *File Header record appears once per file

    *AA record appears once per bill

    *HH record appears once per bill

2. From there, create a routine in C# that reads the contents of the BillFile.rpt and imports the data into the attached access database (.mdb), into its corresponding tables and fields.

3. Finally, create a routing in C# that connects to the "Billing" database, retrieves the contents of both tables, and exports the records associated with an account in a CSV formatted file, outlined in the attached BillingReport.txt. File should include a header and one line pers unique customer record and any bills associated to that customer.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

* C# 7.0 or higher
* Visual Studio 2022 or higher
* .NET 6.0 or higher

### Installing

You will want to build the solution yourself in Visual Studio, as the program is a console application and the location of the database needs to be specified in the code.

1. Clone the repository to your local machine
2. Open the solution file in Visual Studio
3. Build the solution
4. Run the program

## Testing the program

Please utilize the BillFile.xml from .testfiles and the Billing.mdb database. If you wish to make your own database, the expected schema is in DatabaseManager.cs.

Testing program itself can be done step by step by following the menu prompts sequentially.

### Expected output

* BillFile-mmddyyyy.rpt
* BillReport-mmddyyyy.txt
* Updated Billing.mdb

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
