using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;

namespace WM.Data.EF.Repositories
{
    public class ChatRepository : EFRepository<Chat, int>, IChatRepository
    {
        public ChatRepository(AppDbContext context) : base(context)
        {
        }
    }
}
