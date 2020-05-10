﻿
using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class Project : DomainEntity<int>
    {
        public Project()
        {
            CreatedDate = DateTime.Now;
        }

        public string Name { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public int Room { get; set; }
        public virtual Room RoomTable { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public virtual ICollection<Tutorial> Tutorials { get; set; } = new List<Tutorial>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}