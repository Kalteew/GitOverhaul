namespace GitOverhaul.Api.Features.OpenAi;

public static class GenerateOnStartup
{
    public static void TriggerDelayedSchemaGeneration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        Task.Run(async () =>
        {
            await Task.Delay(10_000); // attendre 10s après app.Run pour s'assurer que tout est prêt

            using var client = new HttpClient();
            var url = "http://127.0.0.1:8080/openai/schema";

            try { await client.GetAsync(url); } catch { /* no-op */ }
        });
    }
}