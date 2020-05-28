using System;
using System.Collections.Generic;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{

    public class User : DomainEntity<int>
    {
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int OCID { get; set; }
        public int LevelOC { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public string ImageURL { get; set; }
        public string AccessTokenLineNotify { get; set; }
        public byte[] ImageBase64 { get; set; }
        public bool isLeader { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<NotificationDetail> NotificationDetails { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Follow> Follows { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Deputy> Deputies { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<Manager> Managers { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }

    }
}
