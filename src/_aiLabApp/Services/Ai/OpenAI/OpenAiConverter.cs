namespace _aiLabApp.Services.Ai.OpenAI
{
    public class OpenAiConverter : IAiServiceConverter
    {
        public string RoleToString(AiChatRole role)
        {
            return role switch
            {
                AiChatRole.System => "system",
                AiChatRole.User => "user",
                AiChatRole.Assistant => "assistant",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
