using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Application.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _mapperConfig;
        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            MapperConfiguration mapperConfig
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _mapperConfig = mapperConfig;
            _unitOfWork = unitOfWork;
        }

        public async Task<User> Edit(string username)
        {
            var item = await _userRepository.FindAll().FirstOrDefaultAsync(x => x.Username == username);
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash("1", out passwordHash, out passwordSalt);
            item.PasswordHash = passwordHash;
            item.PasswordSalt = passwordSalt;

           await _unitOfWork.Commit();

            return item;
        }

        public async Task<User> FindByNameAsync(string username)
        {
            var item = await _userRepository.FindAll().FirstOrDefaultAsync(x => x.Username == username);
            if (item != null)
                return item;

            return null;
        }

        public async Task<User> GetById(int Id)
        {
            return await _userRepository.FindAll().FirstOrDefaultAsync(x => x.ID == Id);
        }

        public async Task<Role> GetRolesAsync(int role)
        {
            return await _roleRepository.FindAll().FirstOrDefaultAsync(x => x.ID == role);
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _userRepository.FindAll().FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _userRepository.AddAsync(user);

          await _unitOfWork.Commit();

            return user;
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
    }
}
