using aksnvl.messaging;
using aksnvl.storage;
using asknvl.messaging;
using asknvl.storage;
using coeffbot.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace coeffbot.Models.messages
{
    public abstract class MessageProcessorBase : ViewModelBase, IMessageUpdater
    {
        #region vars
        protected Dictionary<string, StateMessage> state_messages = new();
        protected Dictionary<string, StateMessage> numbr_messages = new();
        protected Dictionary<string, StateMessage> adm_numbr_messages = new();

        IStorage<Dictionary<string, StateMessage>> stateMessageStorage;
        IStorage<Dictionary<string, StateMessage>> numbrMessageStorage;
        IStorage<Dictionary<string, StateMessage>> admNumbrMessageStorage;

        string geotag;
        string token;
        ITelegramBotClient bot;
        #endregion

        #region properties        
        public abstract ObservableCollection<messageControlVM> MessageTypes { get; }

        int totalNumbered;
        public int TotalNumbered
        {
            get => totalNumbered;
            set => this.RaiseAndSetIfChanged(ref totalNumbered, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addNumberedCmd { get; }
        public ReactiveCommand<Unit, Unit> clearNumberedCmd { get; }
        #endregion

        public MessageProcessorBase(string geotag, string token, ITelegramBotClient bot)
        {
            this.geotag = geotag;
            this.bot = bot;          
            this.token = token;

            #region commands
            addNumberedCmd = ReactiveCommand.Create(() => {
                AddNumberedMessageRequest?.Invoke();            
            });
            clearNumberedCmd = ReactiveCommand.Create(() => { 
                numbr_messages.Clear();
                numbrMessageStorage.save(numbr_messages);
                TotalNumbered = numbr_messages.Count();
            });
            #endregion
        }

        #region public        
        public async void Add(string code, Message message)
        {
            if (MessageTypes == null)
                return;

            var found = MessageTypes.Any(t => t.Code.Equals(code));
            if (!found)
                return;

            var pattern = await StateMessage.Create(bot, message, geotag, token);
            //AutoChange pm_autochange = new AutoChange()
            //{
            //    OldText = "@booowos",
            //    NewText = pm
            //};
            //var autochanges = new List<AutoChange>() { pm_autochange };
            //pattern.MakeAutochange(autochanges);
            pattern.Id = state_messages.Count();

            if (state_messages.ContainsKey(code))
                state_messages[code] = pattern;
            else
                state_messages.Add(code, pattern);

            stateMessageStorage.save(state_messages);

            Debug.WriteLine($"{code}");
            StateMessageUpdatedEvent?.Invoke(code, true);
        }

        public async void Add(Message message, bool isadmin)
        {
            var pattern = await StateMessage.Create(bot, message, geotag, token);
            

            if (!isadmin)
            {
                pattern.Id = numbr_messages.Count();
                numbr_messages.Add($"{pattern.Id}", pattern);
                numbrMessageStorage.save(numbr_messages);
                TotalNumbered = numbr_messages.Count();
            }
            else
            {
                pattern.Id = adm_numbr_messages.Count();
                adm_numbr_messages.Add($"{pattern.Id}", pattern);
                admNumbrMessageStorage.save(adm_numbr_messages);
            }
        }

        public void Init()
        {
            stateMessageStorage = new Storage<Dictionary<string, StateMessage>>($"{geotag}_state.json", "messages", state_messages);
            numbrMessageStorage = new Storage<Dictionary<string, StateMessage>>($"{geotag}_numbr.json", "messages", numbr_messages);
            admNumbrMessageStorage = new Storage<Dictionary<string, StateMessage>>($"{geotag}_adm_numbr.json", "messages", adm_numbr_messages);

            state_messages = stateMessageStorage.load();
            foreach (var item in state_messages)
            {
                StateMessageUpdatedEvent.Invoke(item.Key, true);
            }

            numbr_messages = numbrMessageStorage.load();
            TotalNumbered = numbr_messages.Count();

            adm_numbr_messages = admNumbrMessageStorage.load(); 

        }

        public void Clear(bool isadmin)
        {            
            if (isadmin)
            {
                adm_numbr_messages.Clear();
                admNumbrMessageStorage.save(adm_numbr_messages);
            }
        }

        public abstract StateMessage GetMessage(string status, string? playerid = null, string? pm = null, string? url = null);
        public abstract StateMessage GetNumberedMessage(string? playerid);
        public abstract StateMessage GetAdmNumberedMessage(string? playerid, int index);

        public async Task UpdateMessageRequest(string code)
        {

            state_messages.Remove(code);
            stateMessageStorage.save(state_messages);
            StateMessageUpdatedEvent?.Invoke(code, false);

            var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
            if (found != null)
            {
                await Task.Run(() =>
                {
                    UpdateStateMessageRequestEvent?.Invoke(found.Code, found.Description);
                });
            }
        }



        public async Task ShowMessageRequest(string code)
        {
            if (state_messages.ContainsKey(code))
            {
                await Task.Run(() =>
                {
                    ShowStateMessageRequestEvent?.Invoke(state_messages[code], code);
                });
            }            
        }
        #endregion

        #region callbacks
        public event Action<string, string> UpdateStateMessageRequestEvent;
        public event Action<StateMessage, string> ShowStateMessageRequestEvent;
        public event Action<string, bool> StateMessageUpdatedEvent;

        public event Action AddNumberedMessageRequest;        
        #endregion
    }
}
