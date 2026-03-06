using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GCashSimulator
{
    class Program
    {
        private static List<Account> accounts = new List<Account>();
        private static Account currentAccount = null;
        private const int MAX_RETRIES = 3;
        private const decimal MINIMUM_DEPOSIT = 50000m;

        static void Main(string[] args)
        {
            InitializeSampleAccounts();

            while (true)
            {
                Console.Clear();
                if (currentAccount == null)
                    ShowLoginMenu();
                else
                    ShowMainMenu();
            }
        }

        static void InitializeSampleAccounts()
        {
            accounts.Add(new Account("Testing D. Account", "09123456789", "1234", 5000.00m, AccountType.SAVINGS));
            accounts.Add(new Account("Maria Santos", "09189876543", "5678", 100000.00m, AccountType.PREMIUM));
            accounts.Add(new Account("Pedro Reyes", "09155678901", "9012", 2500.50m, AccountType.SAVINGS));

            accounts[0].AddTransaction("DEPOSIT", 5000.00m, 5000.00m);
            accounts[1].AddTransaction("DEPOSIT", 100000.00m, 100000.00m);
            accounts[2].AddTransaction("DEPOSIT", 2500.50m, 2500.50m);
        }

        static void ShowLoginMenu()
        {
            Console.WriteLine("=== Gcash(Inspired) SIMULATOR ===\n");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register New Account");
            Console.WriteLine("3. View Sample Accounts");
            Console.WriteLine("4. Exit");
            Console.Write("\nSelect: ");

            switch (Console.ReadLine())
            {
                case "1": Login(); break;
                case "2": Register(); break;
                case "3": ShowSampleAccounts(); break;
                case "4": Environment.Exit(0); break;
                default: ShowMessage("Invalid option!"); break;
            }
        }

        static void ShowSampleAccounts()
        {
            Console.Clear();
            Console.WriteLine("=== SAMPLE ACCOUNTS ===\n");
            foreach (var acc in accounts)
            {
                Console.WriteLine($"Name: {acc.AccountName}");
                Console.WriteLine($"Phone: {acc.PhoneNumber}");
                Console.WriteLine($"Type: {acc.Type}");
                Console.WriteLine($"Status: {(acc.IsLocked ? "LOCKED" : "ACTIVE")}");
                Console.WriteLine($"Balance: P{acc.Balance:N2}");
                Console.WriteLine("------------------------\n");
            }
            Console.WriteLine("Sample PINs: 1234, 5678, 9012");
            Console.ReadKey();
        }

        static void Login()
        {
            Console.Clear();
            Console.WriteLine("=== LOGIN ===");
            Console.Write("Phone Number: ");
            string phone = Console.ReadLine();

            var account = accounts.FirstOrDefault(a => a.PhoneNumber == phone);
            if (account == null)
            {
                ShowMessage("Account not found!");
                return;
            }

            if (account.IsLocked)
            {
                ShowMessage("Account is locked due to too many incorrect PIN attempts.");
                return;
            }

            int attempts = 0;
            while (attempts < 3)
            {
                Console.Write("PIN: ");
                string pin = ReadPin();
                if (account.ValidatePin(pin))
                {
                    account.ResetFailedAttempts();
                    currentAccount = account;
                    ShowMessage($"Welcome {account.AccountName}!");
                    return;
                }
                else
                {
                    attempts++;
                    account.IncrementFailedAttempt();
                    if (attempts >= 3)
                    {
                        ShowMessage("Too many incorrect PIN attempts. Account is now LOCKED.");
                        return;
                    }
                    Console.WriteLine($"Invalid PIN. Attempt {attempts} of 3.");
                }
            }
        }

        static void Register()
        {
            Console.Clear();
            Console.WriteLine("=== REGISTER ACCOUNT ===");

            string name;
            while (true)
            {
                Console.Write("Full Name (First and Last): ");
                name = Console.ReadLine();
                if (IsValidFullName(name)) break;
                ShowMessage("Invalid name! Must contain at least first and last name, letters only.");
            }
            string phone;

            while (true)
            {
                Console.Write("Phone Number (11 digits starting with 09): ");
                phone = Console.ReadLine();

                if (phone.Length == 11 && phone.StartsWith("09") && phone.All(char.IsDigit))
                    break;

                Console.WriteLine("Invalid phone number! It must start with 09 and contain 11 digits.");
            }

            string pin;
            while (true)
            {
                Console.Write("Create 4-digit PIN: ");
                pin = ReadPin();
                Console.Write("Confirm PIN: ");
                string confirm = ReadPin();
                if (pin == confirm && pin.Length == 4) break;
                ShowMessage("PINs do not match or not 4 digits!");
            }

            decimal initial = 0;
            while (true)
            {
                Console.Write("Initial Deposit: ");
                if (decimal.TryParse(Console.ReadLine(), out initial) && initial >= 0) break;
                ShowMessage("Invalid amount!");
            }

            var newAccount = new Account(name, phone, pin, initial);
            if (initial > 0)
                newAccount.AddTransaction("DEPOSIT", initial, initial);

            accounts.Add(newAccount);
            ShowMessage($"Account created! Account Number: {newAccount.AccountNumber}");
        }

        static bool IsValidFullName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var parts = name.Trim().Split(' ');
            if (parts.Length < 2) return false;
            return parts.All(p => Regex.IsMatch(p, @"^[a-zA-Z]+$"));
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine($"=== Welcome {currentAccount.AccountName} ===");
            Console.WriteLine($"Balance: P{currentAccount.Balance:N2}");
            Console.WriteLine($"Account Type: {currentAccount.Type}\n");

            Console.WriteLine("1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Send Money");
            Console.WriteLine("4. Transaction History");
            Console.WriteLine("5. Change PIN");
            Console.WriteLine("6. Upgrade to Premium");
            Console.WriteLine("7. Logout");

            Console.Write("\nSelect: ");
            switch (Console.ReadLine())
            {
                case "1": Deposit(); break;
                case "2": Withdraw(); break;
                case "3": SendMoney(); break;
                case "4": ViewHistory(); break;
                case "5": ChangePin(); break;
                case "6": UpgradeAccount(); break;
                case "7": Logout(); break;
                default: ShowMessage("Invalid option!"); break;
            }
        }

        static void Deposit()
        {
            int attempts = 0;

            while (attempts < MAX_RETRIES)
            {
                Console.Clear();
                Console.WriteLine("=== DEPOSIT ===");
                Console.WriteLine($"Current Balance: P{currentAccount.Balance:N2}");

                decimal maxDeposit = currentAccount.Type == AccountType.PREMIUM ? 100000m : 10000m;
                Console.WriteLine($"Deposit Limit: P{maxDeposit:N2}\n");

                Console.Write("Enter amount to deposit: P");
                string input = Console.ReadLine();

                if (!decimal.TryParse(input, out decimal amount) || amount <= 0)
                {
                    ShowMessage("Invalid amount!");
                    attempts++;
                    continue;
                }

                if (amount > maxDeposit)
                {
                    ShowMessage($"Deposit exceeds the limit! Max deposit for your account is P{maxDeposit:N2}.");
                    attempts++;
                    continue;
                }

                Console.Write("Enter PIN to confirm: ");
                string pin = ReadPin();

                if (!currentAccount.ValidatePin(pin))
                {
                    currentAccount.IncrementFailedAttempt();

                    if (currentAccount.IsLocked)
                    {
                        ShowMessage("Account locked due to too many incorrect PIN attempts!");
                        Logout();
                        return;
                    }

                    ShowMessage($"Invalid PIN! Attempt {currentAccount.FailedPinAttempts} of 3.");
                    attempts++;
                    continue;
                }

                currentAccount.ResetFailedAttempts();

                decimal oldBalance = currentAccount.Balance;
                currentAccount.Balance += amount;

                currentAccount.AddTransaction("DEPOSIT", amount, currentAccount.Balance);

                ShowTransactionResult("DEPOSIT", amount, oldBalance, currentAccount.Balance);
                return;
            }

            ShowMessage("Too many failed attempts. Returning to menu...");
        }
        static void Withdraw()
        {
            int attempts = 0;
            decimal limit = currentAccount.GetWithdrawalLimit();

            while (attempts < MAX_RETRIES)
            {
                Console.Clear();
                Console.WriteLine("=== WITHDRAW ===");
                Console.WriteLine($"Attempt {attempts + 1} of {MAX_RETRIES}");
                Console.WriteLine($"Current Balance: P{currentAccount.Balance:N2}");
                Console.WriteLine($"Withdrawal Limit: P{limit:N2}\n");

                Console.Write("Enter amount to withdraw: P");
                string input = Console.ReadLine();

                if (!decimal.TryParse(input, out decimal amount) || amount <= 0)
                {
                    ShowMessage("Invalid amount! Must be a positive number.");
                    attempts++;
                    continue;
                }

                if (amount > currentAccount.Balance)
                {
                    ShowMessage("Insufficient balance!");
                    attempts++;
                    continue;
                }

                if (amount > limit)
                {
                    ShowMessage($"Amount exceeds withdrawal limit of P{limit:N2}!");
                    attempts++;
                    continue;
                }

                Console.Write("Enter PIN to confirm: ");
                string pin = ReadPin();

                if (!currentAccount.ValidatePin(pin))
                {
                    currentAccount.IncrementFailedAttempt();
                    if (currentAccount.IsLocked)
                    {
                        ShowMessage("Account locked due to too many incorrect PIN attempts!");
                        Logout();
                        return;
                    }

                    ShowMessage($"Invalid PIN! Attempt {currentAccount.FailedPinAttempts} of 3.");
                    attempts++;
                    continue;
                }

                
                currentAccount.ResetFailedAttempts();
                decimal oldBalance = currentAccount.Balance;
                currentAccount.Balance -= amount;
                currentAccount.AddTransaction("WITHDRAW", amount, currentAccount.Balance);
                ShowTransactionResult("WITHDRAW", amount, oldBalance, currentAccount.Balance);
                return;
            }

            ShowMessage("Too many failed attempts. Returning to menu...");
        }

        static void SendMoney()
        {
            Console.Write("Recipient Phone: ");
            string phone = Console.ReadLine();

            var recipient = accounts.FirstOrDefault(a => a.PhoneNumber == phone);
            if (recipient == null)
            {
                ShowMessage("Recipient not found!");
                return;
            }

            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                ShowMessage("Invalid amount!");
                return;
            }

            if (amount > currentAccount.Balance)
            {
                ShowMessage("Insufficient balance!");
                return;
            }

            currentAccount.Balance -= amount;
            recipient.Balance += amount;

            currentAccount.AddTransaction("SEND", amount, currentAccount.Balance, recipient.AccountName);
            recipient.AddTransaction("RECEIVED", amount, recipient.Balance, currentAccount.AccountName);

            ShowMessage("Money sent successfully!");
        }

        static void ViewHistory()
        {
            Console.Clear();
            Console.WriteLine("=== TRANSACTION HISTORY ===\n");
            foreach (var t in currentAccount.TransactionHistory.OrderByDescending(t => t.Date))
            {
                Console.WriteLine(t.ToString());
            }
            Console.ReadKey();
        }

        static void ChangePin()
        {
            Console.Write("Current PIN: ");
            string oldPin = ReadPin();
            Console.Write("New PIN: ");
            string newPin = ReadPin();

            if (currentAccount.ChangePin(oldPin, newPin))
                ShowMessage("PIN changed successfully!");
            else
                ShowMessage("Incorrect current PIN!");
        }

        static void UpgradeAccount()
        {
            if (currentAccount.Type == AccountType.PREMIUM)
            {
                ShowMessage("Already Premium!");
                return;
            }

            if (currentAccount.Balance < 500)
            {
                ShowMessage("Need P500 to upgrade!");
                return;
            }

            currentAccount.Balance -= 500;
            currentAccount.Type = AccountType.PREMIUM;
            currentAccount.AddTransaction("UPGRADE", 500, currentAccount.Balance);

            ShowMessage("Account upgraded to PREMIUM!");
        }

        static void Logout()
        {
            currentAccount = null;
            ShowMessage("Logged out successfully.");
        }

        static void ShowTransactionResult(string type, decimal amount, decimal oldBalance, decimal newBalance)
        {
            Console.Clear();
            Console.WriteLine("=== TRANSACTION SUCCESSFUL ===\n");
            Console.WriteLine($"Type: {type}");
            Console.WriteLine($"Amount: P{amount:N2}");
            Console.WriteLine($"Previous Balance: P{oldBalance:N2}");
            Console.WriteLine($"New Balance: P{newBalance:N2}");
            Console.ReadKey();
        }

        static string ReadPin()
        {
            string pin = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (char.IsDigit(key.KeyChar) && pin.Length < 4)
                {
                    pin += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b");
                }
            }
            Console.WriteLine();
            return pin;
        }

        static void ShowMessage(string message)
        {
            Console.WriteLine($"\n{message}");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}