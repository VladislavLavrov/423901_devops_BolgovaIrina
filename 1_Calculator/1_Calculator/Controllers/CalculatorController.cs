using _1_Calculator.Data;
using _1_Calculator.Models;
using _1_Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace _1_Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly KafkaProducerService<Null, string> _producer;

        public CalculatorController(CalculatorContext context, KafkaProducerService<Null, string> producer)
        {
            _producer = producer;
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var model = new CalculatorViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation,
            };
            // Отправка данных в Kafka
            await SendDataToKafka(dataInputVariant);
            // Перенаправление на страницу Index
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Callback([FromBody] DataInputVariant inputData)
        {
            SaveDataAndResult(inputData);
            return Ok();
        }
        private DataInputVariant SaveDataAndResult(DataInputVariant inputData)
        {
            _context.DataInputVariants.Add(inputData);
            _context.SaveChanges();
            return inputData;
        }
        private async Task SendDataToKafka(DataInputVariant dataInputVariant)
        {
            var json = JsonSerializer.Serialize(dataInputVariant);
            await _producer.ProduceAsync("1_Calculator", new Message<Null, string>
            { Value = json });
        }
    }
}


