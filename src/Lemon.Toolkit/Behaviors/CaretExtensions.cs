using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Lemon.Toolkit.Behaviors
{


    public static class CaretExtensions
    {
        // 定义附加属性
        public static readonly AttachedProperty<bool> IsCaretBlinkingProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "IsCaretBlinking",
                typeof(CaretExtensions));

        static CaretExtensions()
        {
            // 监听属性变化
            IsCaretBlinkingProperty.Changed.AddClassHandler<Control>(OnIsCaretBlinkingChanged);
        }

        // 获取附加属性值
        public static bool GetIsCaretBlinking(Control control)
        {
            return control.GetValue(IsCaretBlinkingProperty);
        }

        // 设置附加属性值
        public static void SetIsCaretBlinking(Control control, bool value)
        {
            control.SetValue(IsCaretBlinkingProperty, value);
        }

        // 属性变化时的处理逻辑
        private static void OnIsCaretBlinkingChanged(Control control, AvaloniaPropertyChangedEventArgs args)
        {
            if (control is TextBox textBox && args.NewValue is bool isBlinking)
            {
                if (isBlinking)
                {
                    // 启动闪烁动画
                    var animation = new Animation
                    {
                        Duration = TimeSpan.FromSeconds(1),
                        IterationCount = IterationCount.Infinite,
                        Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0),
                            Setters = { new Setter(TextBox.CaretBrushProperty, Brushes.Lime) }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(0.5),
                            Setters = { new Setter(TextBox.CaretBrushProperty, Brushes.Transparent) }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1),
                            Setters = { new Setter(TextBox.CaretBrushProperty, Brushes.Lime) }
                        }
                    }
                    };
                    animation.RunAsync(textBox);
                }
                else
                {
                    // 停止闪烁动画
                    textBox.ClearValue(TextBox.CaretBrushProperty);
                }
            }
        }
    }
}
