using System;
using System.Diagnostics;
namespace OpenTK_PathTracer
{
    class Program
    {
        static void Main()
        {
            try
            {
                using MainWindow mainWindow = new MainWindow();
                mainWindow.Run(Math.Min(OpenTK.DisplayDevice.Default.RefreshRate, 144));
            }
            catch (Exception ex)
            {
                int line = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                string fileName = System.IO.Path.GetFileName(new StackTrace(ex, true).GetFrame(0).GetFileName());
                Console.WriteLine("\n====== Exception ======");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                Console.WriteLine($"Filename: {fileName}");
                Console.WriteLine($"Line: {line}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("==== Enter to Exit ====");
                Console.ReadLine();
            }
        }
    }
}
