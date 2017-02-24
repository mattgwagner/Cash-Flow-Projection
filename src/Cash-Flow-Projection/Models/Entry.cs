using System;
using System.Collections.Generic;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Entry
    {
        /// <summary>
        /// When the entry occurred
        /// </summary>
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public String Description { get; set; }

        public Decimal Amount { get; set; }

        /// <summary>
        /// If true, this entry denotes the balance at the given datetime
        /// </summary>
        public Boolean IsBalance { get; set; }
    }

    public static class Balance
    {
        public static IEnumerable<KeyValuePair<DateTime, Decimal>> GetDailyBalance(this IEnumerable<Entry> entries, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate) throw new ArgumentOutOfRangeException("startDate should before endDate");

            var current = startDate;

            while (current < endDate)
            {
                yield return new KeyValuePair<DateTime, Decimal>(current, BalanceAsOf(entries, current));

                current = current.AddDays(1);
            }
        }

        public static Decimal BalanceAsOf(this IEnumerable<Entry> entries, DateTime asOf)
        {
            var last_balance_entry =
                entries
                .Where(entry => entry.IsBalance)
                .Where(entry => entry.Date < asOf)
                .OrderByDescending(entry => entry.Date)
                .FirstOrDefault();

            if (last_balance_entry == null)
            {
                // This shouldn't happen, since we should always start with the initial balance?

                throw new Exception("No balance entries found in entry list!");
            }

            var delta_since_last_balance =
                entries
                .Where(entry => entry.Date >= last_balance_entry.Date)
                .Where(entry => entry.Date <= asOf)
                .Sum(entry => entry.Amount);

            return last_balance_entry.Amount - delta_since_last_balance;
        }
    }
}