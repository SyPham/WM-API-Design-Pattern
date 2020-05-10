using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class DeputyConfiguration : DbEntityConfiguration<Deputy>
    {
        public override void Configure(EntityTypeBuilder<Deputy> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.User).WithMany(c => c.Deputies).OnDelete(DeleteBehavior.NoAction);
            // etc.
        }
    }
}
