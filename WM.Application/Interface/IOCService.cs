using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.OC;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;

namespace WM.Application.Interface
{

    public interface IOCService
    {
        Task<List<TreeView>> GetListTree();
        Task<bool> IsExistsCode(int ID);
        Task<bool> Delete(int ID);
        Task<bool> Rename(TreeViewRename level);
        Task<IEnumerable<TreeViewOC>> GetListTreeOC(int parentID, int id);
        Task<object> ListOCIDofUser(int ocid);
        Task<object> CreateOC(CreateOCViewModel task);
        Task<object> CreateSubOC(CreateOCViewModel task);
    }
}
