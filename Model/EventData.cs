using System;
using System.Collections.Generic;
using System.Text;
using MKES.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MKES.Model
{
    public class EventModel
    {
        public Guid AggregateId { get; set; }
        public string EventName { get; set; }
        public JObject Data { get; set; }

        public static EventModel GetEventModelFromEvent(Event @event)
        {
            return new EventModel()
            {
                AggregateId = @event.AggregateId,
                EventName = @event.GetType().Name,
                Data = JObject.FromObject(@event)
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
