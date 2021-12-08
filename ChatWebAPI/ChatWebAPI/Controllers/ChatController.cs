using AutoMapper;
using ChatWebAPI.Entities;
using ChatWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public ChatController(ApplicationDBContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("Create")]
        public async Task<ActionResult<CreatedChatResponse>> CreateChat([FromBody] CreateChatRequest newChat)
        {
            var exists = await context.Chats.AnyAsync(chat => chat.Name == newChat.Name);
            if (exists)
            {
                return BadRequest($"Chat already created {newChat.Name}");
            }

            var chat = mapper.Map<Chat>(newChat);
            chat.CreationDate = DateTime.Now;
            context.Add(chat);
            await context.SaveChangesAsync();
            return mapper.Map<CreatedChatResponse>(chat);
        }

        [HttpGet("Join/{chatId:int}/{userId:int}")]
        public ActionResult JoinChat(int chatId, int userId)
        {
            return Ok();
        }

        [HttpDelete("Close/{chatId:int}")]
        public async Task<ActionResult<CreatedChatResponse>> CloseChat(int chatId)
        {
            var exists = await context.Chats.AnyAsync(chat => chat.Id == chatId);

            if (!exists)
            {
                return NotFound();
            }

            context.Remove(new Chat() { Id = chatId });
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("List/Chats")]
        public async Task<ActionResult<List<CreatedChatResponse>>> GetChats()
        {
            var chats = await context.Chats.ToListAsync();
            return mapper.Map<List<CreatedChatResponse>>(chats);
        }

        [HttpGet("ListLast/{amount:int}/Messages/{chatId:int}")]
        public async Task<ActionResult<List<SavedMessage>>> GetLastMessage(int amount, int chatId)
        {
            var messages = await context.Messages.Where(message => message.ChatId == chatId).ToListAsync();
            return mapper.Map<List<SavedMessage>>(messages.TakeLast(amount > 50 ? 50 : amount));
        }

        [HttpPost("Send/Message")]
        public async Task<ActionResult<SavedMessage>> SendMessage([FromBody] MessageRequest messageRequest)
        {
            var message = mapper.Map<Message>(messageRequest);
            message.TimeStamp = DateTime.Now;
            context.Add(message);
            await context.SaveChangesAsync();
            return mapper.Map<SavedMessage>(message);
        }

        [HttpGet("Read/Message/{chatId:int}/{lastMessageId:int}")]
        public async Task<ActionResult<List<SavedMessage>>> ReceiveMessage(int chatId, int lastMessageId)
        {
            var messages = await context.Messages.Where(message => message.ChatId == chatId && message.Id > lastMessageId).ToListAsync();
            return mapper.Map<List<SavedMessage>>(messages);
        }
    }
}
