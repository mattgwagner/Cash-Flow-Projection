﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        private IEnumerable<Entry> Entries { get; }

        public Dashboard(IEnumerable<Entry> entries)
        {
            this.Entries = entries.ToList();
        }

        [DataType(DataType.Currency)]
        public virtual Decimal? CurrentBalance => Entries.CurrentBalance();

        [DataType(DataType.Date)]
        public virtual DateTime? BalanceAsOf => Entries.GetLastBalanceEntry()?.Date;

        [DataType(DataType.Currency), Display(Name = "Minimum Balance")]
        public virtual Decimal MinimumBalance => Rows.Select(row => row.Cash).Min();

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
                        Cash = Balance.GetBalanceOn(Entries, entry.Date)
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
            public virtual String RowClass => Cash < Decimal.Zero ? "danger" : Cash < 500 ? "warning" : string.Empty;

            public virtual String AmountClass => (Account == Account.Cash && Amount > Decimal.Zero) ? "success" : string.Empty;

            [DataType(DataType.Currency)]
            public virtual Decimal Cash { get; set; }

            [DataType(DataType.Currency)]
            public virtual Decimal CreditCard { get; set; }
        }
    }
}