using System.Text.RegularExpressions;
using WinSCP;

namespace CodedThought.Core.Transport {

    public class WinSCPWrapper {

        public delegate void LogEventHandler(object sender, LogEventArgs e);

        public event LogEventHandler LogEvent;

        public string FTPHost { get; set; }
        public int FTPPort { get; set; }
        public string FTPSSHKeyPath { get; set; }
        public string FTPSSHKeyPassphrase { get; set; }
        public string FTPUsername { get; set; }
        public string FTPPassword { get; set; }
        public string FTPSshHostKeyFingerprint { get; set; }
        public bool PreserveTimestamp { get; set; }
        public Protocol ProtocolToUse { get; set; }
        public OverwriteMode OverwriteMode { get; set; }

        public WinSCPWrapper() {
            ProtocolToUse = Protocol.Sftp;
            PreserveTimestamp = true;
        }

        public bool TestConnection() {
            SessionOptions sessionOptions = SetUpSessionOptions();

            using Session session = new();
            session.ExecutablePath = GetWinSCPExecutable();
            session.Open(sessionOptions);
            return session.Opened;
        }

        public SessionOptions SetUpSessionOptions() {
            SessionOptions sessionOptions = new() {
                Protocol = ProtocolToUse,
                HostName = FTPHost,
                PortNumber = FTPPort,
                SshHostKeyFingerprint = FTPSshHostKeyFingerprint
            };
            if (FTPSshHostKeyFingerprint == String.Empty) {
                sessionOptions.SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny;
            } else {
                sessionOptions.SshHostKeyFingerprint = FTPSshHostKeyFingerprint;
            }

            if (FTPSSHKeyPath != string.Empty && FTPSSHKeyPath != null) {
                sessionOptions.SshPrivateKeyPath = FTPSSHKeyPath;
                sessionOptions.PrivateKeyPassphrase = FTPSSHKeyPassphrase;
                // Typically in an SSH connection the username is required.
                if (FTPUsername != "")
                    sessionOptions.UserName = FTPUsername;
            } else {
                sessionOptions.UserName = FTPUsername;
                sessionOptions.Password = FTPPassword;
            }
            return sessionOptions;
        }

        /// <summary>Gets the RemoteDirectoryInfo object for the remote path. This allows for better file iterations.</summary>
        /// <param name="RemotePath"></param>
        /// <returns></returns>
        public RemoteDirectoryInfo GetDirectoryInfo(String RemotePath) {
            try {
                SessionOptions sessionOptions = SetUpSessionOptions();

                using Session session = new();
                session.ExecutablePath = GetWinSCPExecutable();
                session.Open(sessionOptions);
                return session.ListDirectory(RemotePath);
            } catch (Exception) {
                throw;
            }
        }

        public void PutFiles(String localPathWithFilename, String RemotePath) {
            try {
                string fileName = System.IO.Path.GetFileName(localPathWithFilename);
                LogEvent?.Invoke(this, new LogEventArgs("PutFiles", $"Connecting to {ProtocolToUse.ToString().ToUpper()} on {FTPHost}", LogEventArgs.LogLevel.INFO));
                SessionOptions sessionOptions = SetUpSessionOptions();

                using Session session = new();
                session.ExecutablePath = GetWinSCPExecutable();
                session.Open(sessionOptions);
                LogEvent?.Invoke(this, new LogEventArgs("PutFiles", $"Connected to {FTPHost}.", LogEventArgs.LogLevel.DEBUG));

                TransferOptions transferOptions = new() {
                    TransferMode = TransferMode.Binary,
                    PreserveTimestamp = this.PreserveTimestamp,
                    OverwriteMode = this.OverwriteMode
                };
                transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
                LogEvent?.Invoke(this, new LogEventArgs("PutFiles", $"Transferring file, {fileName}.", LogEventArgs.LogLevel.DEBUG));
                TransferOperationResult transferResult = session.PutFiles(localPathWithFilename, RemotePath, false, transferOptions);
                transferResult.Check();
                foreach (TransferEventArgs transfer in transferResult.Transfers.Cast<TransferEventArgs>()) {
                    LogEvent?.Invoke(this, new LogEventArgs("PutFiles", $"Upload of {fileName} succeeded.", LogEventArgs.LogLevel.INFO));
                }
            } catch {
                throw;
            }
        }

        public string? GetWinSCPExecutable() {
            return !File.Exists("./WinSCP.exe") ? null : "./WinSCP.exe";
        }

        public void PullFiles(string localPath, string RemotePath) {
            try {
                LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Connecting to {ProtocolToUse.ToString().ToUpper()} on {FTPHost}", LogEventArgs.LogLevel.INFO));
                SessionOptions sessionOptions = SetUpSessionOptions();

                using Session session = new();
                session.ExecutablePath = GetWinSCPExecutable();
                session.Open(sessionOptions);
                LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Connected to {FTPHost}.", LogEventArgs.LogLevel.DEBUG));
                TransferOptions transferOptions = new() {
                    TransferMode = TransferMode.Binary,
                    PreserveTimestamp = this.PreserveTimestamp,
                    OverwriteMode = this.OverwriteMode
                };
                transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
                LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Transferring files.", LogEventArgs.LogLevel.INFO));
                TransferOperationResult transferResult = session.GetFiles(RemotePath, localPath, false, transferOptions);
                transferResult.Check();
                foreach (TransferEventArgs transfer in transferResult.Transfers.Cast<TransferEventArgs>()) {
                    LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Download of  {transfer.FileName} succeeded.", LogEventArgs.LogLevel.INFO));
                }
            } catch (SessionRemoteException srex) {
                throw srex;
            } catch {
                throw;
            }
        }

        public void RemoveFiles(string RemotePath, string fileMask) {
            LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Connecting to {ProtocolToUse.ToString().ToUpper()} on {FTPHost}", LogEventArgs.LogLevel.INFO));
            SessionOptions sessionOptions = SetUpSessionOptions();

            using Session session = new();
            session.ExecutablePath = GetWinSCPExecutable();
            session.Open(sessionOptions);
            LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Connected to {FTPHost}.", LogEventArgs.LogLevel.INFO));
            TransferOptions transferOptions = new() {
                TransferMode = TransferMode.Binary,
                PreserveTimestamp = this.PreserveTimestamp,
                OverwriteMode = this.OverwriteMode
            };
            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
            LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Transferring files.", LogEventArgs.LogLevel.DEBUG));
            RemovalOperationResult operationResult = session.RemoveFiles(RemotePath + "/" + fileMask);
            operationResult.Check();
            foreach (TransferEventArgs transfer in operationResult.Removals.Cast<TransferEventArgs>()) {
                LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Removal of  {transfer.FileName} succeeded.", LogEventArgs.LogLevel.INFO));
            }
        }

        public void RemoveFiles(string RemotePath, List<string> filesToRemove) {
            LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Connecting to {ProtocolToUse.ToString().ToUpper()} on {FTPHost}", LogEventArgs.LogLevel.INFO));
            SessionOptions sessionOptions = SetUpSessionOptions();

            using Session session = new();
            session.ExecutablePath = GetWinSCPExecutable();
            session.Open(sessionOptions);
            LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Connected to {FTPHost}.", LogEventArgs.LogLevel.DEBUG));
            TransferOptions transferOptions = new() {
                TransferMode = TransferMode.Binary,
                PreserveTimestamp = this.PreserveTimestamp,
                OverwriteMode = this.OverwriteMode
            };
            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
            LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Removing {filesToRemove.Count} files.", LogEventArgs.LogLevel.DEBUG));
            foreach (string fileName in filesToRemove) {
                RemovalOperationResult operationResult = session.RemoveFiles(RemotePath + "/" + fileName);
                operationResult.Check();
                foreach (TransferEventArgs transfer in operationResult.Removals.Cast<TransferEventArgs>()) {
                    LogEvent?.Invoke(this, new LogEventArgs("RemoveFiles", $"Removal of  {transfer.FileName} succeeded.", LogEventArgs.LogLevel.INFO));
                }
            }
        }
        /// <summary>
        /// Gets a list of directories from the remote server.
        /// </summary>
        /// <param name="RemotePath"></param>
        /// <param name="regexPattern"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public List<string> GetRemoteDirectories(string RemotePath, string? regexPattern, bool recursive = false) {
            try {
                LogEvent?.Invoke(this, new LogEventArgs("PullFiles", $"Connecting to {ProtocolToUse.ToString().ToUpper()} on {FTPHost}", LogEventArgs.LogLevel.INFO));
                SessionOptions sessionOptions = SetUpSessionOptions();

                using Session session = new();
                session.ExecutablePath = GetWinSCPExecutable();
                session.Open(sessionOptions);
                LogEvent?.Invoke(this, new LogEventArgs("GetRemoteDirectories", $"Connected to {FTPHost}.", LogEventArgs.LogLevel.DEBUG));
                TransferOptions transferOptions = new() {
                    TransferMode = TransferMode.Binary,
                    PreserveTimestamp = this.PreserveTimestamp,
                    OverwriteMode = this.OverwriteMode
                };
                transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
                LogEvent?.Invoke(this, new LogEventArgs("GetRemoteDirectories", $"Retrieving remote directories.", LogEventArgs.LogLevel.INFO));
                RemoteDirectoryInfo transferResult = session.ListDirectory(RemotePath);
                List<string> directories = new();
                // Determine if we should use a regex pattern to find specific directories.
                if (regexPattern != null) {
                    var r = new Regex(regexPattern);
                    foreach (var directory in transferResult.Files.Where(x => x.IsDirectory).Where(x => r.IsMatch(x.Name))) {
                        if (directory.Name != "." && directory.Name != "..") {
                            directories.Add(directory.Name);
                        }
                    }
                } else {
                    foreach (var directory in transferResult.Files.Where(x => x.IsDirectory)) {
                        if (directory.Name != "." && directory.Name != "..") {
                            directories.Add(directory.Name);
                        }
                    }

                }
                return directories;
            } catch (Exception) {

                throw;
            }
        }
    }
}