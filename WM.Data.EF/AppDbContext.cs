using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WM.Data.EF.Configurations;
using WM.Data.EF.Extensions;
using WM.Data.Entities;

namespace WM.Data.EF
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<OC> OCs { get; set; }
        public DbSet<OCUser> OCUsers { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Deputy> Deputies { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationDetail> NotificationDetails { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentDetail> CommentDetails { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tutorial> Tutorials { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<UploadImage> UploadImages { get; set; }
        public DbSet<CheckTask> CheckTasks { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region Identity Config

            //builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims").HasKey(x => x.Id);

            //builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims")
            //    .HasKey(x => x.Id);

            //builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => x.UserId);

            //builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles")
            //    .HasKey(x => new { x.RoleId, x.UserId });

            //builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens")
            //   .HasKey(x => new { x.UserId });

            #endregion Identity Config

            builder.AddConfiguration(new OCUserConfiguration());
            builder.AddConfiguration(new NotificationDetailConfiguration());
            builder.AddConfiguration(new NotificationConfiguration());

            builder.AddConfiguration(new CommentDetailConfiguration());
            builder.AddConfiguration(new CommentConfiguration());

            builder.AddConfiguration(new TagConfiguration());
            builder.AddConfiguration(new DeputyConfiguration());
            builder.AddConfiguration(new ManagerConfiguration());
            builder.AddConfiguration(new TeamMemberConfiguration());

            builder.AddConfiguration(new TaskConfiguration());
            builder.AddConfiguration(new FollowConfiguration());

            //base.OnModelCreating(builder);
        }
        // Set datetime in runtime
        //public override int SaveChanges()
        //{
        //    var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

        //    foreach (EntityEntry item in modified)
        //    {
        //        var changedOrAddedItem = item.Entity as IDateTracking;
        //        if (changedOrAddedItem != null)
        //        {
        //            if (item.State == EntityState.Added)
        //            {
        //                changedOrAddedItem.DateCreated = DateTime.Now;
        //            }
        //            changedOrAddedItem.DateModified = DateTime.Now;
        //        }
        //    }
        //    return base.SaveChanges();
        //}
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                builder.UseSqlServer(connectionString);
                return new AppDbContext(builder.Options);
            }
        }
    }
}
