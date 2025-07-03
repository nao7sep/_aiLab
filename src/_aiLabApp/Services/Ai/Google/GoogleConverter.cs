namespace _aiLabApp.Services.Ai.Google
{
    public static class GoogleConverter
    {
        public static string RoleToString(AiChatRole role)
        {
            return role switch
            {
                AiChatRole.System => "system",
                AiChatRole.User => "user",
                AiChatRole.Assistant => "model",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid AI chat role.")
            };
        }
    }
}
