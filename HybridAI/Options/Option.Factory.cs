using System;
using System.Diagnostics;
using System.IO;

using Newtonsoft.Json;

namespace HybridAI.Options
{
    public partial class Option
    {
        private static readonly Lazy<Option> Lazy = new(LoadOptions);
        private static readonly string OptionFileName = "Options.json";
        private Option()
        {
        }
        public static Option Default => Lazy.Value;

        private static Option LoadOptions()
        {
            if (!File.Exists(OptionFileName))
            {
                return new();
            }

            try
            {
                return JsonConvert.DeserializeObject<Option>(File.ReadAllText(OptionFileName)) ?? new();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Exception occured during loading options");
                Trace.Indent();
                Trace.WriteLine(exception);
                Trace.Unindent();
            }

            return new();
        }

        public static void SaveOptions()
        {
            using var optionsFileStream = File.Open(OptionFileName, FileMode.OpenOrCreate);
            using var optionsStreamWriter = new StreamWriter(optionsFileStream);

            optionsStreamWriter.Write(JsonConvert.SerializeObject(Default, Formatting.Indented));
        }
    }
}
