using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository(DataContext context ,IMapper mapper) : IMessageRepository
    {
        public void AddGroup(Group group)
        {
           context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add( message );
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove( message );
        }

        public async Task<Connection?> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
           return await context.Groups.Include(x => x.Connections)
           .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
           .FirstOrDefaultAsync(); 
        }

        public async Task<Message?> GetMessage(int id)
        {
           return await context.Messages.FindAsync(id);
        }

        public async Task<Group?> GetMessageGroup(string groupName)
        {
            return await context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
           var query = context.Messages
             .OrderByDescending(x => x.MessageSent)
             .AsQueryable();

             query = messageParams.Container switch
             {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.UserName && x.RecipientDeleted==false),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.UserName && x.SenderDeleted==false),
                _ => query.Where(x => x.Recipient.UserName == messageParams.UserName && x.DateRead == null && x.RecipientDeleted==false),
             };

             var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

             return await PagedList<MessageDto>.CreateAsync(messages ,messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var query =  context.Messages
                // .Include(x => x.Sender).ThenInclude(x => x.Photos)
                //  .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                 .Where(x => x.RecipientUserName == currentUserName && x.RecipientDeleted==false && x.SenderUserName == recipientUserName || 
                 x.SenderUserName == currentUserName && x.SenderDeleted==false && x.RecipientUserName == recipientUserName
                 )
                 .OrderBy(x => x.MessageSent)
                 .AsQueryable();
               //   .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
               //   .ToListAsync();

                 var unreadMessages = query.Where(x => x.DateRead == null && x.RecipientUserName == currentUserName).ToList();

                 if (unreadMessages.Count != 0 )
                 {
                    unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                    await context.SaveChangesAsync();
                 }

                //  return mapper.Map<IEnumerable<MessageDto>>(messages);.
                return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
           context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
           return await context.SaveChangesAsync() > 0;
        }
    }
}