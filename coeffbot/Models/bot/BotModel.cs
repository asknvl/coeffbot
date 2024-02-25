using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Model.bot
{
    public class BotModel
    {
        public BotType type { get; set; }
        public string service { get; set; } = "coeff_bot";
        public string geotag { get; set; }
        public string token { get; set; }   
        public string pm { get; set; }        
        public string sources { get; set; }
        public bool? postbacks { get; set; }
        //public List<long> operators_id { get; set; } = new();
        public List<Operators.Operator> operators { get; set; } = new();    
    }

    public enum BotType
    {
        coeff_v0
    }
}
