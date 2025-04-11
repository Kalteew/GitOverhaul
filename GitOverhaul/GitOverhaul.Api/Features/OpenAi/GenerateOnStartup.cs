namespace GitOverhaul.Api.Features.OpenAi;

public static class GenerateOnStartup
{
    public static void TriggerDelayedSchemaGeneration(this WebApplication app)
    {
        Task.Run(async () =>
        {
            await Task.Delay(10_000); // attendre 10s après app.Run pour s'assurer que tout est prêt

            using var client = new HttpClient();
            var url = "http://localhost:8080/openai/schema";

            try { await client.GetAsync(url); } catch { /* no-op */ }
        });
    }
}