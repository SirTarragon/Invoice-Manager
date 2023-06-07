# InvoiceCloud Technical Assessment

This is an assignment by InvoiceCloud for a technical assessment. The repository that this is stored on is a private repository, and is not to be shared with anyone else. The assignment is as follows:

Create a console application in C# that reads the contents of file BillFile.xml, parses its contents and writes out a file in the format specified in document BillsOutput.txt. Please name the export file BillFile-mmddyyyy.rpt.

The values for the fields in square brackes should either be populated from the file, a constant variable referenced in the table below, or be the result of a calculation from values in the file. Fields not existing in the file specification should be omitted from the output file.

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

>*All dates should be in the format MM/DD/YYYY
>
>*Number fields do not require commas
>
>*File Header record appears once per file
>
>*AA record appears once per bill
>
>*HH record appears once per bill

From there, create a routine in C# that reads the contents of the BillFile.rpt and imports the data into the attached access database (.mdb), into its corresponding tables and fields.

Finally, create a routing in C# that connects to the "Billing" database, retrieves the contents of both tables, and exports the records associated with an account in a CSV formatted file, outlined in the attached BillingReport.txt. File should include a header and one line pers unique customer record and any bills associated to that customer.

Please submit the C# project and solution along with a BillFile-mmddyyyy.rpt file, and updated Billing.mdb, and BillingReport.txt files.

Thank you,

InvoiceCloud Team
