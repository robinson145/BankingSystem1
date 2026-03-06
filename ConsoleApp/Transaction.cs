using System;

namespace GCashSimulator
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string RecipientInfo { get; set; }

        public override string ToString()
        {
            string symbol = Type == "DEPOSIT" || Type == "RECEIVED" || Type == "UPGRADE" ? "+" : "-";
            string details = string.IsNullOrEmpty(RecipientInfo) ? "" : $" | {RecipientInfo}";
            return $"{Date:MM/dd/yyyy HH:mm} | {Type} | {symbol}P{Amount:N2} | Balance: P{BalanceAfter:N2}{details}";
        }
    }
}