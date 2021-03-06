﻿using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class ManagerConfiguration : DbEntityConfiguration<Manager>
    {
        public override void Configure(EntityTypeBuilder<Manager> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.User).WithMany(c => c.Managers).OnDelete(DeleteBehavior.NoAction);
            // etc.
        }
    }
}
