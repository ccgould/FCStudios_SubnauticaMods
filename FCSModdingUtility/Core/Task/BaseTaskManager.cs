using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dna;

namespace FCSModdingUtility
{
    /// <summary>
    /// Handles anything to do with Tasks
    /// </summary>
    public class BaseTaskManager : ITaskManager
    {
        #region Task Methods

        public async System.Threading.Tasks.Task Run(Func<System.Threading.Tasks.Task> function, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                await System.Threading.Tasks.Task.Run(function);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async void RunAndForget(Func<System.Threading.Tasks.Task> function, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                await Run(function, origin, filePath, lineNumber);
            }
            catch { }
        }

        public async Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                return await System.Threading.Tasks.Task.Run(function, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async Task<TResult> Run<TResult>(Func<Task<TResult>> function, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                return await System.Threading.Tasks.Task.Run(function);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                return await System.Threading.Tasks.Task.Run(function, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async Task<TResult> Run<TResult>(Func<TResult> function, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                return await System.Threading.Tasks.Task.Run(function);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async System.Threading.Tasks.Task Run(Func<System.Threading.Tasks.Task> function, CancellationToken cancellationToken, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                await System.Threading.Tasks.Task.Run(function, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async void RunAndForget(Func<System.Threading.Tasks.Task> function, CancellationToken cancellationToken, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                await Run(function, origin, filePath, lineNumber);
            }
            catch { }
        }

        public async System.Threading.Tasks.Task Run(Action action, CancellationToken cancellationToken, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                await System.Threading.Tasks.Task.Run(action, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async void RunAndForget(Action action, CancellationToken cancellationToken, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                await Run(action, origin, filePath, lineNumber);
            }
            catch { }
        }

        public async System.Threading.Tasks.Task Run(Action action, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                // Try and run the task
                await System.Threading.Tasks.Task.Run(action);
            }
            catch (Exception ex)
            {
                // Log error
                FrameworkDI.Logger.LogErrorSource(ex.ToString(), origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Throw it as normal
                throw;
            }
        }

        public async void RunAndForget(Action action, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                await Run(action, origin, filePath, lineNumber);
            }
            catch { }
        }

        #endregion
    }
}
