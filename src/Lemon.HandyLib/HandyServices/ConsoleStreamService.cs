using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

public class ConsoleStreamService : IDisposable
{
    private readonly StreamReader _inputReader;
    private readonly TextWriter _originalOutput;
    private readonly TextWriter _originalError;
    private readonly StringWriter _interceptedOutput;
    private readonly StringWriter _interceptedError;
    private readonly ReplaySubject<string> _outputSubject;
    private readonly ReplaySubject<string> _errorSubject;
    private bool _isDisposed;

    public ConsoleStreamService()
    {
        _inputReader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        _originalOutput = Console.Out;
        _originalError = Console.Error;

        _interceptedOutput = new StringWriter();
        _interceptedError = new StringWriter();

        _outputSubject = new ReplaySubject<string>();
        _errorSubject = new ReplaySubject<string>();
        Console.SetOut(_interceptedOutput);
        Console.SetError(_interceptedError);
        Task.Run(InterceptOutputAsync);
        Task.Run(InterceptErrorAsync);
    }
    public IObservable<string?> InputStream => Observable
        .FromAsync(() => _inputReader.ReadLineAsync())
        .Repeat()
        .TakeWhile(line => line != null);

    public IObservable<string> OutputStream => _outputSubject.AsObservable();

    public IObservable<string> ErrorStream => _errorSubject.AsObservable();

    private async Task InterceptOutputAsync()
    {
        while (!_isDisposed)
        {
            var output = _interceptedOutput.ToString();

            if (!string.IsNullOrEmpty(output))
            {
                _outputSubject.OnNext(output);
                _interceptedOutput.GetStringBuilder().Clear();
            }
            await Task.Delay(100);
        }
    }

    private async Task InterceptErrorAsync()
    {
        while (!_isDisposed)
        {
            var error = _interceptedError.ToString();

            if (!string.IsNullOrEmpty(error))
            {
                _errorSubject.OnNext(error);
                _interceptedError.GetStringBuilder().Clear();
            }
            await Task.Delay(100);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);

        _inputReader.Dispose();
        _interceptedOutput.Dispose();
        _interceptedError.Dispose();
        _outputSubject.OnCompleted();
        _errorSubject.OnCompleted();
        _isDisposed = true;
    }
}