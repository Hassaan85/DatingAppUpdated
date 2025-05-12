using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub(PresenceTracker tracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User == null) throw new HubException("Cannot get current user claim");

           var isOnline = await tracker.UserConnected(Context.User.GetUserName() , Context.ConnectionId);
            if (isOnline)await Clients.Others.SendAsync("User is Online" , Context.User?.GetUserName());

            var currentUsers = await tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("Get Online Users" , currentUsers);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
               if (Context.User == null) throw new HubException("Cannot get current user claim");

           var isOffline = await tracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);
          if (isOffline)  await Clients.Others.SendAsync("User is Offline" , Context.User?.GetUserName());

            // var currentUsers = await tracker.GetOnlineUsers();
            // await Clients.All.SendAsync("Get Online Users" , currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}