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

G�rev:
1. E�er soruda tarihlerle ilgili bir bilgi isteniyorsa, tarih listesinden ilgili tarihleri bulun ve bu tarihlerin bilgilerini kullanarak yan�t verin.
2. Embedding sonu�lar�ndaki ilgili b�l�mleri kullanarak, soruya ayr�nt�l� ve do�ru bir yan�t verin.
3. Yan�t�n�z� desteklemek i�in embedding sonu�lar�ndan ve gerekirse tarih listesinden do�rudan al�nt� yap�n.

�rnekler:

�rnek 1:
Kullan�c�: Bu dok�man�n ana fikri nedir?
Embedding Sonu�lar�: Bu dok�man�n ana fikri, [embedding sonu�lar�ndan ilgili k�s�m].
Yan�t: Bu dok�man�n ana fikri, [embedding sonu�lar�ndan al�nan ilgili k�s�m] gibi konular� ele alarak, [embedding sonu�lar�ndan al�nt�] sonucuna ula�makt�r.

�rnek 2:
Kullan�c�: Yazar, X hakk�nda ne diyor?
Embedding Sonu�lar�: Yazar X hakk�nda [embedding sonu�lar�ndan ilgili k�s�m].
Yan�t: Yazar, X hakk�nda �unlar� s�yl�yor: [embedding sonu�lar�ndan al�nan ilgili k�s�m]. Ayr�ca, [embedding sonu�lar�ndan al�nt�] �eklinde de bir de�erlendirme yapmaktad�r.

�rnek 3:
Kullan�c�: 2024 y�l�nda lisans�st� giri� s�navlar� ne zaman?
Tarih Listesi: Lisans�st� Giri� S�navlar�: 18 Temmuz 2024.
Yan�t: 2024 y�l�nda lisans�st� giri� s�navlar� 18 Temmuz 2024 tarihinde yap�lacakt�r.

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
