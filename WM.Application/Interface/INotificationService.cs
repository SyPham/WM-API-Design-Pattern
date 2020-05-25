using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.Notification;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Interface
{
   public interface INotificationService 
    {
        Task<bool> Create(CreateNotifyParams entity);
        System.Threading.Tasks.Task CreateRange(List<CreateNotifyParams> entity);
        Task<bool> Update(Notification entity);
        Task<bool> Delete(int id);
        Task<Notification> GetByID(int id);
        Task<List<Notification>> GetAll();
        Task<PagedList<Notification>> GetAllPaging(int page, int pageSize);
        Task<bool> Seen(int id);
        Task<object> GetAllByUserID(int userid, int page, int pageSize);
        Task<object> GetNotificationByUser(int userid, int page, int pageSize);
    }
}
