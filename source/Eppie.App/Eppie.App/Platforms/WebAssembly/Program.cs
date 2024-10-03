namespace Eppie.App
{
    public class Program
    {
        private static Eppie.App.Shared.App _app;

        public static int Main(string[] args)
        {
            Microsoft.UI.Xaml.Application.Start(_ => _app = new Eppie.App.Shared.App());

            return 0;
        }
    }
}
