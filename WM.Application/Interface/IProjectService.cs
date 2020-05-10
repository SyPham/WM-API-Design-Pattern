using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.Project;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;
using WM.Ultilities.Helpers;

namespace WM.Application.Interface
{
   public interface IProjectService
    {
        Task<bool> Create(Project entity);
        Task<object> Open(int projectId);
        Task<object> Clone(int projectId);
        Task<bool> Update(Project entity);
        Task<bool> Delete(int id);
        Task<Tuple<bool, string>> AddManager(AddManagerViewModel addManager);
        Task<Tuple<bool, string>> AddMember(AddMemberViewModel addMember);
        Task<Project> GetByID(int id);
        Task<object> GetUserByProjectID(int id);
        Task<object> GetUsers();
        Task<List<Project>> GetAll();
        Task<object> GetProjects();
        Task<PagedList<ProjectViewModel>> GetAllPaging(int userid, int page, int pageSize, string keyword);
        Task<List<ProjectViewModel>> GetListProject();
        Task<object> GetProjects(int userid, int page, int pageSize, string projectName);
        Task<object> ProjectDetail(int projectID);
    }
}
