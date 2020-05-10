using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Enums;

namespace WM.Application.ViewModel.Comment
{
    public class TaskHasComment
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public JobType JobType { get; set; }
        public CommentTreeView CommentTreeViews { get; set; }
    }
}
