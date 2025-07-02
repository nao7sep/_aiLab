namespace _aiLabApp.Services.Ai
{
    /// <summary>
    /// Represents a text message content for AI chat.
    /// </summary>
    public class AiTextContent : AiChatContent
    {
        public override AiContentType ContentType => AiContentType.Text;
        public string Text { get; }

        public AiTextContent(string text)
        {
            Text = text;
        }
    }
}
