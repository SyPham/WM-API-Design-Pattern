using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class OCUserConfiguration : DbEntityConfiguration<OCUser>
    {
        public override void Configure(EntityTypeBuilder<OCUser> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.Property(c => c.OCID).IsRequired();
            entity.HasKey(c =>new { c.OCID, c.UserID });
            entity.HasIndex(c =>new { c.UserID });
            // etc.
        }
    }
}
