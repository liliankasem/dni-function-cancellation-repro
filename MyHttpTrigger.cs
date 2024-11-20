using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyFunctionApp
{
    public class MyHttpTrigger(ILogger<MyHttpTrigger> logger)
    {
        private readonly ILogger<MyHttpTrigger> _logger = logger;

        [Function(nameof(MyHttpTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(10000, cancellationToken);

                req.Query.TryGetValue("name", out var name);

                return string.IsNullOrEmpty(name) ? new OkObjectResult("Welcome to Azure Functions!") : new OkObjectResult($"Welcome to Azure Functions, {name}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("A cancellation token was received. Taking precautionary actions.");

                // Take precautions like noting how far along you are with processing the batch
                await Task.Delay(3000);

                return new ObjectResult("Invocation cancelled") { StatusCode = 503 };
            }
        }
    }
}
