using aksnvl.messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace coeffbot.Models.messages 
{
    public class StateMessage : PushMessageBase
    {
        #region const        
        #endregion

        public StateMessage()
        {

        }

        public static async Task<StateMessage> Create(ITelegramBotClient bot, Message pattern, string geotag, string token)
        {
            StateMessage res = new StateMessage();
            res.Message = pattern;

            string fileId = null;
            Telegram.Bot.Types.File fileInfo;
            string filePath = null;

            await Task.Run(async () => {

                switch (res.Message.Type)
                {
                    case  Telegram.Bot.Types.Enums.MessageType.Text:
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        fileId = res.Message.Photo.Last().FileId;
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Video:
                        fileId = res.Message.Video.FileId;
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Document:
                        fileId = res.Message.Document.FileId;
                        break;
                }

                if (fileId != null)
                {
                    fileInfo = await bot.GetFileAsync(fileId);
                    filePath = fileInfo.FilePath;

                    //var fileName = filePath.Split('/').Last();

                    //string destinationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "messages", $"sources", geotag);
                    //if (!Directory.Exists(destinationFilePath))
                    //    Directory.CreateDirectory(destinationFilePath);

                    //destinationFilePath = Path.Combine(destinationFilePath, fileName);

                    //await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    //await bot.DownloadFileAsync(
                    //    filePath: filePath,
                    //    destination: fileStream);

                    //string token = "6559653927~AAHiDiuI9dzlWWb1pAWayZQlXBuuxRJKzwU";

                    res.FilePath = Path.Combine($"C:\\bots\\data\\{token.Replace(":", "~")}", filePath);
                }

            });

            return res;

        }

        public StateMessage Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<StateMessage>(serialized);
        }

    }
}
