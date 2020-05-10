using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Data.Entities;

namespace WM.Application.Interface
{
   public interface IAuthService
    {
        Task<User> Login(string username, string password);
        Task<User> Register(User user, string password);
        Task<User> FindByNameAsync(string username);
        Task<User> GetById(int Id);
        Task<Role> GetRolesAsync(int role);

        Task<User> Edit(string username);
    }
}
