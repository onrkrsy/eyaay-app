using ChatWithDocuments.Models;
using LangChain.DocumentLoaders;
using LangChain.Providers.OpenAI;
using LangChain.Providers;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LangChain.Databases.Sqlite;
using LangChain.Extensions;
using static LangChain.Chains.Chain;
using LangChain.Chains;
using LangChain.Providers.Anyscale.Predefined;
namespace ChatWithDocuments.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration Configuration;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _logger = logger;
            _environment = environment;
            Configuration = configuration;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }
 
               
        [HttpPost("GetAnswer/{pdfId}")]
        public async Task<string> GetAnswerFromPdf(string pdfId, [FromBody] string question)
        {
            string openAiApiKey = Configuration["OPENAI_API_KEY"];
            var provider = new OpenAiProvider(openAiApiKey);
            var llm = new Gpt35TurboModel(provider); 
            var embeddingModel = new TextEmbeddingV3SmallModel(provider);

            using var vectorDatabase = new SqLiteVectorDatabase(dataSource: $"vectors.db");
            var vectorCollection = await vectorDatabase.GetCollectionAsync(pdfId.Replace("-", ""));

            var promptTemplate =
                @"Use the following pieces of context to answer the question at the end. If the answer is not in context then just say that you don't know, don't try to make up an answer.  Always quote the context in your answer. Always reply in Turkish.
        {context}
        Question: {text}
        Helpful Answer:";

            var chain =
                Set(question)
                | RetrieveSimilarDocuments(vectorCollection, embeddingModel, amount: 3)
                | CombineDocuments(outputKey: "context")
                | Template(promptTemplate)
                | LLM(llm.UseConsoleForDebug());

            var chainAnswer = await chain.RunAsync("text", CancellationToken.None);

            return chainAnswer;
        }
    
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
