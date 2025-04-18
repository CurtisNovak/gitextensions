using System.Collections.Concurrent;
using System.Text;
using GitExtensions.Extensibility;
using GitUI;
using NUnit.Framework;

namespace CommonTestUtils
{
    public sealed class MockExecutable : IExecutable
    {
        private readonly ConcurrentDictionary<string, ConcurrentStack<(string output, int? exitCode, string? error)>> _outputStackByArguments = new();
        private readonly ConcurrentDictionary<string, int> _commandArgumentsSet = new();
        private readonly List<MockProcess> _processes = [];
        private int _nextCommandId;

        public string Command => "mock-git.exe";
        public string WorkingDir => ".";
        public string PrefixArguments => "";

        public IDisposable StageOutput(string arguments, string output, int? exitCode = 0, string? error = null)
        {
            ConcurrentStack<(string output, int? exitCode, string? error)> stack = _outputStackByArguments.GetOrAdd(
                arguments,
                args => new ConcurrentStack<(string output, int? exitCode, string? error)>());

            stack.Push((output, exitCode, error));

            return new DelegateDisposable(
                () =>
                {
                    if (_outputStackByArguments.TryGetValue(arguments, out ConcurrentStack<(string output, int? exitCode, string? error)> queue) &&
                        queue.TryPeek(out (string output, int? exitCode, string? error) item) &&
                        output == item.output && error == item.error)
                    {
                        throw new AssertionException($"Staged output should have been consumed.\nArguments: {arguments}\nOutput: {output}\nError: {error}");
                    }
                });
        }

        public IDisposable StageCommand(string arguments)
        {
            int id = Interlocked.Increment(ref _nextCommandId);
            _commandArgumentsSet[arguments] = id;

            return new DelegateDisposable(
                () =>
                {
                    if (_commandArgumentsSet.TryGetValue(arguments, out int storedId) && storedId != id)
                    {
                        throw new AssertionException($"Staged command should have been consumed.\nArguments: {arguments}");
                    }
                });
        }

        public bool Exists()
        {
            return true;
        }

        public void Verify()
        {
            ClassicAssert.IsEmpty(_outputStackByArguments, "All staged output should have been consumed.");
            ClassicAssert.IsEmpty(_commandArgumentsSet, "All staged output should have been consumed.");

            foreach (MockProcess process in _processes)
            {
                process.Verify();
            }
        }

        public IProcess Start(
            ArgumentString arguments,
            bool createWindow,
            bool redirectInput,
            bool redirectOutput,
            Encoding outputEncoding,
            bool useShellExecute = false,
            bool throwOnErrorExit = true,
            CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"mock-git {arguments}");

            if (_outputStackByArguments.TryRemove(arguments, out ConcurrentStack<(string output, int? exitCode, string? error)> queue) &&
                queue.TryPop(out (string output, int? exitCode, string? error) item))
            {
                if (queue.Count == 0)
                {
                    _outputStackByArguments.TryRemove(arguments, out _);
                }

                MockProcess process = new(item.output, item.exitCode, item.error);

                _processes.Add(process);
                return process;
            }

            if (_commandArgumentsSet.TryRemove(arguments, out _))
            {
                MockProcess process = new();
                _processes.Add(process);
                return process;
            }

            throw new Exception("Unexpected arguments: " + arguments);
        }

        public string GetWorkingDirectory()
        {
            return WorkingDir;
        }

        private sealed class MockProcess : IProcess
        {
            public MockProcess(string? output, int? exitCode = 0, string? error = null)
            {
                StandardOutput = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(output ?? "")));
                StandardError = error ?? "";
                StandardInput = new StreamWriter(new MemoryStream());
                _exitCode = exitCode;
            }

            public MockProcess()
            {
                StandardOutput = new StreamReader(new MemoryStream());
                StandardError = "";
                StandardInput = new StreamWriter(new MemoryStream());
                _exitCode = 0;
            }

            private int? _exitCode;
            public StreamWriter StandardInput { get; }
            public StreamReader StandardOutput { get; }
            public string StandardError { get; }

            public void Kill(bool entireProcessTree)
            {
            }

            public int WaitForExit()
            {
                return ThreadHelper.JoinableTaskFactory.Run(WaitForExitAsync);
            }

            public Task<int> WaitForExitAsync()
            {
                if (_exitCode.HasValue)
                {
                    return Task.FromResult(_exitCode.Value);
                }
                else
                {
                    CancellationTokenSource cts = new();
                    CancellationToken ct = cts.Token;
#pragma warning disable VSTHRD103 // Cancel synchronously blocks. Await CancelAsync instead.
                    cts.Cancel();
#pragma warning restore VSTHRD103
                    return Task.FromCanceled<int>(ct);
                }
            }

            public Task<int> WaitForExitAsync(CancellationToken token)
            {
                return Task.FromResult(0);
            }

            public void WaitForInputIdle()
            {
                // TODO implement if needed
            }

            public void Dispose()
            {
                // TODO implement if needed
            }

            public void Verify()
            {
                // all output should have been read
                ClassicAssert.AreEqual(StandardOutput.BaseStream.Length, StandardOutput.BaseStream.Position);

                // Only verify if std input is not closed.
                // ExecutableExtensions.ExecuteAsync will close std input when writeInput action is specified
                if (StandardInput.BaseStream is not null && StandardInput.BaseStream.CanRead)
                {
                    // no input should have been written (yet)
                    ClassicAssert.AreEqual(0, StandardInput.BaseStream.Length);
                }
            }
        }

        private sealed class DelegateDisposable : IDisposable
        {
            private readonly Action _disposeAction;

            public DelegateDisposable(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                _disposeAction();
            }
        }
    }
}
