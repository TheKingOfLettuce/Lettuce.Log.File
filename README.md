# Lettuce.Log.File

A logging destination that logs to a file, with optional rolling behavior

[![Build](https://github.com/TheKingOfLettuce/Lettuce.Log.File/actions/workflows/Build.yml/badge.svg)](https://github.com/TheKingOfLettuce/Lettuce.Log.File/actions/workflows/Build.yml)

## Quick Start

`Lettuce.Log.File` is available on [nuget](https://www.nuget.org/packages/Lettuce.Log.File)

```csharp
Log.Logger.AddFileDestination("ABSOLUTE_LOG_PATH/MyLog.log");

Log.Info("This logs an information message");
Log.Error("This logs an error!");

Log.Logger.Dispose(); // ensure file is written to
```

See [Lettuce.Log.Core](https://github.com/TheKingOfLettuce/Lettuce.Log.Core) for how to use the logger.

### File Destination
This `ILogDestination` writes text to a file. By default, the log file does not roll, and will stream text to a single file. 

In order to get file rolling behavior, you must pass how many files to roll and provide the size of the file in kilobytes (defaults to 1024) to reach before rolling.

The following line configures the `FileDestination` to log to a file until it hits 10MB** in size before rolling up to 3 times, with the newest logs being preserved.

_**File size is estimated for performance reasons, actual file size may differ by some bytes_
```csharp
Log.Logger.AddFileDestination("ABSOLUTE_LOG_PATH/MyLog.log", 3, 10240);
```

File names are appended with their roll count. If you logged enough that all files rolled over with the above configuration, your directory will look like this:
```
ABSOLUTE_LOG_PATH
--  MyLog.log
--  MyLog__1.log
--  MyLog__2.log
--  MyLog__3.log
```