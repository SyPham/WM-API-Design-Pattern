using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class Participant: DomainEntity<int>
    {
        public int UserID { get; set; }
        public int RoomID { get; set; }
    }
}
