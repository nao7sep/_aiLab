namespace _aiLabApp.Services
{
    public enum LogMessageType
    {
        Info,
        Warning,
        Error
    }

    public static class LogMessageTypeExtensions
    {
        public static ConsoleColor GetColor(this LogMessageType type)
        {
            return type switch
            {
                LogMessageType.Info => ConsoleColor.Cyan,
                LogMessageType.Warning => ConsoleColor.Yellow,
                LogMessageType.Error => ConsoleColor.Red,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Invalid LogMessageType: {type}")
            };
        }
    }
}
