using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Markdig;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace Lemon.Toolkit.Controls
{
    public class MarkdownViewer : UserControl
    {
        private readonly HtmlPanel _htmlPanel;
        private readonly StringBuilder _markdownBuffer = new();
        private readonly MarkdownPipeline _pipeline;

        public MarkdownViewer()
        {
            _htmlPanel = new HtmlPanel
            {
                Background = Brushes.White,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Content = _htmlPanel;

            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public async Task LoadMarkdownAsync(IAsyncEnumerable<string> markdownStream)
        {
            await foreach (var chunk in markdownStream)
            {
                _markdownBuffer.Append(chunk);
                await UpdateHtmlContent();
                await Task.Delay(10);
            }
        }

        private async Task UpdateHtmlContent()
        {
            var html = Markdown.ToHtml(_markdownBuffer.ToString(), _pipeline);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _htmlPanel.Text = html;
            });


        }
    }
}
