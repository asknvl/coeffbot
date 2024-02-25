using coeffbot.Models.bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace coeffbot.rest
{
    public class PushRequestProcessor : IRequestProcessor, IPushObservable
    {
        #region vars
        List<IPushObserver> pushObservers = new List<IPushObserver>();
        #endregion

        #region public
        public void Add(IPushObserver observer)
        {
            if (!pushObservers.Contains(observer))
                pushObservers.Add(observer);
        }

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {            
            HttpStatusCode code = HttpStatusCode.BadRequest;
            string responseText = "Incorrect parameters";

            try
            {
                await Task.Run(async () =>
                {

                    var pushdata = JsonConvert.DeserializeObject<PushRequestDto>(data);
                    int cntr = 0;

                    Task.Run(async () =>
                    {
                        foreach (var item in pushdata.data)
                        {
                            var geotag = item.geotag;
                            var observer = pushObservers.FirstOrDefault(o => o.GetGeotag().Equals(geotag));
                            if (observer != null) { 
                                try
                                {
                                    bool res = await observer.Push(item.tg_id, item.code, item.notification_id);
                                    if (res)
                                        cntr++;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        });

                    code = HttpStatusCode.OK;
                    responseText = $"{code.ToString()}";

                    //responseText = JsonConvert.SerializeObject(inactiveUsers);
                });

            }
            catch (Exception ex)
            {

            }

            return (code, responseText);

        }

        public void Remove(IPushObserver observer)
        {
            pushObservers.Remove(observer);
        }
        #endregion
    }

    public class PushInfoDto
    {
        [JsonRequired]
        public string geotag { get; set; }
        [JsonRequired]
        public long tg_id { get; set; }
        [JsonRequired]
        public string code { get; set; }
        [JsonRequired]
        public int notification_id { get; set; }
    }

    public class PushRequestDto
    {
        [JsonRequired]
        public List<PushInfoDto> data { get; set; } = new();
    }

    public class InactiveUsers
    {
        public string geotag { get; set; }
        public List<long> data { get; set; } = new();
        public InactiveUsers(string geotag)
        {
            this.geotag = geotag;
        }
    }

}
