
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class Deputy : DomainEntity<int>
    {
        public int UserID { get; set; }
        public int TaskID { get; set; }
        public virtual User User { get; set; }
        public virtual Task Task { get; set; }
    }
}
