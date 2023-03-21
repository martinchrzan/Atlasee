using System;

namespace VisFileManager.Messenger
{
    public interface ISubscription
    {
        Type Type { get; }

        string MessageId { get; set; }

        void InvokeMethod(object args);

        Guid Id { get; }
    }
}
