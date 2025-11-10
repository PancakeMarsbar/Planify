using Microsoft.Extensions.Logging;
using Planify.Services;

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


            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>();

            builder.Services.AddSingleton<IUserState, UserState>();
#if DEBUG
            b.Logging.AddDebug();
#endif
            return b.Build();
        }
    }
}
