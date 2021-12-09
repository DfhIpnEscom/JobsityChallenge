using AutoMapper;
using ChatWebAPI.Entities;
using ChatWebAPI.Models;
using ChatWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IBotService botService;

        public ChatController(ApplicationDBContext context, IMapper mapper, IBotService botService)
        {
            this.context = context;
            this.mapper = mapper;
            this.botService = botService;
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
        public async Task<ActionResult> JoinChat(int chatId, int userId)
        {
            var chatExists = await context.Chats.AnyAsync(chat => chat.Id == chatId);

            if (!chatExists)
            {
                return NotFound("Invalid chat");
            }

            context.Add(new ChatUser() { ChatId = chatId, UserId = userId });
            await context.SaveChangesAsync();
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

            var chatUserExists = await context.ChatsUsers.AnyAsync(chatUser => chatUser.ChatId == chatId);

            if (chatUserExists)
            {
                var delete = context.ChatsUsers.Where(chatUser => chatUser.ChatId == chatId);
                context.ChatsUsers.RemoveRange(delete);
            }

            var messageExists = await context.Messages.AnyAsync(message => message.ChatId == chatId);

            if (messageExists)
            {
                var delete = context.Messages.Where(message => message.ChatId == chatId);
                context.Messages.RemoveRange(delete);
            }

            context.Chats.Remove(new Chat() { Id = chatId });
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
            
            var chatExists = await context.ChatsUsers.AnyAsync(chatUser => chatUser.ChatId == message.ChatId && chatUser.UserId == message.UserId);

            if (!chatExists)
            {
                return NotFound("Invalid chat or user");
            }

            Regex pattern = new Regex(@"^/stock=(?<stock_code>[A-Z]*.?[A-Z]*)$");
            Match stockCode = pattern.Match(message.Content);
            if (stockCode.Success)
            {
                var code = stockCode.Groups["stock_code"].Value;

                var closeValue = await botService.GetCloseValue(code);
               
                return new SavedMessage() { Content = $"{code} quote is ${closeValue} per share", TimeStamp = DateTime.Now };
            }
            
            message.TimeStamp = DateTime.Now;
            context.Add(message);
            await context.SaveChangesAsync();
            return mapper.Map<SavedMessage>(message);
        }

        [HttpGet("Read/Message/{chatId:int}/{userId:int}/{lastMessageId:int}")]
        public async Task<ActionResult<List<SavedMessage>>> ReceiveMessage(int chatId, int userId, int lastMessageId)
        {
            var chatExists = await context.ChatsUsers.AnyAsync(chatUser => chatUser.ChatId == chatId && chatUser.UserId == userId);

            if (!chatExists)
            {
                return NotFound("Invalid chat or user");
            }

            var messages = await context.Messages.Where(message => message.ChatId == chatId && message.Id > lastMessageId).ToListAsync();
            return mapper.Map<List<SavedMessage>>(messages);
        }
    }
}
