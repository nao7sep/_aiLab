namespace _aiLabApp.Services
{
    public class ConsoleWriter
    {
        public void WriteInfo(string message)
        {
            Write(LogMessageType.Info, message);
        }

        public void WriteWarning(string message)
        {
            Write(LogMessageType.Warning, message);
        }

        public void WriteError(string message)
        {
            Write(LogMessageType.Error, message);
        }

        private void Write(LogMessageType type, string message)
        {
            Console.ForegroundColor = type.GetColor();
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
