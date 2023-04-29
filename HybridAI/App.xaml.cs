using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HybridAI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Current.DispatcherUnhandledException += DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;

            Directory.CreateDirectory("Logs");

            Trace.AutoFlush = true;
            Trace.Listeners.Add(
                new TextWriterTraceListener(
                    File.CreateText(
                        Path.ChangeExtension(
                            Path.Combine("Logs", DateTime.Now.ToString("yyyy M H-m-s")), "txt"))));

            Trace.Listeners.Add(new ConsoleTraceListener());

            Trace.TraceInformation("App launching");
        }

        private static void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
        }

        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException(e.ExceptionObject);
        }

        private static new void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
        }

        private static void UnhandledException(object exceptionObject)
        {
            Trace.TraceError("Unhandled exception occured:");
            Trace.Indent();
            Trace.WriteLine(exceptionObject);
            Trace.Unindent();

            var restart = true;

            // To prevent infinite restarts due to unexpected situations,
            // the application will not be restarted if it crashes too many times in a short time.
            var elapsedCrashTime = DateTime.Now - Options.Properties.Settings.LastCrashTime;
            if (elapsedCrashTime.TotalSeconds <= 60)
            {
                Trace.TraceWarning("Detects too many crashes in a short time and will not restart");
                Trace.TraceWarning($"It's been {elapsedCrashTime} since the last crash");
                restart = false;
            }

            try
            {
                Options.Properties.Settings.LastCrashTime = DateTime.Now;
                Options.Properties.SaveProperties();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Failed to save options, cannot record crash time");
                Trace.Indent();
                Trace.WriteLine(exception);
            }

            var processFileName = Process.GetCurrentProcess().MainModule?.FileName;
            if (restart && processFileName != null)
            {
                Trace.TraceInformation("Restarting");
                Process.Start(processFileName);
            }

            if (processFileName == null)
            {
                Trace.TraceWarning("Unable to restart, main module is null");
            }

            Environment.FailFast("Unhandled exception", exceptionObject as Exception);
        }
    }
}
