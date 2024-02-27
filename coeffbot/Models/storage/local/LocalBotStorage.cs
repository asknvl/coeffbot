using aksnvl.storage;
using asknvl.storage;
using coeffbot.Model.bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motivebot.Model.storage.local
{
    public class LocalBotStorage : IBotStorage 
    {
        #region vars
        IStorage<List<BotModel>> storage; 
        #endregion
        #region properties
        List<BotModel> BotModels { get; set; } = new List<BotModel>();
        #endregion
        public LocalBotStorage() {
            var subdir = Path.Combine("bots");
            storage = new Storage<List<BotModel>>("bots.json", subdir, BotModels);
            Load();
        }

        #region public
        public void Add(BotModel bot)
        {
            var found = BotModels.Any(m => m.geotag.Equals(bot.geotag));
            if (!found)
                BotModels.Add(bot);
            else
                throw new BotStorageException($"Бот с геотегом {bot.geotag} уже существует");

            storage.save(BotModels);
        }

        public void Remove(string geotag)
        {
            var found = BotModels.FirstOrDefault(m => m.geotag.Equals(geotag));
            if (found != null)
                BotModels.Remove(found);

            storage.save(BotModels);
        }

        public List<BotModel> GetAll()
        {
            Load();
            return BotModels;
        }

        public void Load()
        {
            try
            {
                BotModels = storage.load();

            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось загрузить данные");
            }
        }

        public void Save()
        {
            try
            {
                storage.save(BotModels);

            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось сохранить данные");
            }
        }

        public void Update(BotModel bot)
        {
            try
            {
                var found = BotModels.FirstOrDefault(m => m.geotag.Equals(bot.geotag));
                if (found != null)
                {
                    found.token = bot.token;
                    found.postbacks = bot.postbacks;    
                    found.pm = bot.pm;
                    found.landing = bot.landing;
                    found.sources = bot.sources;    
                    storage.save(BotModels);
                }
            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось обновить данные");
            }
        }       
        #endregion
    }
}
