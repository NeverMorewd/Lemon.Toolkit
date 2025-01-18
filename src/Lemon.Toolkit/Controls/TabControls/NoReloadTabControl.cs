using Avalonia.Controls;
using System;

namespace Lemon.Toolkit.Controls.TabControls
{
    public class NoReloadTabControl : TabControl
    {
        protected override Type StyleKeyOverride => typeof(TabControl);
    }
}
