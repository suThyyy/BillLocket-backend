using BillLocket.Domain.Entities;
using BillLocket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BillLocket.Infrastructure.Persistence;

public class BillLocketDbContext
    : IdentityDbContext<ApplicationUser>
{
    public BillLocketDbContext(
        DbContextOptions<BillLocketDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShareBill> ShareBills => Set<ShareBill>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.FamilyId)
                .IsRequired();

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.FamilyId);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
