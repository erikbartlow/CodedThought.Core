# CodedThought.Core.Transport
## A .NET Core file transport wrapper library

### Installation

1. Reference the package in Nuget.

### Usage

#### WinScp
```c sharp
Currently Support Version:  6.1.1

    WinSCPWrapper ftp = new() {
        FTPHost = settingsManager.SFTPHostAddress,
        FTPPort = settingsManager.SFTPHostPort,
        ProtocolToUse = WinSCP.Protocol.Sftp,
        FTPSSHKeyPath = $"{settingsManager.SFTPSSHKeyLocation}\\{settingsManager.SFTPPrivateKey}",
        FTPUsername = settingsManager.SFTPUsername,
        FTPSSHKeyPassphrase = settingsManager.SFTPPrivateKeyPassphrase,
        FTPSshHostKeyFingerprint = settingsManager.SFTPHostKeyFingerprint
    };
    string fileWithPath = $"{settingsManager.RemoteOutputPath}".Replace("\\\\", "\\");
    string localPath = settingsManager.OutputFilePath;
    ftp.PullFiles(settingsManager.OutputFilePath, settingsManager.RemoteOutputPath);

```