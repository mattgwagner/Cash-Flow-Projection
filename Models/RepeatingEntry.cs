using System;
using System.ComponentModel.DataAnnotations;

namespace Cash_Flow_Projection.Models
{
    public class RepeatingEntry
    {
        [DataType(DataType.Date)]
        public DateTime FirstDate { get; set; } = DateTime.Today;

        public String Description { get; set; }

        public Decimal Amount { get; set; }

        public int RepeatInterval { get; set; } = 1;

        public RepeatUnit Unit { get; set; } = RepeatUnit.Months;

        public int RepeatIterations { get; set; } = 12;

        public AccountType Account { get; set; } = AccountType.Cash;

        public enum RepeatUnit
        {
            Days,

            Weeks,

            Months
        }
    }
}