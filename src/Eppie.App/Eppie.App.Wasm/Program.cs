using Microsoft.UI.Xaml;
using System;

namespace Eppie.App.Wasm
{
    public sealed class Program
    {
        private static App _app;

        static int Main(string[] args)
        {
            Microsoft.UI.Xaml.Application.Start(_ => _app = new AppHead());

            return 0;
        }
    }
}
