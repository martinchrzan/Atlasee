using System;

namespace VisFileManager.Messenger
{
    public interface IMessenger
    {
        Guid Subscribe<TPayload>(string messageId, Action<TPayload> callback);

        void Send<TPayload>(string messageId, TPayload payload);

        void Unsubscribe(Guid id);
    }
}
