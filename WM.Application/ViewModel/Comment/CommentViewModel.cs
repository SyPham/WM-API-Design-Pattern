using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Comment
{
    public class CommentViewModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public bool Seen { get; set; }
        public int UserID { get; set; }
        public int ParentID { get; set; }
        public byte[] ImageBase64 { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
