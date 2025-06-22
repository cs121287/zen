using System;
using System.Windows.Forms;

namespace ZenGardenGenerator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Configure application settings for modern Windows Forms
                ApplicationConfiguration.Initialize();
                
                // Set up global exception handlers
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
                // Run the main form
                using var mainForm = new ZenGardenForm();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal application error: {ex.Message}\n\nDetails: {ex}", 
                    "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled thread exception: {e.Exception.Message}\n\nDetails: {e.Exception}", 
                "Thread Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Unhandled domain exception: {ex.Message}\n\nDetails: {ex}", 
                    "Domain Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}