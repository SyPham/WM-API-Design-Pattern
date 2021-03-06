﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.OCUser;
using WM.Application.ViewModel.Project;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Interface
{
   public interface IUserService
    {
        Task<bool> Create(UserViewModel entity);
        Task<bool> Update(User entity);
        Task<bool> Delete(int id);
        Task<bool> ResetPassword(int id);
        Task<User> GetByID(int id);
        Task<List<User>> GetAll();
        Task<List<string>> GetUsernames();
        Task<bool> UploapProfile(int id, byte[] image);
        Task<object> GetListUser();
        Task<bool> ChangeAvatar(int userid, string imagePath);
        Task<PagedList<ListViewModel>> GetAllPaging(int page, int pageSize, string keyword);
    }
}
