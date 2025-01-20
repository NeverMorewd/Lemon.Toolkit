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
    private readonly Subject<string> _outputSubject;
    private readonly Subject<string> _errorSubject;
    private bool _isDisposed;

    public ConsoleStreamService()
    {
        // 读取控制台输入流
        _inputReader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);

        // 保存原始的 Console.Out 和 Console.Error
        _originalOutput = Console.Out;
        _originalError = Console.Error;

        // 创建 StringWriter 来拦截输出和错误
        _interceptedOutput = new StringWriter();
        _interceptedError = new StringWriter();

        // 创建 Subject 来发布拦截到的输出和错误
        _outputSubject = new Subject<string>();
        _errorSubject = new Subject<string>();

        // 重定向 Console.Out 和 Console.Error 到自定义的 TextWriter
        Console.SetOut(_interceptedOutput);
        Console.SetError(_interceptedError);

        // 启动任务来监听拦截的输出和错误
        Task.Run(InterceptOutputAsync);
        Task.Run(InterceptErrorAsync);
    }

    /// <summary>
    /// 订阅控制台输入流
    /// </summary>
    public IObservable<string?> InputStream => Observable
        .FromAsync(() => _inputReader.ReadLineAsync())
        .Repeat()
        .TakeWhile(line => line != null);

    /// <summary>
    /// 订阅拦截到的标准输出流
    /// </summary>
    public IObservable<string> OutputStream => _outputSubject.AsObservable();

    /// <summary>
    /// 订阅拦截到的标准错误流
    /// </summary>
    public IObservable<string> ErrorStream => _errorSubject.AsObservable();

    private async Task InterceptOutputAsync()
    {
        while (!_isDisposed)
        {
            // 读取拦截到的输出
            var output = _interceptedOutput.ToString();

            if (!string.IsNullOrEmpty(output))
            {
                // 发布输出
                _outputSubject.OnNext(output);

                // 清空 StringWriter
                _interceptedOutput.GetStringBuilder().Clear();
            }

            // 等待一段时间再检查
            await Task.Delay(100);
        }
    }

    private async Task InterceptErrorAsync()
    {
        while (!_isDisposed)
        {
            // 读取拦截到的错误
            var error = _interceptedError.ToString();

            if (!string.IsNullOrEmpty(error))
            {
                // 发布错误
                _errorSubject.OnNext(error);

                // 清空 StringWriter
                _interceptedError.GetStringBuilder().Clear();
            }

            // 等待一段时间再检查
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// 释放资源并恢复原始输出流和错误流
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        // 恢复原始的 Console.Out 和 Console.Error
        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);

        // 释放资源
        _inputReader.Dispose();
        _interceptedOutput.Dispose();
        _interceptedError.Dispose();
        _outputSubject.OnCompleted();
        _errorSubject.OnCompleted();
        _isDisposed = true;
    }
}