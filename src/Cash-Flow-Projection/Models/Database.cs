using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Cash_Flow_Projection.Models
{
    public class Database : DbContext
    {
        public DbSet<Entry> Entries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Filename=Data.db");
        }

        static Database()
        {
            using (var db = new Database { })
            {
                // Check if table exists, create if not

                db.Database.ExecuteSqlCommand("create table if not exists Entries (id text, Date datetime, Description text, Amount decimal, IsBalance boolean");

                // Seed database(?)

                if (!db.Entries.Any())
                {
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 2, 24), Amount = 6279.06m, Description = "Balance", IsBalance = true });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 2, 28), Amount = -64, Description = "Check" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -50, Description = "Amanda" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -1900, Description = "Mortgage" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -71.84m, Description = "Car Insurance" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 1), Amount = -300, Description = "Tuition" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 3), Amount = -359.65m, Description = "USAA Loan" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 3), Amount = -100, Description = "529 Plan Contrib" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 4), Amount = -291.74m, Description = "Car Payment" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 8), Amount = -50, Description = "Amanda" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 9), Amount = -140.79m, Description = "Electric" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 10), Amount = 3021.65m, Description = "PayDay" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 14), Amount = -123.04m, Description = "Spectrum" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -120, Description = "Lawn" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -50, Description = "Amanda" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 15), Amount = -71.84m, Description = "Car Insurance" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 6), Amount = -87.59m, Description = "Dental" });
                    db.Entries.Add(new Entry { Date = new DateTime(2017, 3, 22), Amount = -50, Description = "Amanda" });

                    db.SaveChanges();
                }
            }
        }
    }
}