namespace _aiLabApp.Services.Ai.xAI
{
    public static class XaiRoleConverter
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
