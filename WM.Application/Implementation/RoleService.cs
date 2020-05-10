using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{

    public class RoleService : IRoleService, ICommonService<Role>
    {
        private readonly IRoleRepository _roleRepository;
        IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _mapperConfig;

        public RoleService(
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            MapperConfiguration mapperConfig
            )
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _mapperConfig = mapperConfig;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Create(Role entity)
        {
           await _roleRepository.AddAsync(entity);

            try
            {
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public async Task<bool> Delete(int id)
        {
            var entity =await _roleRepository.FindByIdAsync(id);
            _roleRepository.Remove(entity);

            try
            {
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public async Task<List<Role>> GetAll()
        {
            return await _roleRepository.FindAll().ToListAsync();
        }

        public async Task<PagedList<Role>> GetAllPaging(int page, int pageSize, string text)
        {
            var source = _roleRepository.FindAll();
            if(!text.IsNullOrEmpty())
            {
                source = source.Where(x => x.Name.ToLower().Contains(text.ToLower()));
            }
            return await PagedList<Role>.CreateAsync( source, page, pageSize);

        }

        public async Task<Role> GetByID(int id)
        {
            return await _roleRepository.FindByIdAsync(id);
        }

        public async Task<bool> Update(Role entity)
        {
            _roleRepository.Update(entity);

            try
            {
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
    }
}
