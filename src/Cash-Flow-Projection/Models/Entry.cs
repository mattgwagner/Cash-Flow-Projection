using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        public IEnumerable<Entry> Entries { get; set; }

        [DataType(DataType.Currency)]
        public Decimal CurrentBalance { get { return Entries.BalanceAsOf(DateTime.UtcNow); } }
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
        static Balance()
        {
            Entries.Add(new Entry { Date = new DateTime(2017, 2, 24), Amount = 6279.06m, Description = "Balance", IsBalance = true });
            Entries.Add(new Entry { Date = new DateTime(2017, 2, 28), Amount = -64, Description = "Check" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -50, Description = "Amanda" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -1900, Description = "Mortgage" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -71.84m, Description = "Car Insurance" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -300, Description = "Tuition" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 3), Amount = -359.65m, Description = "USAA Loan" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 3), Amount = -100, Description = "529 Plan Contrib" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 4), Amount = -291.74m, Description = "Car Payment" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 8), Amount = -50, Description = "Amanda" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 9), Amount = -140.79m, Description = "Electric" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 10), Amount = 3021.65m, Description = "PayDay" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 14), Amount = -123.04m, Description = "Spectrum" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -120, Description = "Lawn" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -50, Description = "Amanda" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -71.84m, Description = "Car Insurance" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 6), Amount = -87.59m, Description = "Dental" });
            Entries.Add(new Entry { Date = new DateTime(2017, 3, 22), Amount = -50, Description = "Amanda" });
        }

        public static ConcurrentBag<Entry> Entries { get; } = new ConcurrentBag<Entry>();

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