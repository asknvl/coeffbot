using coeffbot.rest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Models.bot
{
    public interface IStatusObserver
    {
        string GetGeotag();
        Task UpdateStatus(StatusUpdateDataDto updateData);
    }
}
