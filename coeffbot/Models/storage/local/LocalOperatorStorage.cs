using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aksnvl.storage;
using asknvl.storage;
using Avalonia.Controls;
using coeffbot.Model.bot;
using coeffbot.Operators;
using motivebot.Model.storage;

namespace coeffbot.Models.storage.local
{
    public class LocalOperatorStorage : IOperatorStorage
    {
        #region vars
        IStorage<List<BotOperators>> storage;
        #endregion

        #region properties
        List<BotOperators> BotOperators { get; set; } = new();
        #endregion

        public LocalOperatorStorage()
        {
            var subdir = Path.Combine("operators");
            storage = new Storage<List<BotOperators>>("operators.json", subdir, BotOperators);
            Load();
        }

        #region public
        public void Load()
        {
            try
            {
                BotOperators = storage.load();

            }
            catch (Exception ex)
            {
                throw new BotStorageException("Не удалось загрузить данные");
            }
        }

        public void Save()
        {
            try
            {
                storage.save(BotOperators);

            }
            catch (Exception ex)
            {
                throw new BotStorageException("Не удалось сохранить данные");
            }
        }

        public void Add(string geotag, Operator op)
        {
            var foundBotOperator = BotOperators.FirstOrDefault(bo => bo.geotag.Equals(geotag));
            if (foundBotOperator == null) { 
                BotOperators botOperators = new BotOperators();
                botOperators.geotag = geotag;
                botOperators.Operators.Add(op);
            }                
            else
            {
                var exist = foundBotOperator.Operators.FirstOrDefault(o => o.letters.Equals(op.letters));
                if (exist == null)
                    foundBotOperator.Operators.Add(op);
                else
                {
                    exist.first_name = op.first_name;
                    exist.last_name = op.last_name;


                    //exist.permissions.Clear();
                    //foreach (var p in op.permissions)
                    //    exist.permissions.Add(p);

                    exist.tg_id = op.tg_id;
                }
            }

            //UpdatedEvent?.Invoke();
            storage.save(BotOperators);
        }

        public void Update(List<BotOperators> botOperators)
        {
            BotOperators.Clear();

            foreach (var item in botOperators)
                BotOperators.Add(item);

            storage.save(BotOperators);
            //UpdatedEvent?.Invoke();
        }

        public void Add(string geotag)
        {
            var found = BotOperators.Any(bo => bo.geotag.Equals(geotag));
            if (!found)
            {
                BotOperators botOperators = new BotOperators();
                botOperators.geotag = geotag;
                BotOperators.Add(botOperators);
                storage.save(BotOperators);
                UpdatedEvent?.Invoke();
            }
        }

        public List<Operator> GetAll(string geotag)
        {
            List<Operator> res = new();
            var found = BotOperators.FirstOrDefault(bo => bo.geotag.Equals(geotag));
            if (found != null)
                res = found.Operators.ToList();            
            return res;
        }

        public List<BotOperators> GetAll()
        {
            return BotOperators;
        }

        public Operator GetOperator(string geotag, long tg_id)
        {
            Operator res = null;

            var botOperators = BotOperators.FirstOrDefault(bo => bo.geotag.Equals(geotag));
            if (botOperators != null)
            {
                var op = botOperators.Operators.FirstOrDefault(op => op.tg_id == tg_id);
                res = op;
            }

            return res;
        }

        public void Remove(string geotag, Operator op)
        {
            var botOperators = BotOperators.FirstOrDefault(bo => bo.geotag.Equals(geotag));
            if (botOperators != null)
            {
                var found = botOperators.Operators.FirstOrDefault(o => o.letters.Equals(op.letters));
                if (found != null)
                {
                    botOperators.Operators.Remove(found);
                    storage.save(BotOperators);
                }

            }

        }


        #endregion

        #region events
        public event Action UpdatedEvent;
        #endregion
    }
}
