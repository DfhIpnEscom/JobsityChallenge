using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Models
{
    public class MessageRequest
    {
        public string Content { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
