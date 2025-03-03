using MassTransit;
using ML.Infrastructure;
using ML.Infrastructure.Email;
using ML.Service;
using ML.Service.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.AddInfrastructureWorkerServices();

RabbitMqSettings queueSettings = builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>()!;

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<NotificationConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(queueSettings.Host, "/", h =>
        {
            h.Username(queueSettings.Username!);
            h.Password(queueSettings.Password!);
        });

        cfg.ReceiveEndpoint("notification-queue", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
        cfg.UseInstrumentation();
    });
});

builder.Services.AddScoped<NotificationConsumer>();

var host = builder.Build();
host.Run();
