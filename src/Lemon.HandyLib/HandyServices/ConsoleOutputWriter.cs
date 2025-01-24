using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lemon.HandyLib.HandyServices
{
    internal class ConsoleOutputWriter : TextWriter
    {
        private readonly TextWriter consoleOutput;
        private readonly Action<string> outputTextAction;

        internal ConsoleOutputWriter(Action<string> anOutputTextAction)
        {
            consoleOutput = Console.Out;
            outputTextAction = anOutputTextAction;
        }

        public override void WriteLine(string? value)
        {
            value ??= "null";
            consoleOutput.WriteLine(value);
            outputTextAction?.Invoke($"{value}");
        }

        public override void WriteLine(object? value)
        {
            value ??= "null";
            consoleOutput.WriteLine(value);
            outputTextAction?.Invoke($"{value}");
        }

        public override void Write(object? value)
        {
            value ??= "null";
            consoleOutput.WriteLine(value);
            outputTextAction?.Invoke($"{value}");
        }

        public override void Write(string? value)
        {
            value ??= "null";
            consoleOutput.WriteLine(value);
            outputTextAction?.Invoke($"{value}");
        }

        public override Encoding Encoding => consoleOutput.Encoding;
    }
}
