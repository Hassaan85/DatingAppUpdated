using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
       public class MessagesController(IMessageRepository messageRepository , IUserRepository userRepository ,IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto ) 
        {
            var userName  = User.GetUserName();

            if (userName == createMessageDto.RecipientUsername.ToLower())
                        return BadRequest ("You cannot Message Yourself");

        
            var sender = await userRepository.GetUserByUserNameAsync(userName);
            var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null  || sender == null || sender.UserName==null || recipient.UserName==null) return BadRequest ("Cannot Send Message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content

            };

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) return  Ok (mapper.Map<MessageDto>(message));

            return BadRequest("Failed to save Message");

        }

        [HttpGet]
        public async Task <ActionResult<IEnumerable<MessageDto>>>GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();

            var messages = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);

            return messages;

        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>>GetMessageThread(string username)
        {
            var currentUserName = User.GetUserName();

            return Ok (await messageRepository.GetMessageThread(currentUserName, username));
        }

       [HttpDelete("{id}")]

       public async Task<ActionResult>DeleteMessage(int id)

       {
         var username = User.GetUserName();

         var message = await messageRepository.GetMessage(id);

         if (message == null) return BadRequest("Cannot Delete this Message");
         
         if (message.SenderUserName != username && message.RecipientUserName != username) 
           return Forbid();

         if (message.SenderUserName == username) message.SenderDeleted=true;
         if (message.RecipientUserName == username) message.RecipientDeleted=true;

         if (message is {SenderDeleted : true , RecipientDeleted : true}) {
            messageRepository.DeleteMessage(message);
         }

         if (await messageRepository.SaveAllAsync()) return Ok ();

         return BadRequest("Problem deleting the message");
       }
        
    }
}