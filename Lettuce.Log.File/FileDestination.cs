using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lettuce.Log.Core;
using System;
using System.IO;

namespace Lettuce.Log.File {

    /// <summary>
    /// Logs messages to a file, with the option of rolling behavior at certain file sizes <br/>
    /// Flushing is NOT automatic, dispose of Logger before shutdown to ensure logs are written
    /// </summary>
    public sealed class FileDestination : ILogDestination, IDisposable {
        private readonly string _filePath;
        private readonly string _dirPath;
        private readonly uint _rollingSize;
        private readonly uint _rollingCount;
        private readonly uint _checkRollingSize;

        private FileStream _file;
        private StreamWriter _fileWriter;
        private uint _estimatedSize;
        private bool _disposed;

        /// <summary>
        /// Constructs the file writer
        /// </summary>
        /// <param name="absoluteFilePath">the absolute file path to log to</param>
        /// <param name="rollingCount">the number of log files to roll, set to 0 for no rolling, defaults to 0</param>
        /// <param name="rollingSizeKB">the size in kilobytes before rolling file, used only if <paramref name="rollingCount"/> is > 0, defaults to 1024</param>
        /// <exception cref="ArgumentException">throws if absolute file path if null, or if rolling settings are mis-configured</exception>
        public FileDestination(string absoluteFilePath, uint rollingCount = 0, uint rollingSizeKB = 1024) {
            if (string.IsNullOrEmpty(absoluteFilePath)) {
                throw new ArgumentException("File path is null", nameof(absoluteFilePath));
            }
            _dirPath = Path.GetDirectoryName(absoluteFilePath) ?? throw new ArgumentException("Provided path has no directory info", nameof(absoluteFilePath));

            _filePath = absoluteFilePath;
            if (rollingCount > 0 && rollingSizeKB == 0) {
                throw new ArgumentException("Rolling size must be non-zero if rolling count is non-zero", nameof(rollingSizeKB));
            }

            _rollingCount = rollingCount;
            _rollingSize = rollingSizeKB * 1024;
            _checkRollingSize = _rollingSize >> 1 >> 1; // divide by 4

            InitFileStreams();
        }

        /// <summary>
        /// Logs a message to the underlying file
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="level">the logging level of the message</param>
        public void LogMessage(string message, LogEventLevel level) {
            if (_disposed)
                throw new ObjectDisposedException("File destination has been disposed");
            _fileWriter.WriteLine(message);
            _fileWriter.Flush();

            if (_rollingCount > 0) {
                CheckFileRolling(message);
            }
        }

        /// <summary>
        /// Disposes of the underlying file streams, ensuring writes are fully flushed
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _fileWriter?.Flush();
            _file?.Flush(true);
            if (_file != null) {
                CheckFileRolling();
            }
            _fileWriter?.Dispose();
            _file?.Dispose();
            _disposed = true;
        }

        #if NET5_0_OR_GREATER
        [MemberNotNull(nameof(_file))]
        [MemberNotNull(nameof(_fileWriter))]
        #endif
        private void InitFileStreams() {
            if (!Directory.Exists(_dirPath)) {
                Directory.CreateDirectory(_dirPath);
            }
            _file = System.IO.File.Open(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _fileWriter = new StreamWriter(_file, Encoding.UTF8);
        }

        private void CheckFileRolling(string message) {
            _estimatedSize += (uint)Encoding.UTF8.GetByteCount(message) + (uint)Environment.NewLine.Length;
            if (_estimatedSize < _checkRollingSize) {
                return;
            }

            _file.Flush(true);
            CheckFileRolling();
            _estimatedSize = 0;
        }

        private void CheckFileRolling() {
            if (_file.Length >= _rollingSize) {
                RollFile();
            }
        }

        private void RollFile() {
            _fileWriter.Dispose();
            _file.Dispose();

            RollFileRecurse(_filePath, GetRollingFileName(this, 1), 1);
            InitFileStreams();
        }

        private void RollFileRecurse(string fileA, string fileB, int currentCount) {
            if (currentCount < _rollingCount && System.IO.File.Exists(fileB)) {
                RollFileRecurse(fileB, GetRollingFileName(this, ++currentCount), currentCount);
            }

            #if NET5_0_OR_GREATER
            System.IO.File.Move(fileA, fileB, true);
            #else
            System.IO.File.Copy(fileA, fileB, true);
            System.IO.File.Delete(fileA);
            #endif
        }

        private static string GetRollingFileName(FileDestination file, int rollCount) {
            string newFileName = Path.GetFileNameWithoutExtension(file._filePath) + $"__{rollCount}" + Path.GetExtension(file._filePath);
            return Path.Combine(file._dirPath, newFileName);
        }
    }

    /// <summary>
    /// Extension class to add a <see cref="FileDestination"/> to a <see cref="Logger"/>
    /// </summary>
    public static class FileExtension {

        /// <summary>
        /// Adds a <see cref="FileDestination"/> fluently to a <see cref="Logger"/>
        /// </summary>
        /// <param name="logger">the logger to add to</param>
        /// <param name="absoluteFilePath">the absolute file path to log to</param>
        /// <param name="rollingCount">the number of log files to roll, set to 0 for no rolling, defaults to 0</param>
        /// <param name="rollingSizeKB">the size in kilobytes before rolling file, used only if <paramref name="rollingCount"/> is > 0, defaults to 1024</param>
        /// <returns>the logger with the <see cref="FileDestination"/> added to it</returns>
        /// <seealso cref="FileDestination(string, uint, uint)"/>
        public static Logger AddFileDestination(this Logger logger, string absoluteFilePath, uint rollingCount = 0, uint rollingSizeKB = 1024) {
            logger.AddDestination(new FileDestination(absoluteFilePath, rollingCount, rollingSizeKB));
            return logger;
        }
    }
}