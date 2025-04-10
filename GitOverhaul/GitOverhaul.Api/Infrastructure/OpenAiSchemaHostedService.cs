using GitOverhaul.Api.Tools;

namespace GitOverhaul.Api.Infrastructure;

public class OpenAiSchemaHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public OpenAiSchemaHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        GenerateOpenAiSchema.Run(scope.ServiceProvider);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}