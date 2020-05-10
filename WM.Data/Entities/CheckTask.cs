using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class CheckTask : DomainEntity<int>
    {
        public DateTime CreatedDate { get; set; }
        public string Function { get; set; }
    }
}
