using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite; 
//using System.Data.Entity;
using System.Reflection.Metadata;
using EGUI_Stage2.Models;

namespace EGUI_Stage2.Data
{
    public class AppDbContext : IdentityDbContext<User> //IdentityDbContext<User,ApplicationRole,string>
    {
        //Users are preincluded by using IdentityDbContext instead of just DbContext as base class.
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; }
        public DbSet<VisitSlot> VisitSlots { get; set; }

        protected readonly IConfiguration Configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ApplicationUserRole>(entity =>
            //{
            //    //entity
            //    //.HasKey(nameof(IdentityUserRole<string>.UserId), nameof(IdentityUserRole<string>.RoleId));

            //    entity
            //      .HasOne(x => x.Role)
            //      .WithMany(x => x.UserRoles)
            //      .HasForeignKey(x => x.RoleId);

            //    entity
            //      .HasOne(x => x.User)
            //      .WithMany(x => x.UserRoles)
            //      .HasForeignKey(x => x.UserId);
            //});




            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity
            //        .HasMany(x => x.DoctorSchedule)
            //        .WithOne(x => x.Doctor)
            //        .OnDelete(DeleteBehavior.Cascade);
            //});
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x=>x.Id).ValueGeneratedOnAdd();
                entity
                    .HasMany(x => x.ScheduleEntries)
                    .WithOne(x => x.Schedule)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ScheduleEntry>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity
                    .HasOne(x=>x.Schedule)
                    .WithMany(x => x.ScheduleEntries)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(x => x.VisitSlots)
                    .WithOne(x => x.ParentScheduleEntry)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<VisitSlot>(entity =>
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
