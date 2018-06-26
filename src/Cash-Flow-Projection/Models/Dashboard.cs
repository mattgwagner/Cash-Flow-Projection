using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        private IEnumerable<Entry> Entries { get; }

        public Dashboard(Database db, DateTime? thru = null)
        {
            CheckingBalance = db.Entries.CurrentBalance(Account.Cash);
            CreditBalance = db.Entries.CurrentBalance(Account.Credit);

            Entries = db.Entries.SinceBalance(thru ?? DateTime.Today.AddMonths(4)).ToList();
        }

        [DataType(DataType.Currency)]
        public virtual Decimal CheckingBalance { get; }

        [DataType(DataType.Currency)]
        public virtual Decimal CreditBalance { get; }

        [DataType(DataType.Currency), Display(Name = "Minimum Balance")]
        public virtual Decimal MinimumBalance => Rows.Select(row => row.CashBalance).Min();

        public virtual IEnumerable<Row> Rows
        {
            get
            {
                Decimal credit = CreditBalance;
                Decimal cash = CheckingBalance;

                foreach (var entry in Entries.Where(e => !e.IsBalance).OrderBy(e => e.Date).ThenByDescending(e => e.Amount))
                {
                    switch (entry.Account)
                    {
                        case Account.Cash:
                            cash += entry.Amount;
                            break;

                        case Account.Credit:
                            credit += entry.Amount;
                            break;
                    }

                    yield return new Row
                    {
                        id = entry.id,
                        Amount = entry.Amount,
                        Description = entry.Description,
                        Date = entry.Date,
                        Account = entry.Account,
                        IsBalance = entry.IsBalance,
                        CashBalance = cash,
                        CreditBalance = credit
                    };
                }
            }
        }

        public virtual String ChartData
        {
            get
            {
                Decimal balance = CheckingBalance;

                var entries =
                    Entries
                     .Where(entry => entry.IsBalance == false)
                     .Where(entry => entry.Account == Account.Cash)
                     .GroupBy(entry => entry.Date.Date)
                     .OrderBy(group => group.Key)
                     .Select(group => new
                     {
                         Date = group.Key.ToString("yyyy-MM-dd"),
                         Balance = (balance += group.Sum(entry => entry.Amount))
                     })
                     .ToList();

                return JsonConvert.SerializeObject(entries);
            }
        }

        public class Row : Entry
        {
            public const Decimal CashWarningThreshold = 500;

            public virtual String RowClass
            {
                get
                {
                    if (Account == Account.Cash)
                    {
                        if (CashBalance < Decimal.Zero) return "danger";

                        if (CashBalance < CashWarningThreshold) return "warning";
                    }

                    return string.Empty;
                }
            }

            public virtual String AmountClass
            {
                get
                {
                    switch (Account)
                    {
                        case Account.Credit:
                            return Amount < Decimal.Zero ? "success" : string.Empty;

                        case Account.Cash:
                        default:
                            return Amount > Decimal.Zero ? "success" : string.Empty;
                    }
                }
            }

            [DataType(DataType.Currency)]
            public virtual Decimal CashBalance { get; set; }

            [DataType(DataType.Currency)]
            public virtual Decimal CreditBalance { get; set; }
        }
    }
}