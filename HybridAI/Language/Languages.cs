using System.Collections.Generic;

namespace HybridAI.Language
{
    internal static class Languages
    {
        public static List<string> LanguageFileNames = new()
        {
            "zh-cn.xaml",
            "en-us.xaml"
        };

        public static List<string> LanguageDisplayNames = new()
        {
            "简体中文",
            "English (United States)"
        };

        /// <summary>
        /// https://learn.microsoft.com/zh-cn/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        /// </summary>
        public static List<int> LanguageCodes = new()
        {
            0x0804,
            0x0409
        };
    }
}
