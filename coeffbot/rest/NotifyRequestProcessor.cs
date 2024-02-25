using coeffbot.Models.bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.rest
{
    public class NotifyRequestProcessor : IRequestProcessor, INotifyObservable
    {
        #region vars
        List<INotifyObserver> notifyObservers = new List<INotifyObserver>();

        public void Add(INotifyObserver observer)
        {
            if (!notifyObservers.Contains(observer))
                notifyObservers.Add(observer);
        }

        public void Remove(INotifyObserver observer)
        {
            notifyObservers.Remove(observer);
        }
        #endregion

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {

            HttpStatusCode code = HttpStatusCode.BadRequest;
            string responseText = "Incorrect parameters";

            try
            {
                var notifyData = JsonConvert.DeserializeObject<NotifyData>(data);
                
                foreach (var observer in notifyObservers)
                {
                    try
                    {
                        observer.Notify(notifyData);
                    } catch (NotImplementedException ex)
                    {

                    } catch (Exception ex)
                    {
                        throw;
                    }
                }

                code = HttpStatusCode.OK;
                responseText = $"{code}";

            } catch (Exception ex)
            {
            }

            return (code, responseText);
        }        
    }

    public class NotifyData
    {
        public string? timestamp_sent { get; set; }
        public List<NotifyDTO> data { get; set; } = new();
    }

    public class NotifyDTO
    {
        public long closer_tg_id { get; set; }
        public LeadInfoDTO lead_info { get; set; } = new();
    }

    public class LeadInfoDTO
    {
        public List<string> sources { get; set; } = new();
        public long lead_tg_id { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? username { get; set; }
        public string? offer_link { get; set; }
        public string? timestamp_created { get; set; }
    }
}
