using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Lemon.ModuleNavigation.Abstracts;
using Markdig;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Views;

public partial class MarkDownView : UserControl, IView
{
    private readonly StringBuilder _markdownBuffer = new();
    private readonly MarkdownPipeline _pipeline;
    private const int BatchSize = 5;
    private readonly bool usePrism = false;
    private ScrollBar? _scrollViewer;

    public MarkDownView()
    {
        InitializeComponent();

        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            //.UsePrism()
            .Build();
        _htmlPanel.BaseStylesheet = GetBaseStylesheet();
        //_htmlPanel.BaseStylesheet = @"
        //    body { font-family: Arial, sans-serif; margin: 10px; }
        //    pre { background-color: #f5f5f5; padding: 10px; border-radius: 4px; }
        //    code { font-family: Consolas, monospace; }
        //    h1, h2, h3 { color: #333; }
        //    ul, ol { margin-left: 20px; }
        //";

        LoadMarkdownAsync(GetMarkdownStreamAsync());
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        //_scrollViewer ??= _htmlPanel.FindDescendantOfType<ScrollViewer>(includeSelf: true);
    }

    public async Task LoadMarkdownAsync(IAsyncEnumerable<string> markdownStream)
    {
        var batchBuffer = new StringBuilder();
        await foreach (var chunk in markdownStream)
        {
            batchBuffer.Append(chunk);
            if (batchBuffer.Length >= BatchSize)
            {
                _markdownBuffer.Append(batchBuffer.ToString());
                batchBuffer.Clear();
                await UpdateHtmlContent();
                await Task.Delay(5);
            }
        }
        if (batchBuffer.Length > 0)
        {
            _markdownBuffer.Append(batchBuffer.ToString());
            await UpdateHtmlContent();
        }
    }

    private async Task UpdateHtmlContent()
    {
        var markdown = _markdownBuffer.ToString();
        var html = Markdown.ToHtml(markdown, _pipeline);
        
        var wrappedHtml = $@"
            <div class='markdown-body'>
                {html}
            </div>";

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _htmlPanel.Text = wrappedHtml;
            scrollViewer.ScrollToEnd();
            //_scrollViewer ??= _htmlPanel.FindDescendantOfType<ScrollBar>(includeSelf: true);
            //if (_scrollViewer != null)
            //{
            //    if (_scrollViewer.ViewportSize > _htmlPanel..Height)
            //    {
            //        _scrollViewer.Value = _scrollViewer.ViewportSize;
            //    }
            //    else
            //    {
            //        _scrollViewer.Value = _htmlPanel.DesiredSize.Height;
            //    }


            //}
        });
    }


    private string GetBaseStylesheet()
    {
        if (usePrism)
        {
            // Prism.js
            return @"
                body { font-family: Arial, sans-serif; margin: 10px; }
                pre { margin: 0; }
                /* Prism.js VS������ʽ */
                code[class*=""language-""],
                pre[class*=""language-""] {
                    color: #393A34;
                    font-family: Consolas, Monaco, 'Andale Mono', 'Ubuntu Mono', monospace;
                    font-size: 1em;
                    text-align: left;
                    white-space: pre;
                    word-spacing: normal;
                    word-break: normal;
                    word-wrap: normal;
                    line-height: 1.5;
                    padding: 1em;
                    background: #f8f8f8;
                    border: 1px solid #e0e0e0;
                    border-radius: 4px;
                }
                .token.comment { color: #008000; }
                .token.string { color: #A31515; }
                .token.keyword { color: #0000FF; }
                .token.number { color: #098658; }
                .token.function { color: #795E26; }
            ";
        }
        else
        {
            return @"
                body { font-family: Arial, sans-serif; margin: 10px; }
                pre { margin: 0; }
                /* Highlight.js VS������ʽ */
                .hljs {
                    display: block;
                    overflow-x: auto;
                    padding: 1em;
                    background: #f8f8f8;
                    color: #000;
                    border: 1px solid #e0e0e0;
                    border-radius: 4px;
                }
                .hljs-comment { color: #008000; }
                .hljs-string { color: #A31515; }
                .hljs-keyword { color: #0000FF; }
                .hljs-number { color: #098658; }
                .hljs-function { color: #795E26; }
            ";
        }
    }

    public static async IAsyncEnumerable<string> GetMarkdownStreamAsync()
    {
        var markdownContent = @"# Markdown 语法完整测试

## 1. 标题演示

# 一级标题
## 二级标题
### 三级标题
#### 四级标题
##### 五级标题
###### 六级标题

## 2. 文本格式化

这是普通文本

**这是加粗文本**

*这是斜体文本*

***这是加粗斜体文本***

~~这是删除线文本~~

`这是行内代码`

## 3. 引用

> 这是一级引用
>> 这是二级引用
>>> 这是三级引用

## 4. 列表

### 无序列表
- 项目1
- 项目2
  - 子项目2.1
  - 子项目2.2
- 项目3

### 有序列表
1. 第一项
2. 第二项
   1. 子项2.1
   2. 子项2.2
3. 第三项

### 任务列表
- [x] 已完成任务
- [ ] 未完成任务
- [x] 另一个已完成任务

## 5. 代码块

### Python 代码
```python
def hello_world():
    print('Hello, World!')
    for i in range(5):
        print(f'Count: {i}')

# 调用函数
hello_world()
```

### C# 代码
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(""Hello, World!"");
        var numbers = Enumerable.Range(1, 5);
        foreach (var num in numbers)
        {
            Console.WriteLine($""Count: {num}"");
        }
    }
}
```

### JavaScript 代码
```javascript
function helloWorld() {
    console.log('Hello, World!');
    for (let i = 0; i < 5; i++) {
        console.log(`Count: ${i}`);
    }
}

// 调用函数
helloWorld();
```

## 6. 表格

| 表头1 | 表头2 | 表头3 |
|-------|--------|--------|
| 单元格1 | 单元格2 | 单元格3 |
| 单元格4 | 单元格5 | 单元格6 |
| 左对齐 | 居中 | 右对齐 |
| :-- | :--: | --: |

## 7. 链接和图片

[这是一个链接](https://www.example.com)

![这是一个图片](https://via.placeholder.com/150)

## 8. 水平线

---

***

___

## 9. 数学公式

行内公式：$E = mc^2$

独立公式：
$$
\frac{n!}{k!(n-k)!} = \binom{n}{k}
$$

## 10. HTML支持

<div style=""color: blue; padding: 10px; border: 1px solid gray;"">
    这是一个自定义样式的 div
</div>

<details>
<summary>点击展开</summary>
这是展开的内容
</details>

## 11. 脚注

这是一个带有脚注的文本[^1]

[^1]: 这是脚注的内容

## 12. 定义列表

术语 1
: 定义 1

术语 2
: 定义 2a
: 定义 2b

## 13. emoji 表情

:smile: :heart: :thumbsup: :star:

## 14. 特殊字符转义

\*这不是斜体\*
\#这不是标题\#
\[这不是链接\]

## 15. 高亮标记

==这是高亮文本==

## 16. 上标和下标

水的化学式是 H~2~O

二次方表示: X^2^
";

        foreach (var character in markdownContent)
        {
            yield return character.ToString();
            await Task.Delay(5);
        }
    }
}