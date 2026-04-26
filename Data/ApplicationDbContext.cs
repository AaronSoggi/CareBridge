using MediApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Medication> Medications {get;set;}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Medication>().HasKey(i => i.Id);
        builder.Entity<Medication>().Property(i => i.Name).IsRequired();
        builder.Entity<Medication>().Property(i => i.Dose).IsRequired();
        builder.Entity<Medication>().Property(i => i.Instructions).HasMaxLength(500);

        builder.Entity<Medication>()
        .HasOne(i => i.User)
        .WithMany(i => i.Medications)
        .HasForeignKey(i => i.UserId);

        builder.Entity<UserProfile>()
        .HasOne(i => i.User)
        .WithOne(i => i.Profile)
        .HasForeignKey<UserProfile>(y => y.UserId);


        base.OnModelCreating(builder);


    }
}
