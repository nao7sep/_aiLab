using System.Text;

namespace _aiLabApp.Services
{
    public class Logger : IDisposable
    {
        private readonly string _filePath;
        private StreamWriter? _writer;
        private bool _disposed = false;
        private static readonly object _lock = new object();

        public Logger(string filePath)
        {
            if (!Path.IsPathFullyQualified(filePath))
                throw new ArgumentException("Log file path must be fully qualified.", nameof(filePath));
            _filePath = filePath;
        }

        private void EnsureWriter()
        {
            if (_writer == null)
            {
                var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(stream, new UTF8Encoding(true));
            }
        }

        private void Write(LogMessageType type, string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            var line = $"[{timestamp} {type}] {message}";
            lock (_lock)
            {
                EnsureWriter();
                _writer!.WriteLine(line);
                _writer.Flush();
            }
        }

        public void WriteInfo(string message) => Write(LogMessageType.Info, message);
        public void WriteWarning(string message) => Write(LogMessageType.Warning, message);
        public void WriteError(string message) => Write(LogMessageType.Error, message);

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    _writer?.Dispose();
                    _writer = null;
                    _disposed = true;
                }
            }
        }
    }
}
