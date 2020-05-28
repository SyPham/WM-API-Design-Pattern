using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Application.ViewModel.Project;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _mapperConfig;
        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            MapperConfiguration mapperConfig
            )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mapperConfig = mapperConfig;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> ChangeAvatar(int userid, string imagePath)
        {
            try
            {
                var item =  _userRepository.FindAll().FirstOrDefault(x=>x.ID == userid);
                item.ImageBase64 = Convert.FromBase64String(imagePath);
               await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
                return true;
            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<bool> Create(UserViewModel entity)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(entity.Password, out passwordHash, out passwordSalt);
            var user = _mapper.Map<User>(entity);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _userRepository.AddAsync(user);

            try
            {
              await  _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public async Task<bool> Delete(int id)
        {
            var item = _userRepository.FindAll().FirstOrDefault(x => x.ID == id);
            _userRepository.Remove(item);
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

        public async Task<List<User>> GetAll()
        {
            return await _userRepository.FindAll().Where(x => x.Username != "admin").ToListAsync();
        }

        public async Task<PagedList<ListViewModel>> GetAllPaging(int page, int pageSize, string keyword)
        {
            var source =  _userRepository.FindAll().Where(x => x.Username != "admin").OrderByDescending(x => x.ID).Select(x => new ListViewModel { isLeader = x.isLeader, ID = x.ID, Username = x.Username, Email = x.Email, RoleName = x.Role.Name, RoleID = x.RoleID }).AsQueryable();
            if (!keyword.IsNullOrEmpty())
            {
                source = source.Where(x => x.Username.Contains(keyword) || x.Email.Contains(keyword));
            }
            return await PagedList<ListViewModel>.CreateAsync(source, page, pageSize);
        }

        public async Task<User> GetByID(int id)
        {
            return await _userRepository.FindAll().FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task<object> GetListUser()
        {
            return await _userRepository.FindAll().Where(x => x.Username != "admin").Select(x => new { x.ID, x.Username, x.Email, RoleName = x.Role.Name, x.RoleID }).ToListAsync();
        }

        public async Task<List<string>> GetUsernames()
        {
            return await _userRepository.FindAll().Where(x => x.Username != "admin").Select(x => x.Username).ToListAsync();
        }

        public async Task<bool> ResetPassword(int id)
        {
            byte[] passwordHash, passwordSalt;
            var item = _userRepository.FindAll().FirstOrDefault(x => x.ID == id);
            string pass = "1";
            CreatePasswordHash(pass, out passwordHash, out passwordSalt);
            try
            {
                item.PasswordHash = passwordHash;
                item.PasswordSalt = passwordSalt;
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> Update(User entity)
        {
            var item =await _userRepository.FindAll().FirstOrDefaultAsync(x => x.ID == entity.ID);
            item.Username = entity.Username;
            item.Email = entity.Email;
            item.RoleID = entity.RoleID;
            item.isLeader = entity.isLeader;
            if (item.PasswordHash.IsNullOrEmpty() && item.PasswordSalt.IsNullOrEmpty())
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("1", out passwordHash, out passwordSalt);

                item.PasswordHash = passwordHash;
                item.PasswordSalt = passwordSalt;
            }
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

        public async Task<bool> UploapProfile(int id, byte[] image)
        {
            var item = _userRepository.FindAll().FirstOrDefault(x => x.ID == id);
            if (item == null)
            {
                return false;
            }

            try
            {
                item.ImageBase64 = image;
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
