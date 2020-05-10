using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class LikeComment : DomainEntity<int>
    {
        public int UserID { get; set; }
        public int CommentID { get; set; }
    }
}
