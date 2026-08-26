using HistorialClinico.Api.Data;
using HistorialClinico.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HistorialClinico.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHostedService<RabbitMQConsumer>();

            builder.Services.AddDbContext<HistorialClinicoDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("HistorialClinicoConnection")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
