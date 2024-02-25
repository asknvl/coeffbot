using aksnvl.messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace coeffbot.Models.messages
{
    public interface IMessageProcessor
    {
        Task<PushMessageBase> GetMessage(string status,
                                            string link = null,
                                            string pm = null,
                                            string uuid = null,
                                            string channel = null,
                                            bool? isnegative = false);
        void Add(Message message, string pm);
        void Clear();
       
    }
}
