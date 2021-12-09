using ChatWebAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChatWebAPI.Services
{
    public class BotService : IBotService
    {
        public async Task<string> GetCloseValue(string stockCode)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://stooq.com/q/l/?s={stockCode}&f=sd2t2ohlcv&h&e=csv");
            HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            string closeValue = string.Empty;
            if (results.Contains("\n"))
                closeValue = results.Split('\n')[1].Split(',')[6];
            sr.Close();
            return closeValue;
        }
    }
}
