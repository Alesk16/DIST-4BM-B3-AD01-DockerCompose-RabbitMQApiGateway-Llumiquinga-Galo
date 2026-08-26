using HistorialClinico.Api.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HistorialClinico.Api.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQConsumer(
            IConfiguration configuration,
            ILogger<RabbitMQConsumer> logger,
            IServiceScopeFactory scopeFactory
            )
        {
            _configuration = configuration;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var queueName = _configuration["RabbitMQ:QueueName"]!;

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensaje = Encoding.UTF8.GetString(body);

                var evento = JsonSerializer.Deserialize<PacienteCreadoEvento>(mensaje);

                if (evento != null)
                {
                    _logger.LogInformation(
                        "Paciente creado recibido. IdPaciente: {IdPaciente}",
                        evento.IdPaciente
                    );

                    // NOTA: a diferencia de Inventario (que crea una fila de stock automática
                    // por cada libro), aqui NO se crea un historial vacio, porque un historial
                    // clinico necesita Diagnostico/Tratamiento/Fecha reales de una consulta.
                    // Este consumer solo registra que el paciente existe; sirve como paso
                    // previo si luego quieren validar el IdPaciente antes de un POST de historial.

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<Data.HistorialClinicoDBContext>();

                    // Ejemplo opcional de validacion/cacheo -- descomentar si lo necesitan:
                    // var existePaciente = await dbContext.PacientesValidos
                    //     .AnyAsync(p => p.IdPaciente == evento.IdPaciente);
                }

                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
