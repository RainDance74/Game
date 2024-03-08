using System.Reflection;

using UserService;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Title = Assembly.GetExecutingAssembly().GetName().Name!;
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        IHost host = builder.Build();
        host.Run();
    }
}