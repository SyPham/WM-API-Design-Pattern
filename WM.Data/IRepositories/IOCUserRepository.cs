using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;

namespace WM.Data.IRepositories
{
  public interface IOCUserRepository: IDisposable
    {
        void Add(OCUser oCUser);
        void Update(OCUser oCUser);
        void Remove(OCUser oCUser);
        IQueryable<OCUser> FindAll();
        void RemoveMultiple(List<OCUser> oCUsers);
        Task<OCUser> FindByID(int OCID, int UserID);
    }
}
