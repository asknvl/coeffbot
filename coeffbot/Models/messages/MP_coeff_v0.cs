using asknvl.messaging;
using coeffbot.ViewModels;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace coeffbot.Models.messages
{
    public class MP_coeff_v0 : MessageProcessorBase
    {

        #region vars
        Random random = new Random();
        #endregion

        #region properties
        public override ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion

        public MP_coeff_v0(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>() { 
            
                new messageControlVM(this)
                {
                    Code = "match",
                    Description = "Игрок найден"
                },

                new messageControlVM(this)
                {
                    Code = "start",
                    Description = "Стартовое сообщение"
                },

                new messageControlVM(this)
                {
                    Code = "start_ok",
                    Description = "Инструкция к боту"
                },

                new messageControlVM(this)
                {
                    Code = "lose",
                    Description = "Пост на лендинг"
                }
            
            };
        }

        #region private
        protected virtual InlineKeyboardMarkup getMatchMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "💰 YES", callbackData: $"match_ok") };            
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getStartMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "✅START✅", callbackData: $"start_ok") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getNextKefMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];

            buttons[0] = new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData(text: "✅ WIN", callbackData: $"win"),
                InlineKeyboardButton.WithCallbackData(text: "❌ LOSE", callbackData: $"lose")
            };

            buttons[1] = new InlineKeyboardButton[] {
                InlineKeyboardButton.WithCallbackData(text: "⛔️ STOP", callbackData: $"stop"),                
            };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getConnectMarkup()
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[1][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "CONNECT", callbackData: $"win") };
            return buttons;
        }

        protected virtual InlineKeyboardMarkup getLoseMarkup(string url)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[2][];
            buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: "💸TOP UP BALANCE💸", $"{url}") };
            buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "🚀CONTIUNUE RECEIVING SIGNALS🚀", callbackData: $"win") };
            return buttons;
        }
        #endregion

        public override StateMessage GetMessage(string code, string? playerid = null, string? pm = null, string? url = null)
        {
            
            InlineKeyboardMarkup markUp = null!;

            switch (code)
            {
                case "match":
                    markUp = getMatchMarkup();                    
                    break;

                case "start":
                    markUp = getStartMarkup();
                    break;

                case "lose":
                    markUp = getLoseMarkup(url: url);
                    break;

                default:
                    break;

            }

            StateMessage msg = null!;
            StateMessage _msg = null!;
            List<AutoChange> autoChange;

            if (state_messages.ContainsKey(code))
            {
                msg = state_messages[code];//.Clone();

                switch (code)
                {
                    case "start":
                    case "match":

                        autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "playerid.chng",
                                NewText = $"{playerid}"
                            }
                        };

                        _msg = msg.Clone();
                        _msg.MakeAutochange(autoChange);
                        _msg.Message.ReplyMarkup = markUp;
                        return _msg;

                    case "lose":
                        autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "https://landing.chng",
                                NewText = $"{url}"
                            }
                        };

                        _msg = msg.Clone();
                        _msg.MakeAutochange(autoChange);
                        _msg.Message.ReplyMarkup = markUp;
                        return _msg;
                }

                msg.Message.ReplyMarkup = markUp;
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;
            }

            return msg;
        }

        public override StateMessage GetNumberedMessage(string? playerid)
        {
            var keys = numbr_messages.Keys.ToList();
            int index = random.Next(0, keys.Count);
            var key = $"{keys[index]}";

            var msg = numbr_messages[key];

            List<AutoChange> autoChange = new List<AutoChange>()
            {
                new AutoChange() {
                    OldText = "playerid.chng",
                    NewText = $"{playerid}"
                }
            };

            var _msg = msg.Clone();
            _msg.MakeAutochange(autoChange);
            _msg.Message.ReplyMarkup = getNextKefMarkup();

            return _msg;
        }
    }
}
