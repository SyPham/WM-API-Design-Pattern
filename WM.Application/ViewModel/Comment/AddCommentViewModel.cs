using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Comment
{
    public class AddCommentViewModel
    {
        public int ID { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public int ParentID { get; set; }
        public string Content { get; set; }
        public string TaskCode { get; set; }
        public int Level { get; set; }
        // public WM.Data.Enums.ClientRouter ClientRouter { get; set; }
    }
}
