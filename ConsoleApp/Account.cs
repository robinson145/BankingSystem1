using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GCashSimulator
{
    public class Account
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PhoneNumber { get; set; }
        private string _pin;
        public decimal Balance { get; set; }
        public List<Transaction> TransactionHistory { get; set; }
        public DateTime CreatedAt { get; set; }
        public AccountType Type { get; set; }
        public bool IsLocked { get; set; }
        public int FailedPinAttempts { get; set; }

        public Account(string accountName, string phoneNumber, string pin, decimal initialBalance = 0, AccountType type = AccountType.SAVINGS)
        {
            AccountNumber = "GC" + DateTime.Now.ToString("yyyyMMdd") + new Random().Next(1000, 9999);
            AccountName = accountName;
            PhoneNumber = phoneNumber;
            _pin = pin;
            Balance = initialBalance;
            TransactionHistory = new List<Transaction>();
            CreatedAt = DateTime.Now;
            Type = type;
            IsLocked = false;
            FailedPinAttempts = 0;
        }

        public bool ValidatePin(string inputPin) => _pin == inputPin;

        public bool ChangePin(string oldPin, string newPin)
        {
            if (_pin != oldPin) return false;
            _pin = newPin;
            return true;
        }

        public void AddTransaction(string type, decimal amount, decimal balanceAfter, string recipientInfo = null)
        {
            TransactionHistory.Add(new Transaction
            {
                Date = DateTime.Now,
                Type = type,
                Amount = amount,
                BalanceAfter = balanceAfter,
                RecipientInfo = recipientInfo
            });
        }

        public decimal GetWithdrawalLimit()
        {
            return Type == AccountType.PREMIUM ? 50000m : 10000m;
        }

        public void ResetFailedAttempts()
        {
            FailedPinAttempts = 0;
        }

        public void IncrementFailedAttempt()
        {
            FailedPinAttempts++;
            if (FailedPinAttempts >= 3)
            {
                IsLocked = true;
            }
        }
    }
}