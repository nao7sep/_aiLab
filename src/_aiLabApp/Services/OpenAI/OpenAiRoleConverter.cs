namespace _aiLabApp.Services.OpenAI
{
    public static class OpenAiRoleConverter
    {
        public static string ToProviderRoleString(AiChatRole role)
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
