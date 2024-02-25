using coeffbot.Model.bot;
using coeffbot.Models.bot;
using coeffbot.Models.storage;
using coeffbot.Models.storage.local;
using coeffbot.Operators;
using coeffbot.rest;
using coeffbot.WS;
using motivebot.Model.storage;
using motivebot.Model.storage.local;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class mainVM : LifeCycleViewModelBase
    {

        #region vars
        IBotStorage botStorage;        
        IBotFactory botFactory;        
        IOperatorStorage operatorStorage;
        #endregion

        #region properties
        public ObservableCollection<BotBase> Bots { get; set; } = new();
        public ObservableCollection<BotBase> SelectedBots { get; set; } = new(); 
        
        BotBase selectedBot;
        public BotBase SelectedBot
        {
            get => selectedBot;
            set
            {
                SubContent = value;
                this.RaiseAndSetIfChanged(ref selectedBot, value);                
            }
        }

        object subContent;
        public object SubContent
        {
            get => subContent;
            set
            {
                this.RaiseAndSetIfChanged(ref subContent, value);
                if (subContent is SubContentVM) {
                    ((SubContentVM)subContent).OnCloseRequest += () =>
                    {
                        SubContent = null;
                    };
                }
            }
        }

        loggerVM logger;
        public loggerVM Logger {
            get => logger;
            set => this.RaiseAndSetIfChanged(ref logger, value);
        }

        operatorsVM operators;
        public operatorsVM OperatorsVM
        {
            get => operators;
            set => this.RaiseAndSetIfChanged(ref operators, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> removeCmd { get;  }
        public ReactiveCommand<Unit, Unit> editCmd { get; }        
        #endregion
        public mainVM()
        {

            Logger = new loggerVM();            

            //RestService restService = new RestService(Logger);

            //PushRequestProcessor pushRequestProcessor = new PushRequestProcessor();
            //StatusUpdateRequestProcessor statusUpdateRequestProcessor = new StatusUpdateRequestProcessor();
            //NotifyRequestProcessor notifyRequestProcessor = new NotifyRequestProcessor();

            //restService.RequestProcessors.Add(pushRequestProcessor);
            //restService.RequestProcessors.Add(statusUpdateRequestProcessor);
            //restService.RequestProcessors.Add(notifyRequestProcessor);


            //restService.Listen();

            botStorage = new LocalBotStorage();
            operatorStorage = new LocalOperatorStorage();

            botFactory = new BotFactory(operatorStorage, botStorage);

            var models = botStorage.GetAll();

            OperatorsVM = new operatorsVM(operatorStorage);            
            
            foreach (var model in models)
            {                
                var bot = botFactory.Get(model, logger);
                Bots.Add(bot);

                //pushRequestProcessor.Add(bot);
                //statusUpdateRequestProcessor.Add(bot);
                //notifyRequestProcessor.Add(bot);

                operatorStorage.Add(model.geotag);                
            }

            #region commands
            addCmd = ReactiveCommand.Create(() => {

                SelectedBot = null;

                var addvm = new addBotVM();
                addvm.BotCreatedEvent += (model) => {
                    try
                    {
                        botStorage.Add(model);
                    } catch (Exception ex)
                    {
                        throw;                        
                    }

                    var bot = botFactory.Get(model, logger);
                    Bots.Add(bot);

                    operatorStorage.Add(model.geotag);

                    //pushRequestProcessor.Add(bot);
                    //statusUpdateRequestProcessor.Add(bot);
                    //notifyRequestProcessor.Add(bot);
                };

                addvm.CancelledEvent += () => {                    
                };

                SubContent = addvm;
            });
            removeCmd = ReactiveCommand.Create(() =>
            {

                if (SelectedBot == null)
                    return;

                if (SelectedBot.IsActive)
                    return;

                var geotag = SelectedBot.Geotag;
                try
                {
                    botStorage.Remove(geotag);
                }
                catch (Exception ex)
                {
                    throw;                    
                }

                Bots.Remove(SelectedBot);
                //pushRequestProcessor.Remove(SelectedBot);

            });

            editCmd = ReactiveCommand.Create(() => {
                if (SelectedBot == null) 
                    return;
                var geotag = SelectedBot.Geotag;
                var editvm = new editBotVM(botStorage, SelectedBot);

            });            
            #endregion
        }

    }
}
