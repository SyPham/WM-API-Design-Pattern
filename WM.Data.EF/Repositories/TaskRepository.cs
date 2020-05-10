using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{
    public class TaskRepository : EFRepository<Entities.Task, int>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
        }
    }
}
