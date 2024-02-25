using coeffbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace coeffbot.Models.messages
{
    public interface IMessageProcessorFactory
    {
        MessageProcessorBase Get(BotType type, string geotag, string token, ITelegramBotClient bot);
    }
}
