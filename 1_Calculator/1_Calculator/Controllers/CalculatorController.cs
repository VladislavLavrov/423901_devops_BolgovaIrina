using _1_Calculator.Data;
using _1_Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace _1_Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;

        public CalculatorController(CalculatorContext context)
        {
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
        public IActionResult Calculate(CalculatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (model.Num1 == null || model.Num2 == null || model.Operation == null)
            {
                model.ErrorMessage = "Заполните все поля.";
                return View("Index", model);
            }

            double result = 0;
            switch (model.Operation.Value)
            {
                case Operation.Add:
                    result = model.Num1.Value + model.Num2.Value;
                    break;
                case Operation.Subtract:
                    result = model.Num1.Value - model.Num2.Value;
                    break;
                case Operation.Multiply:
                    result = model.Num1.Value * model.Num2.Value;
                    break;
                case Operation.Divide:
                    if (model.Num2.Value == 0)
                    {
                        model.ErrorMessage = "Деление на ноль запрещено.";
                        return View("Index", model);
                    }
                    result = model.Num1.Value / model.Num2.Value;
                    break;
            }

            model.Result = result;

            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = model.Num1.Value.ToString(),
                Operand_2 = model.Num2.Value.ToString(),
                Type_operation = model.Operation.Value.ToString()
            };
            _context.DataInputVariants.Add(dataInputVariant);
            _context.SaveChanges();

            return View("Index", model);
        }
    }
}


