using System.Diagnostics;

namespace GitExtensions.Extensibility;

/// <summary>
/// Defines a process instance.
/// </summary>
/// <remarks>
/// This process will either be running or exited.
/// </remarks>
public interface IProcess : IDisposable
{
    /// <summary>
    /// Gets an object that facilitates writing to the process's standard input stream.
    /// </summary>
    /// <remarks>
    /// To access the underlying <see cref="Stream"/>, dereference <see cref="StreamWriter.BaseStream"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">This process's input was not redirected
    /// when calling <see cref="IExecutable.Start"/>.</exception>
    StreamWriter StandardInput { get; }

    /// <summary>
    /// Gets an object that facilitates writing to the process's standard output stream.
    /// </summary>
    /// <remarks>
    /// To access the underlying <see cref="Stream"/>, dereference <see cref="StreamWriter.BaseStream"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">This process's output was not redirected
    /// when calling <see cref="IExecutable.Start"/>.</exception>
    StreamReader StandardOutput { get; }

    /// <summary>
    /// Gets the redirected output from the process's standard error stream.
    /// </summary>
    /// <exception cref="InvalidOperationException">This process's output was not redirected
    /// when calling <see cref="IExecutable.Start"/>.</exception>
    string StandardError { get; }

    /// <summary>
    /// Kill the process at once.
    /// </summary>
    /// <param name="entireProcessTree">Specifies whether to kill all child processes, too.</param>
    void Kill(bool entireProcessTree = false);

    /// <summary>
    /// Blocks the calling thread until the process exits, or when this object is disposed.
    /// </summary>
    /// <returns>The process's exit code, or <c>null</c> if this object was disposed before the process exited.</returns>
    int WaitForExit();

    /// <summary>
    /// Returns a task that completes when the process exits, or when this object is disposed.
    /// </summary>
    /// <returns>A task that yields the process's exit code, or throws an exception if this object was disposed before the process exited.</returns>
    Task<int> WaitForExitAsync();

    /// <summary>
    /// Returns a cancellable task that completes when the process exits, or when this object is disposed.
    /// </summary>
    /// <returns>A task that yields the process's exit code, or throws an exception if this object was disposed before the process exited.</returns>
    Task<int> WaitForExitAsync(CancellationToken token);

    /// <summary>
    /// Waits for the process to reach an idle state.
    /// </summary>
    /// <see cref="Process.WaitForInputIdle()"/>
    void WaitForInputIdle();
}
