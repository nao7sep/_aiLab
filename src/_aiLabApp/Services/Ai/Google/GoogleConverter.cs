namespace _aiLabApp.Services.Ai.Google
{
    public class GoogleConverter : IAiServiceConverter
    {
        public string RoleToString(AiChatRole role)
        {
            return role switch
            {
                AiChatRole.System => "system",
                AiChatRole.User => "user",
                AiChatRole.Assistant => "model",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
