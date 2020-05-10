using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Ultilities.Helpers;

namespace WM.Application.Interface
{
   public interface ICommonService<T>
    {
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(int id);
        Task<T> GetByID(int id);
        Task<List<T>> GetAll();
        Task<PagedList<T>> GetAllPaging(int page, int pageSize, string text);
    }
}
