namespace _aiLabApp.Services.Ai.Google
{
    public static class GoogleRoleConverter
    {
        public static string ToProviderRoleString(AiChatRole role)
        {
            // Gemini/Vertex AI uses "user" and "model" (assistant)
            return role switch
            {
                AiChatRole.System => "system", // Some APIs may ignore or treat as special
                AiChatRole.User => "user",
                AiChatRole.Assistant => "model",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
