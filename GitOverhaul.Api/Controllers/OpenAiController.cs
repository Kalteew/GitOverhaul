using GitOverhaul.Api.Tools;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace GitOverhaul.Api.Controllers;

// Ce contrôleur est maintenant inutile : la génération du schéma se fait automatiquement au démarrage
// [ApiController]
// [Route("api/openai")]
// public class OpenAiController(ISwaggerProvider swaggerProvider) : ControllerBase
// {
//     [HttpGet("schema")]
//     public IActionResult GenerateSchema()
//     {
//         GenerateOpenAiSchema.Run(swaggerProvider);
//         return Ok();
//     }
// }