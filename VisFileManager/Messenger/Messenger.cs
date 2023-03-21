using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace VisFileManager.Messenger
{
    [Export(typeof(IMessenger))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Messenger : IMessenger
    {
        private readonly List<ISubscription> _subscribers;
        private object _subscribersLock = new object();

        public Messenger()
        {
            _subscribers = new List<ISubscription>();
        }

        public Guid Subscribe<TPayload>(string messageId, Action<TPayload> callback)
        {
            lock (_subscribersLock)
            {
                var id = Guid.NewGuid();
                //Add to the subscriber list
                _subscribers.Add(new Subscription<TPayload>()
                {
                    MessageId = messageId,
                    TypedCallback = callback,
                    Id = id
                });

                return id;
            }
        }

        public void Send<TPayload>(string messageId, TPayload payload)
        {
            IEnumerable<ISubscription> subs;
            lock (_subscribersLock)
            {
                //Get all subscribers that match the message and payload type
                subs = _subscribers.Where(x => x.MessageId == messageId && x.Type == typeof(TPayload)).ToList();
            }

            foreach (ISubscription sub in subs)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    sub.InvokeMethod(payload);
                });
            }
                
        }

        public void Unsubscribe(Guid id)
        {
            lock(_subscribersLock)
            {
                var subscriber = _subscribers.FirstOrDefault(it => it.Id == id);
                if (subscriber != null)
                {
                    _subscribers.Remove(subscriber);
                }
            }
        }
    }
}
