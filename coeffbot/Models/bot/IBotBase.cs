using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Model.bot
{
    public interface IBotBase
    { 
        string Geotag { get; set; }
        string Name { get; set; }
        string Token { get; set; }
        ObservableCollection<long> Operators { get; }
        bool IsActive { get; set; }
    }

    public enum State
    {
        free,
        waiting_new_state_message, //Детерменированные сообщения
        waiting_new_numbered_message //Сообщения которые просто идут по порядку или в хаотичном порядке
    }
}
