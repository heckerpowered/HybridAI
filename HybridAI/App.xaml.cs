using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using HybridAI.AI;

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

            Server.Client.DefaultRequestHeaders.Connection.Add("keep-alive");
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
            Trace.WriteLine(exceptionObject.ToString());
            Trace.Unindent();
            Trace.TraceInformation("Restarting");

            var processFileName = Process.GetCurrentProcess().MainModule?.FileName;
            if (processFileName != null)
            {
                Process.Start(processFileName);
            }

            Environment.FailFast("Unhandled exception", exceptionObject as Exception);
        }
    }
}
