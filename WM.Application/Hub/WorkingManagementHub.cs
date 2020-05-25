using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WM.Application.Interface;
using WM.Data.IRepositories;
using WM.Ultilities.Helpers;

namespace WM.Application.Hub
{
    public class WorkingManagementHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskService _taskService;
        public WorkingManagementHub(IUserRepository userRepository, ITaskService taskService)
        {
            _userRepository = userRepository;
            _taskService = taskService;
        }
        private async Task<string> GetUsername(string user)
        {
            try
            {
                var userid = user.ToInt();
                return (await _userRepository.FindAll().FirstOrDefaultAsync(x => x.ID.Equals(userid))).Username.ToTitleCase();
            }
            catch (Exception ex)
            {
                return "Someone";
                throw;
            }
            throw new NotImplementedException();
        }
        public async System.Threading.Tasks.Task CheckAlert(string user)
        {
            var model = await _taskService.TaskListIsLate();
            await Clients.All.SendAsync("ReceiveCheckAlert", user);
            if (model.Item1.Count > 0)
                await Clients.All.SendAsync("ReceiveMessageForCurd", string.Join(",", model.Item1));
            if (model.Item2.Count > 0)
                await Clients.All.SendAsync("ReceiveMessage", string.Join(",", model.Item2));

        }
        public async System.Threading.Tasks.Task Online(string user, string message)
        {
            // var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
            await Clients.All.SendAsync("ReceiveOnline", user, message);
        }
        public async System.Threading.Tasks.Task SendMessage(string user, string message)
        {
            // var id = Context.ConnectionId;//"LzX9uE94Ovlp6Yx8s6PvhA"
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public System.Threading.Tasks.Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public System.Threading.Tasks.Task SendMessageToUser(string connectionId, string message)
        {
            return Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }

        public override async System.Threading.Tasks.Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public async System.Threading.Tasks.Task JoinGroup(string group, string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("ReceiveJoinGroup", user, await GetUsername(user));

        }
        public async System.Threading.Tasks.Task Typing(string group, string user)
        {
            await Clients.Group(group).SendAsync("ReceiveTyping", user, await GetUsername(user));
        }
        public async System.Threading.Tasks.Task StopTyping(string group, string user)
        {
            await Clients.Group(group).SendAsync("ReceiveStopTyping", user);
        }
        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
