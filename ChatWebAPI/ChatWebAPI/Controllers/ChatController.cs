using AutoMapper;
using ChatWebAPI.Entities;
using ChatWebAPI.Models;
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
            Regex pattern = new Regex(@"^/stock=(?<stock_code>[A-Z]*.?[A-Z]*)$");
            Match stockCode = pattern.Match(messageRequest.Content);
            if (stockCode.Success)
            {
                var code = stockCode.Groups["stock_code"].Value;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://stooq.com/q/l/?s={code}&f=sd2t2ohlcv&h&e=csv");
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string results = sr.ReadToEnd();
                string closeValue = string.Empty;
                if (results.Contains("\n"))
                    closeValue = results.Split('\n')[1].Split(',')[6];
                sr.Close();
                return new SavedMessage() { Content = $"{code} quote is ${closeValue} per share", TimeStamp = DateTime.Now };
            }
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
