
using Application.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController: ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        public IActionResult PublishRabbitMQ(Order order)
        {
            try
            {
                /* código exemplo publicador */

                // cria conexão com RabbitMQ
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = ConnectionFactory.DefaultUser,
                    Password = ConnectionFactory.DefaultPass,
                    Port = AmqpTcpEndpoint.UseDefaultPort
                };

                // cria conexão
                var connection = factory.CreateConnection();

                // cria a canal de comunicação com RabbitMQ
                var channel = connection.CreateModel();


                //Cria a fila caso não exista
                channel.QueueDeclare(queue: "orderQueue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                 // body
                 string message = JsonSerializer.Serialize(order);
                 var body = Encoding.UTF8.GetBytes(message);


                //Envia a mensagem para fila
                channel.BasicPublish(exchange: String.Empty,
                                    routingKey: "orderQueue",
                                    //basicProperties: properties,
                                    basicProperties: null,
                                    body: body);

                /* fim código exemplo publicador */

                return Accepted(order);
            }
            catch(Exception ex)
            {
                _logger.LogError("Errro ao tenter criar um novo pedido.", ex);

                return new StatusCodeResult(500);
            }
        }
    }
}
