using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GmailTidy.Core
{
    /// <summary>
    /// Provides helper methods to create and manage scheduled tasks via Windows Task Scheduler.
    /// </summary>
    public static class Scheduler
    {
        /// <summary>
        /// Creates a daily scheduled task that runs the specified executable at the given time.
        /// </summary>
        /// <param name="taskName">The name of the scheduled task.</param>
        /// <param name="time">The start time in HH:mm format (24-hour clock).</param>
        /// <param name="exePath">The full path to the executable to run.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task CreateDailyTaskAsync(string taskName, string time, string exePath)
        {
            if (string.IsNullOrWhiteSpace(taskName)) throw new ArgumentException("Task name cannot be null or empty", nameof(taskName));
            if (string.IsNullOrWhiteSpace(time)) throw new ArgumentException("Time cannot be null or empty", nameof(time));
            if (string.IsNullOrWhiteSpace(exePath)) throw new ArgumentException("Executable path cannot be null or empty", nameof(exePath));

            // Ensure the executable path is quoted for schtasks.
            string quotedExe = $"\"{exePath}\"";
            string args = $"/Create /SC DAILY /TN \"{taskName}\" /TR {quotedExe} /ST {time} /F";
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            return RunProcessAsync(psi);
        }

        /// <summary>
        /// Deletes a scheduled task with the specified name.
        /// </summary>
        /// <param name="taskName">The name of the task to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task DeleteTaskAsync(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName)) throw new ArgumentException("Task name cannot be null or empty", nameof(taskName));
            var args = $"/Delete /TN \"{taskName}\" /F";
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            return RunProcessAsync(psi);
        }

        private static Task RunProcessAsync(ProcessStartInfo startInfo)
        {
            var tcs = new TaskCompletionSource<bool>();
            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            process.Exited += (s, e) =>
            {
                process.Dispose();
                tcs.TrySetResult(true);
            };
            process.Start();
            return tcs.Task;
        }
    }
}
