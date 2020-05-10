using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{

    public class OCRepository : EFRepository<OC, int>, IOCRepository
    {
        public OCRepository(AppDbContext context) : base(context)
        {
        }
    }
}
