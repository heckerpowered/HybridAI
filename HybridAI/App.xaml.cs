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
            Directory.CreateDirectory("CrashReports");

            var fileName = Path.ChangeExtension(Path.Combine("CrashReports", DateTime.Now.ToString("yyyy M H-m-s")), "txt");
            using var streamWriter = File.CreateText(fileName);

            streamWriter.Write(exceptionObject.ToString());
            streamWriter.Flush();

            var processFileName = Process.GetCurrentProcess().MainModule?.FileName;
            if (processFileName != null)
            {
                Process.Start(processFileName);
            }
            Environment.Exit(-1);
        }
    }
}
