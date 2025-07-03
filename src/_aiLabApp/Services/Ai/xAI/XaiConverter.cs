namespace _aiLabApp.Services.Ai.xAI
{
    public class XaiConverter : IAiServiceConverter
    {
        public string RoleToString(AiChatRole role)
        {
            return role switch
            {
                AiChatRole.System => "system",
                AiChatRole.User => "user",
                AiChatRole.Assistant => "assistant",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid AI chat role.")
            };
        }
    }
}
