using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class CommentDetail : DomainEntity<int>
    {
        [ForeignKey("CommentID")]
        public int CommentID { get; set; }
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        public bool Seen { get; set; }
        public virtual Comment Comment { get; set; }
        public virtual User Users { get; set; }
    }
}
