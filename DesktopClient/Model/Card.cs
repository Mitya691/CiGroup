using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.Model
{
    class Card
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SourceSilo { get; set; }
        public string Destination {  get; set; }
        public string TargetSilo { get; set; }
        public double Weight1 { get; set;}
        public double Weight2 { get; set;}
        public double MainWeight { get; set;}
    }
}
