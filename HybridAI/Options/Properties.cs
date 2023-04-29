using System;

namespace HybridAI.Options
{
    public partial class Properties
    {
        public DateTime LastCrashTime { get; set; }

        public bool ExplicitEncryptChatHistory { get; set; }
        public bool SavePassword { get; set; }
        public int LanguageIndex { get; set; }
        public bool AutoCheckUpdate { get; set; }
    }
}
