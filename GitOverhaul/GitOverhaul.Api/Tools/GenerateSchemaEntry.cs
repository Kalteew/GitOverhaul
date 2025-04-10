using GitOverhaul.Api.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;

public class GenerateSchemaEntry
{
    public static void Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
            })
            .Build();

        var swagger = host.Services.GetRequiredService<ISwaggerProvider>();
        GenerateOpenAiSchema.Run(swagger);
    }
}