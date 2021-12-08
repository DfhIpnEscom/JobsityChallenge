using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Models
{
    public class SavedMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
