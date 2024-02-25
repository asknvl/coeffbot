using asknvl.logger;
using coeffbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Models.bot
{
    public interface IBotFactory
    {
        BotBase Get(BotModel model, ILogger logger);
    }
}
