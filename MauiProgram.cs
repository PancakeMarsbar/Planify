using Microsoft.Extensions.Logging;

namespace Planify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var b = MauiApp.CreateBuilder();
            b.UseMauiApp<App>()
             .ConfigureFonts(f =>
             {
                 f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                 f.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
             });

#if DEBUG
            b.Logging.AddDebug();
#endif
            return b.Build();
        }
    }
}
