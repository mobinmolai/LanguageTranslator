using LanguageTranslator.Helper.Multilingual;
using LanguageTranslator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LanguageAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IConfiguration config;

        public EmployeeController(ILogger<EmployeeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            config = configuration;
        }

        [HttpGet]
        public Employee Get()
        {
            Employee employee = new Employee() { Id = 1, Name = "test", Description = "Text to translate" };
            LanguageTranslator<Employee> alertDefinitionsViewTranslator = new LanguageTranslator<Employee>(employee, config, nameof(employee));
            return alertDefinitionsViewTranslator.TranslateAsync(Language.French).Result;
        }
    }
}
