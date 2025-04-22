using FanCommander;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<FanCommanderSettings>(
    builder.Configuration.GetSection("FanCommanderSettings")
);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
