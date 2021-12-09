using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWebAPI.Services.Interfaces
{
    public interface IBotService
    {
        Task<string> GetCloseValue(string stockCode);
    }
}
