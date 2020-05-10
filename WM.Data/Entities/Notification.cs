using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class Notification : DomainEntity<int>
    {
        public Notification()
        {
            CreatedTime = DateTime.Now;
        }

        [ForeignKey("UserID")]
        public int UserID { get; set; }
        [ForeignKey("TaskID")]
        public int TaskID { get; set; }
        public string Message { get; set; }
        public string Function { get; set; }
        public string URL { get; set; }
        public DateTime CreatedTime { get; set; }
        public virtual NotificationDetail NotificationDetails { get; set; }
        public virtual User User { get; set; }
    }
}
