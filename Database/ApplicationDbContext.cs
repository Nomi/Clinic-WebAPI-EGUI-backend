using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite; 
//using System.Data.Entity;
using System.Reflection.Metadata;
using EGUI_Stage2.Models;

namespace EGUI_Stage2.Database
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> 
    {
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleForDay> ScheduleEntries { get; set; }
        public DbSet<Visit> Visits { get; set; }

        protected readonly IConfiguration Configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x=>x.Id).ValueGeneratedOnAdd();
                entity
                    .HasMany(x => x.ScheduleForEachDay)
                    .WithOne(x => x.ScheduleCurrent)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ScheduleForDay>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity
                    .HasOne(x=>x.ScheduleCurrent)
                    .WithMany(x => x.ScheduleForEachDay)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(x => x.VisitSlots)
                    .WithOne(x => x.ParentScheduleEntry)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Visit>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity
                    .HasOne(x=>x.ParentScheduleEntry)
                    .WithMany(x=>x.VisitSlots)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
