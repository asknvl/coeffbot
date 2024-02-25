using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Models.messages
{
    public interface IMessageUpdater
    {
        Task UpdateMessageRequest(string code);
        Task ShowMessageRequest(string code);
        event Action<string, string> UpdateStateMessageRequestEvent;
        event Action<string, bool> StateMessageUpdatedEvent;        
    }
}
