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
}