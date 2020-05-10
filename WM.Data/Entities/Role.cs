
using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class Role : DomainEntity<int>
    {
        public string Name { get; set; }
    }
}
