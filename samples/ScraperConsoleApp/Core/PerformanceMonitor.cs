using System;
using System.Diagnostics;
using System.IO;

namespace ScraperConsoleApp.Core
{
    public class PerformanceMonitor
    {
        private long _startMemory;
        private long _endMemory;
        private Stopwatch _stopwatch;
        private Process _process;

        public PerformanceMonitor()
        {
            _stopwatch = new Stopwatch();
            _process = Process.GetCurrentProcess();
        }

        public void StartMonitoring()
        {
            _startMemory = GC.GetTotalMemory(true);
            _stopwatch.Restart();
            _process.Refresh(); // Get current process details
        }

        public void StopMonitoring()
        {
            _stopwatch.Stop();
            _endMemory = GC.GetTotalMemory(false);
            _process.Refresh(); // Update process details
        }

        public long GetElapsedTime()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        public long GetMemoryUsed()
        {
            return _endMemory - _startMemory;
        }

        public long GetProcessMemoryUsed()
        {
            return _process.WorkingSet64; 
        }

        public long GetPrivateMemorySize()
        {
            return _process.PrivateMemorySize64; 
        }

        public long GetDiskUsage(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }

        public void DisplayResults(string testName, string filePath)
        {
            Console.WriteLine($"\nPerformance results for {testName}:");
            Console.WriteLine($"Elapsed Time: {_stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Memory Used (Heap): {_endMemory - _startMemory} bytes");
            Console.WriteLine($"Process Memory Used: {GetProcessMemoryUsed()} bytes");
            Console.WriteLine($"Private Memory Size: {GetPrivateMemorySize()} bytes");
            Console.WriteLine($"Disk Usage: {GetDiskUsage(filePath)} bytes (file size)");
        }
    }
}
