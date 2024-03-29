﻿using System;
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
                StackFrame frame = new StackTrace(ex, true).GetFrame(0);
                Console.WriteLine("\n====== Exception ======");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                Console.WriteLine($"Filename: {System.IO.Path.GetFileName(frame.GetFileName())}");
                Console.WriteLine($"Line: {frame.GetFileLineNumber()}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("===== Enter to exit =====");
                Console.ReadLine();
            }
        }
    }
}
