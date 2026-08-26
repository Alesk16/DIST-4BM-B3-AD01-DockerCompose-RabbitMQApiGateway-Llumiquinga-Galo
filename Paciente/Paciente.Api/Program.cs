
using Paciente.Api.Data;
using Paciente.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Paciente.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Registrar el servicio RabbitMQPublisher para inyección de dependencias
            builder.Services.AddScoped<RabbitMQPublisher>();

            builder.Services.AddDbContext<PacienteDBContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("PacientesConnection")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
