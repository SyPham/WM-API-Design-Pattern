using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
  public  class Room : DomainEntity<int>
    {
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public bool Type { get; set; }
    }
}
