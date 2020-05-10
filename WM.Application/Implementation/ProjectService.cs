using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WM.Application.Interface;
using WM.Application.ViewModel.Chat;
using WM.Application.ViewModel.Notification;
using WM.Application.ViewModel.Project;
using WM.Data.Entities;
using WM.Data.Enums;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Implementation
{

    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IManagerRepository _managerRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly MapperConfiguration _mapperConfig;
        public ProjectService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IProjectRepository projectRepository,
            IRoomRepository roomRepository,
            IUserRepository userRepository,
            ITagRepository tagRepository,
            IManagerRepository managerRepository,
            ITeamMemberRepository teamMemberRepository,
            ITaskRepository taskRepository,
            IFollowRepository followRepository,
            MapperConfiguration mapperConfig,
            INotificationRepository notificationRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _roomRepository = roomRepository;
            _managerRepository = managerRepository;
            _teamMemberRepository = teamMemberRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _followRepository = followRepository;
            _notificationRepository = notificationRepository;
            _mapperConfig = mapperConfig;
        }
        public async Task<bool> Create(Project entity)
        {
            try
            {
                entity.CreatedByName = (await _userRepository.FindAll().FirstOrDefaultAsync(x=>x.ID == entity.CreatedBy)).Username ?? "";
                await _projectRepository.AddAsync(entity);
                await _unitOfWork.Commit();

                var room = new Room
                {
                    Name = entity.Name,
                    ProjectID = entity.ID
                };
                await _roomRepository.AddAsync(room);
                await _unitOfWork.Commit();

                var update = await _projectRepository.FindAll().FirstOrDefaultAsync(x => x.ID.Equals(room.ProjectID));
                update.Room = room.ID;
                await _unitOfWork.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        private async Task<Project> CreateForClone(Project entity)
        {
            try
            {
                entity.ID = 0;
                entity.Name = entity.Name + "(clone)";
                entity.CreatedByName = (await _userRepository.FindAll().FirstOrDefaultAsync(x => x.ID == entity.CreatedBy)).Username ?? "";
               await _projectRepository.AddAsync(entity);
                await _unitOfWork.Commit();

                var room = new Room
                {
                    Name = entity.Name,
                    ProjectID = entity.ID
                };
               await  _roomRepository.AddAsync(room);
                await _unitOfWork.Commit();

                var update = await _projectRepository.FindAll().FirstOrDefaultAsync(x => x.ID.Equals(room.ProjectID));
                update.Room = room.ID;
                await _unitOfWork.Commit();

                return entity;
            }
            catch (Exception)
            {
                return entity;

            }
        }
        public async Task<bool> Delete(int id)
        {
            try
            {
                var entity =await  _projectRepository.FindByIdAsync(id);

                if (entity == null)
                {
                    return false;
                }
                _roomRepository.Remove(await _roomRepository.FindAll().FirstOrDefaultAsync(_ => _.ProjectID == id));
                _managerRepository.RemoveMultiple(await _managerRepository.FindAll().Where(_ => _.ProjectID == id).ToListAsync());
                _teamMemberRepository.RemoveMultiple(await _teamMemberRepository.FindAll().Where(_ => _.ProjectID == id).ToListAsync());

                var listTask = await _taskRepository.FindAll().Where(_ => _.ProjectID == id).ToListAsync();
                _tagRepository.RemoveMultiple(await _tagRepository.FindAll().Where(_ => listTask.Select(x => x.ID).Contains(_.TaskID)).ToListAsync());
                _taskRepository.RemoveMultiple(listTask);
                _followRepository.RemoveMultiple(await _followRepository.FindAll().Where(_ => listTask.Select(x => x.ID).Contains(_.TaskID)).ToListAsync());

                _projectRepository.Remove(entity);

                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private IEnumerable<TreeViewTask> GetAllDescendants(IEnumerable<TreeViewTask> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
            return rootNodes.Concat(descendants);
        }
        public async Task<object> Clone(int projectId)
        {
            try
            {
                var entity = await _projectRepository.FindByIdAsync(projectId);
                var project = await CreateForClone(entity);
                if (entity == null)
                {
                    return false;
                }
                var manager = await _managerRepository.FindAll().Where(_ => _.ProjectID == projectId).ToListAsync();
                manager.ForEach(item =>
                {
                    item.ProjectID = project.ID;
                });
                var member = await _teamMemberRepository.FindAll().Where(_ => _.ProjectID == projectId).ToListAsync();
                member.ForEach(item =>
                {
                    item.ProjectID = project.ID;
                });

               await _managerRepository.AddMultipleAsync(manager);
               await _teamMemberRepository.AddMultipleAsync(member);

                var tasksForClone = await _taskRepository.FindAll().Where(_ => _.ProjectID == projectId).ToListAsync();

                await CloneTask(tasksForClone, project.ID);
                await _unitOfWork.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async System.Threading.Tasks.Task CloneTask(List<Data.Entities.Task> tasks, int projectId)
        {
            int parentID = 0;
            foreach (var item in tasks)
            {
                if (item.JobTypeID == JobType.Project)
                {

                    if (item.Level == 1)
                    {
                        parentID = 0;
                    }
                    var itemAdd = new Data.Entities.Task
                    {
                        JobName = item.JobName,
                        JobTypeID = item.JobTypeID,
                        periodType = item.periodType,
                        OCID = item.OCID,
                        CreatedBy = item.CreatedBy,
                        FromWhoID = item.FromWhoID,
                        Priority = item.Priority,
                        Level = item.Level,
                        ProjectID = projectId,
                        ParentID = parentID

                    };
                    itemAdd.CreatedDate = DateTime.Now;
                    await _taskRepository.AddAsync(itemAdd);
                    await _unitOfWork.Commit();
                    parentID = itemAdd.ID;
                }

            }
        }
        public async Task<List<Project>> GetAll()
        {
            return await _projectRepository.FindAll().ToListAsync();
        }
        public async Task<List<ProjectViewModel>> GetListProject()
        {
            return _mapper.Map<List<ProjectViewModel>>(await _projectRepository.FindAll().ToListAsync());
        }

        public async Task<PagedList<ProjectViewModel>> GetAllPaging(int userid, int page, int pageSize, string keyword)
        {
            var source = await _projectRepository.FindAll()
                .Include(x => x.TeamMembers)
                .ThenInclude(x => x.User)
                .Include(x => x.Managers)
                .ThenInclude(x => x.User)
                .OrderByDescending(x => x.ID)
                  .Where(_ => _.Managers.Select(a => a.UserID).Contains(userid)
                || _.TeamMembers.Select(a => a.UserID).Contains(userid)
                || _.CreatedBy == userid)
                .Select(x => new ProjectViewModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    CreatedByName = x.CreatedByName,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate.ToString("dd MMM, yyyy"),
                    Room = x.Room,
                    Status = x.Status,
                    Members = x.TeamMembers.Select(a => a.UserID).ToList(),
                    Manager = x.Managers.Select(a => a.UserID).ToList(),
                })
                .ToListAsync();
            if (!keyword.IsNullOrEmpty())
            {
                source = source.Where(x => x.Name.ToLower().Contains(keyword.Trim().ToLower()) || x.CreatedByName.ToLower().Contains(keyword.Trim().ToLower())).ToList();
            }
            return PagedList<ProjectViewModel>.Create(source, page, pageSize);
        }

        public async Task<Project> GetByID(int id)
        {
            return await _projectRepository.FindByIdAsync(id);
        }

        public async Task<bool> Update(Project entity)
        {
            var item = await _projectRepository.FindByIdAsync(entity.ID);
            item.Name = entity.Name;
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

        public async Task<Tuple<bool, string>> AddManager(AddManagerViewModel addManager)
        {
            try
            {
                var listUsers = new List<int>();
                if (addManager.Users.Length > 0)
                {
                    //get old manager list
                    var oldManagers = await _managerRepository.FindAll().Where(x => x.ProjectID == addManager.ProjectID).Select(x => x.UserID).ToArrayAsync();
                    //new manager list from client
                    var newManagers = addManager.Users;
                    //get value of old managers list without value in new manager list
                    var withOutInOldManagers = newManagers.Except(oldManagers).ToArray();
                    if (withOutInOldManagers.Length > 0)
                    {
                        var managers = new List<Manager>();
                        foreach (var pic in withOutInOldManagers)
                        {
                            managers.Add(new Manager
                            {
                                UserID = pic,
                                ProjectID = addManager.ProjectID
                            });
                        }
                        await _managerRepository.AddMultipleAsync(managers);
                        var project = await _projectRepository.FindByIdAsync(addManager.ProjectID);
                        var user = await _userRepository.FindByIdAsync(addManager.UserID);
                        string urlResult = $"/project/detail/{project.ID}";
                        var message = $"The {user.Username.ToTitleCase()} account has assigned you as manager of {project.Name} project";
                        var notify = new CreateNotifyParams
                        {
                            AlertType = AlertType.Manager,
                            Message = message,
                            Users = withOutInOldManagers.Distinct().ToList(),
                            URL = urlResult,
                            UserID = addManager.UserID
                        };
                        var addNotify = _mapper.Map<Notification>(notify);
                        await _notificationRepository.AddAsync(addNotify);
                        listUsers.AddRange(withOutInOldManagers);
                    }
                    else
                    {
                        //Day la userID se bi xoa
                        var withOutInNewManagers = oldManagers.Where(x => !newManagers.Contains(x)).ToArray();
                        var listDeleteManagers = await _managerRepository.FindAll().Where(x => withOutInNewManagers.Contains(x.UserID) && x.ProjectID.Equals(addManager.ProjectID)).ToListAsync();
                        _managerRepository.RemoveMultiple(listDeleteManagers);
                    }
                }
                await _unitOfWork.Commit();
                return Tuple.Create(true, string.Join(",", listUsers.Distinct().ToArray()));
            }
            catch (Exception)
            {
                return Tuple.Create(false, "");

            }
        }

        public async Task<Tuple<bool, string>> AddMember(AddMemberViewModel addMember)
        {
            try
            {
                var listUsers = new List<int>();

                if (addMember.Users.Length > 0)
                {
                    //get old member list
                    var oldMembers = await _teamMemberRepository.FindAll().Where(x => x.ProjectID == addMember.ProjectID).Select(x => x.UserID).ToArrayAsync();
                    //new member list from client
                    var newMembers = addMember.Users;
                    //get value of old members list without value in new member list
                    var withOutInOldMembers = newMembers.Except(oldMembers).ToArray();
                    if (withOutInOldMembers.Length > 0)
                    {
                        var members = new List<TeamMember>();
                        foreach (var pic in withOutInOldMembers)
                        {
                            members.Add(new TeamMember
                            {
                                UserID = pic,
                                ProjectID = addMember.ProjectID
                            });
                        }
                        await _teamMemberRepository.AddMultipleAsync(members);
                        var project = await _projectRepository.FindByIdAsync(addMember.ProjectID);
                        var user = await _userRepository.FindByIdAsync(addMember.UserID);
                        string urlResult = $"/project-detail/{project.ID}";
                        var message = $"The {user.Username.ToTitleCase()} account has assigned you as member of {project.Name} project";

                        var notify = new CreateNotifyParams
                        {
                            AlertType = AlertType.Member,
                            Message = message,
                            Users = withOutInOldMembers.Distinct().ToList(),
                            URL = urlResult,
                            UserID = addMember.UserID
                        };
                        var addNotify = _mapper.Map<Notification>(notify);
                        await _notificationRepository.AddAsync(addNotify);
                        listUsers.AddRange(withOutInOldMembers);
                    }
                    else
                    {
                        //Day la userID se bi xoa
                        var withOutInNewMembers = oldMembers.Where(x => !newMembers.Contains(x)).ToArray();
                        var listDeleteMembers = await _teamMemberRepository.FindAll().Where(x => withOutInNewMembers.Contains(x.UserID) && x.ProjectID.Equals(addMember.ProjectID)).ToListAsync();
                        _teamMemberRepository.RemoveMultiple(listDeleteMembers);
                    }
                }

                await _unitOfWork.Commit();
                return Tuple.Create(true, string.Join(",", listUsers.Distinct().ToArray()));
            }
            catch (Exception)
            {
                return Tuple.Create(false, "");
            }
        }

        public async Task<object> GetUsers()
        {
            return await _userRepository.FindAll().Where(x => x.RoleID != 1).Select(x => new { x.ID, x.Username }).ToListAsync();
        }

        public async Task<object> GetUserByProjectID(int id)
        {
            try
            {
                var item = await _managerRepository.FindAll().Include(x => x.Project).Include(x => x.User).FirstOrDefaultAsync(x => x.ProjectID == id);
                return new
                {
                    status = true,
                    room = item.Project.Room,
                    title = item.Project.Name,
                    createdBy = item.Project.CreatedBy,
                    selectedManager = await _managerRepository.FindAll().Include(x => x.User).Where(x => x.ProjectID == id).Select(x => new { ID = x.User.ID, Username = x.User.Username }).ToArrayAsync(),
                    selectedMember = await _teamMemberRepository.FindAll().Include(x => x.User).Where(x => x.ProjectID == id).Select(x => new { ID = x.User.ID, Username = x.User.Username }).ToArrayAsync(),
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                };
            }

        }

        public async Task<object> GetProjects(int userid, int page, int pageSize, string projectName)
        {
            var members = _teamMemberRepository.FindAll().Where(_ => _.UserID == userid).Select(x => x.ProjectID).ToArray();

            var model = await _projectRepository.FindAll()
                .Include(x => x.TeamMembers)
                .ThenInclude(x => x.User)
                .Include(x => x.Managers)
                .ThenInclude(x => x.User).Select(x => new
                {
                    x.ID,
                    x.Name,
                    Manager = x.TeamMembers.Select(a => a.User.Username).ToArray(),
                    ManagerID = x.TeamMembers.Select(a => a.User.ID).ToArray(),
                    Members = x.TeamMembers.Select(a => a.User.Username).ToArray(),
                    MemberIDs = x.TeamMembers.Select(a => a.User.ID).ToArray(),
                    x.CreatedBy
                }).ToListAsync();
            model = model.Where(_ => _.ManagerID.Contains(userid) || _.MemberIDs.Contains(userid) || _.CreatedBy == userid).ToList();
            if (!projectName.IsNullOrEmpty())
            {
                projectName = projectName.Trim().ToLower();
                model = model.Where(x => x.Name.ToLower().Contains(projectName)).ToList();
            }
            var totalCount = model.Count();
            model = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new
            {
                project = model,
                data = model,
                total = (int)Math.Ceiling(totalCount / (double)pageSize),
                totalPage = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public Task<object> GetProjects()
        {
            throw new NotImplementedException();
        }
        public async Task<object> ProjectDetail(int projectID)
        {
            var item = await _projectRepository.FindByIdAsync(projectID);
            return item;
        }
        public async Task<object> Open(int projectId)
        {
            var model = await _projectRepository.FindByIdAsync(projectId);
            if (model == null)
                return new
                {
                    status = false
                };
            model.Status = !model.Status;
            await _unitOfWork.Commit();
            return new
            {
                status = true,
                result = model.Status
            };
        }
    }
}
