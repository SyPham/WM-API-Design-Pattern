using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class Manager : DomainEntity<int>
    {
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        [ForeignKey("ProjectID")]
        public int ProjectID { get; set; }
        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}
