using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Entities
{
    public class ChatUser
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
