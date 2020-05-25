using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WM.Application.ViewModel.Line;

namespace WM.Application.Interface
{
   public interface ILineService
    {
        string GetAuthorizeUri();
        Task SendMessage(MessageParams msg);
        Task SendWithSticker(MessageParams msg);
        Task SendWithPicture(MessageParams msg);
        Task<string> FetchToken(string code);
    }
}
