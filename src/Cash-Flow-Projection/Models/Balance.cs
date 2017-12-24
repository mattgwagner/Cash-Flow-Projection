﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public static class Balance
    {
        public static Decimal CurrentBalance(this IEnumerable<Entry> entries, Account account = Account.Cash)
        {
            return GetLastBalanceEntry(entries, account)?.Amount ?? Decimal.Zero;
        }

        public static IEnumerable<Entry> SinceBalance(this IEnumerable<Entry> entries, DateTime end)
        {
            // Includes the last balance entry

            var last_balance = GetLastBalanceEntry(entries)?.Date;

            return
                entries
                .Where(entry => entry.Date >= last_balance)
                .Where(entry => entry.Date < end)
                .OrderBy(entry => entry.Date);
        }

        public static Entry GetLastBalanceEntry(this IEnumerable<Entry> entries, Account account = Account.Cash)
        {
            return
                entries
                .Where(entry => entry.Account == account)
                .Where(entry => entry.IsBalance)
                .OrderByDescending(entry => entry.Date)
                .FirstOrDefault();
        }

        public static Decimal GetBalanceOn(this IEnumerable<Entry> entries, DateTime asOf, Account account = Account.Cash)
        {
            var last_balance = GetLastBalanceEntry(entries, account).Date;

            var delta_since_last_balance =
                entries
                .Where(entry => entry.Account == account)
                .Where(entry => !entry.IsBalance)
                .Where(entry => entry.Date >= last_balance)
                .Where(entry => entry.Date <= asOf)
                .Sum(entry => entry.Amount);

            return CurrentBalance(entries, account) + delta_since_last_balance;
        }
    }
}