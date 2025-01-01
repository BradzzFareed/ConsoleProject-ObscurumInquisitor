using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleProject
{
    internal class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;
        static void Main(string[] args)
        {
            try
            {
                IntPtr handle = GetConsoleWindow();
                ShowWindow(handle, SW_MAXIMIZE);
                
                GameMenu consoleProject = new GameMenu();
                consoleProject.StartTitle();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
        
    }
}