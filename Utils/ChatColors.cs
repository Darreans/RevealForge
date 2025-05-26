namespace RevealForge.Utils
{
    public static class ChatColors
    {
        public const string TextHex = "#FFFFFF";
        public const string AdminNameHex = "#2ECC40";
        public const string CommandHex = "#FFDC00";

        private static string Format(string message, string colorHex)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(colorHex) || !colorHex.StartsWith("#") || !((colorHex.Length == 4) || (colorHex.Length == 7) || (colorHex.Length == 5) || (colorHex.Length == 9)))
            {
                return message;
            }
            return $"<color={colorHex}>{message}</color>";
        }

        public static string FormatText(string message) => Format(message, TextHex);
        public static string FormatAdminName(string message) => Format(message, AdminNameHex);
        public static string FormatCommand(string message) => Format(message, CommandHex);
    }
}