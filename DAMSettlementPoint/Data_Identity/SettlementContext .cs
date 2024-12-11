using Microsoft.EntityFrameworkCore;
using DAMSettlementPoint.Entities.Models;



namespace DAMSettlementPoint.Data_Identity
{
    public class SettlementContext : DbContext
    {
        public DbSet<SettlementPoint> SettlementPoints { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=AMMAR\\SQLEXPRESS;Database=Settlement;User ID=sa;Password=Klose@123;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettlementPoint>(entity =>
            {
                entity.HasIndex(e => new { e.DeliveryDate, e.HourEnding, e.SettlementPointName })
                    .IsUnique();

                entity.Property(e => e.SettlementPointPrice)
                    .HasPrecision(10, 2);

                entity.Property(e => e.SettlementPointName)
                    .HasMaxLength(50);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}



