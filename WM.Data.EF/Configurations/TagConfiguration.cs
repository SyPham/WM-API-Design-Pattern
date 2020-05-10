using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class TagConfiguration : DbEntityConfiguration<Tag>
    {
        public override void Configure(EntityTypeBuilder<Tag> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.User).WithMany(c => c.Tags).OnDelete(DeleteBehavior.NoAction);
            // etc.
        }
    }
}
