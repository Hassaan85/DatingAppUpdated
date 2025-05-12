using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(IMessageRepository messageRepository , IUserRepository userRepository , IMapper mapper , IHubContext<PresenceHub> presenceHub) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];
            if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot Join Group");
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId , groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("Updated Group" , group);

            var messages = await  messageRepository.GetMessageThread(Context.User.GetUserName() , otherUser!);

            await Clients.Caller.SendAsync("Receive Message Thread" , messages);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
           var group =  await RemoveFromMessageGroup();
           await Clients.Group(group.Name).SendAsync("Updated Group" , group);
            await base.OnDisconnectedAsync(exception);
        }


        public async Task  SendMessage(CreateMessageDto createMessageDto)
        {
            var userName  = Context.User?.GetUserName()?? throw new Exception("Could not get User");

            if (userName == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot Message Yourself");

        
            var sender = await userRepository.GetUserByUserNameAsync(userName);
            var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null  || sender == null || sender.UserName==null || recipient.UserName==null) 
            throw new HubException ("Cannot Send Message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content

            };

            var groupname = GetGroupName(sender.UserName , recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupname);

            if (group != null && group.Connections.Any(c => c.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null && connections?.Count != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("New Message Received" , new {username = sender.UserName, knownAs = sender.knownAs});
                }
            }

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) 
            
            {
                // var  group = GetGroupName(sender.UserName , recipient.UserName);

                 await Clients.Group(groupname).SendAsync("New Message" , mapper.Map<MessageDto>(message));
            }

          

        }

        private async Task<Group>AddToGroup(string groupName)
        {
            var username = Context.User?.GetUserName()?? throw new Exception("Cannot Get Username");
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection{ConnectionId = Context.ConnectionId , UserName= username};

            if (group == null)
            {
                group = new Group{ Name = groupName};
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if ( await messageRepository.SaveAllAsync()) return group;

            throw new HubException ("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup ()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            // var connection = await messageRepository.GetConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null && group != null)
            {
                messageRepository.RemoveConnection(connection);
               if (await messageRepository.SaveAllAsync()) return group;
            }

            throw new Exception ("Failed to remove from group");
        }

        private  string GetGroupName ( string caller , string? other )
        {
            var stringCompare = string.CompareOrdinal(caller , other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }

    

}