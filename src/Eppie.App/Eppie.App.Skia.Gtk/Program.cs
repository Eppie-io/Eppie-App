using GLib;
using System;
using Uno.UI.Runtime.Skia;

namespace Eppie.App.Skia.Gtk
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExceptionManager.UnhandledException += delegate (UnhandledExceptionArgs expArgs)
            {
                Console.WriteLine("GLIB UNHANDLED EXCEPTION" + expArgs.ExceptionObject.ToString());
                expArgs.ExitApplication = true;
            };

            var host = new GtkHost(() => new AppHead(), args);

            host.Run();
        }
    }
}