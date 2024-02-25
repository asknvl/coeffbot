using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asknvl.messaging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aksnvl.messaging
{
    public class PushMessageBase
    {
        
        #region properties
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Message Message { get; set; }
        [JsonProperty]
        public string FilePath { get; set; }
        [JsonIgnore]
        public string fileId { get; set; } = null;
        #endregion

        public PushMessageBase()
        {
        }

        #region helpers
        protected (string, MessageEntity[]? entities) autoChange(string text, MessageEntity[]? entities, List<AutoChange> autoChanges)
        {
            string resText = text;
            List<MessageEntity>? resEntities = entities?.ToList();

            if (text == null)
                return (null, null);

            //foreach (var autochange in autoChanges)
            //{
            //    resEntities = resEntities?.OrderBy(e => e.Offset).ToList();
            //    int indexReplace = resText.IndexOf(autochange.OldText);
            //    if (indexReplace == -1)
            //        continue;

            //    resText = resText.Replace(autochange.OldText, autochange.NewText);

            //    if (resEntities != null)
            //    {
            //        int delta = autochange.NewText.Length - autochange.OldText.Lengt+		System.Linq.Enumerable.Where<TSource> возвращено	{System.Linq.Enumerable.WhereListIterator<Telegram.Bot.Types.MessageEntity>}	System.Linq.Enumerable.WhereListIterator<Telegram.Bot.Types.MessageEntity>

            //        var found = resEntities.Where(e => e.Offset == indexReplace).ToList();

            //        foreach (var item in found)
            //        {
            //            int ind = resEntities.IndexOf(item);
            //            resEntities[ind].Length += delta;
            //        }

            //        if (found != null && found.Count > 0)
            //        {
            //            var indexEntity = resEntities.IndexOf(found[0]);
            //            for (int i = indexEntity + 1; i < resEntities.Count; i++)
            //            {
            //                if (resEntities[i].Offset > indexReplace)
            //                    resEntities[i].Offset += delta;
            //            }
            //        }

            //    }
            //}

            foreach (var autochange in autoChanges)
            {
                resEntities = resEntities?.OrderBy(e => e.Offset).ToList();

                int indexReplace = resText.IndexOf(autochange.OldText);
                //-
                while (indexReplace != -1)
                {
                    resText = resText.Remove(indexReplace, autochange.OldText.Length).Insert(indexReplace, autochange.NewText);

                    if (resEntities != null)
                    {

                        var isReplacedEntity = resEntities.Any(e => e.Offset == indexReplace);

                        int delta = autochange.NewText.Length - autochange.OldText.Length;

                        if (isReplacedEntity)
                        {
                            var found = resEntities.Where(e => e.Offset <= indexReplace && indexReplace < e.Offset + e.Length).ToList();

                            foreach (var item in found)
                            {
                                int ind = resEntities.IndexOf(item);
                                resEntities[ind].Length += delta;
                            }

                            if (found != null && found.Count > 0)
                            {
                                var indexEntity = resEntities.IndexOf(found[0]);
                                for (int i = indexEntity + 1; i < resEntities.Count; i++)
                                {
                                    if (resEntities[i].Offset > indexReplace)
                                        resEntities[i].Offset += delta;
                                }
                            }
                        }
                        else
                        {
                            var found = resEntities.Where(e => e.Offset >= indexReplace).ToList();
                            foreach (var item in found)
                            {
                                int ind = resEntities.IndexOf(item);
                                resEntities[ind].Offset += delta;
                            }

                            var fulltextEntity = resEntities.FirstOrDefault(e => e.Offset == 0 && e.Length == text.Length);
                            if (found != null)
                            {
                                int ind = resEntities.IndexOf(fulltextEntity);
                                resEntities[ind].Length += delta;
                            }

                        }

                    }

                    indexReplace = resText.IndexOf(autochange.OldText);
                }
            }

            return (resText, resEntities?.ToArray());
        }

        string? autoChange(string text, List<AutoChange> autoChanges)
        {

            if (text == null)
                return null;

            string res = text;

            foreach (var autochange in autoChanges)
            {
                res = res.Replace(autochange.OldText, autochange.NewText);
            }

            return res;
        }

        MessageEntity[]? filterEntities(MessageEntity[] input, List<AutoChange> autoChanges)
        {
            if (input == null)
                return null;

            var entities = input;
            try
            {
                foreach (var item in entities)
                {

                    switch (item.Type)
                    {
                        case MessageEntityType.TextLink:
                            item.Url = autoChange(item.Url, autoChanges);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return entities;
        }

        void swapMarkupLink(InlineKeyboardMarkup markup, List<AutoChange> autoChanges)
        {
            foreach (var button in markup.InlineKeyboard)
            {
                foreach (var item in button)
                {
                    foreach (var autochange in autoChanges)
                        item.Url = item.Url.Replace(autochange.OldText.Replace("@", ""), autochange.NewText.Replace("@", ""));
                }
            }
        }

        async Task<int> sendTextMessage(long id, ITelegramBotClient bot, IReplyMarkup? markup = null)
        {
            var m = await bot.SendTextMessageAsync(
                    chatId: id,
                    text: Message.Text,
                    entities: Message.Entities,                    
                    replyMarkup: (markup == null) ? Message.ReplyMarkup : markup,
                    cancellationToken: new CancellationToken());
            return m.MessageId;
        }

        async Task<int> sendPhotoMessage(long id, ITelegramBotClient bot, IReplyMarkup? markup = null)
        {
            int messageId;
            var file = await bot.GetFileAsync(Message.Photo.Last().FileId);
            fileId = file.FileId;

            var sent = await bot.SendPhotoAsync(id,
                   photo: InputFile.FromFileId(fileId),
                   caption: Message.Caption,
                   replyMarkup: Message.ReplyMarkup,
                   captionEntities: Message.CaptionEntities);
            messageId = sent.MessageId;

            return messageId;
        }

        async Task<int> sendVideoMessage(long id, ITelegramBotClient bot, IReplyMarkup? markup = null, string? thumb_path = null)
        {
            int messageId;
            var file = await bot.GetFileAsync(Message.Video.FileId);
            fileId = file.FileId;

            var sent = await bot.SendVideoAsync(id,
                   video: InputFile.FromFileId(fileId),
                   //width: Message.Video.Width,
                   //height: Message.Video.Height,

                   duration: Message.Video.Duration,
                   caption: Message.Caption,
                   supportsStreaming: true,
                   replyMarkup: Message.ReplyMarkup,
                   captionEntities: Message.CaptionEntities);

            messageId = sent.MessageId;
            return messageId;
        }

        async Task<int> sendVideoNoteMessage(long id, ITelegramBotClient bot, IReplyMarkup? markup = null)
        {
            int messageId;
            var file = await bot.GetFileAsync(Message.VideoNote.FileId);
            fileId = file.FileId;

            var sent = await bot.SendVideoNoteAsync(id,
                videoNote: InputFile.FromFileId(fileId));
            messageId = sent.MessageId;
            return messageId;
        }

        async Task<int> sendDocumentMessage(long id, ITelegramBotClient bot, IReplyMarkup? markup = null)
        {
            int messageId;
            var file = await bot.GetFileAsync(Message.Document.FileId);
            fileId = file.FileId;

            var sent = await bot.SendDocumentAsync(id,
                document: InputFile.FromFileId(fileId),
                caption: Message.Caption,
                replyMarkup: Message.ReplyMarkup,
                captionEntities: Message.CaptionEntities);
            messageId = sent.MessageId;

            return messageId;
        }

        async Task<int> send(long id, ITelegramBotClient bot, IReplyMarkup? markup = null, string? thumb_path = null)
        {
            int messageId;
            switch (Message.Type)
            {
                case MessageType.Text:
                    messageId =  await sendTextMessage(id, bot, markup);
                    break;

                case MessageType.Photo:
                    messageId = await sendPhotoMessage(id, bot, markup);
                    break;
                                    
                case MessageType.Video:               
                    messageId = await sendVideoMessage(id, bot, markup, thumb_path);                    
                    break;

                case MessageType.VideoNote:
                    messageId = await sendVideoNoteMessage(id, bot, markup);
                    break;

                case MessageType.Document:
                    messageId = await sendDocumentMessage(id, bot, markup);
                    break;

                default:
                    messageId = 0;
                    break;
            }

            return messageId;
        }

        #endregion
        public virtual void MakeAutochange(List<AutoChange> autoChanges)
        {

            if (Message.ReplyMarkup != null)
                swapMarkupLink(Message.ReplyMarkup, autoChanges);

            switch (Message.Type)
            {
                case MessageType.Text:
                    (Message.Text, Message.Entities) = autoChange(Message.Text, filterEntities(Message.Entities, autoChanges), autoChanges);
                    break;

                case MessageType.Photo:
                case MessageType.Video:
                case MessageType.Document:
                    (Message.Caption, Message.CaptionEntities) = autoChange(Message.Caption, filterEntities(Message.CaptionEntities, autoChanges), autoChanges);
                    break;

            }
        }

        public virtual async Task<int> Send(long id, ITelegramBotClient bot, IReplyMarkup markup = null, string? thumb_path = null)
        {
            int messageId = 0;

            if (Message != null)
            {
                await Task.Run(async () =>
                {

                    try
                    {
                        messageId = await send(id, bot, markup, thumb_path);

                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToLower().Contains("wrong file"))
                        {
                            Console.WriteLine("Resending with fileId = null");
                            fileId = null;
                            messageId = await send(id, bot, markup);
                        }
                        else
                            throw;
                    }
                });
            }
           
            return messageId;
        }

        public void Clear()
        {
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }

            
        }

    }
}
