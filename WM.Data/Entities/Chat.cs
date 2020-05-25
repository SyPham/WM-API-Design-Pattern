using System;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class Chat : DomainEntity<int>
    {
        public Chat()
        {
            CreatedTime = DateTime.Now;
        }
        public int RoomID { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
