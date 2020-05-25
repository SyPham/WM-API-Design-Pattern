using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Application.ViewModel.Line;
using WM.Application.ViewModel.Notification;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly ILineService _lineService;
        private MapperConfiguration _configMapper;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationDetailRepository _notificationDetailRepository;
        const int ADMIN = 1;
        public NotificationService(INotificationRepository notificationRepository, 
                                    IMapper mapper,
                                    MapperConfiguration configMapper,
                                    IUserRepository userRepository,
                                    IUnitOfWork unitOfWork,
                                    INotificationDetailRepository notificationDetailRepository,
                                    ILineService lineService)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _configMapper = configMapper;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _notificationDetailRepository = notificationDetailRepository;
            _lineService = lineService;
        }

        public async Task<bool> Create(CreateNotifyParams entity)
        {
            try
            {
                var accessTokenLines = _userRepository.FindAll().Where(x => entity.Users.Contains(x.ID)).Select(x => x.AccessTokenLineNotify).ToList();
                foreach (var token in accessTokenLines)
                {
                    await _lineService.SendMessage(new MessageParams { Message = entity.Message, Token = token });
                }
                var item = new Notification
                {
                    TaskID = entity.TaskID,
                    Message = entity.Message,
                    URL = entity.URL,
                    Function = entity.AlertType.ToString()
                };
                if (entity.UserID == 0 || entity.UserID == null)

                    item.UserID = ADMIN;
                else
                    item.UserID = entity.UserID.Value;
               await _notificationRepository.AddAsync(item);
               await _unitOfWork.Commit();

                if (entity.Users.Count > 0 || entity.Users != null)
                {
                    var details = new List<NotificationDetail>();
                    foreach (var user in entity.Users)
                    {
                        details.Add(new NotificationDetail
                        {
                            NotificationID = item.ID,
                            UserID = user,
                            Seen = false
                        });
                    }
                  await  _notificationDetailRepository.AddMultipleAsync(details);
                }
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async System.Threading.Tasks.Task CreateRange(List<CreateNotifyParams> entity)
        {
            foreach (var item in entity)
            {
                await Create(item);
            }
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _notificationRepository.FindByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            _notificationRepository.Remove(entity);
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

        public async Task<List<Notification>> GetAll()
        {
            return await _notificationRepository.FindAll().ToListAsync();
        }

        public async Task<object> GetAllByUserID(int userid, int page, int pageSize)
        {
            var list = _notificationDetailRepository.FindAll()
                 .Where(x => x.UserID == userid)
                 .Include(x => x.Notification).ThenInclude(x => x.User)
                 .Include(x => x.Notification).ThenInclude(x => x.NotificationDetails).ThenInclude(x => x.User)
                 .Include(x => x.User).ProjectTo<NotificationViewModel>(_configMapper);
            // var listAsync = await model.ToListAsync();
            //var list =  _mapper.Map<List<NotificationViewModel>>(model);
            var total = 0;
            var listID = new List<int>();

            foreach (var item in list)
            {
                if (item.Seen == false)
                {
                    total++;
                    listID.Add(item.ID);
                }
            }
            var paging = await PagedList<NotificationViewModel>.CreateAsync(list, page, pageSize);

            return new
            {
                model = paging.OrderByDescending(x => x.ID).ToList(),
                total,
                paging.TotalCount
            };
        }

        public Task<PagedList<Notification>> GetAllPaging(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<Notification> GetByID(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetNotificationByUser(int userid, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Seen(int id)
        {
            var entity = await _notificationDetailRepository.FindByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            entity.Seen = true;
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

        public async Task<bool> Update(Notification entity)
        {
            var item = await _notificationRepository.FindByIdAsync(entity.ID);
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
