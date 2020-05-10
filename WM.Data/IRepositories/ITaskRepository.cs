using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Infrastructure.Interfaces;

namespace WM.Data.IRepositories
{
   public interface ITaskRepository : IRepository<Entities.Task, int>
    {
    }
}
