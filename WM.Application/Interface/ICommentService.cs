using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.Comment;
using WM.Data.Entities;
using WM.Infrastructure.Interfaces;

namespace WM.Application.Interface
{
   public interface ICommentService
    {
        Task<Tuple<bool, string, Comment>> Add(AddCommentViewModel comment, int currentUser);
        //Task<object> Delete();
        Task<Tuple<bool, string, Comment>> AddSub(AddSubViewModel subcomment);
        Task<object> Seen(int comtID, int userID);
        Task<IEnumerable<CommentTreeView>> GetAllTreeView(int taskid, int userid);
        Task<IEnumerable<TaskHasComment>> GetAllCommentWithTask(int userid);
        Task<bool> UploadImage(List<UploadImage> uploadImages);
    }
}
