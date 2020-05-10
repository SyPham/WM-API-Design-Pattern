using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;

namespace WM.Data.IRepositories
{
   public interface IHistoryRepository : IRepository<History, int>
    {
    }
}
