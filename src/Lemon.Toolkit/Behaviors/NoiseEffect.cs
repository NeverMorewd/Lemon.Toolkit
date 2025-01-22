using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace Lemon.Toolkit.Behaviors
{
    public static class NoiseEffect
    {
        // 定义附加属性 IsEnabled
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<Canvas, bool>("IsEnabled", typeof(NoiseEffect), false, false);

        // 定义附加属性 DotSize
        public static readonly AttachedProperty<int> DotSizeProperty =
            AvaloniaProperty.RegisterAttached<Canvas, int>("DotSize", typeof(NoiseEffect), 2, false);

        // 定义附加属性 DotCount
        public static readonly AttachedProperty<int> DotCountProperty =
            AvaloniaProperty.RegisterAttached<Canvas, int>("DotCount", typeof(NoiseEffect), 10000, false);

        // 定义附加属性 UpdateInterval
        public static readonly AttachedProperty<TimeSpan> UpdateIntervalProperty =
            AvaloniaProperty.RegisterAttached<Canvas, TimeSpan>("UpdateInterval", typeof(NoiseEffect), TimeSpan.FromMilliseconds(100));

        static NoiseEffect()
        {
            // 当 IsEnabled 属性变化时，启动或停止噪点效果
            IsEnabledProperty.Changed.Subscribe(OnIsEnabledChanged);
        }

        private static void OnIsEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> args)
        {
            if (args.Sender is Canvas canvas)
            {
                if (args.NewValue.Value)
                {
                    StartNoiseEffect(canvas);
                }
                else
                {
                    StopNoiseEffect(canvas);
                }
            }
        }

        private static void StartNoiseEffect(Canvas canvas)
        {
            var timer = new DispatcherTimer
            {
                Interval = GetUpdateInterval(canvas)
            };
            timer.Tick += (s, e) => DrawNoise(canvas);
            timer.Start();
            canvas.Tag = timer; // 将 timer 存储在 Tag 中以便后续停止
        }

        private static void StopNoiseEffect(Canvas canvas)
        {
            if (canvas.Tag is DispatcherTimer timer)
            {
                timer.Stop();
                canvas.Tag = null;
            }
        }

        private static void DrawNoise(Canvas canvas)
        {
            var random = new Random();
            canvas.Children.Clear();

            int dotSize = GetDotSize(canvas);
            int dotCount = GetDotCount(canvas);

            for (int i = 0; i < dotCount; i++)
            {
                int x = random.Next((int)canvas.Bounds.Width);
                int y = random.Next((int)canvas.Bounds.Height);

                byte gray = (byte)random.Next(256);
                var color = Color.FromRgb(gray, gray, gray);

                var dot = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = dotSize,
                    Height = dotSize,
                    Fill = new SolidColorBrush(color)
                };

                Canvas.SetLeft(dot, x);
                Canvas.SetTop(dot, y);

                canvas.Children.Add(dot);
            }
        }

        // Getter 和 Setter 方法
        public static bool GetIsEnabled(Canvas canvas) => canvas.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(Canvas canvas, bool value) => canvas.SetValue(IsEnabledProperty, value);

        public static int GetDotSize(Canvas canvas) => canvas.GetValue(DotSizeProperty);
        public static void SetDotSize(Canvas canvas, int value) => canvas.SetValue(DotSizeProperty, value);

        public static int GetDotCount(Canvas canvas) => canvas.GetValue(DotCountProperty);
        public static void SetDotCount(Canvas canvas, int value) => canvas.SetValue(DotCountProperty, value);

        public static TimeSpan GetUpdateInterval(Canvas canvas) => canvas.GetValue(UpdateIntervalProperty);
        public static void SetUpdateInterval(Canvas canvas, TimeSpan value) => canvas.SetValue(UpdateIntervalProperty, value);
    }
}
