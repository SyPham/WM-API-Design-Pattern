
using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class OC : DomainEntity<int>
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
