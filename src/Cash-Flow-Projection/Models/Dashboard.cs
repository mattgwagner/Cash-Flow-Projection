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

        public Dashboard(Database db, DateTime? thru)
        {
            CheckingBalance = db.Entries.CurrentBalance(Account.Cash);
            CreditBalance = db.Entries.CurrentBalance(Account.Credit);

            Entries = db.Entries.SinceBalance(thru ?? DateTime.Today.AddMonths(4));
        }

        [DataType(DataType.Currency)]
        public virtual Decimal? CheckingBalance { get; }

        [DataType(DataType.Currency)]
        public virtual Decimal? CreditBalance { get; }

        [DataType(DataType.Currency), Display(Name = "Minimum Balance")]
        public virtual Decimal MinimumBalance => Rows.Select(row => row.CashBalance).Min();

        public virtual IEnumerable<Row> Rows
        {
            get
            {
                foreach (var entry in Entries.Where(e => !e.IsBalance).OrderBy(e => e.Date))
                {
                    yield return new Row
                    {
                        id = entry.id,
                        Amount = entry.Amount,
                        Description = entry.Description,
                        Date = entry.Date,
                        Account = entry.Account,
                        IsBalance = entry.IsBalance,
                        CashBalance = Balance.GetBalanceOn(Entries, entry.Date, Account.Cash),
                        CreditBalance = Balance.GetBalanceOn(Entries, entry.Date, Account.Credit)
                    };
                }
            }
        }

        public virtual String ChartData
        {
            get
            {
                var entries =
                    Entries
                     .Where(entry => entry.IsBalance == false)
                     .GroupBy(entry => entry.Date.Date)
                     .OrderBy(group => group.Key)
                     .Select(group => new
                     {
                         Date = group.Key.ToString("yyyy-MM-dd"),
                         Balance = Entries.GetBalanceOn(group.Key)
                     })
                     .ToList();

                return JsonConvert.SerializeObject(entries);
            }
        }

        public class Row : Entry
        {
            public virtual String RowClass => CashBalance < Decimal.Zero ? "danger" : CashBalance < 500 ? "warning" : string.Empty;

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