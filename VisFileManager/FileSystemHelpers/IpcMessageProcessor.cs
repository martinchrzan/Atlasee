using NamedPipeWrapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using VisFileManager.Shared;

namespace VisFileManager.FileSystemHelpers
{
    public class IpcMessageProcessor
    {
        public delegate void ProcessMessageAction(NamedPipeConnection<IpcMessage, IpcMessage> connection, IpcMessage message);

        private readonly ProcessMessageAction _readyToStartAction;
        private readonly ProcessMessageAction _inProgressAction;
        private readonly ProcessMessageAction _errorAction;
        private readonly ProcessMessageAction _finishedAction;
        private readonly ProcessMessageAction _cancelledAction;
        private readonly ProcessMessageAction _fileLockedAction;
        private readonly ProcessMessageAction _fileNameTakenAction;
        private bool _cancelled = false;
        private object _cancelLock = new object();

        public IpcMessageProcessor(
            ProcessMessageAction readyToStartAction,
            ProcessMessageAction inProgressAction,
            ProcessMessageAction errorAction,
            ProcessMessageAction finishedAction,
            ProcessMessageAction cancelledAction,
            ProcessMessageAction fileLockedAction,
            ProcessMessageAction fileNameTakenAction,
            CancellationTokenSource cancellationTokenSource)
        {
            _readyToStartAction = readyToStartAction;
            _inProgressAction = inProgressAction;
            _errorAction = errorAction;
            _finishedAction = finishedAction;
            _cancelledAction = cancelledAction;
            _fileLockedAction = fileLockedAction;
            _fileNameTakenAction = fileNameTakenAction;
            cancellationTokenSource.Token.Register(Cancel);
        }

        public IpcMessage LatestMessage { get; private set; }

        private NamedPipeConnection<IpcMessage, IpcMessage> CurrentConnection { get; set; }

        public void Cancel()
        {
            lock (_cancelLock)
            {
                if (!_cancelled)
                {
                    Task.Run(() =>
                    {
                        CurrentConnection.PushMessage(LatestMessage.ChangeServerState(ServerState.Cancelling));
                        _cancelled = true;
                    });
                }
            }
        }

        public void ProcessMessage(IpcMessage message, NamedPipeConnection<IpcMessage, IpcMessage> connection)
        {
            LatestMessage = message;
            CurrentConnection = connection;

            switch (message.ClientCurrentState)
            {
                case ClientState.Cancelled:
                    _cancelledAction.Invoke(connection, message);
                    break;
                case ClientState.Error:
                    _errorAction.Invoke(connection, message);
                    break;
                case ClientState.Finished:
                    _finishedAction.Invoke(connection, message);
                    break;
                case ClientState.InProgress:
                    _inProgressAction.Invoke(connection, message);
                    break;
                case ClientState.ReadyToStart:
                    _readyToStartAction.Invoke(connection, message);
                    break;
                case ClientState.FileLocked:
                    _fileLockedAction.Invoke(connection, message);
                    break;
                case ClientState.FileNameTaken:
                    _fileNameTakenAction.Invoke(connection, message);
                    break;
            }
        }
    }
}
