using StreamArtist.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreamArtist
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.b
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Add global exception handlers.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(OnThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

            System.Windows.Forms.Application.Run(new Main());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            // Log the exception.
            LoggingService.Instance.Log($"Unhandled exception caught: {ex}");

            if (ex?.Message.Contains("obs", StringComparison.CurrentCultureIgnoreCase)==true && ex?.Message.Contains("json", StringComparison.CurrentCultureIgnoreCase)==true)
            {
                LoggingService.Instance.Log("OBS lib suffered unknown failure. Might be ok to continue.");
            }
            else
            {

                // Show a message to the user.
                MessageBox.Show($"An unexpected error occurred: {ex?.Message}\n\nThe application will now close. Please check the logs for more details.", "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // It's generally safest to terminate the application.
                System.Windows.Forms.Application.Exit();
            }
        }
    }
}