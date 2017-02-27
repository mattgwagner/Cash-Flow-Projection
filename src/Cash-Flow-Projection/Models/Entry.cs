using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        public IEnumerable<Entry> Entries { get; set; }
    }

    public sealed class Entry
    {
        /// <summary>
        /// A unique identifer generated for the entry
        /// </summary>
        public String id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Date the entry occurred
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public String Description { get; set; }

        /// <summary>
        /// The amount of the transaction, negative represents cash expenditures, positive represents income.
        ///
        /// If the entry is a balance snapshot, this represents the balance at this point in time.
        /// </summary>
        [DataType(DataType.Currency)]
        public Decimal Amount { get; set; }

        /// <summary>
        /// If true, this entry denotes the snapshot cash balance at the given datetime
        /// </summary>
        public Boolean IsBalance { get; set; }
    }

    public static class Balance
    {
        public static Entry GetLastBalanceEntry(this IEnumerable<Entry> entries, DateTime? asOf = null)
        {
            return
                entries
                .Where(entry => entry.IsBalance)
                .Where(entry => entry.Date <= (asOf ?? DateTime.UtcNow))
                .OrderByDescending(entry => entry.Date)
                .FirstOrDefault();
        }

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
            var delta_since_last_balance =
                entries
                .Where(entry => !entry.IsBalance)
                .Where(entry => entry.Date >= GetLastBalanceEntry(entries, asOf).Date)
                .Where(entry => entry.Date <= asOf)
                .Sum(entry => entry.Amount);

            return GetLastBalanceEntry(entries, asOf).Amount + delta_since_last_balance;
        }
    }
}