using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Line
{
    public class MessageParams
    {
        public string Token { get; set; }
        public string Message { get; set; }
        public string StickerPackageId { get; set; }
        public string StickerId { get; set; }
        public string FileUri { get; set; }
        public string Filename { get; set; }
    }
}
