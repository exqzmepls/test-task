using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask
{
    public class Event
    {
        public string EventId { get; set; }

        public string Timestamp { get; set; }

        public string EventDescription { get; set; }

        public bool IsAlarmEvent { get; set; }

        public string ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string Zoneid { get; set; }

        public string Comment { get; set; }
    }
}
