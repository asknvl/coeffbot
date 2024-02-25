using coeffbot.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motivebot.Model.storage
{
    public interface IBotStorage
    {
        void Load();
        void Save();
        void Add(BotModel bot);
        void Remove(string geotag);
        void Update(BotModel bot);
        List<BotModel> GetAll();
        
    }

    public class BotStorageException : Exception
    {        
        public BotStorageException(string msg) : base(msg) { }
    }
}
