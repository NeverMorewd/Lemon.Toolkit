using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
namespace Lemon.HandyLib.HandyServices;
public class ConsoleStreamService : IDisposable
{
    private readonly StreamReader _inputReader;
    private readonly TextWriter _originalOutput;
    private readonly TextWriter _originalError;
    private readonly ReplaySubject<string> _outputSubject;
    private readonly ReplaySubject<string> _errorSubject;
    private bool _isDisposed;
    private readonly ConsoleOutputWriter _consoleOutputWriter;

    public ConsoleStreamService()
    {
        _inputReader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        _originalOutput = Console.Out;
        _originalError = Console.Error;

        _outputSubject = new ReplaySubject<string>();
        _errorSubject = new ReplaySubject<string>();

        _consoleOutputWriter = new ConsoleOutputWriter(line => 
        {
            _outputSubject.OnNext(line);
        });
        Console.SetOut(_consoleOutputWriter);
        Console.SetError(_consoleOutputWriter);
    }
    public IObservable<string?> InputStream => Observable
        .FromAsync(() => _inputReader.ReadLineAsync())
        .Repeat()
        .TakeWhile(line => line != null);

    public IObservable<string> OutputStream => _outputSubject.AsObservable();

    public IObservable<string> ErrorStream => _errorSubject.AsObservable();


    public void Dispose()
    {
        if (_isDisposed) return;

        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);

        _inputReader.Dispose();
        _outputSubject.OnCompleted();
        _errorSubject.OnCompleted();
        _isDisposed = true;
    }
}