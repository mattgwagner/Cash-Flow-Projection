using System;
using System.ComponentModel.DataAnnotations;

namespace Cash_Flow_Projection.Models
{
    public class Entry
    {
        /// <summary>
        /// A unique identifer generated for the entry
        /// </summary>
        public String id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Date the entry occurred
        /// </summary>
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:MMM dd}")]
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>
        /// A short, visible description of the transaction
        /// </summary>
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

        /// <summary>
        /// Which account this transaction applies to
        /// </summary>
        public Account Account { get; set; } = Account.Cash;
    }

    public enum Account : byte
    {
        Cash,

        Credit
    }
}