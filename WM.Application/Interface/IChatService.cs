﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;

namespace WM.Application.Interface
{
   public interface IChatService
    {
        Task<object> GetAllMessageByRoomAndProject(int roomid);
        Task<object> AddMessageGroup(int roomid, string message);
        Task<object> Remove(int projectid, int roomid);
        Task<int> JoinGroup(int projectid);
        Task<Chat> AddMessageGroup(int roomid, string message, int userid);
        Task<bool> UploadImage(List<UploadImage> uploadImages);
    }
}
