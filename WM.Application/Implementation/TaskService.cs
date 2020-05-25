using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Hub;
using WM.Application.Interface;
using WM.Application.ViewModel.Notification;
using WM.Application.ViewModel.Project;
using WM.Application.ViewModel.Task;
using WM.Data.Entities;
using WM.Data.Enums;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly INotificationService _notificationService;
        private readonly ITaskRepository _taskRepository;
        private readonly IDeputyRepository _deputyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly ITutorialRepository _tutorialRepository;
        private readonly IFollowRepository _followRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IHubContext<WorkingManagementHub> _hubContext;
        public TaskService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IHistoryRepository historyRepository,
            ITutorialRepository tutorialRepository,
            IFollowRepository followRepository,
            ITagRepository tagRepository,
            IProjectRepository projectRepository,
            IDeputyRepository deputyRepository,
            INotificationService notificationService,
            ITaskRepository taskRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<WorkingManagementHub> hubContext,
            MapperConfiguration configMapper,
            IConfiguration configuration
            )
        {
            _notificationService = notificationService;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
            _userRepository = userRepository;
           _historyRepository = historyRepository;
            _tutorialRepository = tutorialRepository;
            _followRepository = followRepository;
            _tagRepository = tagRepository;
            _projectRepository = projectRepository;
            _deputyRepository = deputyRepository;
        }
        #region CURD
        public async Task<object> CreateSubTask(CreateTaskViewModel task)
        {
            try
            {
                var listUsers = new List<int>();
                // task.DueDate = task.DueDate.ToStringFormatDateTime();
                // add
                if (!await _taskRepository.FindAll().AnyAsync(x => x.ID == task.ID))
                {
                    var item = _mapper.Map<Data.Entities.Task>(task);

                    //Level cha tang len 1 va gan parentid cho subtask
                    var taskParent =await _taskRepository.FindByIdAsync(item.ParentID);
                    item.Level = taskParent.Level + 1;
                    item.ParentID = task.ParentID;
                    item.JobTypeID = taskParent.JobTypeID;
                    await _taskRepository.AddAsync(item);
                    await _unitOfWork.Commit();
                    await CloneCode(item);
                    if (task.PIC != null)
                    {
                        var tags = new List<Tag>();
                        foreach (var pic in task.PIC)
                        {
                            tags.Add(new Tag
                            {
                                UserID = pic,
                                TaskID = item.ID
                            });
                        }
                        await _tagRepository.AddMultipleAsync(tags);
                        listUsers.AddRange(tags.Select(x => x.UserID));
                    }
                    if (task.Deputies != null)
                    {
                        var deputies = new List<Deputy>();
                        foreach (var deputy in task.Deputies)
                        {
                            deputies.Add(new Deputy
                            {
                                UserID = deputy,
                                TaskID = item.ID
                            });
                        }
                        await _deputyRepository.AddMultipleAsync(deputies);
                        listUsers.AddRange(deputies.Select(x => x.UserID));

                    }
                     await _unitOfWork.Commit();
                    if (listUsers.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                        await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                    }
                    return true;

                }
                else //update
                {
                    //var edit = _taskRepository.Find(task.ID);
                    //edit.Priority = task.Priority.ToUpper();
                    //edit.JobName = task.JobName;
                    //edit.Priority = task.Priority;
                    //edit.OCID = task.OCID;
                    //edit.FromWhoID = task.FromWhoID;
                    //edit = CheckDuedate(edit, task);
                    //if (task.PIC != null)
                    //{
                    //    var tags = new List<Tag>();
                    //    var listDelete = await _tagRepository.Where(x => task.PIC.Contains(x.UserID) && x.TaskID == edit.ID).ToListAsync();
                    //    if (listDelete.Count > 0)
                    //    {
                    //        _tagRepository.RemoveRange(listDelete);
                    //    }

                    //    foreach (var pic in task.PIC)
                    //    {
                    //        tags.Add(new Tag
                    //        {
                    //            UserID = pic,
                    //            TaskID = edit.ID
                    //        });
                    //        await _tagRepository.AddRangeAsync(tags);
                    //    }
                    //    listUsers.AddRange(tags.Select(x => x.UserID));

                    //}
                    //if (task.Deputies != null)
                    //{
                    //    var deputies = new List<Deputy>();
                    //    var listDelete = await _deputyRepository.Where(x => task.Deputies.Contains(x.UserID) && x.TaskID == edit.ID).ToListAsync();
                    //    if (listDelete.Count > 0)
                    //    {
                    //        _deputyRepository.RemoveRange(listDelete);
                    //    }

                    //    foreach (var deputy in task.Deputies)
                    //    {
                    //        deputies.Add(new Deputy
                    //        {
                    //            UserID = deputy,
                    //            TaskID = edit.ID
                    //        });
                    //        await _deputyRepository.AddRangeAsync(deputies);
                    //        listUsers.AddRange(deputies.Select(x => x.UserID));
                    //    }
                    //}
                    // await _unitOfWork.Commit();
                    //if (listUsers.Count > 0)
                    //{
                    //    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                    //    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                    //}

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Helper For Create Task
        private async Task<Tuple<List<int>>> AlertDeadlineChanging(AlertDeadline alert, Data.Entities.Task task, int userid, List<int> users)
        {
            var projectName = string.Empty;
            if (task.ProjectID > 0)
            {
                var project = await _projectRepository.FindByIdAsync(task.ProjectID.Value);
                projectName = project.Name;
            }
            var user = await _userRepository.FindByIdAsync(userid);
            string urlResult = $"/todolist/{task.JobName.ToUrlEncode()}";
            var listUsers = new List<int>();
            switch (alert)
            {
                case AlertDeadline.Weekly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.ChangeWeekly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, AlertType.ChangeWeekly, task.DueDateTime),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case AlertDeadline.Monthly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.ChangeMonthly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, AlertType.ChangeMonthly, task.DueDateTime),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case AlertDeadline.Quarterly:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.ChangeQuarterly,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, AlertType.ChangeQuarterly),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                case AlertDeadline.Deadline:
                    await _notificationService.Create(new CreateNotifyParams
                    {
                        AlertType = AlertType.ChangeDeadline,
                        Message = CheckMessage(task.JobTypeID, projectName, user.Username, task.JobName, AlertType.ChangeDeadline),
                        Users = users.ToList(),
                        TaskID = task.ID,
                        URL = urlResult,
                        UserID = userid
                    });
                    listUsers.AddRange(users);
                    break;
                default:
                    break;
            }
            return Tuple.Create(listUsers);
        }
        private object GetAlertDueDate()
        {
            var date = DateTime.Now.Date;
            var list = _taskRepository.FindAll().Where(x => x.periodType == PeriodType.SpecificDate && x.CreatedDate.Date == date).Select(x => new
            {
                x.CreatedDate,
                x.DueDateTime
            }).ToList();
            return list;
        }
        private string AlertMessage(string username, string jobName, string project, bool isProject, AlertType alertType, DateTime deadline = new DateTime())
        {
            var message = string.Empty;
            switch (alertType)
            {
                case AlertType.Done:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has already finished the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has already finished the task name ' {jobName} '";
                    break;
                case AlertType.Remark:
                    break;
                case AlertType.Undone:
                    break;
                case AlertType.UpdateRemark:
                    break;
                case AlertType.Assigned:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} assigned you the task name ' {jobName} ' ";
                    break;
                case AlertType.ChangeDeputy:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as deputy of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has assigned you as deputy of the task name ' {jobName} '";
                    break;
                case AlertType.Manager:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as manager of {project} project";
                    break;
                case AlertType.Member:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has assigned you as member of {project} project";
                    break;
                case AlertType.ChangeDeadline:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
                    break;
                case AlertType.ChangeWeekly:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
                    break;
                case AlertType.ChangeMonthly:
                    if (isProject)
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} ' in {project} project";
                    else
                        message = $"{username.ToTitleCase()} has changed deadline to {deadline} of the task name ' {jobName} '";
                    break;
                default:
                    break;
            }
            return message;
        }
        private string CheckMessage(JobType jobtype, string project, string username, string jobName, AlertType alertType, DateTime deadline = new DateTime())
        {
            var message = string.Empty;
            switch (jobtype)
            {
                case JobType.Project:
                    message = AlertMessage(username, jobName, project, true, alertType, deadline);
                    break;
                case JobType.Routine:
                case JobType.Abnormal:
                    message = AlertMessage(username, jobName, project, false, alertType, deadline);
                    break;
            }
            return message;
        }
        private async System.Threading.Tasks.Task CloneCode(Data.Entities.Task task)
        {
            var createCode = await _taskRepository.FindByIdAsync(task.ID);
            createCode.Code = $"{task.ID}-{task.periodType}-{task.JobTypeID}";
            await _unitOfWork.Commit();
        }
        private async Task<List<int>> AddDeputy(CreateTaskViewModel task, Data.Entities.Task item)
        {
            var listUsers = new List<int>();
            var deputies = new List<Deputy>();
            foreach (var deputy in task.Deputies)
            {
                deputies.Add(new Deputy
                {
                    UserID = deputy,
                    TaskID = item.ID
                });
            }
            await _deputyRepository.AddMultipleAsync(deputies);
            await _unitOfWork.Commit();
            var projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _projectRepository.FindByIdAsync(item.ProjectID.Value);
                projectName = project.Name;
            }
            var user = await _userRepository.FindByIdAsync(task.UserID);
            string urlResult = $"/todolist/{item.JobName.ToUrlEncode()}";
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = AlertType.ChangeDeputy,
                Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, AlertType.ChangeDeputy),
                Users = task.Deputies.ToList(),
                TaskID = item.ID,
                URL = urlResult,
                UserID = task.UserID
            });
            listUsers.AddRange(task.Deputies);
            return listUsers;
        }
        private async Task<List<int>> AddPIC(CreateTaskViewModel task, Data.Entities.Task item)
        {
            var listUsers = new List<int>();
            var tags = new List<Tag>();
            foreach (var pic in task.PIC)
            {
                tags.Add(new Tag
                {
                    UserID = pic,
                    TaskID = item.ID
                });
            }
            await _tagRepository.AddMultipleAsync(tags);
            await _unitOfWork.Commit();

            var user = await _userRepository.FindByIdAsync(task.UserID);
            var projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _projectRepository.FindByIdAsync(item.ProjectID.Value);
                projectName = project.Name;
            }
            string urlResult = $"/todolist/{item.JobName.ToUrlEncode()}";
            string message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, AlertType.Assigned);
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = AlertType.Assigned,
                Message = message,
                Users = task.PIC.ToList(),
                TaskID = item.ID,
                URL = urlResult,
                UserID = task.UserID
            });
            listUsers.AddRange(task.PIC);
            return listUsers;
        }

        // Edit
        private async Task<List<int>> EditPIC(CreateTaskViewModel task, Data.Entities.Task edit)
        {
            var listUsers = new List<int>();
            //Lay la danh sach assigned
            var oldPIC = await _tagRepository.FindAll().Where(x => x.TaskID == edit.ID).Select(x => x.UserID).ToArrayAsync();
            var oldPICTemp = oldPIC;
            var newPIC = task.PIC;
            //loc ra danh sach cac ID co trong newPIC ma khong co trong oldPIC
            var withOutInOldPIC = newPIC.Except(oldPIC).ToArray();
            // var withOutInNewPIC = oldPIC.Except(newPIC).ToArray();
            if (newPIC.Count() == 0 && oldPIC.Count() > 0)
            {

                var listDeletePIC = await _tagRepository.FindAll().Where(x => oldPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _tagRepository.RemoveMultiple(listDeletePIC);
                await _unitOfWork.Commit();

            }
            if (oldPIC.Count() == 1 && newPIC.Count() == 1 && !oldPIC.SequenceEqual(newPIC))
            {
                var listDeletePIC = await _tagRepository.FindAll().Where(x => oldPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _tagRepository.RemoveMultiple(listDeletePIC);
                await _unitOfWork.Commit();

            }
            //xoa het thang cu them lai tu dau
            if (withOutInOldPIC.Length > 0)
            {
                var tags = new List<Tag>();
                foreach (var pic in withOutInOldPIC)
                {
                    tags.Add(new Tag
                    {
                        UserID = pic,
                        TaskID = edit.ID
                    });
                }
                if (tags.Count > 0)
                {
                    await _tagRepository.AddMultipleAsync(tags);
                }
                var projectName = string.Empty;
                if (edit.ProjectID > 0)
                {
                    var project = await _projectRepository.FindByIdAsync(edit.ProjectID.Value);
                    projectName = project.Name;
                }
                var user = await _userRepository.FindByIdAsync(task.UserID);
                string urlResult = $"/todolist/{edit.JobName.ToUrlEncode()}";
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = AlertType.Assigned,
                    Message = CheckMessage(edit.JobTypeID, projectName, user.Username, edit.JobName, AlertType.Assigned),
                    Users = withOutInOldPIC.ToList(),
                    TaskID = edit.ID,
                    URL = urlResult,
                    UserID = task.UserID
                });
                listUsers.AddRange(withOutInOldPIC);
                //Day la userID se bi xoa
                var withOutInNewPIC = oldPIC.Where(x => !newPIC.Contains(x)).ToArray();
                var listDeletePIC = await _tagRepository.FindAll().Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                await _tagRepository.AddMultipleAsync(listDeletePIC);
                await _unitOfWork.Commit();

            }
            else
            {
                // Day la userID se bi xoa
                var withOutInNewPIC = oldPIC.Where(x => !newPIC.Contains(x)).ToArray();
                var listDeletePIC = await _tagRepository.FindAll().Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _tagRepository.RemoveMultiple(listDeletePIC);
                await _unitOfWork.Commit();

            }
            return listUsers;
        }
        private async Task<List<int>> EditDeputy(CreateTaskViewModel task, Data.Entities.Task edit)
        {
            var listUsers = new List<int>();
            //Lay la danh sach assigned
            var oldDeputies = await _deputyRepository.FindAll().Where(x => x.TaskID == edit.ID).Select(x => x.UserID).ToArrayAsync();
            var newDeputies = task.Deputies;
            //loc ra danh sach cac ID co trong newPIC ma khong co trong oldPIC
            var withOutInOldDeputy = newDeputies.Except(oldDeputies).ToArray();
            if (newDeputies.Count() == 0 && oldDeputies.Count() > 0)
            {
                var listDeleteDeputy = await _deputyRepository.FindAll().Where(x => oldDeputies.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _deputyRepository.RemoveMultiple(listDeleteDeputy);
                await _unitOfWork.Commit();

            }
            if (oldDeputies.Count() == 1 && newDeputies.Count() == 1 && !oldDeputies.SequenceEqual(newDeputies))
            {
                var listDeleteDeputy = await _deputyRepository.FindAll().Where(x => oldDeputies.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _deputyRepository.RemoveMultiple(listDeleteDeputy);
                await _unitOfWork.Commit();

            }
            if (withOutInOldDeputy.Length > 0)
            {
                var deputies = new List<Deputy>();
                foreach (var deputy in withOutInOldDeputy)
                {
                    deputies.Add(new Deputy
                    {
                        UserID = deputy,
                        TaskID = edit.ID
                    });
                }
                if (deputies.Count > 0)
                {
                    await _deputyRepository.AddMultipleAsync(deputies);
                }
                var projectName = string.Empty;
                if (edit.ProjectID > 0)
                {
                    var project = await _projectRepository.FindByIdAsync(edit.ProjectID.Value);
                    projectName = project.Name;
                }
                var user = await _userRepository.FindByIdAsync(task.UserID);
                string urlResult = $"/todolist/{edit.JobName.ToUrlEncode()}";
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = AlertType.ChangeDeputy,
                    Message = CheckMessage(edit.JobTypeID, projectName, user.Username, edit.JobName, AlertType.ChangeDeputy),
                    Users = withOutInOldDeputy.ToList(),
                    TaskID = edit.ID,
                    URL = urlResult,
                    UserID = task.UserID
                });
                //Day la userID se bi xoa
                var withOutInNewPIC = oldDeputies.Where(x => !newDeputies.Contains(x)).ToArray();
                var listDeletePIC = await _deputyRepository.FindAll().Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _deputyRepository.RemoveMultiple(listDeletePIC);
                await _unitOfWork.Commit();

                listUsers.AddRange(withOutInOldDeputy);
            }
            else
            {
                //Day la userID se bi xoa
                var withOutInNewPIC = oldDeputies.Where(x => !newDeputies.Contains(x)).ToArray();
                var listDeletePIC = await _deputyRepository.FindAll().Where(x => withOutInNewPIC.Contains(x.UserID) && x.TaskID.Equals(edit.ID)).ToListAsync();
                _deputyRepository.RemoveMultiple(listDeletePIC);
                await _unitOfWork.Commit();

            }
            return listUsers;
        }
        #endregion
        public async Task<Tuple<bool, string, object>> CreateTask(CreateTaskViewModel task)
        {
            try
            {
                // task.DueDate = task.DueDate;
                var listUsers = new List<int>();
                if (!await _taskRepository.FindAll().AnyAsync(x => x.ID == task.ID))
                {
                    var item = _mapper.Map<Data.Entities.Task>(task);
                    item.Level = 1;
                    await _taskRepository.AddAsync(item);
                    await _unitOfWork.Commit();
                    await CloneCode(item);
                    if (task.PIC.Count() > 0)
                    {
                        listUsers.AddRange(await AddPIC(task, item));
                    }
                    if (task.Deputies.Count() > 0)
                    {
                        listUsers.AddRange(await AddDeputy(task, item));

                    }
                    if (listUsers.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                        await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                    }
                    return Tuple.Create(true, string.Join(",", listUsers.Distinct()), GetAlertDueDate());

                }
                else
                {
                    var edit =await _taskRepository.FindByIdAsync(task.ID);
                    edit.Priority = task.Priority.ToUpper();
                    edit.JobName = task.JobName;
                    edit.Priority = task.Priority;
                    edit.DepartmentID = task.DepartmentID;
                    edit.FromWhoID = task.FromWhoID;

                    if (task.PIC.Count() >= 0)
                    {
                        listUsers.AddRange(await EditPIC(task, edit));
                    }
                    if (task.Deputies.Count() >= 0)
                    {
                        listUsers.AddRange(await EditDeputy(task, edit));
                    }
                    var pics = await _tagRepository.FindAll().Where(x => x.TaskID.Equals(edit.ID)).Select(x => x.UserID).ToListAsync();
                    switch (task.periodType)
                    {
                        case PeriodType.Daily:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var daily = await AlertDeadlineChanging(AlertDeadline.Daily, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(daily.Item1);
                            }
                            break;
                        case PeriodType.Weekly:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var weekly = await AlertDeadlineChanging(AlertDeadline.Weekly, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(weekly.Item1);
                            }
                            break;
                        case PeriodType.Monthly:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var mon = await AlertDeadlineChanging(AlertDeadline.Monthly, edit, edit.FromWhoID, pics);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                                listUsers.AddRange(mon.Item1);
                            }

                            break;
                        case PeriodType.SpecificDate:
                            if (!task.DueDate.Equals(edit.DueDateTime))
                            {
                                var due = await AlertDeadlineChanging(AlertDeadline.Deadline, edit, edit.FromWhoID, pics);
                                listUsers.AddRange(due.Item1);
                                edit.DueDateTime = task.DueDate.ToParseStringDateTime();
                            }
                            break;
                        default:
                            break;
                    }
                }
                await _unitOfWork.Commit();
                if (listUsers.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUsers.Distinct()), GetAlertDueDate());
                    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));
                }
                return Tuple.Create(true, string.Join(",", listUsers.Distinct()), GetAlertDueDate());
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, "", new object());
            }
        }

        #region Hepler For Delete 
        public async Task<IEnumerable<TreeViewTask>> GetListTree(int parentID, int id)
        {
            var listTasks = await _taskRepository.FindAll()
               .Where(x => (x.Status == false && x.FinishedMainTask == false) || (x.Status == true && x.FinishedMainTask == false))
               .Include(x => x.User)
               .OrderBy(x => x.Level).ToListAsync();
            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var beAssigneds = _tagRepository.FindAll().Where(x => x.TaskID == item.ID)
                     .Include(x => x.User)
                     .Select(x => new BeAssigned { ID = x.User.ID, Username = x.User.Username }).ToList();
                TreeViewTask levelItem = new TreeViewTask
                {
                    ID = item.ID,
                    Level = item.Level,
                    ParentID = item.ParentID
                };
                tasks.Add(levelItem);
            }

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                DueDate = c.DueDate,
                                JobName = c.JobName,
                                Level = c.Level,
                                ProjectID = c.ProjectID,
                                CreatedBy = c.CreatedBy,
                                CreatedDate = c.CreatedDate,
                                From = c.From,
                                ProjectName = c.ProjectName,
                                state = c.state,
                                PriorityID = c.PriorityID,
                                Priority = c.Priority,
                                Follow = c.Follow,
                                PIC = c.PIC,
                                Histories = c.Histories,
                                PICs = c.PICs,
                                JobTypeID = c.JobTypeID,
                                FromWho = c.FromWho,
                                FromWhere = c.FromWhere,
                                BeAssigneds = c.BeAssigneds,
                                Deputies = c.Deputies,
                                VideoLink = c.VideoLink,
                                VideoStatus = c.VideoStatus,
                                DeputiesList = c.DeputiesList,
                                DeputyName = c.DeputyName,
                                Tutorial = c.Tutorial,
                                ModifyDateTime = c.ModifyDateTime,
                                periodType = c.periodType,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();
            return hierarchy;
        }
        private List<TreeViewTask> GetChildren(List<TreeViewTask> tasks, int parentid)
        {
            return tasks
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new TreeViewTask()
                    {
                        ID = c.ID,
                        DueDate = c.DueDate,
                        JobName = c.JobName,
                        Level = c.Level,
                        ProjectID = c.ProjectID,
                        CreatedBy = c.CreatedBy,
                        CreatedDate = c.CreatedDate,
                        From = c.From,
                        ProjectName = c.ProjectName,
                        state = c.state,
                        PriorityID = c.PriorityID,
                        Priority = c.Priority,
                        Follow = c.Follow,
                        PIC = c.PIC,
                        Histories = c.Histories,
                        PICs = c.PICs,
                        JobTypeID = c.JobTypeID,
                        FromWho = c.FromWho,
                        FromWhere = c.FromWhere,
                        BeAssigneds = c.BeAssigneds,
                        Deputies = c.Deputies,
                        VideoLink = c.VideoLink,
                        VideoStatus = c.VideoStatus,
                        DeputiesList = c.DeputiesList,
                        //DueDateDaily = c.DueDateDaily,
                        //DueDateWeekly = c.DueDateWeekly,
                        //DueDateMonthly = c.DueDateMonthly,
                        //SpecificDate = c.SpecificDate,
                        DeputyName = c.DeputyName,
                        Tutorial = c.Tutorial,
                        ModifyDateTime = c.ModifyDateTime,
                        periodType = c.periodType,
                        children = GetChildren(tasks, c.ID)
                    })
                    .OrderByDescending(x => x.ID)
                    .ToList();
        }
        public IEnumerable<TreeViewTask> GetAllTaskDescendants(IEnumerable<TreeViewTask> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllTaskDescendants(x.children));
            return rootNodes.Concat(descendants);
        }
        #endregion
        public async Task<object> Delete(int id, int userid)
        {
            try
            {
                var item = await _taskRepository.FindByIdAsync(id);
                if (!item.CreatedBy.Equals(userid))
                    return false;
                var tasks = await GetListTree(item.ParentID, item.ID);
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToList();

                _tagRepository.RemoveMultiple(await _tagRepository.FindAll().Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _deputyRepository.RemoveMultiple(await _deputyRepository.FindAll().Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _followRepository.RemoveMultiple(await _followRepository.FindAll().Where(x => arrTasks.Contains(x.TaskID)).ToListAsync());
                _taskRepository.RemoveMultiple(await _taskRepository.FindAll().Where(x => arrTasks.Contains(x.ID)).ToListAsync());
                _tutorialRepository.RemoveMultiple(await _tutorialRepository.FindAll().Where(x => arrTasks.Contains(x.TaskID ?? 0)).ToListAsync());

                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Helper For Done
        private bool ValidPeriod(Data.Entities.Task task, out string message)
        {
            var currenDate = DateTime.Now.ToString("dd MMM, yyyy");
            switch (task.periodType)
            {
                case PeriodType.Daily:
                    // var date = task.DueDateDaily.ToParseStringDateTime().Date;
                    // var result = PeriodComparator(date);
                    message = "";
                    return true;
                case PeriodType.Weekly:
                    var weekly = task.DueDateTime.Date.Subtract(TimeSpan.FromDays(3));
                    var resultW = PeriodComparator(weekly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateTime.Subtract(TimeSpan.FromDays(3)):dd MMMM, yyyy} to {task.DueDateTime:dd MMMM, yyyy}";
                    return resultW > 0 ? true : false;
                case PeriodType.Monthly:
                    var monthly = task.DueDateTime.Date.Subtract(TimeSpan.FromDays(10));
                    var resultM = PeriodComparator(monthly);
                    message = $"Today is on {currenDate}. You can only finish this task from {task.DueDateTime.Subtract(TimeSpan.FromDays(10)):dd MMMM, yyyy} to {task.DueDateTime:dd MMMM, yyyy}";
                    return resultM > 0 ? true : false;
                case PeriodType.SpecificDate:
                    message = "";
                    return true;
                default:
                    message = "";
                    return false;
            }
        }
        /// <summary>
         /// <0 − If CurrentDate is earlier than comparedate
         /// =0 − If CurrentDate is the same as comparedate
         /// >0 − If CurrentDate is later than comparedate
         /// </summary>
         /// <param name="comparedate"></param>
         /// <returns>Result</returns>
        private int PeriodComparator(DateTime comparedate)
        {
            DateTime systemDate = DateTime.Now;
            int res = DateTime.Compare(systemDate, comparedate);
            return res;
        }
        private Data.Entities.Task ToFindParentByChild(IQueryable<Data.Entities.Task> rootNodes, int taskID)
        {
            var parentItem = rootNodes.Any(x => x.ID.Equals(taskID));
            if (!parentItem)
                return null;
            var parent = rootNodes.FirstOrDefault(x => x.ID.Equals(taskID)).ParentID;
            if (parent == 0)
                return rootNodes.FirstOrDefault(x => x.ID.Equals(taskID));
            else
                return ToFindParentByChild(rootNodes, parent);
        }
        private IEnumerable<TreeViewTask> AsTreeView(int parentID, int id)
        {
            var listTasks = _taskRepository.FindAll()
               .Include(x => x.User)
               .OrderBy(x => x.Level).AsQueryable();
            var tasks = new List<TreeViewTask>();
            foreach (var item in listTasks)
            {
                var levelItem = new TreeViewTask
                {
                    ID = item.ID,
                    Level = item.Level,
                    ParentID = item.ParentID
                };
                tasks.Add(levelItem);
            }

            List<TreeViewTask> hierarchy = new List<TreeViewTask>();

            hierarchy = tasks.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewTask
                            {
                                ID = c.ID,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                state = c.state,
                                children = GetChildren(tasks, c.ID)
                            })
                            .ToList();
            return hierarchy;
        }
        private List<int> GetListUserRelateToTask(int taskId, bool isProject)
        {
            var task = _taskRepository.FindByIdAsync(taskId);
            var listPIC = _tagRepository.FindAll().Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            var listFollow = _followRepository.FindAll().Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            var listDeputie = _deputyRepository.FindAll().Where(_ => _.TaskID.Equals(taskId)).Select(_ => _.UserID).ToList();
            if (isProject)
                return listPIC.Union(listFollow).ToList();
            else
                return listPIC.Union(listFollow).Union(listDeputie).ToList();
        }
        private async Task<List<int>> AlertTasksIsLate(TreeViewTask item, string message, bool isProject)
        {
            var notifyParams = new CreateNotifyParams
            {
                TaskID = item.ID,
                Users = GetListUserRelateToTask(item.ID, isProject),
                Message = message,
                URL = $"/todolist/{item.JobName.ToUrlEncode()}",
                AlertType = AlertType.BeLate
            };
            if (notifyParams.Users.Count > 0)
            {
                await _notificationService.Create(notifyParams);
                return notifyParams.Users;
            }
            return new List<int>();
        }
        private async Task<List<int>> AlertTask(Data.Entities.Task item, int userid)
        {
            var pathName = "history";
            var projectName = string.Empty;
            var userList = new List<int>();
            var user = await _userRepository.FindByIdAsync(userid);
            if (item.ProjectID > 0)
            {
                var project = await _projectRepository.FindByIdAsync(item.ProjectID.Value);
                projectName = project.Name;
                if (item.Level == 1 && item.periodType == PeriodType.SpecificDate)
                    item.FinishedMainTask = true;
            }
            string urlTodolist = $"/{pathName}/{item.JobName.ToUrlEncode()}";
            userList.Add(item.FromWhoID);
            userList.AddRange(_tagRepository.FindAll().Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToList());
            userList.AddRange(_deputyRepository.FindAll().Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToList());
            await _notificationService.Create(new CreateNotifyParams
            {
                AlertType = AlertType.Done,
                Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, AlertType.Done),
                Users = userList.Distinct().Where(x => x != userid).ToList(),
                TaskID = item.ID,
                URL = urlTodolist,
                UserID = userid
            });
            return userList;
        }
        private async Task<List<int>> AlertFollowTask(Data.Entities.Task item, int userid)
        {
            string projectName = string.Empty;
            if (item.ProjectID > 0)
            {
                var project = await _projectRepository.FindByIdAsync(item.ProjectID.Value);
                projectName = project.Name;
                if (item.Level == 1 && item.periodType == PeriodType.SpecificDate)
                    item.FinishedMainTask = true;
            }
            var user = await _userRepository.FindByIdAsync(userid);
            var listUserfollowed = await _followRepository.FindAll().Where(x => x.TaskID == item.ID).Select(x => x.UserID).ToListAsync();
            string urlResult = $"/follow/{item.JobName.ToUrlEncode()}";
            if (listUserfollowed.Count() > 0)
            {
                await _notificationService.Create(new CreateNotifyParams
                {
                    AlertType = AlertType.Done,
                    Message = CheckMessage(item.JobTypeID, projectName, user.Username, item.JobName, AlertType.Done),
                    Users = listUserfollowed.Distinct().Where(x => x != userid).ToList(),
                    TaskID = item.ID,
                    URL = urlResult,
                    UserID = userid
                });
            }
            return listUserfollowed;
        }
        private bool CheckDailyOntime(Data.Entities.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckWeeklyOntime(Data.Entities.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckMonthlyOntime(Data.Entities.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckSpecificDateOntime(Data.Entities.Task update)
        {
            return PeriodComparator(update.DueDateTime) <= 0 ? true : false;
        }
        private bool CheckPeriodOnTime(Data.Entities.Task task)
        {
            switch (task.periodType)
            {
                case PeriodType.Daily:
                    return CheckDailyOntime(task);
                case PeriodType.Weekly:
                    return CheckWeeklyOntime(task);
                case PeriodType.Monthly:
                    return CheckMonthlyOntime(task);
                case PeriodType.SpecificDate:
                    return CheckSpecificDateOntime(task);
                default:
                    return false;
            }
        }
        private string UpdateDueDateViaPeriodHisoty(Data.Entities.Task task)
        {
            switch (task.periodType)
            {
                case PeriodType.Daily:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case PeriodType.Weekly:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case PeriodType.Monthly:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                case PeriodType.SpecificDate:
                    return task.DueDateTime.ToSafetyString().ToParseStringDateTime().ToString("d MMM, yyyy hh:mm:ss tt");
                default:
                    return "";
            }
        }
        private async Task<bool> CheckUpdateDueDateTodolist(Data.Entities.Task update)
        {
            var flag = false;
            var dueDate = DateTime.MinValue;
            var check = await _taskRepository.FindAll().Where(x => x.Code.Equals(update.Code)).ToListAsync();
            foreach (var item in check)
            {
                switch (update.periodType)
                {
                    case PeriodType.Daily:
                        dueDate = update.DueDateTime.AddDays(1);
                        if (item.DueDateTime.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case PeriodType.Weekly:
                        dueDate = update.DueDateTime.AddDays(7);
                        if (item.DueDateTime.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    case PeriodType.Monthly:
                        dueDate = update.DueDateTime.AddMonths(1);
                        if (item.DueDateTime.Equals(dueDate))
                        {
                            flag = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            return flag;
        }
        private async System.Threading.Tasks.Task UpdateDueDateViaPeriod(Data.Entities.Task task)
        {
            var update = await _taskRepository.FindByIdAsync(task.ID);
            var check = await CheckUpdateDueDateTodolist(update);
            if (check)
            {
                switch (task.periodType)
                {
                    case PeriodType.Daily:
                        update.DueDateTime = task.DueDateTime.AddDays(1);
                        break;
                    case PeriodType.Weekly:
                        update.DueDateTime = task.DueDateTime.AddDays(7);
                        break;
                    case PeriodType.Monthly:
                        update.DueDateTime = task.DueDateTime.AddMonths(1);
                        break;
                    default:
                        break;
                }
                await _unitOfWork.Commit();
            }
        }
        private async Task<bool> PushTaskToHistory(History history)
        {
            try
            {
                await _historyRepository.AddAsync(history);
                await _unitOfWork.Commit();
                return true;
            }
            catch
            {
                return false;

                throw;
            }
        }
        private async Task<Data.Entities.Task> CheckPeriodToPushTaskToHistory(Data.Entities.Task task)
        {
            var update = await _taskRepository.FindByIdAsync(task.ID);
            var history = new History
            {
                TaskID = update.ID,
                TaskCode = update.Code,
                Status = CheckPeriodOnTime(update),
                Deadline = UpdateDueDateViaPeriodHisoty(update)
            };
            await UpdateDueDateViaPeriod(update);
            await PushTaskToHistory(history);
            await _unitOfWork.Commit();

            return update;
        }
        DateTime MapDueDateTime(Data.Entities.Task item)
        {
            var result = DateTime.MinValue;
            switch (item.periodType)
            {
                case PeriodType.Daily:
                    if (item.DueDateTime.AddDays(1).DayOfWeek == System.DayOfWeek.Sunday)
                        result = item.DueDateTime.AddDays(2);
                    else
                        result = item.DueDateTime.AddDays(1);
                    break;
                case PeriodType.Weekly:
                    result = item.DueDateTime.AddDays(7);
                    break;
                case PeriodType.Monthly:
                    if (item.DueDateTime.AddMonths(1).DayOfWeek == System.DayOfWeek.Sunday)
                        result = item.DueDateTime.AddMonths(1).AddDays(1);
                    else
                        result = item.DueDateTime.AddMonths(1);
                    break;
                default:
                    break;
            }
            return result;
        }
        private async Task<bool> CheckExistTask(Data.Entities.Task task)
        {
            var currentDate = DateTime.Now;
            switch (task.periodType)
            {
                case PeriodType.Daily:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                case PeriodType.Weekly:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                case PeriodType.Monthly:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(task.DueDateTime));
                default:
                    return false;
            }
        }
        private async Task<Data.Entities.Task> CreateTaskAsync(Data.Entities.Task task)
        {
            await _taskRepository.AddAsync(task);
            await _unitOfWork.Commit();
            return task;
        }
        private async Task<List<int>> CloneSingleTask(Data.Entities.Task task)
        {
            var userListForHub = new List<int>();
            using var transaction = _unitOfWork.BeginTransaction();

            int old = task.ID;
            var newTask = new Data.Entities.Task
            {
                JobName = task.JobName,
                OCID = task.OCID,
                FromWhoID = task.FromWhoID,
                Priority = task.Priority,
                ProjectID = task.ProjectID,
                JobTypeID = task.JobTypeID,
                DueDateTime = MapDueDateTime(task),
                Code = task.Code,
                periodType = task.periodType,
                CreatedBy = task.CreatedBy,
                Level = task.Level,

            };
            //Kiem tra cai task chuan bi clone nay da ton tai chua
            var check = await CheckExistTask(newTask);
            if (!check)
            {
                await CreateTaskAsync(newTask);
                userListForHub.Add(newTask.FromWhoID);
                userListForHub.AddRange(await ClonePIC(old, newTask.ID));
            }
            await transaction.CommitAsync();
            return userListForHub.Distinct().ToList();
        }

        private async Task<List<int>> ClonePIC(int oldTaskid, int newTaskID)
        {

            var userlistForHub = new List<int>();
            var pic = _tagRepository.FindAll().Where(x => x.TaskID == oldTaskid).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = newTaskID, UserID = item.UserID });
            }
            await _tagRepository.AddMultipleAsync(list);
            await _unitOfWork.Commit();

            var deputies = _deputyRepository.FindAll().Where(x => x.TaskID == oldTaskid).ToList();
            var list2 = new List<Deputy>();
            foreach (var item in deputies)
            {
                list2.Add(new Deputy { TaskID = newTaskID, UserID = item.UserID });
            }
            await _deputyRepository.AddMultipleAsync(list2);
            await _unitOfWork.Commit();

            var follows = _followRepository.FindAll().Where(x => x.TaskID == oldTaskid).ToList();
            var list3 = new List<Follow>();
            foreach (var item in follows)
            {
                list3.Add(new Follow { TaskID = newTaskID, UserID = item.UserID });
            }
            await _followRepository.AddMultipleAsync(list3);
            await _unitOfWork.Commit();

            var tutorials = _tutorialRepository.FindAll().Where(x => x.TaskID == oldTaskid).ToList();
            var list4 = new List<Tutorial>();
            foreach (var item in tutorials)
            {
                list4.Add(new Tutorial { TaskID = newTaskID, Name = item.Name, Level = item.Level, ParentID = item.ParentID, Path = item.Path, URL = item.URL });
            }
            await _tutorialRepository.AddMultipleAsync(list4);
            await _unitOfWork.Commit();
            userlistForHub.AddRange(pic.Select(x => x.UserID));
            userlistForHub.AddRange(deputies.Select(x => x.UserID));
            userlistForHub.AddRange(follows.Select(x => x.UserID));
            return userlistForHub.Distinct().ToList();
        }
        private async Task<bool> CheckExistTaskForMultiTask(Data.Entities.Task task)
        {
            var currentDate = DateTime.Now;
            var dueDateTime = MapDueDateTime(task);
            switch (task.periodType)
            {
                case PeriodType.Daily:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                case PeriodType.Weekly:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                case PeriodType.Monthly:
                    return await _taskRepository.FindAll().AnyAsync(x => x.Code == task.Code && x.DueDateTime.Equals(dueDateTime));
                default:
                    return false;
            }
        }
        private async Task<List<int>> CloneRelatedTable(CloneTaskViewModel task)
        {
            var userlistForHub = new List<int>();
            var pic = _tagRepository.FindAll().Where(x => x.TaskID == task.IDTemp).ToList();
            var list = new List<Tag>();
            foreach (var item in pic)
            {
                list.Add(new Tag { TaskID = task.ID, UserID = item.UserID });
            }
            await _tagRepository.AddMultipleAsync(list);
            await _unitOfWork.Commit();

            var deputies = _deputyRepository.FindAll().Where(x => x.TaskID == task.IDTemp).ToList();
            var list2 = new List<Deputy>();
            foreach (var item in deputies)
            {
                list2.Add(new Deputy { TaskID = task.ID, UserID = item.UserID });
            }
            await _deputyRepository.AddMultipleAsync(list2);
            await _unitOfWork.Commit();

            var follows = _followRepository.FindAll().Where(x => x.TaskID == task.IDTemp).ToList();
            var list3 = new List<Follow>();
            foreach (var item in follows)
            {
                list3.Add(new Follow { TaskID = task.ID, UserID = item.UserID });
            }
            await _followRepository.AddMultipleAsync(list3);
            await _unitOfWork.Commit();

            var tutorials = _tutorialRepository.FindAll().Where(x => x.TaskID == task.IDTemp).ToList();
            var list4 = new List<Tutorial>();
            foreach (var item in tutorials)
            {
                list4.Add(new Tutorial { TaskID = task.ID, Name = item.Name, Level = item.Level, ParentID = item.ParentID, Path = item.Path, URL = item.URL });
            }
            await _tutorialRepository.AddMultipleAsync(list4);
            await _unitOfWork.Commit();

            userlistForHub.AddRange(pic.Select(x => x.UserID));
            userlistForHub.AddRange(deputies.Select(x => x.UserID));
            userlistForHub.AddRange(follows.Select(x => x.UserID));

            return userlistForHub.Distinct().ToList();

        }
        async System.Threading.Tasks.Task UpdateTaskForMultiTask(List<CloneTaskViewModel> listTemp)
        {
            var update = _taskRepository.FindAll().Where(x => listTemp.Select(a => a.ID).Contains(x.ID)).ToList();

            update.ForEach(item =>
            {
                if (item.Level > 1)
                {
                    item.ParentID = listTemp.FirstOrDefault(x => x.IDTemp == item.ParentID).ID;
                }
            });
            await _unitOfWork.Commit();


        }
        private async Task<List<int>> CloneMultiTask(List<Data.Entities.Task> tasks)
        {
            var listTemp = new List<CloneTaskViewModel>();
            var userListForHub = new List<int>();
            using var transaction = _unitOfWork.BeginTransaction();

            foreach (var item in tasks)
            {
                var check = await CheckExistTaskForMultiTask(item);
                if (!check)
                {
                    var temp = _mapper.Map<CloneTaskViewModel>(item);
                    temp.IDTemp = item.ID;
                    temp.ParentTemp = item.ParentID;
                    var task = new Data.Entities.Task();
                    task.Code = item.Code;
                    task.JobName = item.JobName;
                    task.ParentID = item.ParentID;
                    task.Level = item.Level;
                    task.ProjectID = item.ProjectID;
                    task.CreatedBy = item.CreatedBy;
                    task.OCID = item.OCID;
                    task.FromWhoID = item.FromWhoID;
                    task.Priority = item.Priority;
                    task.periodType = item.periodType;
                    task.DueDateTime = MapDueDateTime(item);
                    task.JobTypeID = item.JobTypeID;
                    task.CreatedDate = DateTime.Now;
                    var taskModel = await CreateTaskAsync(task);
                    temp.ID = taskModel.ID;
                    userListForHub.Add(taskModel.FromWhoID);
                    listTemp.Add(temp);
                }
            }
            foreach (var item in listTemp)
            {
                userListForHub.AddRange(await CloneRelatedTable(item));
            }
            await UpdateTaskForMultiTask(listTemp);
            await transaction.CommitAsync();
            return userListForHub.Distinct().ToList();
        }
        #endregion

        public async Task<Tuple<bool, bool, string>> Done(int id, int userid)
        {
            try
            {
                var listUserAlertHub = new List<int>();

                var item = await _taskRepository.FindByIdAsync(id);
                string mes = string.Empty;
                var check = ValidPeriod(item, out mes);
                if (!check)
                    return Tuple.Create(false, false, mes);
                if (item.Status)
                {
                    return Tuple.Create(false, false, "This task was completed!");
                }
                var listTasks = _taskRepository.FindAll();
                var rootTask = ToFindParentByChild(listTasks, item.ID);
                var tasks = AsTreeView(rootTask.ParentID, rootTask.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();
                var seftAndDescendants = _taskRepository.FindAll().Where(x => taskDescendants.Contains(x.ID)).ToList();
                // Kiem tra neu task level = 1 va khong co con chau nao thi chuyen qua history sau do cap nhap lai due date
                //var decendants = seftAndDescendants.Where(x => !seftAndDescendants.Select(x => x.ID).Contains(item.ID));
                // Neu task hien tai la main task thi kiem tra xem tat ca con chau da hoan thanh chua neu chua thi return
                if (seftAndDescendants.Where(x => x.Level > 1).Count() > 0 && item.Level == 1)
                {
                    return Tuple.Create(false, false, "Please finish all sub-tasks!");
                }
                item.Status = true;
                item.ModifyDateTime = DateTime.Now.ToString("dd MMM, yyyy hh:mm:ss tt");
                await _unitOfWork.Commit();

                listUserAlertHub.AddRange(await AlertTask(item, userid));
                listUserAlertHub.AddRange(await AlertFollowTask(item, userid));
                await CheckPeriodToPushTaskToHistory(item);
                if (seftAndDescendants.Count() == 1 && item.Level == 1)
                {
                    //Clone them cai moi voi period moi
                    await CloneSingleTask(item);
                }
                // Neu task hien tai level 1 va co con chau thi kiem tra neu con chua done thi return
                if (seftAndDescendants.Where(x => x.Level > 1).Count() >= 2 && item.Level == 1)
                {
                    int count = 0;
                    var temp = true;

                    //Kiem tra list con chau neu count > 1 tuc la co 2 con chua hoan thanh => return
                    seftAndDescendants.Where(x => x.Level > 1).ToList().ForEach(x =>
                    {
                        if (x.Status == false)
                        {
                            count++;
                            temp = false;
                        }
                    });
                    if (!temp && count > 1)
                        return Tuple.Create(false, false, "Please finish all sub-tasks!");
                }
                // Neu day la main task  va task nay khong co con thi thong bao cho nhung user lien quan va chuyen no qua history
                //if (decendants.Count() == 0 && item.Level == 1)
                //{
                //    await AlertTask(item, userid);
                //    await AlertFollowTask(item, userid);
                //    await CheckPeriodToPushTaskToHistory(item);
                //}

                // Neu khong fai la main thi kiem tra xem co bao nhieu task hoan thanh roi.
                // Neu chi con task minh chua hoan thanh thi chuyen cha qua history
                if (item.Level > 1 && seftAndDescendants.Where(x => x.Level > 1).Count() >= 1)
                {

                    var temp = true;
                    int count = 0;
                    // trong list nay khong co task hien tai neu count = 0 tuc la con moi task hien tai chua hoan thanh
                    // Add task cha cua task hien tai vao history
                    var taskTemp = seftAndDescendants.Where(x => x.Level > 1 && x.ID != id).ToList();
                    taskTemp.ForEach(x =>
                    {
                        if (x.Status == false)
                        {
                            temp = false;
                            count++;
                        }
                    });
                    // dieu kien nay de push task cha va task hien tai vao db
                    if (temp && count == 0)
                    {
                        var parent = await _taskRepository.FindByIdAsync(item.ParentID);
                        parent.ModifyDateTime = DateTime.Now.ToString("dd MMM, yyyy hh:mm:ss tt");
                        parent.Status = true;
                        parent.FinishedMainTask = true;
                        item.Status = true;
                        item.FinishedMainTask = true;
                        await _unitOfWork.Commit();
                        await CheckPeriodToPushTaskToHistory(parent);
                    }
                    // Tao them 1 bo moi trong todolist
                    if (!temp && count >= 1)
                    {
                        //Update Status task con hien tai
                        await CloneMultiTask(seftAndDescendants);
                    }
                }
                if (listUserAlertHub.Count > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", string.Join(",", listUserAlertHub.ToArray()), "message");
                    await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUserAlertHub.Distinct()));
                }

                return Tuple.Create(true, true, string.Join(",", listUserAlertHub.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Tuple.Create(false, false, "");
            }
        }

        public async Task<object> Follow(int userid, int taskid)
        {
            try
            {
                var taskModel = await _taskRepository.FindByIdAsync(taskid);
                var tasks = await GetListTree(taskModel.ParentID, taskModel.ID);
                var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();

                var listTasks = await _taskRepository.FindAll().Where(x => arrTasks.Contains(x.ID)).ToListAsync();
                if (_followRepository.FindAll().Any(x => x.TaskID == taskid && x.UserID == userid))
                {
                    _followRepository.Remove(_followRepository.FindAll().FirstOrDefault(x => x.TaskID == taskid && x.UserID == userid));
                    await _unitOfWork.Commit();

                    return true;

                }
                var listSubcribes = new List<Follow>();
                listTasks.ForEach(task =>
                {
                    listSubcribes.Add(new Follow { TaskID = task.ID, UserID = userid });
                });
                await _followRepository.AddMultipleAsync(listSubcribes);
                await _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<object> Undo(int id)
        {
            if (!await _taskRepository.FindAll().AnyAsync(x => x.ID == id))
                return false;
            try
            {
                var listUsers = new List<int>();
                var listTasks = _taskRepository.FindAll();
                var item = await _taskRepository.FindByIdAsync(id);
                var rootTask = ToFindParentByChild(listTasks, item.ID);
                var tasks = AsTreeView(rootTask.ParentID, rootTask.ID);
                //Tim tat ca con chau
                var taskDescendants = GetAllTaskDescendants(tasks).Select(x => x.ID).ToList();
                var seftAndDescendants = await _taskRepository.FindAll().Where(x => taskDescendants.Contains(x.ID)).ToListAsync();
                if (seftAndDescendants.Count == 1)
                {
                    var his = await _historyRepository.FindAll().FirstOrDefaultAsync(x => x.TaskID == id);
                    _historyRepository.Remove(his);
                    item.Status = false;
                    await _unitOfWork.Commit();
                }
                if (seftAndDescendants.Count > 1)
                {
                    var his = await _historyRepository.FindAll().Where(x => seftAndDescendants.Select(x => x.ID).Contains(x.TaskID)).ToListAsync();
                    var arrs = await _taskRepository.FindAll().Where(x => seftAndDescendants.Select(a => a.ID).Contains(x.ID)).ToListAsync();
                    arrs.ForEach(task =>
                    {
                        task.Status = false;
                        task.FinishedMainTask = false;
                    });
                    _historyRepository.RemoveMultiple(his);
                    await _unitOfWork.Commit();

                }
                var tags = _tagRepository.FindAll().Where(a => seftAndDescendants.Select(x => x.ID).Contains(a.UserID)).Select(x => x.UserID).ToList();
                var deputies = _deputyRepository.FindAll().Where(a => seftAndDescendants.Select(x => x.ID).Contains(a.UserID)).Select(x => x.UserID).ToList();
                listUsers.AddRange(tags);
                listUsers.AddRange(deputies);
                await _hubContext.Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", listUsers.Distinct()));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<object> Unsubscribe(int id, int userid)
        {
            try
            {
                if (_followRepository.FindAll().Any(x => x.TaskID == id && x.UserID == userid))
                {
                    var sub = await _followRepository.FindAll().FirstOrDefaultAsync(x => x.TaskID == id && x.UserID == userid);
                    var taskModel = await _taskRepository.FindByIdAsync(sub.TaskID);

                    var tasks = await GetListTree(taskModel.ParentID, taskModel.ID);
                    var arrTasks = GetAllTaskDescendants(tasks).Select(x => x.ID).ToArray();

                    var listTasks = await _taskRepository.FindAll().Where(x => arrTasks.Contains(x.ID)).Select(x => x.ID).ToListAsync();


                    var listSub = await _followRepository.FindAll().Where(x => listTasks.Contains(x.TaskID) && x.UserID == userid).ToListAsync();
                    _followRepository.RemoveMultiple(listSub);

                    await _unitOfWork.Commit();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<object> UpdateDueDateTime()
        {
            throw new NotImplementedException();
        }

        public async Task<object> UpdateTask(UpdateTaskViewModel task)
        {
            if (!await _taskRepository.FindAll().AnyAsync(x => x.ID == task.ID))
                return false;

            var update = await _taskRepository.FindByIdAsync(task.ID);
            update.JobName = task.JobName;
            update.FromWhoID = task.FromWhoID;
            update.CreatedBy = task.CreatedBy;
            update.Status = task.Status;
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
        #endregion
        /// <summary>
        /// Common method
        /// </summary>
        /// <returns></returns>
        private IQueryable<Data.Entities.Task> GetAllTasks()
        {
            var listTasks = _taskRepository.FindAll()
                                .Include(x => x.User)
                                .Include(x => x.Tags).ThenInclude(x => x.User)
                                .Include(x => x.Follows).ThenInclude(x => x.User)
                                .Include(x => x.Deputies).ThenInclude(x => x.User)
                                .Include(x => x.Project).ThenInclude(x => x.Managers)
                                .Include(x => x.Project).ThenInclude(x => x.TeamMembers)
                                .Include(x => x.OC)
                                .Include(x => x.Tutorial)
                                .AsQueryable();
            return listTasks;
        }

       

        #region Filter
        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid)
        {
            try
            {
                var listTasks = _taskRepository.FindAll().Where(x =>
                            (x.Tags.Select(x => x.UserID).Contains(userid)
                            || x.Deputies.Select(x => x.UserID).Contains(userid)
                            || x.FromWhoID == userid
                            || x.CreatedBy == userid)
                            && x.Status == false
                    )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();
                if (!beAssigned.IsNullOrEmpty() && beAssigned == "BeAssigned")
                {
                    listTasks = listTasks.Where(x => x.Tags.Select(x => x.UserID).Contains(userid)).AsQueryable();
                }
                if (!assigned.IsNullOrEmpty() && assigned == "Assigned")
                {
                    listTasks = listTasks.Where(x => x.FromWhoID == userid || x.CreatedBy == userid).Distinct().AsQueryable();
                }
                var sortTaskList = await listTasks.ToListAsync();
                //Flatten task
                var all = _mapper.Map<List<TreeViewTask>>(sortTaskList);
                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();

                var flatten = tree.Flatten(x => x.ChildNodes).ToHashSet();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<HashSet<HierarchyNode<TreeViewTask>>>(itemWithOutParent).Where(x => x.Entity.periodType.Equals(PeriodType.Daily) && x.Entity.DueDate.Date.CompareTo(DateTime.Now.Date) <= 0 && x.Entity.DueDate.Date.CompareTo(DateTime.MinValue) != 0).ToList();
                tree = tree.Concat(map).ToList();

                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
            }
        }
        public async Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(Status status, int userid)
        {
            try
            {
                var listTasks = GetAllTasks().Where(x =>
                            (x.Tags.Select(x => x.UserID).Contains(userid)
                            || x.Deputies.Select(x => x.UserID).Contains(userid)
                            || x.FromWhoID == userid
                            || x.CreatedBy == userid)
                            && x.Status == false
                    )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1 && x.DueDateTime.Date.CompareTo(DateTime.MinValue) != 0).Distinct();
                if (status != Status.Unknown)
                {
                    switch (status)
                    {
                        case Status.Done:
                            listTasks = listTasks.Where(x => x.Status == true).AsQueryable();
                            break;
                        case Status.Undone:
                            listTasks = listTasks.Where(x => x.Status == false).AsQueryable();
                            break;
                    }
                }
                var sortTaskList = await listTasks.ToListAsync();
                //Flatten task
                var all = _mapper.Map<List<TreeViewTask>>(sortTaskList);
                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToHashSet();
                var itemWithOutParent = all.Where(x => !flatten.Select(x => x.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<HashSet<HierarchyNode<TreeViewTask>>>(itemWithOutParent).Where(x => x.Entity.periodType.Equals(PeriodType.Daily) && x.Entity.DueDate.Date.CompareTo(DateTime.Now.Date) <= 0).ToList();
                tree = tree.Concat(map).ToList();

                return tree;
            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }
        }
        public async Task<List<HierarchyNode<TreeViewTaskForHistory>>> HistoryFilterByDueDateTime(int userid, string start, string end)
        {
            try
            {


                var listTasks = await _historyRepository.FindAll()
                    .Join(_taskRepository.FindAll()
                    .Include(x => x.OC)
                    .Include(x => x.Tutorial)
                    .Include(x => x.Tags).ThenInclude(x => x.User)
                     .Include(x => x.Deputies).ThenInclude(x => x.User)
                     .Where(x =>
                         x.Tags.Select(x => x.UserID).Contains(userid)
                      || x.Deputies.Select(x => x.UserID).Contains(userid)
                      || x.FromWhoID.Equals(userid)
                      || x.CreatedBy.Equals(userid)
                      ).Distinct()
                    ,
                    his => his.TaskID,
                    task => task.ID,
                    (his, task) => new
                    {
                        task,
                        his
                    }).Select(x => new Data.Entities.Task
                    {
                        ID = x.his.TaskID,
                        CreatedBy = x.task.CreatedBy,
                        Status = x.task.Status,
                        CreatedDate = x.task.CreatedDate,
                        ParentID = x.task.ParentID,
                        Level = x.task.Level,
                        ProjectID = x.task.ProjectID,
                        JobName = x.task.JobName,
                        OCID = x.task.OCID,
                        FromWhoID = x.task.FromWhoID,
                        Priority = x.task.Priority,
                        FinishedMainTask = x.task.FinishedMainTask,
                        JobTypeID = x.task.JobTypeID,
                        periodType = x.task.periodType,
                        User = x.task.User,
                        DepartmentID = x.task.DepartmentID,
                        DueDateTime = x.task.DueDateTime,
                        ModifyDateTime = x.his.ModifyDateTime.ToString("dd MMM, yyyy hh:mm:ss tt"),
                        Code = x.task.Code,
                        Deputies = x.task.Deputies,
                        Tags = x.task.Tags,
                        Project = x.task.Project,
                        OC = x.task.OC,
                        Tutorial = x.task.Tutorial
                    }).ToListAsync();
                if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
                {
                    var timespan = new TimeSpan(0, 0, 0);
                    var startDate = start.ToParseStringDateTime().Date;
                    var endDate = end.ToParseStringDateTime().Date;
                    listTasks = listTasks.Where(x =>
                    x.DueDateTime.Date >= startDate.Date && x.DueDateTime.Date <= endDate.Date
                    ).ToList();
                }
                var all = _mapper.Map<List<TreeViewTaskForHistory>>(listTasks);
                all = all.ToList();
                var tree = all.Where(x => x.PICs.Count > 0)
                   .AsHierarchy(x => x.ID, x => x.ParentID)
                   .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID));
                var map = _mapper.Map<List<HierarchyNode<TreeViewTaskForHistory>>>(itemWithOutParent);
                tree = tree.Concat(map).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return null;

                throw;
            }
        }
        private IQueryable<Data.Entities.Task> Fillter(IQueryable<Data.Entities.Task> listTasks, string sort, string priority, int userid, string startDate, string endDate, string weekdays, string monthly, string quarterly)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).AsQueryable();
            }

            //Loc theo weekdays
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateTime.ToSafetyString().ToLower().Equals(weekdays.ToLower())).AsQueryable();
            }
            //loc theo thang
            if (!monthly.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateTime.ToSafetyString().ToLower().Equals(monthly.ToLower())).AsQueryable();
            }

            if (!sort.IsNullOrEmpty())
            {
                sort = sort.ToLower();
                if (sort == JobType.Project.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(JobType.Project)).OrderByDescending(x => x.ProjectID).AsQueryable();
                if (sort == JobType.Routine.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(JobType.Routine)).OrderByDescending(x => x.CreatedDate).AsQueryable();
                if (sort == JobType.Abnormal.ToSafetyString().ToLower())
                    listTasks = listTasks.Where(x => x.JobTypeID.Equals(JobType.Abnormal)).OrderByDescending(x => x.CreatedDate).AsQueryable();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
      
        #endregion

        #region Load Data
        public async Task<List<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "")
        {
            try
            {
                //A: Setup and stuff you don't want timed
                // PublishhMessage("Good morning!");
                //  await _notificationService.Create(new CreateNotifyParams { Message = "Test", TaskID = 3675, AlertType = AlertType.BeLate, URL = "/todolist/Demo-daily-is-late" });
                var listTasks = GetAllTasks()
                    .Where(x => !x.DueDateTime.Equals(DateTime.MinValue))
                    .Where(x =>
                               (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                               && x.Status == false && x.Tags.Count > 0
                    )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1 && x.DueDateTime.Date.CompareTo(DateTime.MinValue) != 0).Distinct();
                var listtasksfillter = await Fillter(listTasks, sort, priority, userid, startDate, endDate, weekdays, monthly, quarterly).ToListAsync();

                var all = _mapper.Map<List<Data.Entities.Task>, List<TreeViewTask>>(listtasksfillter,
                    opt => opt.AfterMap((src, dest) =>
                    {
                        dest.ForEach(item =>
                        {
                            item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";
                        });
                    }));

                // convert qua tree
                var tree = all.Where(x => x.PICs.Count > 0).AsHierarchy(x => x.ID, x => x.ParentID).ToList();

                // convert lai qua list phang
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();

                // loc ra nhung item chua co trong tree
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
                throw;
            }
        }
       
        #region Sort For Routine
        private IQueryable<Data.Entities.Task> SortRoutine(IQueryable<Data.Entities.Task> listTasks, string sort, string priority)
        {

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        #endregion
        public async Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid)
        {
            try
            {
                // var user = _context.Users.Find(userid);
                // var model =await Notification();
                var jobtype = JobType.Routine;
                if (ocid == 0)
                    return new List<HierarchyNode<TreeViewTask>>();
                var listTasks = GetAllTasks()
                   .Where(x => x.Status == false && x.JobTypeID.Equals(jobtype) && x.OCID == ocid)
                    .Where(x =>
                               (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                    )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();
                var listTasksSort = await SortRoutine(listTasks, sort, priority).ToListAsync();

                var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();
                all.ForEach(item =>
                {
                    item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";
                });
                //all = all.Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDate.Date.CompareTo(DateTime.Now.Date) != 1)
                //                      .ToList();
                var tree = all
              .AsEnumerable()
              .AsHierarchy(x => x.ID, x => x.ParentID)
              .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                tree = tree.Concat(itemWithOutParent).OrderByDescending(x => x.Entity.ID).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return new List<HierarchyNode<TreeViewTask>>();
                throw;
            }
        }

        #region Filter For Project
        private IQueryable<Data.Entities.Task> FilterTaskDetail(IQueryable<Data.Entities.Task> listTasks, string priority)
        {
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        #endregion
        public async Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int? projectid = null)
        {
            projectid = projectid ?? 0;
            if (!await _projectRepository.FindAll().AnyAsync(x => x.ID == projectid)) return new List<HierarchyNode<TreeViewTask>>();
            var jobtype = JobType.Project;
            var listTasks = GetAllTasks()
                .Where(x => x.JobTypeID.Equals(jobtype) && x.ProjectID == projectid && x.Status == false)
                .Where(x =>
                              (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                   )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();

            var filterTasksList = await FilterTaskDetail(listTasks, priority)
                 .ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(filterTasksList).ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            all = all.Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDate.Date.CompareTo(DateTime.Now.Date) != 1)
                                      .ToList();
            var tree = all.AsHierarchy(x => x.ID, x => x.ParentID).ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
            { Entity = x }).ToList();
            // var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
            tree = tree.Concat(itemWithOutParent).ToList();
            return tree;
        }

        #region Filter For Abnormal
        private IQueryable<Data.Entities.Task> FilterAbnormal(IQueryable<Data.Entities.Task> listTasks, int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            if (!startDate.IsNullOrEmpty() && !endDate.IsNullOrEmpty())
            {
                var timespan = new TimeSpan(0, 0, 0);
                var start = DateTime.ParseExact(startDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;
                var end = DateTime.ParseExact(endDate, "MM-dd-yyyy", CultureInfo.InvariantCulture).Date;

                listTasks = listTasks.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate.Date <= end.Date).AsQueryable();
            }
            if (!weekdays.IsNullOrEmpty())
            {
                listTasks = listTasks.Where(x => x.DueDateTime.Equals(weekdays)).AsQueryable();
            }

            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        #endregion
        public async Task<List<HierarchyNode<TreeViewTask>>> Abnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays)
        {
            var jobtype = JobType.Abnormal;
            if (ocid == 0)
                return new List<HierarchyNode<TreeViewTask>>();
            var listTasks = GetAllTasks()
                    .Where(x => x.Status == false && x.JobTypeID.Equals(jobtype) && x.OCID == ocid)
                    .Where(x =>
                              (x.Tags.Select(x => x.UserID).Contains(userid)
                               || x.Deputies.Select(x => x.UserID).Contains(userid)
                               || x.FromWhoID == userid
                               || x.CreatedBy == userid)
                    )
                    .Where(x => !x.periodType.Equals(PeriodType.Daily) || x.periodType.Equals(PeriodType.Daily) && x.DueDateTime.Date.CompareTo(DateTime.Now.Date) != 1).Distinct();

            var listTasksSort = await FilterAbnormal(listTasks, ocid, priority, userid, startDate, endDate, weekdays).ToListAsync();
            var all = _mapper.Map<List<TreeViewTask>>(listTasksSort).ToList();
            all = all.Where(x =>
                       x.PICs.Contains(userid)
                    || x.CreatedBy == userid
                    || x.FromWhoID == userid
                    || x.Deputies.Contains(userid)
                    ).Distinct().ToList();
            all.ForEach(item =>
            {
                item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

            });
            var tree = all
              .AsEnumerable()
              .AsHierarchy(x => x.ID, x => x.ParentID)
              .ToList();
            var flatten = tree.Flatten(x => x.ChildNodes).ToList();
            var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
            { Entity = x }).ToList();
            tree = tree.Concat(itemWithOutParent).ToList();
            return tree;
        }

        #region Sort For Follow
        private IQueryable<Data.Entities.Task> SortFollow(IQueryable<Data.Entities.Task> listTasks, string sort, string priority)
        {

            if (!sort.IsNullOrEmpty())
            {
                if (sort == "project")
                    listTasks = listTasks.Where(x => x.ProjectID > 0).AsQueryable();
                if (sort == "routine")
                    listTasks = listTasks.Where(x => x.ProjectID == 0).AsQueryable();
            }
            if (!priority.IsNullOrEmpty())
            {
                priority = priority.ToUpper();
                listTasks = listTasks.Where(x => x.Priority.Equals(priority)).AsQueryable();
            }
            return listTasks;
        }
        #endregion
        public async Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0)
        {
            try
            {
                var listTasks = GetAllTasks().Where(x => x.Follows.Select(x => x.UserID).Contains(userid));

                var sortTasksList = await SortFollow(listTasks, sort, priority).ToListAsync();
                var all = _mapper.Map<List<TreeViewTask>>(sortTasksList).ToList();
                all.ForEach(item =>
                {
                    item.Follow = item.Follows.Any(x => x.TaskID == item.ID && x.UserID == userid) ? "Yes" : "No";

                });
                var tree = all
                   .AsEnumerable()
                   .AsHierarchy(x => x.ID, x => x.ParentID)
                   .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTask>
                { Entity = x }).ToList();
                //  var map = _mapper.Map<List<HierarchyNode<TreeViewTask>>>(itemWithOutParent);
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {

                return new List<HierarchyNode<TreeViewTask>>();
            }
        }

        public async Task<List<HierarchyNode<TreeViewTaskForHistory>>> History(int userid, string start, string end)
        {
            try
            {


                var listTasks = _historyRepository.FindAll()
                    .Join(_taskRepository.FindAll()
                    .Include(x => x.OC)
                    .Include(x => x.Tutorial)
                    .Include(x => x.Tags).ThenInclude(x => x.User)
                     .Include(x => x.Deputies).ThenInclude(x => x.User)
                     .Where(x =>
                         x.Tags.Select(x => x.UserID).Contains(userid)
                      || x.Deputies.Select(x => x.UserID).Contains(userid)
                      || x.FromWhoID.Equals(userid)
                      || x.CreatedBy.Equals(userid)
                      ).Distinct()
                    ,
                    his => his.TaskID,
                    task => task.ID,
                    (his, task) => new
                    {
                        task,
                        his
                    }).Select(x => new Data.Entities.Task
                    {
                        ID = x.his.TaskID,
                        CreatedBy = x.task.CreatedBy,
                        Status = x.task.Status,
                        CreatedDate = x.task.CreatedDate,
                        ParentID = x.task.ParentID,
                        Level = x.task.Level,
                        ProjectID = x.task.ProjectID,
                        JobName = x.task.JobName,
                        OCID = x.task.OCID,
                        FromWhoID = x.task.FromWhoID,
                        Priority = x.task.Priority,
                        FinishedMainTask = x.task.FinishedMainTask,
                        JobTypeID = x.task.JobTypeID,
                        periodType = x.task.periodType,
                        User = x.task.User,
                        DepartmentID = x.task.DepartmentID,
                        DueDateTime = x.task.DueDateTime,
                        ModifyDateTime = x.his.ModifyDateTime.ToString("dd MMM, yyyy hh:mm:ss tt"),
                        Code = x.task.Code,
                        Deputies = x.task.Deputies,
                        Tags = x.task.Tags,
                        Project = x.task.Project,
                        OC = x.task.OC,
                        Tutorial = x.task.Tutorial
                    }).AsQueryable();
                if (!start.IsNullOrEmpty() && !end.IsNullOrEmpty())
                {
                    var timespan = new TimeSpan(0, 0, 0);
                    var startDate = start.ToParseStringDateTime().Date;
                    var endDate = end.ToParseStringDateTime().Date;
                    listTasks = listTasks.Where(x => x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date).AsQueryable();
                }
                var fillterTasks = await listTasks.ToListAsync();
                var all = _mapper.Map<List<TreeViewTaskForHistory>>(fillterTasks);
                all = all.ToList();
                var tree = all
                   .AsHierarchy(x => x.ID, x => x.ParentID)
                   .ToList();
                var flatten = tree.Flatten(x => x.ChildNodes).ToList();
                var itemWithOutParent = all.Where(x => !flatten.Select(a => a.Entity.ID).Contains(x.ID)).Select(x => new HierarchyNode<TreeViewTaskForHistory>
                { Entity = x }).ToList();
                //var map = _mapper.Map<List<HierarchyNode<TreeViewTaskForHistory>>>(itemWithOutParent);
                tree = tree.Concat(itemWithOutParent).ToList();
                return tree;
            }
            catch (Exception ex)
            {
                return null;

                throw;
            }
        }
        #endregion

        #region Heplers
        public Task<object> From(int userid)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetCodeLineAsync(string code, string state)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetDeputies()
        {
            throw new NotImplementedException();
        }

        public Task<List<ProjectViewModel>> GetListProject()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetListUser(int userid, int projectid)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CheckNotify
        public Task<Tuple<List<int>, List<int>>> TaskListIsLate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
