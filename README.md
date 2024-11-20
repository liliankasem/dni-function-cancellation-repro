# Cancellation Repro

### Client Disconnect

- `dotnet build && dotnet run`
- Invoke `http://localhost:7071/api/MyHttpTrigger`
- Abort the request before the Task delay completes

Logs from core tools:

```log
[2024-11-20T20:32:35.246Z] Host lock lease acquired by instance ID '000000000000000000000000303B82C0'.
[2024-11-20T20:32:40.907Z] Executing 'Functions.MyHttpTrigger' (Reason='This function was programmatically called via the host APIs.', Id=60a4f9c0-7f79-4af4-b897-c06231378b43)
[2024-11-20T20:32:41.036Z] C# HTTP trigger function processed a request.
[2024-11-20T20:32:45.720Z] A cancellation token was received. Taking precautionary actions.
[2024-11-20T20:32:49.022Z] Function 'MyHttpTrigger', Invocation id '60a4f9c0-7f79-4af4-b897-c06231378b43': An exception was thrown by the invocation.
[2024-11-20T20:32:49.022Z] Result: Function 'MyHttpTrigger', Invocation id '60a4f9c0-7f79-4af4-b897-c06231378b43': An exception was thrown by the invocation.
[2024-11-20T20:32:49.022Z] Exception: System.ObjectDisposedException: Request has finished and HttpContext disposed.
[2024-11-20T20:32:49.022Z] Object name: 'HttpContext'.
[2024-11-20T20:32:49.022Z]    at Microsoft.AspNetCore.Http.DefaultHttpContext.ThrowContextDisposed()
[2024-11-20T20:32:49.022Z]    at Microsoft.AspNetCore.Http.DefaultHttpContext.get_Features()
[2024-11-20T20:32:49.022Z]    at Microsoft.AspNetCore.Routing.RoutingHttpContextExtensions.GetRouteData(HttpContext httpContext)
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore.FunctionsHttpProxyingMiddleware.TryHandleHttpResult(Object result, FunctionContext context, HttpContext httpContext, Boolean isInvocationResult) in D:\a\_work\1\s\extensions\Worker.Extensions.Http.AspNetCore\src\FunctionsMiddleware\FunctionsHttpProxyingMiddleware.cs:line 77
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore.FunctionsHttpProxyingMiddleware.Invoke(FunctionContext context, FunctionExecutionDelegate next) in D:\a\_work\1\s\extensions\Worker.Extensions.Http.AspNetCore\src\FunctionsMiddleware\FunctionsHttpProxyingMiddleware.cs:line 56
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.FunctionsApplication.InvokeFunctionAsync(FunctionContext context) in D:\a\_work\1\s\src\DotNetWorker.Core\FunctionsApplication.cs:line 96
[2024-11-20T20:32:49.023Z] Stack:    at Microsoft.AspNetCore.Http.DefaultHttpContext.ThrowContextDisposed()
[2024-11-20T20:32:49.023Z]    at Microsoft.AspNetCore.Http.DefaultHttpContext.get_Features()
[2024-11-20T20:32:49.023Z]    at Microsoft.AspNetCore.Routing.RoutingHttpContextExtensions.GetRouteData(HttpContext httpContext)
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore.FunctionsHttpProxyingMiddleware.TryHandleHttpResult(Object result, FunctionContext context, HttpContext httpContext, Boolean isInvocationResult) in D:\a\_work\1\s\extensions\Worker.Extensions.Http.AspNetCore\src\FunctionsMiddleware\FunctionsHttpProxyingMiddleware.cs:line 77
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore.FunctionsHttpProxyingMiddleware.Invoke(FunctionContext context, FunctionExecutionDelegate next) in D:\a\_work\1\s\extensions\Worker.Extensions.Http.AspNetCore\src\FunctionsMiddleware\FunctionsHttpProxyingMiddleware.cs:line 56
[2024-11-20T20:32:49.023Z]    at Microsoft.Azure.Functions.Worker.FunctionsApplication.InvokeFunctionAsync(FunctionContext context) in D:\a\_work\1\s\src\DotNetWorker.Core\FunctionsApplication.cs:line 96.
[2024-11-20T20:32:49.043Z] Executed 'Functions.MyHttpTrigger' (Failed, Id=60a4f9c0-7f79-4af4-b897-c06231378b43, Duration=8144ms)
[2024-11-20T20:32:49.043Z] System.Private.CoreLib: Exception while executing function: Functions.MyHttpTrigger. Microsoft.Azure.WebJobs.Script.Grpc: Failed to proxy request with ForwarderError: RequestCanceled. System.Net.Http: The operation was canceled. System.Net.Sockets: Unable to read data from the transport connection: Operation canceled. Operation canceled.
```

See [client-disconnect.log](client-disconnect.log) file for output from host debugger.

### Host CTS Cancelled

For this repro you need to make a change to the host code and start the project via the Host debugger.

- Update HandleCancellationTokenParameter to cancel the CTS:

```csharp
// WorkerFunctionInvoker.cs : HandleCancellationTokenParameter()
private CancellationToken HandleCancellationTokenParameter(object input)
{
    if (input == null)
    {
        return CancellationToken.None;
    }

    // return (CancellationToken)input;
    var cts = new CancellationTokenSource();
    cts.CancelAfter(5000);
    return cts.Token;
}
```

- Start the host debugger, with the `AzureWebJobsScriptRoot` set to the root of this repository/project

See [cts-cancelled.log](cts-cancelled.log) file for output from host debugger.