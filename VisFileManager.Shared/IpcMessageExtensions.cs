using System;

namespace VisFileManager.Shared
{
    public static class IpcMessageExtensions
    {
        public static IpcMessage CreateErrorMessage(this IpcMessage ipcMessage, string error)
        {
            ipcMessage.ClientCurrentState = ClientState.Error;
            ipcMessage.Error = error;
            ipcMessage.MessageId = Guid.NewGuid();
            return ipcMessage;
        }

        public static IpcMessage SetProgress(this IpcMessage ipcMessage, double totalProgress, double partialProgress, string currentSpeed)
        {
            ipcMessage.TotalProgress = totalProgress;
            ipcMessage.PartialProgress = partialProgress;
            ipcMessage.CurrentSpeed = currentSpeed;
            ipcMessage.ClientCurrentState = ClientState.InProgress;
            return ipcMessage;
        }

        public static IpcMessage ChangeClientState(this IpcMessage ipcMessage, ClientState newState)
        {
            ipcMessage.ClientCurrentState = newState;
            ipcMessage.MessageId = Guid.NewGuid();
            return ipcMessage;
        }

        public static IpcMessage ChangeServerState(this IpcMessage ipcMessage, ServerState newState)
        {
            ipcMessage.ServerCurrentState = newState;
            ipcMessage.MessageId = Guid.NewGuid();
            return ipcMessage;
        }

        public static IpcMessage CreateReadyToCloseMessage(this IpcMessage message)
        {
            return ChangeServerState(message, ServerState.ClientCanCloseConnection);
        }
    }
}
