using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaVGames
{
    internal class Game
    {
        public int id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public DateTime? date { get; set; }
        public Boolean multiplayer { get; set; }
    }
}
