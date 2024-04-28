using Aspire.Hosting.Dapr;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var stateStore = builder.AddDaprStateStore("statestore");
var pubSub = builder.AddDaprPubSub("pubsub");

builder.AddProject<Projects.AspireDaprDemo_AliceService>("alice")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        // Loads the resiliency policy for service invocation from alice to bob
        ResourcesPaths = [Path.Combine("..", "resources")],
    })
    .WithReference(stateStore)
    .WithReference(pubSub);

builder.AddProject<Projects.AspireDaprDemo_BobService>("bob")
    .WithDaprSidecar()
    .WithReference(stateStore)
    .WithReference(pubSub);

builder.AddNpmApp("carol", Path.Combine("..", "AspireDaprDemo.CarolService"), "watch")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithEnvironment("NODE_TLS_REJECT_UNAUTHORIZED", builder.Environment.IsDevelopment() ? "0" : "1")
    .WithDaprSidecar()
    .WithReference(stateStore)
    .WithReference(pubSub);

builder.Build().Run();
