using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.EF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WM.Data.Entities;
namespace WM.Data.EF.Configurations
{
    public class CommentConfiguration : DbEntityConfiguration<Comment>
    {
        public override void Configure(EntityTypeBuilder<Comment> entity)
        {
            //entity.Property(c => c.UserID).IsRequired();
            //entity.HasKey(c => c.ID );
            //entity.HasOne(c => c.User).WithMany(c => c.Comment).OnDelete(DeleteBehavior.NoAction);
            // etc.
            entity.HasOne(e => e.User)                     // Chỉ ra phía một
           .WithMany(user => user.Comments)         // Chỉ ra phía nhiều
           .HasForeignKey("UserID")                 // Chỉ ra tên FK
           .OnDelete(DeleteBehavior.Cascade)            // Ứng xử khi User bị xóa
           .HasConstraintName("FK_Comments_Users_UserID"); // Tự đặt tên Constrain

         
        }
    }
}
