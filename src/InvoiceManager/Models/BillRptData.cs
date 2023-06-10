/**
 * @author: Tyler Pease
 * @github: https://github.com/SirTarragon
 * @date: 06/07/2023
 * */

namespace InvoiceManager.Models
{
    public class BillRptData
    {
        public string AccountNumber { get; set; }
        public string CustomerName { get; set; }
        public string MailAddress1 { get; set; }
        public string MailAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string FormatGUID { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime BillDt { get; set; }
        public DateTime DueDt { get; set; }
        public string BillAmount { get; set; }
        public DateTime NotifOne { get; set; }
        public DateTime NotifTwo { get; set; }
        public string BalanceDue { get; set; }
        public DateTime DateAdded { get; set; }
        public string ServiceAddress { get; set; }
    }
}
