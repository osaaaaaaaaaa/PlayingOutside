using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.HttpGateway.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(option =>
{
    option.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});
builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

var app = builder.Build();

// Swaggerê›íË
app.MapMagicOnionHttpGateway("_", 
    app.Services.GetService < MagicOnion.Server.MagicOnionServiceDefinition>().MethodHandlers,
    GrpcChannel.ForAddress("http://localhost:7000"));
SwaggerOptions options = new SwaggerOptions("MagicOnion", "", "/_/");
options.XmlDocumentPath = Path.Combine(AppContext.BaseDirectory, "Shared.xml");
options.Info.title = "îCà”ÇÃÉ^ÉCÉgÉã";
options.Info.description = "îCà”ÇÃê‡ñæ";
options.Info.version = "1.0.0";

app.MapMagicOnionSwagger("swagger",
    app.Services.GetService<MagicOnion.Server.MagicOnionServiceDefinition>().MethodHandlers,"/_/",options);


app.MapMagicOnionService();
app.Run();