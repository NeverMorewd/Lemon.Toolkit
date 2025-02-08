using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.HandyLib.Toolkits
{
    public static class FileWatcher
    {
        public static IObservable<string> Watch(string filePath, bool seekEnd = true)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            return Observable.Create<string>(observer =>
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamReader = new StreamReader(fileStream, Encoding.UTF8);

                if (seekEnd)
                {
                    streamReader.BaseStream.Seek(0, SeekOrigin.End);
                }

                var watcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(filePath)!,
                    Filter = Path.GetFileName(filePath),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                var subject = new Subject<string>();

                var watcherSubscription = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => { watcher.Changed += h; watcher.Created += h; }, // 监听 `Changed` 和 `Created`
                    h => { watcher.Changed -= h; watcher.Created -= h; })
                    .Throttle(TimeSpan.FromMilliseconds(300)) // 防止触发太频繁
                    .Select(_ => ReadNewLines(streamReader))
                    .Where(content => !string.IsNullOrEmpty(content))
                    .Subscribe(subject.OnNext, observer.OnError);

                // 轮询方式确保获取变更
                //var pollingSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
                //    .Select(_ => new FileInfo(filePath).LastWriteTime)
                //    .DistinctUntilChanged() // 只在文件变更时触发
                //    .Select(_ => ReadNewLines(streamReader))
                //    .Where(content => !string.IsNullOrEmpty(content))
                //    .Subscribe(subject.OnNext, observer.OnError);

                // 初始读取内容
                var initialContent = ReadNewLines(streamReader);
                if (!string.IsNullOrEmpty(initialContent))
                {
                    observer.OnNext(initialContent);
                }

                var subjectSubscription = subject.Subscribe(observer);

                return Disposable.Create(() =>
                {
                    watcherSubscription.Dispose();
                    //pollingSubscription.Dispose();
                    subjectSubscription.Dispose();
                    subject.Dispose();
                    watcher.Dispose();
                    streamReader.Dispose();
                    fileStream.Dispose();
                });
            })
            .ObserveOn(System.Reactive.Concurrency.Scheduler.Default);
        }

        private static string ReadNewLines(StreamReader reader)
        {
            var newContent = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (line == null) break;
                newContent.AppendLine(line);
            }

            return newContent.ToString();
        }
        public static IObservable<string> TailLogFileRx(string filePath)
        {
            return Observable.Create<string>(observer =>
            {
                var subject = new Subject<string>();
                var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath)!)
                {
                    Filter = Path.GetFileName(filePath),
                    EnableRaisingEvents = true
                };

                Task.Run(async () =>
                {
                    // 等待文件存在
                    while (!File.Exists(filePath))
                    {
                        observer.OnNext("等待日志文件创建...");
                        await Task.Delay(1000);
                    }

                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fs, Encoding.UTF8);

                    fs.Seek(0, SeekOrigin.End); // 定位到文件末尾

                    while (!subject.IsDisposed)
                    {
                        string? line = await reader.ReadLineAsync();
                        if (line != null)
                        {
                            observer.OnNext(line);
                        }
                        else
                        {
                            await Task.Delay(100);
                        }
                    }
                });

                // 释放资源
                return () =>
                {
                    subject.Dispose();
                    watcher.Dispose();
                };
            });
        }
        public static async Task TailLogFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("文件不存在，等待创建...");
                using var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath)!);
                watcher.Filter = Path.GetFileName(filePath);
                watcher.EnableRaisingEvents = true;
                var tcs = new TaskCompletionSource<bool>();
                watcher.Created += (s, e) => tcs.TrySetResult(true);
                await tcs.Task;
            }

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            fs.Seek(0, SeekOrigin.End);

            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line != null)
                {
                    Console.WriteLine(line);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
        /// <summary>
        /// https://stackoverflow.com/a/64038750/11043472
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="requiredConsecutiveNul"></param>
        /// <returns>true:binary;false:plaintext</returns>
        public static bool IsBinary(string filePath, int requiredConsecutiveNul = 1)
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';

            int nulCount = 0;

            using var streamReader = new StreamReader(filePath);
            for (var i = 0; i < charsToCheck; i++)
            {
                if (streamReader.EndOfStream)
                    return false;

                if ((char)streamReader.Read() == nulChar)
                {
                    nulCount++;

                    if (nulCount >= requiredConsecutiveNul)
                        return true;
                }
                else
                {
                    nulCount = 0;
                }
            }

            return false;
        }


    }
}
