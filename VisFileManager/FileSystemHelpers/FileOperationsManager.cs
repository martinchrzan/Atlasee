using NamedPipeWrapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisFileManager.Helpers;
using VisFileManager.Settings;
using VisFileManager.Shared;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using Action = VisFileManager.Shared.Action;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(IFileOperationsManager))]
    public class FileOperationsManager : IFileOperationsManager
    {
        private readonly ConcurrentQueue<Guid> _prepareOperationsQueue = new ConcurrentQueue<Guid>();
        private readonly ConcurrentDictionary<Guid, IpcMessageProcessor> _operationProcessors = new ConcurrentDictionary<Guid, IpcMessageProcessor>();
        private readonly IDialogHelper _dialogHelper;
        private readonly IUserSettings _userSettings;
        private NamedPipeServer<IpcMessage> _server;
        private string _operationsExecutablePath;
        
        [ImportingConstructor]
        public FileOperationsManager(IDialogHelper dialogHelper, IUserSettings userSettings)
        {
            _server = new NamedPipeServer<IpcMessage>(SharedStrings.IpcPipeName + Bootstraper.ApplicationInstanceId);
            _server.ClientConnected += Server_ClientConnected;
            _server.ClientMessage += Server_ClientMessage;
            _server.Error += Server_Error;
            _server.Start();
            _dialogHelper = dialogHelper;
            _userSettings = userSettings;
        }
        
        public async Task<bool> RemoveItems(IEnumerable<FormattedPath> itemsToRemove, CancellationTokenSource cancellationTokenSource)
        {
            if (_userSettings.ShowDeleteConfirmationDialog.Value)
            {
                var itemsToRemoveMax = itemsToRemove.Take(5).Select(it => it.Name);
                var canRemoveResult = _dialogHelper.ShowThreeButtonsDialog("Do you want to remove these items?" + Environment.NewLine + string.Join(Environment.NewLine, itemsToRemoveMax), "Yes", "No");
                if (canRemoveResult != true)
                {
                    return await Task.FromResult(false);
                }
            }

            return await Task.Run(() =>
            {
                var operationId = Guid.NewGuid();
                _prepareOperationsQueue.Enqueue(operationId);

                AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                var messageProcessor = new IpcMessageProcessor(
                    readyToStartAction: (con, mess) => SendRequest(new IpcMessage()
                    {
                        Action = Action.Delete,
                        ServerCurrentState = ServerState.InProgress,
                        SourcePaths = itemsToRemove.Select(it => it.Path).ToList(),
                        WholeOperationId = operationId
                    }, con),
                    inProgressAction: (con, mess) => { },
                    errorAction: (con, mess) => { ShowError(con, mess, autoResetEvent); },
                    finishedAction: (con, mess) =>
                   {
                       Finished(con, mess, autoResetEvent);
                   },
                    cancelledAction: (con, mess) =>
                   {
                       Finished(con, mess, autoResetEvent);
                   },
                    fileLockedAction: (con, mess) =>
                    {
                        _dialogHelper.ShowTwoButtonsDialog(
                        "Unable to remove " + mess.CurrentlyProcessingItem + " due being used by another progess!",
                            () => SendRequest(mess.ChangeServerState(ServerState.Retry), con),
                            "Try again",
                            cancellationTokenSource);
                    },
                    fileNameTakenAction: (con, mess) => { },
                    cancellationTokenSource: cancellationTokenSource);

                _operationProcessors.GetOrAdd(operationId, messageProcessor);

                StartOperationsProcessWithCorrectRights(itemsToRemove, Action.Delete, operationId);

                autoResetEvent.WaitOne();

                _operationProcessors.TryRemove(operationId, out IpcMessageProcessor value);

                return messageProcessor.LatestMessage.ClientCurrentState == ClientState.Finished;
            });
        }

        public Task<bool> Rename(FormattedPath oldName, string newName, CancellationTokenSource cancellationTokenSource, Action<bool> runOnComplete=null)
        {
            return Task.Run(() =>
            {
                var operationId = Guid.NewGuid();
                _prepareOperationsQueue.Enqueue(operationId);

                AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                var messageProcessor = new IpcMessageProcessor(
                     readyToStartAction: (con, mess) => SendRequest(new IpcMessage()
                     {
                         Action = Action.Rename,
                         ServerCurrentState = ServerState.InProgress,
                         SourcePaths = new List<string>() { oldName.Path },
                         DestinationPath = newName,
                         WholeOperationId = operationId
                     }, con),
                    inProgressAction: (con, mess) => { },
                    errorAction: (con, mess) => { ShowError(con, mess, autoResetEvent); },
                    finishedAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    cancelledAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    fileLockedAction: (con, mess) =>
                    {
                        _dialogHelper.ShowTwoButtonsDialog("Unable to rename " + mess.CurrentlyProcessingItem + " due being used by another progess!",
                            () => SendRequest(mess.ChangeServerState(ServerState.Retry), con),
                            "Try again",
                            cancellationTokenSource);
                    },
                    fileNameTakenAction: (con, mess) => { },
                    cancellationTokenSource: cancellationTokenSource
                    );

                _operationProcessors.GetOrAdd(operationId, messageProcessor);

                StartOperationsProcessWithCorrectRights(new[] { oldName }, Action.Rename, operationId);

                autoResetEvent.WaitOne();

                _operationProcessors.TryRemove(operationId, out IpcMessageProcessor value);

                var result = false;
                if (messageProcessor.LatestMessage.ClientCurrentState == ClientState.Finished)
                {
                    result = true;
                }

                if (runOnComplete != null)
                {
                    runOnComplete.Invoke(result);
                }

                return result;
            });
        }

        public Task<bool> CreateNewFolder(string destination, CancellationTokenSource cancellationTokenSource)
        {
            return CreateNewItem(destination, cancellationTokenSource, Action.NewFolder);
        }

        public Task<bool> CreateNewTextFile(string destinationPath, CancellationTokenSource cancellationTokenSource)
        {
            return CreateNewItem(destinationPath, cancellationTokenSource, Action.NewTextFile);
        }

        private Task<bool> CreateNewItem(string destination, CancellationTokenSource cancellationTokenSource, Action action)
        {
            return Task.Run(() =>
            {
                var operationId = Guid.NewGuid();
                _prepareOperationsQueue.Enqueue(operationId);

                AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                var messageProcessor = new IpcMessageProcessor(
                     readyToStartAction: (con, mess) => SendRequest(new IpcMessage()
                     {
                         Action = action,
                         ServerCurrentState = ServerState.InProgress,
                         DestinationPath = destination,
                         WholeOperationId = operationId
                     }, con),
                    inProgressAction: (con, mess) => { },
                    errorAction: (con, mess) => { ShowError(con, mess, autoResetEvent); },
                    finishedAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    cancelledAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    fileLockedAction: (con, mess) => { },
                    fileNameTakenAction: (con, mess) => { },
                    cancellationTokenSource: cancellationTokenSource
                    );

                _operationProcessors.GetOrAdd(operationId, messageProcessor);

                StartOperationsProcessWithCorrectRights(new[] { FormattedPath.CreateFormattedPath(Directory.GetParent(destination).FullName) }, action, operationId);

                autoResetEvent.WaitOne();

                _operationProcessors.TryRemove(operationId, out IpcMessageProcessor value);

                return messageProcessor.LatestMessage.ClientCurrentState == ClientState.Finished;
            });
        }


        public async Task<bool> Copy(IEnumerable<FormattedPath> itemsToCopy, FormattedPath destinationFolder, CancellationTokenSource cancellationTokenSource, Action<double, double, string, string> progressReport)
        {
            return await CopyOrMove(itemsToCopy, destinationFolder, cancellationTokenSource, progressReport, false);
        }


        public Task<bool> Move(IEnumerable<FormattedPath> itemsToMove, FormattedPath destinationFolder, CancellationTokenSource cancellationTokenSource, Action<double, double, string, string> progressReport)
        {
            return CopyOrMove(itemsToMove, destinationFolder, cancellationTokenSource, progressReport, true);
        }

        public Task<bool> CopyOrMove(IEnumerable<FormattedPath> itemsToCopy, FormattedPath destinationFolder, CancellationTokenSource cancellationTokenSource, Action<double, double, string,string> progressReport, bool move)
        {
            return Task.Run(() =>
            {
                var operationId = Guid.NewGuid();
                _prepareOperationsQueue.Enqueue(operationId);

                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                Action action = move ? Action.Move : Action.Copy;
                
                var messageProcessor = new IpcMessageProcessor(
                    readyToStartAction: (con, mess) => SendRequest(new IpcMessage()
                    {
                        Action = action,
                        ServerCurrentState = ServerState.InProgress,
                        SourcePaths = itemsToCopy.Select(it => it.Path).ToList(),
                        DestinationPath = destinationFolder.Path,
                        WholeOperationId = operationId
                    }, con),
                    inProgressAction: (con, mess) => { progressReport.Invoke(mess.TotalProgress, mess.PartialProgress, mess.CurrentSpeed, mess.CurrentlyProcessingItem); },
                    errorAction: (con, mess) => { ShowError(con, mess, autoResetEvent); },
                    finishedAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    cancelledAction: (con, mess) =>
                    {
                        Finished(con, mess, autoResetEvent);
                    },
                    fileLockedAction: (con, mess) =>
                    {
                        _dialogHelper.ShowTwoButtonsDialog("Unable to copy " + mess.CurrentlyProcessingItem + " due being used by another progess!",
                            () => SendRequest(mess.ChangeServerState(ServerState.Retry), con),
                            "Try again",
                            cancellationTokenSource);
                    },
                    fileNameTakenAction: (con, mess) => {
                        _dialogHelper.ShowThreeButtonsDialog("Item with the same name " + Path.GetFileName(mess.CurrentlyProcessingItem) + " already exist in this folder!",
                        () => SendRequest(mess.ChangeServerState(ServerState.Overwrite), con),
                        "Overwrite",
                        () => SendRequest(mess.ChangeServerState(ServerState.Skip), con),
                        "Skip",
                        cancellationTokenSource);
                    },
                    cancellationTokenSource: cancellationTokenSource);

                _operationProcessors.GetOrAdd(operationId, messageProcessor);

                if(move)
                {
                    StartMoveOperationsProcessWithCorrectRights(itemsToCopy, destinationFolder, operationId);
                }
                else
                {
                    StartOperationsProcessWithCorrectRights(new[] { destinationFolder }, Action.Copy, operationId);
                }

                autoResetEvent.WaitOne();

                _operationProcessors.TryRemove(operationId, out IpcMessageProcessor value);

                return messageProcessor.LatestMessage.ClientCurrentState == ClientState.Finished;
            });
        }

        private void Finished(NamedPipeConnection<IpcMessage, IpcMessage> con, IpcMessage mess, AutoResetEvent autoResetEvent)
        {
            SendRequest(mess.CreateReadyToCloseMessage(), con);
            autoResetEvent.Set();
        }

        private void ShowError(NamedPipeConnection<IpcMessage, IpcMessage> con, IpcMessage mess, AutoResetEvent autoResetEvent)
        {
            _dialogHelper.ShowError(mess.Error);
            Finished(con, mess, autoResetEvent);
        }

        private void Server_ClientConnected(NamedPipeConnection<IpcMessage, IpcMessage> connection)
        {
            Guid operationId;
            if (_prepareOperationsQueue.TryDequeue(out operationId))
            {
                connection.PushMessage(IpcMessage.CreatePreparingMessage(operationId));
            }
        }

        private void Server_ClientMessage(NamedPipeConnection<IpcMessage, IpcMessage> connection, IpcMessage message)
        {
            if(_operationProcessors.TryGetValue(message.WholeOperationId, out IpcMessageProcessor processor))
            {
                processor.ProcessMessage(message, connection);
            }
        }

        private void Server_Error(Exception exception)
        {
            if(_operationProcessors.Count>0)
            {
                // error is related to some ongoing operation
                _dialogHelper.ShowError("File operations server failed with exception: " + exception.Message);
            }
            Logger.LogError("Pipe server error" + Environment.NewLine + exception.Message);
        }

        private string OperationsExecutablePath
        {
            get
            {
                if(string.IsNullOrEmpty(_operationsExecutablePath))
                {
                    _operationsExecutablePath = Path.Combine(ApplicationInfoProvider.GetApplicationExecutableDirectory(), SharedStrings.FileOperationsExecutableName);
                }
                return _operationsExecutablePath;
            }
        }

        private void SendRequest(IpcMessage ipcMessage, NamedPipeConnection<IpcMessage, IpcMessage> connection)
        {
            connection.PushMessage(ipcMessage);
        }

        private bool StartOperationsProcessWithCorrectRights(IEnumerable<FormattedPath> paths, Action requestedAction, Guid operationid)
        {
            var requiresAdmin = RequiresAdmin(paths, requestedAction);

            return InvokeHelper.StartProcess(OperationsExecutablePath, requiresAdmin, operationid.ToString() + " " + Bootstraper.ApplicationInstanceId);
        }

        private bool StartMoveOperationsProcessWithCorrectRights(IEnumerable<FormattedPath> sourcePaths, FormattedPath destination, Guid operationid)
        {
            var requiresAdmin = RequiresAdmin(sourcePaths, Action.Delete);

            requiresAdmin |= !IOExtensions.AccessRightsChecker.ItemHasPermision(destination.Path, ActionToRights(Action.Copy));

            return InvokeHelper.StartProcess(OperationsExecutablePath, requiresAdmin, operationid.ToString() + " " + Bootstraper.ApplicationInstanceId);
        }

        private bool RequiresAdmin(IEnumerable<FormattedPath> items, Action requestedAction)
        {
            var requiresAdmin = false;
            foreach (var path in items)
            {
                requiresAdmin |= !IOExtensions.AccessRightsChecker.ItemHasPermision(path.Path, ActionToRights(requestedAction));
            }
            return requiresAdmin;
        }

        private System.Security.AccessControl.FileSystemRights ActionToRights(Action action)
        {
            switch (action)
            {
                case Action.Rename:
                    return System.Security.AccessControl.FileSystemRights.Modify;
                case Action.NewTextFile:
                    return System.Security.AccessControl.FileSystemRights.CreateFiles;
                case Action.NewFolder:
                    return System.Security.AccessControl.FileSystemRights.CreateDirectories;
                case Action.Delete:
                    return System.Security.AccessControl.FileSystemRights.Delete;
                case Action.Copy:
                    return System.Security.AccessControl.FileSystemRights.Write;
                    // move is handled elsewhere - combination of delete and write
                    
                    // add other actions
                default:
                    return System.Security.AccessControl.FileSystemRights.FullControl;


            }
        }
    }
}
