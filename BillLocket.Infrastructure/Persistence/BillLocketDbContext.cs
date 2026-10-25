using BillLocket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Infrastructure.Persistence;
public class BillLocketDbContext : DbContext
{
    public BillLocketDbContext(
        DbContextOptions<BillLocketDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShareBill> ShareBills => Set<ShareBill>();
}