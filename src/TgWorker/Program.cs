using System.Reflection;


internal class Program
{
    private static void Main(string[] args)
    {
        Console.Title = Assembly.GetExecutingAssembly().GetName().Name!;
        CreateHostBuilder(args)
            .Build()
            .Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((hostContext, services) =>
        {
            // TODO: Configure services
        });

        return builder;
    }
}