using System;
using System.ComponentModel.DataAnnotations;

namespace Cash_Flow_Projection.Models
{
    public enum AccountType : byte
    {
        Cash,

        Credit,

        Business
    }

    public class Account
    {
        public int Id { get; set; }

        [StringLength(100)]
        public String Name { get; set; }
    }
}