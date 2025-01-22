using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;

namespace Lemon.Toolkit.Behaviors
{
    public static class HandyWindowBehavior
    {
        private static Window? _targetWindow;
        private static bool _mouseDownForWindowMoving = false;
        private static PointerPoint _originalPoint;

        public static readonly AttachedProperty<bool> IsDragEnabledProperty =
            AvaloniaProperty.RegisterAttached<Window, bool>("IsDragEnabled", typeof(HandyWindowBehavior), false);

        public static bool GetIsDragEnabled(Window window)
        {
            return window.GetValue(IsDragEnabledProperty);
        }
        public static void SetIsDragEnabled(Window window, bool value)
        {
            window.SetValue(IsDragEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsShutdownButtonProperty =
            AvaloniaProperty.RegisterAttached<Button, bool>("IsShutdownButton", typeof(HandyWindowBehavior), false);

        public static bool GetIsShutdownButton(Button button)
        {
            return button.GetValue(IsShutdownButtonProperty);
        }
        public static void SetIsShutdownButton(Button button, bool value)
        {
            button.SetValue(IsShutdownButtonProperty, value);
        }
        static HandyWindowBehavior()
        {
            IsDragEnabledProperty.Changed.AddClassHandler<Window>(OnIsDragEnabledChanged);
            IsShutdownButtonProperty.Changed.AddClassHandler<Button>(OnIsShutdownButtonChanged);
        }

        private static void OnIsShutdownButtonChanged(Button button, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool isShutdownButton)
            {
                if (isShutdownButton)
                {
                    button.Click += Button_Click;

                }
                else
                {
                    button.Click -= Button_Click;
                }
            }
        }

        private static void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var root = button.FindAncestorOfType<Window>();
                root?.Close();
            }
        }

        private static void OnIsDragEnabledChanged(Window window, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool isEnabled)
            {
                if (isEnabled)
                {
                    _targetWindow = window;
                    window.PointerPressed += OnPointerPressed;
                    window.PointerMoved += OnPointerMoved;
                    window.PointerReleased += OnPointerReleased;
                }
                else
                {
                    _targetWindow = null;
                    window.PointerPressed -= OnPointerPressed;
                    window.PointerMoved -= OnPointerMoved;
                    window.PointerReleased -= OnPointerReleased;
                }
            }
        }

        private static void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_mouseDownForWindowMoving) return;
            if (_targetWindow != null)
            {

                PointerPoint currentPoint = e.GetCurrentPoint(_targetWindow);
                _targetWindow.Position = new PixelPoint(_targetWindow.Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
                    _targetWindow.Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
            }
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_targetWindow != null)
            {

                if (_targetWindow.WindowState == WindowState.Maximized 
                    || _targetWindow.WindowState == WindowState.FullScreen) return;

                _mouseDownForWindowMoving = true;
                _originalPoint = e.GetCurrentPoint(_targetWindow);
            }
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _mouseDownForWindowMoving = false;
        }
    }
}
