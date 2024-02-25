using asknvl.logger;
using coeffbot.Model.bot;
using coeffbot.Models.storage;
using coeffbot.Models.storage.local;
using coeffbot.Operators;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Models.bot
{
    public class BotFactory : IBotFactory
    {

        #region vars        
        IOperatorStorage operatorStorage;
        IBotStorage botStorage;
        #endregion

        public BotFactory(IOperatorStorage operatorStorage, IBotStorage botStorage)
        {            
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;
        }

        public BotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {
                case BotType.coeff_v0:
                    return new coeff_bot_v0(model, operatorStorage, botStorage, logger); 
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
