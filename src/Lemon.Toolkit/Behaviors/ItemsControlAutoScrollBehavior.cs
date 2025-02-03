using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using System.Collections.Specialized;

namespace Lemon.Toolkit.Behaviors
{
    public class ItemsControlAutoScrollBehavior : Behavior<ItemsControl>
    {
        private ItemsControl? _currentControl;
        private ScrollViewer? _scrollViewer;

        public static readonly StyledProperty<AutoScrollMode> AutoScrollModeProperty =
            AvaloniaProperty.Register<ItemsControlAutoScrollBehavior, AutoScrollMode>(
                nameof(AutoScrollMode),
                AutoScrollMode.ToBottom);
        public AutoScrollMode AutoScrollMode
        {
            get => GetValue(AutoScrollModeProperty);
            set => SetValue(AutoScrollModeProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is not { } itemsControl) return;
            _currentControl = itemsControl;
            _currentControl.ItemsView.CollectionChanged += CollectionChangedHandler;
        }

        private void CollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AutoScrollMode == AutoScrollMode.None) return;

            if(_currentControl is ListBox listBox)
            {
                if(listBox.SelectedIndex > -1)
                {
                    return;
                }
            }

            _scrollViewer ??= _currentControl?.FindDescendantOfType<ScrollViewer>(includeSelf: true);
            if (_scrollViewer == null) return;

            switch (AutoScrollMode)
            {
                case AutoScrollMode.ToBottom:
                    _scrollViewer.ScrollToEnd();
                    break;
                case AutoScrollMode.ToTop:
                    _scrollViewer.ScrollToHome();
                    break;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_currentControl != null)
            {
                _currentControl.ItemsView.CollectionChanged -= CollectionChangedHandler;
            }
        }
    }
    public enum AutoScrollMode
    {
        None,
        ToBottom,
        ToTop
    }
}
