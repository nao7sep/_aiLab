namespace _aiLabApp.Services.Ai.Anthropic
{
    public static class AnthropicRoleConverter
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
