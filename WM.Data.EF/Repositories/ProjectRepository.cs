using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{
    public class ProjectRepository : EFRepository<Project, int>, IProjectRepository
    {
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }
    }
}
