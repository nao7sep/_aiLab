namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents errors that occur during AI service operations.
    /// </summary>
    public class AiServiceException : Exception
    {
        public AiServiceException() { }
        public AiServiceException(string message) : base(message) { }
        public AiServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
