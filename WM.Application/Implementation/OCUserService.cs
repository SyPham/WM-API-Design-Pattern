using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Application.ViewModel.OCUser;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
    public class OCUserService : IOCUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IOCUserRepository _ocUserRepository;
        private readonly IOCRepository _ocRepository;
        private readonly IOCService _oCService;

        public OCUserService(IUnitOfWork unitOfWork, 
                            IUserService userService, 
                            IUserRepository userRepository,
                            IOCUserRepository ocUserRepository,
                            IOCRepository ocRepository,
                            IOCService oCService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _userRepository = userRepository;
            _ocUserRepository = ocUserRepository;
            _ocRepository = ocRepository;
            _oCService = oCService;
        }

        public async Task<object> AddOrUpdate(int userid, int ocid, bool status)
        {
            try
            {
                var item = await _ocUserRepository.FindAll().Include(x => x.OC).FirstOrDefaultAsync(x => x.OCID == ocid && x.UserID == userid);
                var user = await _userRepository.FindByIdAsync(userid);
                //Neu user do chuyen  status ve false thi xoa luon
                if (!status && item != null)
                {
                    user.LevelOC = 0;
                    user.OCID = 0;
                    _ocUserRepository.Remove(item);

                }
                else
                {
                    //Kiem tra xem user do co thuoc phong nao khac khong
                    var item2 = await _ocUserRepository.FindAll().FirstOrDefaultAsync(x => x.UserID == userid);
                    if (item2 != null && item2.Status)
                        return new
                        {
                            status = false,
                            message = "The user has already existed in other department!"
                        };
                    else
                    {
                        var ocModel = await _ocRepository.FindByIdAsync(ocid);
                        user.LevelOC = ocModel.Level;
                        user.OCID = ocid;

                        var oc = new OCUser();
                        oc.OCID = ocid;
                        oc.UserID = userid;
                        oc.Status = true;
                        _ocUserRepository.Add(oc);

                    }

                }
                await _unitOfWork.Commit();
                ////Neu chua co thi them moi
                //if (item == null)
                //{

                //    //Kiem tra xem lai xem trong OCUSer da gan user nay cho department nao chua
                //    var item2 = await _context.OCUsers.FirstOrDefaultAsync(x => x.UserID == userid);
                //    if (item2 != null && item2.Status)
                //        return new
                //        {
                //            status = false,
                //            message = "The user has already existed in other department!"
                //        };
                //    user.LevelOC = item.OC.Level;
                //    var oc = new OCUser();
                //    oc.OCID = ocid;
                //    oc.UserID = userid;
                //    oc.Status = true;
                //    user.OCID = ocid;
                //    _context.OCUsers.Add(oc);
                //    await _context.SaveChangesAsync();
                //}//co roi thi update
                //else
                //{
                //    item.Status = !item.Status;
                //    if (item.Status == true)
                //    {
                //        user.OCID = ocid;
                //        user.LevelOC = item.OC.Level;
                //    }
                //    else
                //    {
                //        user.OCID = 0;
                //        user.LevelOC = 0;
                //    }
                //    await _context.SaveChangesAsync();
                //}

                return new
                {
                    status = true,
                    message = "Successfully!"
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                    message = "Error!"
                };
            }
        }

        public async Task<object> GetListUser(int ocid)
        {
            var source = await _userRepository.FindAll().Select(x => new UserViewModelForOCUser
            {
                ID = x.ID,
                Username = x.Username,
                RoleName = x.Role.Name,
                RoleID = x.RoleID,
                Status = _ocUserRepository.FindAll().Any(a => a.UserID == x.ID && a.OCID == ocid && a.Status == true)
            }).ToListAsync();
            return source;
        }

        public async Task<PagedList<UserViewModelForOCUser>> GetListUser(int page = 1, int pageSize = 10, int ocid = 0, string text = "")
        {
            var source = await _userRepository.FindAll().Select(x => new UserViewModelForOCUser
            {
                ID = x.ID,
                Username = x.Username,
                RoleName = x.Role.Name,
                RoleID = x.RoleID,
                Status = _ocUserRepository.FindAll().Any(a => a.UserID == x.ID && a.OCID == ocid && a.Status == true)
            }).ToListAsync();
            if (!text.IsNullOrEmpty())
            {
                source = source.Where(x => x.Username.ToLower().Contains(text.ToLower())).ToList();
            }
            return PagedList<UserViewModelForOCUser>.Create(source, page, pageSize);
        }
    }
}
