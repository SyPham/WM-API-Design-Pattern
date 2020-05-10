using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;
using WM.Infrastructure.Interfaces;

namespace WM.Data.EF.Repositories
{
    public class NotificationRepository : EFRepository<Notification, int>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }
    }
}
