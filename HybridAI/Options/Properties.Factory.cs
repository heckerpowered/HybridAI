using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace HybridAI.Options
{
    public partial class Properties
    {
        private static readonly Lazy<Properties> Lazy = new(LoadProperties);
        private static readonly string OptionFileName = "Properties.json";
        private Properties()
        {
        }
        public static Properties Settings => Lazy.Value;

        private static Properties LoadProperties()
        {
            if (!File.Exists(OptionFileName))
            {
                return new();
            }

            try
            {
                return JsonConvert.DeserializeObject<Properties>(File.ReadAllText(OptionFileName, Encoding.Unicode)) ?? new();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Exception occurred during loading properties");
                Trace.Indent();
                Trace.WriteLine(exception);
                Trace.Unindent();
            }

            return new();
        }

        public static void SaveProperties()
        {
            Trace.TraceInformation("Saving properties");
            using var optionsFileStream = File.Open(OptionFileName, FileMode.Create);
            using var optionsStreamWriter = new StreamWriter(optionsFileStream, Encoding.Unicode);

            optionsStreamWriter.Write(JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }
    }
}
