
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Minio;

namespace DiplomaGenerator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            // Iniciar o serviço RabbitMQ e processar mensagens da fila
            var processor = host.Services.GetRequiredService<RabbitMQService>();
            processor.StartListening();

            await host.WaitForShutdownAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Adicionar o contexto do banco de dados Postgres
                    services.AddDbContext<DiplomaContext>(options =>
                        options.UseNpgsql("Host=postgres;Database=diploma_db;Username=postgres;Password=postgres"));

                    // Adicionar MinIO client
                    services.AddSingleton(new MinioClient()
                        .WithEndpoint("minio:9000")
                        .WithCredentials("minioadmin", "minioadmin")
                        .Build());


                    // Adicionar serviço RabbitMQ
                    services.AddSingleton<RabbitMQService>();
                    services.AddSingleton<DiplomaProcessor>();
                });
    }
}
