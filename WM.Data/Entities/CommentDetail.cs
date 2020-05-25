using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class CommentDetail : DomainEntity<int>
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public bool Seen { get; set; }
    }
}
