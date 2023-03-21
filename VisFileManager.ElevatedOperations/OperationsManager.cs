using NamedPipeWrapper;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisFileManager.Shared;

namespace VisFileManager.ElevatedOperations
{
    public sealed class OperationsManager : IDisposable
    {
        Guid _currentOperationId;
        private NamedPipeClient<IpcMessage> _client;
        private const int ConnectionTimeoutInMs = 3000;
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        AutoResetEvent _retryAutoResetEvent = new AutoResetEvent(false);
        AutoResetEvent _fileNameTakenAutoResetEvent = new AutoResetEvent(false);
        private bool _skipItem;
        private bool _overwriteItem;
        DateTime _oldDateTime = DateTime.Now;

        public OperationsManager(Guid operationId, Guid applicationInstanceId)
        {
            _currentOperationId = operationId;
            _client = new NamedPipeClient<IpcMessage>(SharedStrings.IpcPipeName + applicationInstanceId);
        }

        public void Start()
        {
            _client.ServerMessage += Client_ServerMessage;
            // Start up the client asynchronously and connect to the specified server pipe.
            // This method will return immediately while the client runs in a separate background thread.
            _client.Start();
            _client.WaitForConnection(ConnectionTimeoutInMs);


            _client.WaitForDisconnection();
            _client.ServerMessage -= Client_ServerMessage;
        }

        private void Client_ServerMessage(NamedPipeConnection<IpcMessage, IpcMessage> connection, IpcMessage message)
        {
            {
                if (_currentOperationId != message.WholeOperationId)
                {
                    return;
                }

                switch (message.ServerCurrentState)
                {
                    case ServerState.Cancelling:
                        _cancellationTokenSource.Cancel();
                        _retryAutoResetEvent.Set();
                        _fileNameTakenAutoResetEvent.Set();
                        break;
                    case ServerState.ClientCanCloseConnection:
                        _client.Stop();
                        break;
                    case ServerState.InProgress:
                        Task.Run(async () => { await ProcessMessage(message); });
                        break;
                    case ServerState.Preparing:
                        SendMessage(message.ChangeClientState(ClientState.ReadyToStart));
                        break;
                    case ServerState.Retry:
                        _retryAutoResetEvent.Set();
                        break;
                    case ServerState.Skip:
                        _skipItem = true;
                        _retryAutoResetEvent.Set();
                        _fileNameTakenAutoResetEvent.Set();
                        break;
                    case ServerState.Overwrite:
                        _overwriteItem = true;
                        _fileNameTakenAutoResetEvent.Set();
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task ProcessMessage(IpcMessage ipcMessage)
        {
            _currentOperationId = ipcMessage.WholeOperationId;

            switch (ipcMessage.Action)
            {
                case Shared.Action.NewFolder:
                    SendMessage(CreateNewFolder(ipcMessage));
                    break;
                case Shared.Action.Rename:
                    SendMessage(Rename(ipcMessage));
                    break;
                case Shared.Action.Delete:
                    SendMessage(Remove(ipcMessage));
                    break;
                case Shared.Action.NewTextFile:
                    SendMessage(CreateNewFile(ipcMessage));
                    break;
                case Shared.Action.Copy:
                    var copyResult = await Copy(ipcMessage, false);
                    SendMessage(copyResult);
                    break;
                case Shared.Action.Move:
                    var moveResult = await Copy(ipcMessage, true);
                    SendMessage(moveResult);
                    break;
                default:
                    // nothing to do
                    break;

            }
        }

        private void SendMessage(IpcMessage ipcMessage)
        {
            _client.PushMessage(ipcMessage);
        }

        private IpcMessage CreateNewFolder(IpcMessage ipcMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(ipcMessage.DestinationPath))
                {
                    return ipcMessage.CreateErrorMessage("Missing new folder name!");
                }

                ipcMessage.CurrentlyProcessingItem = ipcMessage.DestinationPath;

                Directory.CreateDirectory(ipcMessage.DestinationPath);
                return ipcMessage.ChangeClientState(ClientState.Finished);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create new folder: " + ipcMessage.DestinationPath + Environment.NewLine + ex.Message);
                return ipcMessage.CreateErrorMessage(ex.Message);
            }
        }

        private IpcMessage CreateNewFile(IpcMessage ipcMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(ipcMessage.DestinationPath))
                {
                    return ipcMessage.CreateErrorMessage("Missing new text file name!");
                }

                ipcMessage.CurrentlyProcessingItem = ipcMessage.DestinationPath;

                File.WriteAllText(ipcMessage.DestinationPath, string.Empty);

                return ipcMessage.ChangeClientState(ClientState.Finished);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create new file: " + ipcMessage.DestinationPath + Environment.NewLine + ex.Message);
                return ipcMessage.CreateErrorMessage(ex.Message);
            }
        }

        private IpcMessage Rename(IpcMessage ipcMessage)
        {
            try
            {
                if (ipcMessage.SourcePaths == null || ipcMessage.SourcePaths.Count == 0)
                {
                    return ipcMessage.CreateErrorMessage("No source to rename!");
                }
                if (ipcMessage.SourcePaths.Count > 1)
                {
                    return ipcMessage.CreateErrorMessage("Can rename only one item at a time!");
                }

                ipcMessage.CurrentlyProcessingItem = ipcMessage.SourcePaths[0];

                WaitUntilDecisionWhatToDoWithLockedFile(ipcMessage.SourcePaths[0], ipcMessage);

                if (ShouldCancel(ipcMessage))
                {
                    return ipcMessage.ChangeClientState(ClientState.Cancelled);
                }

                if (!_skipItem)
                {
                    Directory.Move(ipcMessage.SourcePaths.First(), ipcMessage.DestinationPath);
                }
                _skipItem = false;
                return ipcMessage.ChangeClientState(ClientState.Finished);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create rename file: " + ipcMessage.DestinationPath + Environment.NewLine + ex.Message);
                return ipcMessage.CreateErrorMessage(ex.Message);
            }
        }

        private IpcMessage Remove(IpcMessage ipcMessage)
        {
            try
            {
                if (ipcMessage.SourcePaths == null || ipcMessage.SourcePaths.Count == 0)
                {
                    return ipcMessage.CreateErrorMessage("No source to rename!");
                }

                if (ShouldCancel(ipcMessage))
                {
                    return ipcMessage.ChangeClientState(ClientState.Cancelled);
                }

                foreach (var path in ipcMessage.SourcePaths)
                {
                    ipcMessage.CurrentlyProcessingItem = path;

                    WaitUntilDecisionWhatToDoWithLockedFile(path, ipcMessage);

                    if (ShouldCancel(ipcMessage))
                    {
                        return ipcMessage.ChangeClientState(ClientState.Cancelled);
                    }

                    if (_skipItem)
                    {
                        _skipItem = false;
                        continue;
                    }

                    var isDir = SharedFunctions.IsDirFile(path);
                    if (isDir == true)
                    {
                        Directory.Delete(path, true);
                    }
                    else if (isDir == false)
                    {
                        File.Delete(path);
                    }
                }

                return ipcMessage.ChangeClientState(ClientState.Finished);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to remove files: " + string.Join(", ", ipcMessage.SourcePaths) + Environment.NewLine + ex.Message);
                return ipcMessage.CreateErrorMessage(ex.Message);
            }
        }

        private async Task<IpcMessage> Copy(IpcMessage ipcMessage, bool move)
        {
            try
            {
                if (ipcMessage.SourcePaths == null || ipcMessage.SourcePaths.Count == 0)
                {
                    return ipcMessage.CreateErrorMessage("No files to copy!");
                }

                if (ShouldCancel(ipcMessage))
                {
                    return ipcMessage.ChangeClientState(ClientState.Cancelled);
                }

                if (move)
                {
                    // enough to check the first file as we can copy/cut from one folder only
                    if (Path.GetDirectoryName(ipcMessage.SourcePaths.First()) == ipcMessage.DestinationPath)
                    {
                        // nothing to move if we are already in that folder
                        return ipcMessage.ChangeClientState(ClientState.Finished);
                    }
                }

                int counter = 0;
                double total = 0;

                foreach (var path in ipcMessage.SourcePaths)
                {

                    ipcMessage.CurrentlyProcessingItem = path;

                    SendProgressUpdate(ipcMessage, total, 0, string.Empty);

                    var requestedName = GetRequestedName(path, ipcMessage.DestinationPath);

                    if (Path.GetDirectoryName(ipcMessage.SourcePaths.First()) == ipcMessage.DestinationPath)
                    {
                        var namePattern = Path.Combine(ipcMessage.DestinationPath, Path.GetFileNameWithoutExtension(path) + " - copy" + Path.GetExtension(path));
                        requestedName = UniqueNameGeneratorHelper.NextAvailableName(namePattern);
                    }

                    WaitUntilDecisionWhatToDoWithTheSameFileName(requestedName, ipcMessage);

                    if (ShouldCancel(ipcMessage))
                    {
                        return ipcMessage.ChangeClientState(ClientState.Cancelled);
                    }

                    if (_skipItem)
                    {
                        _skipItem = false;

                        counter++;
                        total = Math.Min((counter / (double)ipcMessage.SourcePaths.Count) * 100, 100);
                        SendProgressUpdate(ipcMessage, total, 100, string.Empty);
                        continue;
                    }

                    if (!move)
                    {
                        //var destinationName = ipcMessage.DestinationPath;
                        //if (Path.GetDirectoryName(ipcMessage.SourcePaths.First()) == ipcMessage.DestinationPath)
                        //{
                        //    destinationName = Path.Combine(ipcMessage.DestinationPath, Path.GetFileNameWithoutExtension(path) + " - copy" + Path.GetExtension(path));
                        //    var destinationNameAvailable = UniqueNameGeneratorHelper.NextAvailableName(destinationName);
                        //}

                        var result = await IOExtensions.FileTransferManager.CopyWithProgressAsync(path, requestedName, (progress) =>
                        {
                            if (DateTime.Now - _oldDateTime > TimeSpan.FromSeconds(0.3))
                            {
                                SendProgressUpdate(ipcMessage, total, progress.Percentage, progress.GetDataPerSecondFormatted(IOExtensions.SuffixStyle.Windows, 2));
                                _oldDateTime = DateTime.Now;
                            }
                        }, false, _cancellationTokenSource.Token);

                        if (result == IOExtensions.TransferResult.Failed)
                        {
                            return ipcMessage.CreateErrorMessage("Copying of " + path + " failed");
                        }
                        if (result == IOExtensions.TransferResult.Cancelled)
                        {
                            return ipcMessage.ChangeClientState(ClientState.Cancelled);
                        }
                    }
                    else
                    {
                        var result = await IOExtensions.FileTransferManager.MoveWithProgressAsync(path, ipcMessage.DestinationPath, (progress) =>
                        {
                            if (DateTime.Now - _oldDateTime > TimeSpan.FromSeconds(0.3))
                            {
                                SendProgressUpdate(ipcMessage, total, progress.Percentage, progress.GetDataPerSecondFormatted(IOExtensions.SuffixStyle.Windows, 2));
                                _oldDateTime = DateTime.Now;
                            }
                        }, new CancellationToken());

                        if (result == IOExtensions.TransferResult.Failed)
                        {
                            return ipcMessage.CreateErrorMessage("Copying of " + path + " failed");
                        }
                    }
                    counter++;
                    // update after successful
                    total = Math.Min((counter / (double)ipcMessage.SourcePaths.Count) * 100, 100);

                    SendProgressUpdate(ipcMessage, total, 100, string.Empty);
                    // reset value
                    _overwriteItem = false;
                }

                return ipcMessage.ChangeClientState(ClientState.Finished);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to copy files: [SOURCE] " + string.Join(", ", ipcMessage.SourcePaths) + " [DESTINATION] " + ipcMessage.DestinationPath + Environment.NewLine + ex.Message);
                return ipcMessage.CreateErrorMessage(ex.Message);
            }
        }

        private void SendProgressUpdate(IpcMessage ipcMessage, double total, double partial, string speed)
        {
            SendMessage(ipcMessage.SetProgress(total, partial, speed));
        }

        private string GetRequestedName(string path, string destinationPath)
        {
            var isDir = path.IsDirFile();
            if (isDir == true)
            {
                return Path.Combine(destinationPath, new DirectoryInfo(path).Name);
            }
            else if (isDir == false)
            {
                return Path.Combine(destinationPath, Path.GetFileName(path));
            }
            return string.Empty;
        }

        private bool ShouldCancel(IpcMessage message)
        {
            return _cancellationTokenSource.IsCancellationRequested;
        }

        private void WaitUntilDecisionWhatToDoWithLockedFile(string path, IpcMessage message)
        {
            while (!_skipItem && Helpers.AnyFileLocked(path) && !_cancellationTokenSource.IsCancellationRequested)
            {
                SendMessage(message.ChangeClientState(ClientState.FileLocked));
                _retryAutoResetEvent.WaitOne();
                // add sleep for letting other apps remove their handle from locked file
                Thread.Sleep(500);
            }
        }

        private void WaitUntilDecisionWhatToDoWithTheSameFileName(string path, IpcMessage message)
        {
            while (!_skipItem && !_overwriteItem && Helpers.IsFileNameTaken(path) && !_cancellationTokenSource.IsCancellationRequested)
            {
                SendMessage(message.ChangeClientState(ClientState.FileNameTaken));
                _fileNameTakenAutoResetEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
            _retryAutoResetEvent.Dispose();
            _fileNameTakenAutoResetEvent.Dispose();
        }
    }
}
