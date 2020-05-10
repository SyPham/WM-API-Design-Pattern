using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{
    public class OCUserRepository : IOCUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public OCUserRepository(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public void Add(OCUser oCUser)
        {
            _context.Add(oCUser);
        }

        public void Remove(OCUser oCUser)
        {
             _context.Remove(oCUser);
        }

        public void RemoveMultiple(List<OCUser> oCUsers)
        {
            _context.RemoveRange(oCUsers);

        }

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }

        public IQueryable<OCUser> FindAll()
        {
            return _context.OCUsers.AsQueryable();
        }

        public async Task<OCUser> FindByID(int OCID, int UserID)
        {
            return await FindAll().FirstOrDefaultAsync(x=>x.OCID == OCID && x.UserID == UserID);
        }

        public void Update(OCUser oCUser)
        {
            _context.Set<OCUser>().Update(oCUser);
        }


    }
}
