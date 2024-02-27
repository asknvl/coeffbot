using coeffbot.Model.bot;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class addBotVM : SubContentVM
    {
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

        string link;
        public string Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }

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

        bool? postbacks = false;
        public bool? Postbacks
        {
            get => postbacks;
            set => this.RaiseAndSetIfChanged(ref postbacks, value); 
        }

        List<BotType> botTypes = new() {     
            BotType.coeff_v0
        };
        public List<BotType> BotTypes
        {
            get => botTypes;
            set => this.RaiseAndSetIfChanged(ref botTypes, value);  
        }

        BotType type;
        public BotType Type
        {
            get => type;
            set => this.RaiseAndSetIfChanged(ref type, value);  
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        #endregion

        public addBotVM()
        {
            #region commands
            cancelCmd = ReactiveCommand.Create(() => {
                CancelledEvent?.Invoke();
                Close();
            });
            addCmd = ReactiveCommand.Create(() => {
                BotModel model = new BotModel()
                {      
                    type = Type,
                    geotag = Geotag,
                    token = Token,                    
                    postbacks = Postbacks,
                    pm = PM,
                    sources = Sources
                    
                };
                BotCreatedEvent?.Invoke(model);
                Close();
            });
            #endregion
        }

        #region callbacks
        public event Action<BotModel> BotCreatedEvent;
        public event Action CancelledEvent;
        #endregion

    }
}
