using Microsoft.Extensions.Logging;
using Planify.Services;
using CommunityToolkit.Maui;

namespace Planify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var b = MauiApp.CreateBuilder();
            b.UseMauiApp<App>().UseMauiCommunityToolkit()
             .ConfigureFonts(f =>
             {
                 f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                 f.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
             });


            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>();

#if DEBUG
            b.Logging.AddDebug();
#endif
            return b.Build();
        }
    }
}
