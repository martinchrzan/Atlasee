using System;

namespace VisFileManager.Messenger
{
    public sealed class Subscription<T> : ISubscription
    {
        public Type Type { get { return typeof(T); } }
        public string MessageId { get; set; }

        public Action<T> TypedCallback { get; set; }

        public Guid Id { get; set; }

        void ISubscription.InvokeMethod(object args)
        {
            if (!(args is T))
            {
                throw new ArgumentException(String.Concat("args is not type: ", typeof(T).Name), nameof(args));
            }
            TypedCallback.Invoke((T)args);
        }
    }
}
