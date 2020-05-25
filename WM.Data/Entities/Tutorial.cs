using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class Tutorial : DomainEntity<int>
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }

        public string URL { get; set; }
        public string Path { get; set; }
        public int? ProjectID { get; set; }
        public int? TaskID { get; set; }
        public virtual Project Project { get; set; }
        public virtual Task Task { get; set; }
    }
}
