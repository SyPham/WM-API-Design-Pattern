using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class TaskConfiguration : DbEntityConfiguration<Task>
    {
        public override void Configure(EntityTypeBuilder<Task> entity)
        {
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.User).WithMany(c => c.Tasks).OnDelete(DeleteBehavior.Cascade);
            // etc.
        }
    }
}
