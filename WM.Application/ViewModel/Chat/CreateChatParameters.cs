using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Chat
{
    public class CreateChatParameters
    {
        public string RoomID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
    }
}
