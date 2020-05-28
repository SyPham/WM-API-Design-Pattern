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
            //entity.Property(c => c.UserID).IsRequired();
            //entity.Property(c => c.CommentID).IsRequired();
            //entity.HasKey(c => c.ID );
            //entity.HasOne(c => c.Users).WithMany(c => c.CommentDetails).OnDelete(DeleteBehavior.NoAction);
            //entity.HasIndex(c =>new { c.CommentID, c.UserID });
            //// etc.
            entity.HasOne(e => e.Comment)                     // Chỉ ra phía một
                 .WithMany(detail => detail.CommentDetails)         // Chỉ ra phía nhiều
                 .HasForeignKey("CommentID")                 // Chỉ ra tên FK
                 .OnDelete(DeleteBehavior.Cascade)            // Ứng xử khi User bị xóa
                 .HasConstraintName("FK_CommentDetails_Comments_CommentID"); // Tự đặt tên Constrain

        }
    }
}
