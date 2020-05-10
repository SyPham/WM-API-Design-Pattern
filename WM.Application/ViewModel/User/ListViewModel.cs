using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Project
{
    public class ListViewModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool isLeader { get; set; }
    }
}

