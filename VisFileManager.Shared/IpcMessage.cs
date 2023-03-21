using System;
using System.Collections.Generic;

namespace VisFileManager.Shared
{
    public enum Action { NotSet, NewFolder, NewTextFile, Delete, Move, Copy, Rename }

    public enum ServerState {  Preparing, InProgress, Cancelling, Retry, Skip, Overwrite, ClientCanCloseConnection }

    public enum ClientState { ReadyToStart, InProgress, Cancelled, Finished, Error, FileLocked, FileNameTaken }

    [Serializable()]
    public class IpcMessage
    {
        public Action Action { get; set; }

        public ServerState ServerCurrentState { get; set; }

        public ClientState ClientCurrentState { get; set; }

        public List<string> SourcePaths { get; set; }

        public string DestinationPath { get; set; }

        public string CurrentlyProcessingItem { get; set; }

        public double PartialProgress { get; set; }

        public double TotalProgress { get; set; }

        public string CurrentSpeed { get; set; }

        public Guid WholeOperationId { get; set; }

        public Guid MessageId { get; set; }

        public string Error { get; set; }

        public static IpcMessage CreatePreparingMessage(Guid id)
        {
            return new IpcMessage()
            {
                ServerCurrentState = ServerState.Preparing,
                WholeOperationId = id
            };
        }

    }
}
