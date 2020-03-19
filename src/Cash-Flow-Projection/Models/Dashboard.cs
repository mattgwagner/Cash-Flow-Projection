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
            BusinessBalance = db.Entries.CurrentBalance(Account.Business);

            Entries = db.Entries.SinceBalance(thru ?? DateTime.Today.AddMonths(4)).ToList();
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
                        case Account.Cash:
                            cash += entry.Amount;
                            break;

                        case Account.Credit:
                            credit += entry.Amount;
                            break;

                        case Account.Business:
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

            public const Decimal CreditWarningThreshold = 10000;

            public String RowClass => Account switch
            {
                Account.Cash when CashBalance < 0 => "table-danger",
                Account.Cash when CashBalance < CashWarningThreshold => "table-warning",

                Account.Credit when Amount < 0 => "table-info",
                Account.Credit when CreditBalance > CreditWarningThreshold => "table-warning",

                Account.Business when BusinessBalance < 0 => "table-danger",

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
