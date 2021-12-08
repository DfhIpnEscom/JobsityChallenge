using AutoMapper;
using ChatWebAPI.Entities;
using ChatWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Utils
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<CreateChatRequest, Chat>();
            CreateMap<Chat, CreatedChatResponse>();
            CreateMap<MessageRequest, Message>();
            CreateMap<Message, SavedMessage>();
        }
    }
}
