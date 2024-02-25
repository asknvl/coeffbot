using aksnvl.storage;
using coeffbot.Model.bot;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class editBotVM : SubContentVM
    {
        #region properties
        IBotStorage botStorage;
        BotModel botModel;
        #endregion

        #region properties
        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token
        {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        //string link;
        //public string Link
        //{
        //    get => link;
        //    set => this.RaiseAndSetIfChanged(ref link, value);
        //}

        string pm;
        public string PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string sources;
        public string Sources
        {
            get => sources;
            set => this.RaiseAndSetIfChanged(ref sources, value);
        }

        //string channel;
        //public string Channel
        //{
        //    get => channel;
        //    set => this.RaiseAndSetIfChanged(ref channel, value);
        //}

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set => this.RaiseAndSetIfChanged(ref postbacks, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        #endregion

        public editBotVM(IBotStorage botstorage, BotBase bot)
        {

            Geotag = bot.Geotag;
            Token = bot.Token;
            PM = bot.PM;
            Sources = bot.Sources;

            botStorage = botstorage;
            var models = botStorage.GetAll();
            botModel = models.FirstOrDefault(m => m.geotag.Equals(bot.Geotag));

            #region commands
            saveCmd = ReactiveCommand.Create(() => {

                var newModel = new BotModel()
                {
                    geotag = Geotag,
                    token = Token,                    
                    pm = PM,
                    sources = Sources,
                    postbacks = Postbacks,
                };

            });

            cancelCmd = ReactiveCommand.Create(() => {
                
            });
            #endregion
        }

        #region public
        #endregion      
    }
}
