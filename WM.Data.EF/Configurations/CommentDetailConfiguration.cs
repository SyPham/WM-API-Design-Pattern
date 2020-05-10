using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class CommentDetailConfiguration : DbEntityConfiguration<CommentDetail>
    {
        public override void Configure(EntityTypeBuilder<CommentDetail> entity)
        {
            entity.Property(c => c.UserID).IsRequired();
            entity.Property(c => c.CommentID).IsRequired();
            entity.HasKey(c => c.ID );
            entity.HasOne(c => c.Users).WithMany(c => c.CommentDetails).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(c =>new { c.CommentID, c.UserID });
            // etc.
        }
    }
}
