using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Dashboard
    {
        public IEnumerable<Entry> Entries { get; set; }

        [DataType(DataType.Currency)]
        public virtual Decimal? CurrentBalance { get { return Entries.CurrentBalance(); } }

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
    }

    public class RepeatingEntry
    {
        [DataType(DataType.Date)]
        public DateTime FirstDate { get; set; } = DateTime.Today;

        public String Description { get; set; }

        public Decimal Amount { get; set; }

        public int RepeatInterval { get; set; }

        public RepeatUnit Unit { get; set; } = RepeatUnit.Days;

        public int RepeatIterations { get; set; } = 10;

        public enum RepeatUnit
        {
            Days,

            Weeks,

            Months
        }
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
        public static Decimal CurrentBalance(this IEnumerable<Entry> entries)
        {
            return GetLastBalanceEntry(entries)?.Amount ?? Decimal.Zero;
        }

        public static IEnumerable<Entry> SinceBalance(this IEnumerable<Entry> entries)
        {
            // Includes the last balance entry

            return
                entries
                .Where(entry => entry.Date >= GetLastBalanceEntry(entries)?.Date)
                .OrderBy(entry => entry.Date)
                .ToList();
        }

        private static Entry GetLastBalanceEntry(this IEnumerable<Entry> entries)
        {
            return
                entries
                .Where(entry => entry.IsBalance)
                .OrderByDescending(entry => entry.Date)
                .FirstOrDefault();
        }

        public static Decimal GetBalanceOn(this IEnumerable<Entry> entries, DateTime asOf)
        {
            var delta_since_last_balance =
                entries
                .Where(entry => !entry.IsBalance)
                .Where(entry => entry.Date >= GetLastBalanceEntry(entries).Date)
                .Where(entry => entry.Date <= asOf)
                .Sum(entry => entry.Amount);

            return CurrentBalance(entries) + delta_since_last_balance;
        }
    }
}