using System;
using System.Collections.Concurrent;
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

    /// <summary>
    /// Note: Understanding and Avoiding Memory Leaks with Event Handlers and Event Aggregators
    /// http://mark-dot-net.blogspot.jp/2012/10/understanding-and-avoiding-memory-leaks.html
    ///  A weak reference holds a reference to a .NET object, but allows the garbage collector to delete it
    ///  if there are no other regular references to it.
    /// The trouble is, how do you attach a weak event handler to a regular.NET event? The answer is,
    /// with great difficulty, although some clever people have come up with ingenious ways of doing this.
    /// Event aggregators have an advantage here in that they can offer weak references as a feature if wanted,
    /// hiding the complexity of working with weak references from the end user.
    /// For example, the “Messenger” class that comes with MVVM Light uses weak references.
    /// </summary>
    public class WeakEventAggregator
    {
        class WeakAction
        {
            private WeakReference weakReference;
            public WeakAction(object action)
            {
                weakReference = new WeakReference(action);
            }

            public bool IsAlive
            {
                get { return weakReference.IsAlive; }
            }

            public void Execute<TEvent>(TEvent param)
            {
                var action = (Action<TEvent>)weakReference.Target;
                action.Invoke(param);
            }
        }

        private readonly ConcurrentDictionary<Type, List<WeakAction>> subscriptions
            = new ConcurrentDictionary<Type, List<WeakAction>>();

        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            var subscribers = subscriptions.GetOrAdd(typeof(TEvent), t => new List<WeakAction>());
            subscribers.Add(new WeakAction(action));
        }

        public void Publish<TEvent>(TEvent sampleEvent)
        {
            List<WeakAction> subscribers;
            if (subscriptions.TryGetValue(typeof(TEvent), out subscribers))
            {
                subscribers.RemoveAll(x => !x.IsAlive);
                subscribers.ForEach(x => x.Execute<TEvent>(sampleEvent));
            }
        }
    }
}
