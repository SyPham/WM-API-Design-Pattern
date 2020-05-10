using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{
    public class FollowRepository : EFRepository<Follow, int>, IFollowRepository
    {
        public FollowRepository(AppDbContext context) : base(context)
        {
        }
    }
}
