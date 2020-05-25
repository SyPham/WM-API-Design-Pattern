using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class TeamMember : DomainEntity<int>
    {
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}
