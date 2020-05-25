using AutoMapper;
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
using WM.Application.ViewModel.Comment;
using WM.Application.ViewModel.Notification;
using WM.Data.Entities;
using WM.Data.Enums;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
    public class CommentService : ICommentService
    {

        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuaration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskRepository _taskRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentDetailRepository _commentDetailRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUploadImageRepository _uploadImageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<WorkingManagementHub> _hubContext;

        public CommentService(
            IUnitOfWork unitOfWork,
            ITaskRepository taskRepository,
            ITagRepository tagRepository,
            IUploadImageRepository uploadImageRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ICommentRepository commentRepository,
            ICommentDetailRepository commentDetailRepository,
            IMapper mapper,
            IHubContext<WorkingManagementHub> hubContext,
            IConfiguration configuaration,
            INotificationService notificationService)
        {
            _notificationService = notificationService;
            _configuaration = configuaration;
            _unitOfWork = unitOfWork;
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _uploadImageRepository = uploadImageRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _commentRepository = commentRepository;
            _commentDetailRepository = commentDetailRepository;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        private async Task<Tuple<List<int>, string, string>> AlertReplyComment(int taskid, int userid, string comment, ClientRouter clientRouter)
        {
            var task = await _taskRepository.FindByIdAsync(taskid);
            var user = await _userRepository.FindByIdAsync(userid);
            var pics = await _tagRepository.FindAll().Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
            string projectName = string.Empty;
            if (task.ProjectID > 0)
                projectName = (await _projectRepository.FindByIdAsync(task.ProjectID.Value)).Name;
            string message = string.Empty;
            string urlResult = string.Empty;
            switch (clientRouter)
            {
                case ClientRouter.ToDoList:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.History:
                    urlResult = $"/history-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Follow:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.ProjectDetail:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Abnormal:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Routine:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                default:
                    break;
            }
            switch (task.JobTypeID)
            {
                case JobType.Project:
                    message = $"{user.Username.ToTitleCase()} replied to the comment: '{comment}'.";
                    break;
                case JobType.Abnormal:
                case JobType.Routine:
                    message = $"{user.Username.ToTitleCase()} replied to the comment: '{comment}'.";
                    break;
                default:
                    break;
            }
            return Tuple.Create(pics, message, urlResult);
        }
        private async Task<Tuple<List<int>, string, string>> AlertComment(int taskid, int userid, ClientRouter clientRouter)
        {
            var task = await _taskRepository.FindByIdAsync(taskid);
            var user = await _userRepository.FindByIdAsync(userid);
            var pics = await _tagRepository.FindAll().Where(_ => _.TaskID.Equals(taskid)).Select(_ => _.UserID).ToListAsync();
            string projectName = string.Empty;
            if (task.ProjectID > 0)
                projectName = (await _projectRepository.FindByIdAsync(task.ProjectID.Value)).Name;
            string message = string.Empty;
            string urlResult = string.Empty;
            switch (clientRouter)
            {
                case ClientRouter.ToDoList:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.History:
                    urlResult = $"/history-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Follow:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.ProjectDetail:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Abnormal:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                case ClientRouter.Routine:
                    urlResult = $"/todolist-comment/{taskid}/{task.JobName}";
                    break;
                default:
                    break;
            }
            switch (task.JobTypeID)
            {
                case JobType.Project:
                    message = $"{user.Username.ToTitleCase()} commented on your task' {task.JobName}' of {projectName}.";
                    break;
                case JobType.Abnormal:
                case JobType.Routine:
                    message = $"{user.Username.ToTitleCase()} commented on your task '{task.JobName}'.";
                    break;
                default:
                    break;
            }
            return Tuple.Create(pics, message, urlResult);
        }
        public async Task<Tuple<bool, string, Comment>> Add(AddCommentViewModel commentViewModel, int currentUser)
        {
            try
            {
                var comment = _mapper.Map<Comment>(commentViewModel);
                comment.Level = 1;
                await _commentRepository.AddAsync(comment);
                await _unitOfWork.Commit();
                await _commentDetailRepository.AddAsync(new CommentDetail { CommentID = comment.ID, UserID = comment.UserID, Seen = true });
                await _unitOfWork.Commit();

                var alert = await AlertComment(comment.TaskID, comment.UserID, commentViewModel.ClientRouter);
                var task = await _taskRepository.FindByIdAsync(comment.TaskID);
                if (!currentUser.Equals(task.CreatedBy))
                {
                    alert.Item1.Add(task.CreatedBy);
                    var listUsers = alert.Item1.Where(x => x != currentUser).Distinct().ToList();
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.PostComment,
                        Message = alert.Item2,
                        Users = listUsers,
                        TaskID = comment.TaskID,
                        URL = alert.Item3,
                        UserID = comment.UserID
                    });
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", listUsers, "message");

                    return Tuple.Create(true, string.Join(",", listUsers.ToArray()), comment);
                }
                else
                {
                    return Tuple.Create(true, string.Empty, comment);
                }

            }
            catch (Exception)
            {
                return Tuple.Create(false, string.Empty, new Comment());
            }

        }

        public async Task<Tuple<bool, string, Comment>> AddSub(AddSubViewModel subcomment)
        {
            try
            {
                var item = await _commentRepository.FindByIdAsync(subcomment.ParentID);
                var comment = new Comment
                {
                    TaskID = subcomment.TaskID,
                    TaskCode = subcomment.TaskCode,
                    UserID = subcomment.UserID,
                    Content = subcomment.Content,
                };
                if (item.Level == 1)
                {
                    comment.ParentID = subcomment.ParentID;
                    comment.Level = item.Level + 1;
                }
                if (item.Level >= 2)
                {
                    comment.Level = 2;
                    comment.ParentID = item.ParentID;
                }
                await _commentRepository.AddAsync(comment);
                 await _unitOfWork.Commit();
                await _commentDetailRepository.AddAsync(new CommentDetail { CommentID = comment.ID, UserID = comment.UserID, Seen = true });
                 await _unitOfWork.Commit();

                var comtParent = await _commentRepository.FindByIdAsync(subcomment.ParentID);
                //Neu tra loi chinh binh luan cua minh thi khong
                if (subcomment.CurrentUser.Equals(comtParent.UserID))
                    return Tuple.Create(true, string.Empty, comment);
                else
                {
                    var alert = await AlertReplyComment(comment.TaskID, comment.UserID, comtParent.Content, subcomment.ClientRouter);
                    alert.Item1.Add(comtParent.UserID);
                    var listUsers = alert.Item1.Where(x => x != subcomment.CurrentUser).Distinct().ToList();
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.ReplyComment,
                        Message = alert.Item2,
                        Users = listUsers,
                        TaskID = comment.TaskID,
                        URL = alert.Item3,
                        UserID = comment.UserID
                    });
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", listUsers, "message");
                    return Tuple.Create(true, string.Join(",", listUsers.ToArray()), comment);
                }
            }
            catch (Exception)
            {
                return Tuple.Create(false, string.Empty, new Comment());

            }
        }
        private async Task<List<CommentTreeView>> GetAll(int userID)
        {
            var detail = _commentDetailRepository.FindAll();
            return await _commentRepository.FindAll()
                .Join(_userRepository.FindAll(),
                comt => comt.UserID,
                user => user.ID,
                (comt, user) => new { comt, user })
                .Select(_ => new CommentTreeView
                {
                    ID = _.comt.ID,
                    UserID = _.comt.UserID,
                    Username = _.user.Username,
                    ImageBase64 = _.user.ImageBase64,
                    Content = _.comt.Content,
                    ParentID = _.comt.ParentID,
                    TaskID = _.comt.TaskID,
                    Level = _.comt.Level,
                    CreatedTime = _.comt.CreatedTime,
                    Seen = detail.FirstOrDefault(d => d.CommentID.Equals(_.comt.ID) && d.UserID.Equals(userID)) == null ? false : true
                })
                .ToListAsync();
        }
        public List<CommentTreeView> GetChildren(List<CommentTreeView> comments, int parentid)
        {
            var uploadImage = _uploadImageRepository.FindAll();
            var appSettings = _configuaration.GetSection("AppSettings").Get<AppSettings>();
            return comments
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new CommentTreeView()
                    {
                        ID = c.ID,
                        UserID = c.UserID,
                        Username = c.Username,
                        Content = c.Content ?? "",
                        ImageBase64 = c.ImageBase64,
                        ParentID = c.ParentID,
                        CreatedTime = c.CreatedTime,
                        Seen = c.Seen,
                        Level = c.Level,
                        Images = uploadImage.Where(x => x.CommentID == c.ID).Select(x => appSettings.applicationUrl + "/images/comments/" + x.Image).ToList() ?? new List<string>(),
                        children = GetChildren(comments, c.ID)
                    })
                    .OrderByDescending(x => x.CreatedTime)
                    .ToList();
        }
        public async Task<IEnumerable<TaskHasComment>> GetAllCommentWithTask(int userid)
        {
            var listComments = await GetAll(userid);
            List<CommentTreeView> hierarchy = new List<CommentTreeView>();
            hierarchy = listComments.Where(c => c.ParentID.Equals(0))
                            .Select(c => new CommentTreeView()
                            {
                                ID = c.ID,
                                UserID = c.UserID,
                                Username = c.Username,
                                Content = c.Content,
                                ImageBase64 = c.ImageBase64,
                                ParentID = c.ParentID,
                                Seen = c.Seen,
                                TaskID = c.TaskID,
                                CreatedTime = c.CreatedTime,
                                Level = c.Level,
                                children = GetChildren(listComments, c.ID)
                            })
                            .ToList();
            var tasks = _taskRepository.FindAll().ToList().Join(hierarchy,
                t => t.ID,
                ct => ct.TaskID,
                (t, ct) => new
                {
                    t,
                    ct
                }).Select(x => new TaskHasComment
                {
                    TaskID = x.t.ID,
                    TaskName = x.t.JobName,
                    CommentTreeViews = x.ct
                }).ToList();
            return tasks;
        }
        private async Task<List<CommentTreeView>> GetAll(int taskID, int userID)
        {
            //var task =await _context.Tasks.FindAsync(taskID);
            var detail = _commentDetailRepository.FindAll();
            return await _commentRepository.FindAll()
                .Join(_userRepository.FindAll(),
                comt => comt.UserID,
                user => user.ID,
                (comt, user) => new { comt, user })
                .Where(x => x.comt.TaskID.Equals(taskID))
                .Select(_ => new CommentTreeView
                {
                    ID = _.comt.ID,
                    UserID = _.comt.UserID,
                    Username = _.user.Username,
                    ImageBase64 = _.user.ImageBase64,
                    Content = _.comt.Content,
                    Level = _.comt.Level,
                    ParentID = _.comt.ParentID,
                    CreatedTime = _.comt.CreatedTime,
                    Seen = detail.FirstOrDefault(d => d.CommentID.Equals(_.comt.ID) && d.UserID.Equals(userID)) == null ? false : true
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<CommentTreeView>> GetAllTreeView(int taskid, int userid)
        {
            var appSettings = _configuaration.GetSection("AppSettings").Get<AppSettings>();
            var uploadImage = _uploadImageRepository.FindAll();
            var listComments = await GetAll(taskid, userid);
            List<CommentTreeView> hierarchy = new List<CommentTreeView>();
            hierarchy = listComments.Where(c => c.ParentID.Equals(0))
                            .Select(c => new CommentTreeView()
                            {
                                ID = c.ID,
                                UserID = c.UserID,
                                Username = c.Username,
                                Content = c.Content ?? "",
                                ImageBase64 = c.ImageBase64,
                                ParentID = c.ParentID,
                                Seen = c.Seen,
                                CreatedTime = c.CreatedTime,
                                TaskID = c.TaskID,
                                Level = c.Level,
                                Images = uploadImage.Where(x => x.CommentID == c.ID).Select(x => appSettings.applicationUrl + "/images/comments/" + x.Image).ToList() ?? new List<string>(),
                                children = GetChildren(listComments, c.ID)
                            })
                            .ToList();
            return hierarchy.OrderByDescending(x => x.CreatedTime).ToList();
        }

        public async Task<object> Seen(int comtID, int userID)
        {
            try
            {
                var detail = await _commentDetailRepository.FindAll().FirstOrDefaultAsync(d => d.CommentID.Equals(comtID) && d.UserID.Equals(userID));
                if (detail == null)
                {
                    await _commentDetailRepository.AddAsync(new CommentDetail { CommentID = comtID, UserID = userID, Seen = true });
                }
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> UploadImage(List<UploadImage> uploadImages)
        {
            var imagesList = new List<UploadImage>();
            foreach (var item in uploadImages)
            {
                imagesList.Add(new UploadImage
                {
                    CommentID = item.CommentID,
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

        }
    }
}
