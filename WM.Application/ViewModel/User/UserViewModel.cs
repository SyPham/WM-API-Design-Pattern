using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Project
{
   public class UserViewModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int OCID { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public int RoleID { get; set; }
        public bool isLeader { get; set; }
        public IFormFile Photo { get; set; }

    }
}
