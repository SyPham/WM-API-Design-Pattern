using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Hub;
using WM.Application.Interface;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
   public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadImageRepository _uploadImageRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IManagerRepository _manageRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        private readonly IConfiguration _configuaration;
        public ChatService(
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUploadImageRepository uploadImageRepository,
            IParticipantRepository participantRepository,
            IChatRepository chatRepository,
            IManagerRepository manageRepository,
            ITeamMemberRepository teamMemberRepository,
            IHubContext<WorkingManagementHub> hubContext,
            IConfiguration configuaration
            )
        {
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _uploadImageRepository = uploadImageRepository;
            _participantRepository = participantRepository;
            _chatRepository = chatRepository;
            _manageRepository = manageRepository;
            _teamMemberRepository = teamMemberRepository;
            _hubContext = hubContext;
            _configuaration = configuaration;
        }

        public async Task<object> AddMessageGroup(int roomid, string message)
        {
            try
            {
                var project = await _projectRepository.FindAll().FirstOrDefaultAsync(x => x.Room.Equals(roomid));
                var managers = await _manageRepository.FindAll().Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var members = await _teamMemberRepository.FindAll().Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var listAll = managers.Union(members);
                var listChats = new List<Chat>();
                var listParticipants = new List<Participant>();
                foreach (var user in listAll)
                {
                    listChats.Add(new Chat
                    {
                        Message = message,
                        UserID = user,
                        ProjectID = project.ID,
                        RoomID = roomid
                    });
                    listParticipants.Add(new Participant
                    {
                        UserID = user,
                        RoomID = roomid
                    });
                }
                await _participantRepository.AddMultipleAsync(listParticipants);
                await _chatRepository.AddMultipleAsync(listChats);
                await _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<object> GetAllMessageByRoomAndProject(int roomid)
        {
            var userModel = _userRepository.FindAll();
            var uploadImages = _uploadImageRepository.FindAll();
            var appSettings = _configuaration.GetSection("AppSettings").Get<AppSettings>();
            return await _chatRepository.FindAll().Where(x => x.RoomID.Equals(roomid)).Select(x => new ViewModel.Chat.ChatViewModel
            {
                UserID = x.UserID,
                Message = x.Message,
                CreatedTime = x.CreatedTime,
                RoomID = x.RoomID,
                ProjectID = x.ProjectID,
                ImageBase64 = userModel.FirstOrDefault(_ => _.ID.Equals(x.UserID)).ImageBase64,
                Images = uploadImages.Where(_ => _.ChatID == x.ID).Select(_ => appSettings.applicationUrl + "/images/chats/" + _.Image).ToList(),
                Username = userModel.FirstOrDefault(_ => _.ID.Equals(x.UserID)).Username.ToTitleCase()
            }).ToListAsync();
        }
        public async Task<Chat> AddMessageGroup(int roomid, string message, int userid)
        {
            try
            {
                var project = await _projectRepository.FindAll().FirstOrDefaultAsync(x => x.Room.Equals(roomid));
                var managers = await _manageRepository.FindAll().Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var members = await _teamMemberRepository.FindAll().Where(x => x.ProjectID.Equals(project.ID)).Select(x => x.UserID).ToListAsync();
                var listAll = managers.Union(members);
                var listChats = new List<Chat>();
                var listParticipants = new List<Participant>();

                //Neu chua co participan thi them vao
                if (!await _participantRepository.FindAll().AnyAsync(x => x.RoomID == roomid))
                {
                    foreach (var user in listAll)
                    {
                        listParticipants.Add(new Participant
                        {
                            UserID = user,
                            RoomID = roomid
                        });
                    }
                    await _unitOfWork.Commit();

                }
                var chat = new Chat
                {
                    Message = message,
                    UserID = userid,
                    ProjectID = project.ID,
                    RoomID = roomid
                };
                //add message userid
                await _chatRepository.AddAsync(chat);
                await _unitOfWork.Commit();
                await _hubContext.Clients.Group(chat.RoomID.ToString()).SendAsync("ReceiveMessageGroup", chat.RoomID.ToInt());
                return chat;
            }
            catch (Exception ex)
            {
                return new Chat();
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<int> JoinGroup(int projectid)
        {
            //if (!await _context.Projects.AnyAsync(x => x.ID.Equals(projectid)))
            //{
            //    return 0;
            //}
            //if (await _context.Rooms.AnyAsync(x => x.ProjectID.Equals(projectid)))
            //{
            //    return (await _context.Rooms.FirstOrDefaultAsync(x => x.ProjectID.Equals(projectid))).ID;
            //}
            //else
            //{
            //    var project = await _context.Rooms.FirstOrDefaultAsync(x => x.ProjectID.Equals(projectid));
            //    var room = new Room
            //    {
            //        ProjectID = project.ID,
            //        Name = project.Name
            //    };
            //    await _context.AddAsync(room);
            //    await _context.SaveChangesAsync();
            //    return room.ID;
            //}
            throw new NotImplementedException();
        }

        public Task<object> Remove(int projectid, int roomid)
        {
            throw new NotImplementedException();
        }
   
        public async Task<bool> UploadImage(List<UploadImage> uploadImages)
        {
            var imagesList = new List<UploadImage>();
            foreach (var item in uploadImages)
            {
                imagesList.Add(new UploadImage
                {
                    ChatID = item.ChatID,
                    Image = item.Image
                });
            }
            try
            {
                await _uploadImageRepository.AddMultipleAsync(imagesList);
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }


            throw new NotImplementedException();
        }
    }
}
