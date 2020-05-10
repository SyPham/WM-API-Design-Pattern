using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class NotificationDetail : DomainEntity<int>
    {
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        [ForeignKey("NotificationID")]
        public int NotificationID { get; set; }
        public bool Seen { get; set; }
        public virtual User User { get; set; }
        public virtual Notification Notification { get; set; }
    }
    /*
     From system
     henry
     Update Status
     Finish Status
     Seen
    2/19/2020 3:18
     */
}
