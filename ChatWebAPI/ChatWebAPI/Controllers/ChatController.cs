using ChatWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("Create")]
        public ActionResult<CreatedChatResponse> CreateChat(CreateChatRequest newChat)
        {
            return new CreatedChatResponse() { Id = 1, CreationDate = DateTime.Now, Name = newChat.Name };
        }

        [HttpGet("Join/{chatId:int}/{userId:int}")]
        public ActionResult JoinChat(int chatId, int userId)
        {
            return Ok();
        }

        [HttpGet("Close/{chatId:int}")]
        public ActionResult CloseChat(int chatId)
        {
            return Ok();
        }

        [HttpGet("List/Chats")]
        public ActionResult<List<CreatedChatResponse>> GetChats()
        {
            return new List<CreatedChatResponse>();
        }

        [HttpGet("List/Messages/{chatId:int}")]
        public ActionResult<List<string>> GetMessage(int chatId)
        {
            return new List<string>();
        }

        [HttpPost("Send/Message")]
        public ActionResult SendMessage(string message)
        {
            return Ok();
        }

        [HttpGet("Read/Message/{chatId:int}")]
        public ActionResult<List<string>> ReceiveMessage(int chatId)
        {
            return new List<string>();
        }
    }
}
