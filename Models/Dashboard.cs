﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        public DateTime Thru { get; private set; }

        private IEnumerable<Entry> Entries { get; }

        public Dashboard(Database db, DateTime? thru = null)
        {
            Thru = thru ?? DateTime.Today.AddMonths(6);

            CheckingBalance = db.Entries.CurrentBalance(AccountType.Cash);
            CreditBalance = db.Entries.CurrentBalance(AccountType.Credit);
            BusinessBalance = db.Entries.CurrentBalance(AccountType.Business);

            Entries = db.Entries.SinceBalance(Thru).ToList();
        }

        [DataType(DataType.Currency)]
        public virtual Decimal CheckingBalance { get; }

        [DataType(DataType.Currency)]
        public virtual Decimal CreditBalance { get; }

        [DataType(DataType.Currency)]
        public virtual Decimal BusinessBalance { get; }

        [DataType(DataType.Currency), Display(Name = "Minimum Balance")]
        public virtual Decimal MinimumBalance => Rows.Select(row => row.CashBalance).Min();

        public virtual IEnumerable<Row> Rows
        {
            get
            {
                Decimal credit = CreditBalance;
                Decimal cash = CheckingBalance;
                Decimal business = BusinessBalance;

                foreach (var entry in Entries.Where(e => !e.IsBalance).OrderBy(e => e.Date).ThenByDescending(e => e.Amount))
                {
                    switch (entry.Account)
                    {
                        case AccountType.Cash:
                            cash += entry.Amount;
                            break;

                        case AccountType.Credit:
                            credit += entry.Amount;
                            break;

                        case AccountType.Business:
                            business += entry.Amount;
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
                        CreditBalance = credit,
                        BusinessBalance = business
                    };
                }
            }
        }

        public virtual String ChartData
        {
            get
            {
                var entries =
                    Rows
                    .OrderBy(row => row.Date)
                     .Select(row => new
                     {
                         Date = row.Date.ToString("yyyy-MM-dd"),
                         row.CashBalance,
                         row.CreditBalance,
                         row.BusinessBalance
                     });

                return JsonConvert.SerializeObject(entries);
            }
        }

        public class Row : Entry
        {
            public const Decimal CashWarningThreshold = 300;

            public String RowClass => Account switch
            {
                // When cash accounts go negative
                AccountType.Cash when CashBalance < 0 => "table-danger",
                AccountType.Business when BusinessBalance < 0 => "table-danger",

                // Check against a warning threshold
                AccountType.Cash when CashBalance < CashWarningThreshold => "table-warning",
                AccountType.Business when BusinessBalance < CashWarningThreshold => "table-warning",

                AccountType.Credit when Amount < 0 => "table-info", // Credit Card Payments

                AccountType.Business when Amount > 0 => "table-info", // Deposits into Business
                AccountType.Cash when Amount > 0 => "table-info", // Deposits into Checking

                _ => string.Empty
            };

            [DataType(DataType.Currency)]
            public Decimal CashBalance { get; set; }

            [DataType(DataType.Currency)]
            public Decimal CreditBalance { get; set; }

            [DataType(DataType.Currency)]
            public Decimal BusinessBalance { get; set; }
        }
    }
}
