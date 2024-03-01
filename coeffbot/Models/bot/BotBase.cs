using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia;
using Avalonia.X11;
using coeffbot.Models.bot;
using coeffbot.Models.messages;
using coeffbot.Models.storage;
using coeffbot.Operators;
using coeffbot.rest;
using coeffbot.ViewModels;
using HarfBuzzSharp;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace coeffbot.Model.bot
{
    public abstract class BotBase : ViewModelBase, IPushObserver, IStatusObserver, INotifyObserver
    {
        #region const
        public enum UserState
        {
            waiting_password,
            free
        }
        #endregion

        #region vars        
        protected IOperatorStorage operatorStorage;
        protected IBotStorage botStorage;
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
        protected long ID;
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;
        Dictionary<long, UserState> userStates = new();

        protected List<long> userIDs = new List<long>();
        #endregion

        #region properties        
        public abstract BotType Type { get; }

        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string? name;
        public string? Name
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

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set
            {
                if (value == null)
                    value = false;
                this.RaiseAndSetIfChanged(ref postbacks, value);
            }
        }

        string pm;
        public string PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string landing;
        public string Landing
        {
            get => landing;
            set => this.RaiseAndSetIfChanged(ref landing, value);
        }

        string sources;
        public string Sources
        {
            get => sources;
            set => this.RaiseAndSetIfChanged(ref sources, value);
        }

        bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set
            {
                IsEditable = false;
                this.RaiseAndSetIfChanged(ref isActive, value);
            }
        }

        bool isEditable;
        public bool IsEditable
        {
            get => isEditable;
            set => this.RaiseAndSetIfChanged(ref isEditable, value);
        }

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
        }

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        #endregion

        public BotBase(IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger)
        {
            this.logger = logger;            
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            messageProcessorFactory = new MessageProcessorFactory(logger);

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await Start();
            });

            stopCmd = ReactiveCommand.Create(() =>
            {
                Stop();
            });

            editCmd = ReactiveCommand.Create(() => {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,                  
                    pm = PM,
                    landing = Landing,
                    sources = Sources,
                    postbacks = Postbacks
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() => {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;                
                Postbacks = tmpBotModel.postbacks;
                Landing = tmpBotModel.landing;
                PM = tmpBotModel.pm;
                Sources =tmpBotModel.sources;
                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() => {


                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,    
                    pm = PM,
                    landing = Landing,
                    sources = Sources,
                    postbacks = Postbacks
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region helpers
        protected InlineKeyboardMarkup getStopMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "STOP", callbackData: "adm_stop") };
            return buttons;
        }

        protected void setUserState(long userId, UserState userState)
        {
            if (userStates.ContainsKey(userId))
                userStates[userId] = userState;
            else
                userStates.Add(userId, userState);
        }

        protected void removeUser(long userId)
        {
            if (userStates.ContainsKey(userId))
                userStates.Remove(userId);
        }

        protected bool checkUserState(long userId, UserState state)
        {
            if (userStates.ContainsKey(userId))
                return userStates[userId] == state;
            else
                return false;
        }
        #endregion

        #region private
        protected abstract Task processFollowerActions(Message message);

        protected abstract Task processCallbackQuery(CallbackQuery query);        

        protected virtual async Task sendOperatorTextMessage(Operator op, long chat, string text)
        {

            await Task.CompletedTask;
           
        }

        protected virtual async Task processAdminActions(Message message, Operator op)
        {

            var chat = message.From.Id;            

            try
            {

                switch (message.Text)
                {
                    case "/start":                        
                        state = State.free;
                        return;                        

                    case "/add":
                        removeUser(chat);
                        await bot.SendTextMessageAsync(chat, "Добавьте сообщения с коефициентами:");
                        state = State.waiting_adm_numbered_message;
                        return;
                    case "/clear":
                        removeUser(chat);
                        MessageProcessor.Clear(true);
                        await bot.SendTextMessageAsync(chat, "Сообщения удалены");
                        return;
                }

                switch (state)
                {
                    case State.waiting_new_state_message:
                        MessageProcessor.Add(AwaitedMessageCode, message);
                        state = State.free;
                        break;

                    case State.waiting_new_numbered_message:
                        MessageProcessor.Add(message, false);
                        state = State.free;
                        break;

                    case State.waiting_adm_numbered_message:
                        MessageProcessor.Add(message, true);
                        await bot.SendTextMessageAsync(chat, "СТОП", replyMarkup: getStopMarkup());
                        break;

                    case State.free:                        
                        return;

                }                
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            } 
        }

        async Task processSubscribe(Update update)
        {

            long chat = 0;
            string fn = string.Empty;
            string ln = string.Empty;
            string un = string.Empty;
            string direction = "";

            try
            {

                if (update.MyChatMember != null)
                {

                    var mychatmember = update.MyChatMember;

                    chat = mychatmember.From.Id;
                    fn = mychatmember.From.FirstName;
                    ln = mychatmember.From.LastName;
                    un = mychatmember.From.Username;
                    

                    switch (mychatmember.NewChatMember.Status)
                    {
                        case ChatMemberStatus.Member:
                            direction = "UNBLOCK";
                            break;

                        case ChatMemberStatus.Kicked:
                            direction = "BLOCK";                    
                            break;

                        default:
                            return;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processSubscribe: {ex.Message}");
            } finally
            {
                var msg = $"{direction}: {chat} {fn} {ln} {un}";
                logger.inf(Geotag, msg); // logout JOIN or LEFT
            }
        }

        async Task processMessage(Message message)
        {
            long chat = message.Chat.Id;

            var op = operatorStorage.GetOperator(Geotag, chat);
            if (op != null)
            {
                await processAdminActions(message, op);
            }
           
            await processFollowerActions(message);
            
        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            switch (update.Type)
            {
                case UpdateType.MyChatMember:
                    await processSubscribe(update);
                    break;

                case UpdateType.Message:
                    if (update.Message != null)
                        await processMessage(update.Message);
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null)
                        await processCallbackQuery(update.CallbackQuery);
                    break;
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{Geotag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            logger.err(Geotag, ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region public
        public virtual async Task Start()
        {
            logger.inf(Geotag, $"Starting {Type} bot...");

            if (IsActive)
            {
                logger.err(Geotag, "Bot already started");
                return;
            }


#if DEBUG
            server = new TGBotFollowersStatApi("http://185.46.9.229:4000");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));            
#elif DEBUG_TG_SERV

            //server = new TGBotFollowersStatApi("http://185.46.9.229:4000");            
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
            bot = new TelegramBotClient(Token);
#else
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));
#endif

            var u = await bot.GetMeAsync();
            Name = u.Username;
            ID = u.Id;

            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message,
                                                    UpdateType.CallbackQuery,
                                                    UpdateType.MyChatMember,
                                                    UpdateType.ChatMember,
                                                    UpdateType.ChatJoinRequest }
            };

            //MessageProcessor = new MessageProcessor_v0(geotag, bot);
            MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

            if (MessageProcessor != null)
            {
                MessageProcessor.UpdateStateMessageRequestEvent += async (code, description) =>
                {
                    AwaitedMessageCode = code;
                    state = State.waiting_new_state_message;

                    //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));
                    var operators = operatorStorage.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                    foreach (var op in operators)
                    {
                        try
                        {
                            await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"UpdateMessageRequestEvent: {ex.Message}");
                        }
                    }
                };

                MessageProcessor.AddNumberedMessageRequest += async () =>
                {
                    state = State.waiting_new_numbered_message;

                    var operators = operatorStorage.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                    foreach (var op in operators)
                    {
                        try
                        {
                            await bot.SendTextMessageAsync(op.tg_id, $"Перешлите следующее сообщение с коэффициентом");
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"AddNumberedMessageRequest: {ex.Message}");
                        }
                    }
                };

                MessageProcessor.ShowStateMessageRequestEvent += async (message, code) =>
                {
                    //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));                
                    var operators = operatorStorage.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                    foreach (var op in operators)
                    {
                        try
                        {
                            int id = await message.Send(op.tg_id, bot);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"ShowMessageRequestEvent: {ex.Message}");
                        }
                    }
                };

                MessageProcessor.Init();
            }

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            try
            {
                await Task.Run(() => { });
                IsActive = true;
                logger.inf(Geotag, "Bot started");

            }
            catch (Exception ex)
            {
            }
        }

        public virtual async void Stop()
        {
            cts.Cancel();
            IsActive = false;
            logger.inf(Geotag, "Bot stopped");
        }

        public string GetGeotag()
        {
            return Geotag;
        }

        public virtual async Task<bool> Push(long id, string code, int notification_id)
        {

            return false;

            //bool res = false;
            //try
            //{
            //    string status = string.Empty;
            //    string uuid = string.Empty;
            //    (uuid, status) = await server.GetFollowerState(geotag, id);

            //    var push = messageProcessor.GetPush(code, Link, PM, uuid, Channel, false);

            //    if (push != null)
            //    {
            //        try
            //        {

            //            await push.Send(id, bot);
            //            res = true;
            //            logger.inf(Geotag, $"PUSHED: {id} {status} {code}");

            //        }
            //        catch (Exception ex)
            //        {
            //        } finally
            //        {
            //            await server.SlipPush(notification_id, res);
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.err(Geotag, $"Push: {ex.Message}");
            //}
            //return res;
        }

        public abstract Task UpdateStatus(StatusUpdateDataDto updateData);

        public abstract Task Notify(object notifyObject);
        #endregion
    }
}
