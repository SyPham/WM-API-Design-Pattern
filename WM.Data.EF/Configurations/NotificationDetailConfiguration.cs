using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class NotificationDetailConfiguration : DbEntityConfiguration<NotificationDetail>
    {
        public override void Configure(EntityTypeBuilder<NotificationDetail> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.Property(c => c.NotificationID).IsRequired();
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.User).WithMany(c => c.NotificationDetails).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(c =>new { c.NotificationID, c.UserID });
            // etc.
        }
    }
}
