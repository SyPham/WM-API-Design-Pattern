using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Interface
{
  public interface IOCUserService
    {
        Task<object> GetListUser(int ocid);
        Task<PagedList<ViewModel.OCUser.UserViewModelForOCUser>> GetListUser(int page = 1, int pageSize = 10, int ocid = 0, string text = "");
        Task<object> AddOrUpdate(int userid, int ocid, bool status);
    }
}
