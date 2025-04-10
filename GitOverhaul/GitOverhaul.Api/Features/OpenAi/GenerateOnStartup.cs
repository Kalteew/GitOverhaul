namespace GitOverhaul.Api.Features.OpenAi;

public static class GenerateOnStartup
{
    public static void TriggerDelayedSchemaGeneration(this WebApplication app)
    {
        Task.Run(async () =>
        {
            using var client = new HttpClient();
            var url = "http://localhost:8080/openai/schema/generate";

            for (int i = 0; i < 10; i++) // max 10 tentatives
            {
                try
                {
                    var res = await client.GetAsync(url);
                    if (res.IsSuccessStatusCode) return;
                }
                catch { /* ignore */ }

                await Task.Delay(500);
            }
        });
    }
}