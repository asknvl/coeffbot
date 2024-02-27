using aksnvl.messaging;
using asknvl.logger;
using coeffbot.Model.bot;
using coeffbot.Models.messages;
using coeffbot.Models.storage;
using coeffbot.rest;
using motivebot.Model.storage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace coeffbot.Models.bot
{
    public class coeff_bot_v0 : BotBase
    {

        #region const
        const string password = "Amir$$$";
        #endregion

        #region vars
        Dictionary<long, UserState> userStates = new();
        Dictionary<long, string> playerIDs = new();
        #endregion

        public coeff_bot_v0(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
            PM = model.pm;  
            Landing = model.landing;
            Sources = model.sources;
        }

        public override BotType Type => BotType.coeff_v0;


        #region helpers
        void setUserState(long userId, UserState userState)
        {
            if (userStates.ContainsKey(userId))
                userStates[userId] = userState;
            else
                userStates.Add(userId, userState);
        }

        bool checkUserState(long userId, UserState state)
        {
            if (userStates.ContainsKey(userId))
                return userStates[userId] == state;
            else
                return false;
        }

        async Task<string> getPlayerId(long chat) {

            string res = string.Empty;

            if (playerIDs.ContainsKey(chat))
                res = playerIDs[chat];

            else
            {
                try
                {
                    var subscribes = await server.GetUserInfoByTGid(chat);
                    var info = subscribes.FirstOrDefault(s => s.geo.Equals(Sources)); //ПРОВЕРКА НА ИСТОЧНИК РЕГИСТРАЦИИ
                    if (info != null && !string.IsNullOrEmpty(info.player_id))
                    {

                        if (playerIDs.Count > 2048)
                            playerIDs.Clear();

                        playerIDs.Add(chat, info.player_id);
                        res = info.player_id;
                    }
                    else
                        throw new Exception("Нет пользователя с доступным геотегом или ему не присвоен player_id");

                }
                catch (Exception ex)
                {
                    throw new Exception($"getPlayerId: {ex.Message}");
                }
            }

            return res;

        }

        

        async Task<bool> chekPlayersId(long chat, string? fn, string? ln, string? un)
        {
            bool res = false;
            try
            {
                await Task.Run(async () => {

                    await bot.SendTextMessageAsync(chat, "🆔 Matching your account ID...");

                    var msg = MessageProcessor.GetMessage("match");


                    try
                    {
                        var subscribes = await server.GetUserInfoByTGid(chat);
                        
                        var info = subscribes.FirstOrDefault(s => s.geo.Equals(Sources));

                        var player_id = await getPlayerId(chat);

                        var m = MessageProcessor.GetMessage("match", playerid: player_id);
                        await m.Send(chat, bot);

                        logger.inf(Geotag, $"MATCH: {chat} {fn} {ln} {un} {player_id}");

                    }
                    catch (Exception ex)
                    {

                        logger.err(Geotag, $"checkPlayersId: {chat} {ex.Message}");

                        await bot.SendTextMessageAsync(chat, $"❌ Unable to match account ID! ❌");
                        await bot.SendTextMessageAsync(chat, $"✉️ Message {PM} to get an access 🔑");
                    }

                });

            } catch (Exception ex)
            {
                logger.err(Geotag, $"checkPlayersId: {chat} {ex.Message}");
            }
            return res;
        }
        #endregion

        enum UserState
        {
            waiting_password,
            free
        }

        protected override async Task processCallbackQuery(CallbackQuery query)
        {
            long chat = query.Message.Chat.Id;
            PushMessageBase message = null;

            try
            {

                var player_id = await getPlayerId(chat);
                StateMessage m = null!;

                await bot.AnswerCallbackQueryAsync(query.Id);

                switch (query.Data)
                {
                    case "match_ok":                        
                        await bot.SendTextMessageAsync(chat, "❗️INSTRUCTIONS FOR USING BOT SIGNALS\r\n\r\nThe bot issues a signal like this \r\n👇👇👇");
                        m = MessageProcessor.GetMessage("start", playerid: player_id);
                        await m.Send(chat, bot);
                        break;

                    case "start_ok":                        
                        await bot.SendTextMessageAsync(chat, "⏳Wait a few seconds...");
                        await Task.Delay(10000);
                        await bot.SendTextMessageAsync(chat, "🔗CONNECTING TO THE ROUND...");
                        await Task.Delay(20000);
                        m = MessageProcessor.GetNumberedMessage(player_id);
                        await m.Send(chat, bot);
                        break;

                    case "win":                        
                        await bot.SendTextMessageAsync(chat, "⏳ Please wait a few minutes, the bot is calculating the Aviator’s vulnerabilities...");
                        await Task.Delay(5000);
                        await bot.SendTextMessageAsync(chat, "🔐 Just a little bit left");
                        await Task.Delay(10000);
                        await bot.SendTextMessageAsync(chat, "🔗CONNECTING TO THE ROUND...");
                        await Task.Delay(20000);
                        m = MessageProcessor.GetNumberedMessage(player_id);
                        await m.Send(chat, bot);
                        break;

                    case "lose":
                        m = MessageProcessor.GetMessage("lose", url: Landing);///
                        await m.Send(chat, bot);
                        break;

                    case "stop":                        
                        InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
                        buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "CONNECT", callbackData: $"win") };
                        await bot.SendTextMessageAsync(chat, "📲TO CONTINUE RECEIVING SIGNALS, CLICK THE \"CONNECT\" BUTTON", replyMarkup: (InlineKeyboardMarkup)buttons);
                        break;
                }

            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processCallbackQuery: {ex.Message}");
            }
        }



        protected override async Task processFollower(Message message)
        {

            if (message.Text == null)
                return;

            try
            {

                long chat = message.Chat.Id;
                var fn = message.From.Username;
                var ln = message.From.FirstName;
                var un = message.From.LastName;


                string text = message.Text;

                switch (text)
                {
                    case "/start":
                        await bot.SendTextMessageAsync(chat, "🔐ENTER PASSWORD🔐");
                        await bot.SendTextMessageAsync(chat, "⬇️⁣");
                        setUserState(chat, UserState.waiting_password);
                        break;

                    default:

                        if (checkUserState(chat, UserState.waiting_password))
                        {
                            if (text.Equals(password))
                            {
                                await chekPlayersId(chat, fn, ln, un);
                                setUserState(chat, UserState.free);
                            } else
                            {
                                await bot.SendTextMessageAsync(chat, "❗️PLEASE ENTER THE CORRECT PASSWORD");
                                await bot.SendTextMessageAsync(chat, "⬇️⁣");
                            }                               
                        }

                        break;                   
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, $"processFollower: {ex.Message}");
            }
        }

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            throw new NotImplementedException();
        }
    }
}
