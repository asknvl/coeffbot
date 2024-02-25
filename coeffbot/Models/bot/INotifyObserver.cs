using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Models.bot
{
    public interface INotifyObserver
    {
        Task Notify(Object notify);
    }
    
}
