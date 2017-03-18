using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    /// <summary>
    /// Martin Fowler: Event Aggregator Pattern
    /// http://martinfowler.com/eaaDev/EventAggregator.html
    /// This aggregator defines an event named Event where a handler of type Action<object, TEvent>
    /// can subscribe and unsubscribe, and a method Publish fires the event. 
    /// This aggregator is implemented as a singleton to make it easily accessible without needing to create an instance.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class EventAggregator<TEvent> where TEvent : EventArgs
    {
        private static readonly Lazy<EventAggregator<TEvent>> s_eventAggregator =
            new Lazy<EventAggregator<TEvent>>(() => new EventAggregator<TEvent>());

        public static EventAggregator<TEvent> Instance => s_eventAggregator.Value;

        private EventAggregator()
        {
        }

        public event Action<object, TEvent> Event;

        public void Publish(object source, TEvent ev)
        {
            Event?.Invoke(source, ev);
        }
    }
}
