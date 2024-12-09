using Lemon.ModuleNavigation.Avaloniaui;
using Lemon.Toolkit.ViewModels;
using Lemon.Toolkit.Views;
using System;

namespace Lemon.Toolkit.Modules
{
    public class FileSimulateModule : AvaModule<FileSimulateView, FileSimulateViewModel>
    {
        public FileSimulateModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool LoadOnDemand => true;
    }
}
