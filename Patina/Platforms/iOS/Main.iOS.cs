using UIKit;
using Uno.UI.Hosting;

namespace Patina.iOS;

public class EntryPoint
{
    public static void Main(string[] args)
    {
        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseAppleUIKit()
            .Build();

        host.Run();
    }
}
