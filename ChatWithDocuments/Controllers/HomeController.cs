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
        public async Task<string> GetAnswerFromPdf(string pdfId, [FromBody] QuestionModel questionModel)
        {
            string openAiApiKey = Configuration["OPENAI_API_KEY"];

            var provider = new OpenAiProvider(openAiApiKey);

            OpenAiChatModel chatModel;
            switch (questionModel.Model)
            {
                case "Gpt35Turbo":
                    chatModel = new Gpt35TurboModel(provider);
                    break;
                case "Gpt4":
                    chatModel = new Gpt4Model(provider);
                    break;
                case "Gpt4_32k":
                    chatModel = new Gpt4With32KModel(provider);
                    break;
                case "Gpt35TurboInstruct":
                    chatModel = new Gpt35TurboInstructModel(provider);
                    break;
                case "Gpt4TurboPreview":
                    chatModel = new Gpt4TurboPreviewModel(provider);
                    break;
                case "Gpt4VisionPreview":
                    chatModel = new Gpt4VisionPreviewModel(provider);
                    break;
                case "Gpt4Turbo":
                    chatModel = new Gpt4TurboModel(provider);
                    break;
                default:
                    chatModel = new Gpt35TurboModel(provider);
                    break;
            }

            var llm = chatModel;

            var embeddingModel = new TextEmbeddingV3SmallModel(provider);

            using var vectorDatabase = new SqLiteVectorDatabase(dataSource: $"vectors.db");
            var vectorCollection = await vectorDatabase.GetCollectionAsync(pdfId.Replace("-", ""));

            var promptTemplate =
                @"Use the following pieces of context to answer the question at the end. If the answer is not in context, then just say that you don't know; don't try to make up an answer. Always quote the context in your answer. Always reply in Turkish.


 {context}

Görev:
1. Eðer soruda tarihlerle ilgili bir bilgi isteniyorsa, tarih listesinden ilgili tarihleri bulun ve bu tarihlerin bilgilerini kullanarak yanýt verin.
2. Embedding sonuçlarýndaki ilgili bölümleri kullanarak, soruya ayrýntýlý ve doðru bir yanýt verin.
3. Yanýtýnýzý desteklemek için embedding sonuçlarýndan ve gerekirse tarih listesinden doðrudan alýntý yapýn.

Örnekler:

Örnek 1:
Kullanýcý: Bu dokümanýn ana fikri nedir?
Embedding Sonuçlarý: Bu dokümanýn ana fikri, [embedding sonuçlarýndan ilgili kýsým].
Yanýt: Bu dokümanýn ana fikri, [embedding sonuçlarýndan alýnan ilgili kýsým] gibi konularý ele alarak, [embedding sonuçlarýndan alýntý] sonucuna ulaþmaktýr.

Örnek 2:
Kullanýcý: Yazar, X hakkýnda ne diyor?
Embedding Sonuçlarý: Yazar X hakkýnda [embedding sonuçlarýndan ilgili kýsým].
Yanýt: Yazar, X hakkýnda þunlarý söylüyor: [embedding sonuçlarýndan alýnan ilgili kýsým]. Ayrýca, [embedding sonuçlarýndan alýntý] þeklinde de bir deðerlendirme yapmaktadýr.

Örnek 3:
Kullanýcý: 2024 yýlýnda lisansüstü giriþ sýnavlarý ne zaman?
Tarih Listesi: Lisansüstü Giriþ Sýnavlarý: 18 Temmuz 2024.
Yanýt: 2024 yýlýnda lisansüstü giriþ sýnavlarý 18 Temmuz 2024 tarihinde yapýlacaktýr.

Question: {text}
Helpful Answer:
";

            //var promptTemplate =
            //    @"Use the following pieces of context to answer the question at the end. If the answer is not in context then just say that you don't know, don't try to make up an answer.  Always quote the context in your answer. Always reply in Turkish.
            //{context}
            //Question: {text}
            //Helpful Answer:";

            var chain =
                Set(questionModel.Question)
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
    public class QuestionModel
    {
        public string Question { get; set; }
        public string Model { get; set; }
    }
}
