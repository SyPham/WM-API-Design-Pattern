using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class Comment : DomainEntity<int>
    {
        public Comment()
        {
            CreatedTime = DateTime.Now;
        }

        public int TaskID { get; set; }
        public int UserID { get; set; }
        public int ParentID { get; set; }
        public string Content { get; set; }
        public string TaskCode { get; set; }
        public int Level { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
