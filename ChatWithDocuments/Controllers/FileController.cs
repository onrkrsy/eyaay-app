using ChatWithDocuments.Models;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.Providers.OpenAI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LangChain.Extensions;

namespace ChatWithDocuments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration Configuration;

        public FileController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            Configuration = configuration;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<FileUploadResponse>> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new FileUploadResponse { Success = false, FileUrl = "fileUrl", Message = "No file uploaded" });
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                string fileGuid = GenerateUniqueString(10);
                string uniqueFileName = fileGuid + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
                await CreateVectorDbFromPdf(fileUrl, fileGuid, uniqueFileName);
                return Ok(new FileUploadResponse { Success = true, FileUrl = fileUrl, Message = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new FileUploadResponse { Success = false, FileUrl = "fileUrl", Message = "An error occurred" });
            }
        }

        [HttpDelete("delete/{fileGuid}")]
        public IActionResult DeleteFile(string fileGuid)
        {
            string uploadsFolderPath = Path.Combine(_environment.WebRootPath, "uploads");
            string[] files = Directory.GetFiles(uploadsFolderPath);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                int underscoreIndex = fileName.IndexOf('_');
                if (underscoreIndex > -1)
                {
                    string guidPart = fileName.Substring(0, underscoreIndex);
                    if (guidPart == fileGuid)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            return Ok();
        }
        [HttpGet("GetUploadedFiles")]
        public async Task<IActionResult> GetUploadedFiles()
        {
            string uploadsFolderPath = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolderPath))
            {
                return null;
            }

            var files = Directory.GetFiles(uploadsFolderPath);
            var fileInfos = new List<FileInfo>();

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                int underscoreIndex = fileName.IndexOf('_');

                if (underscoreIndex > -1)
                {
                    string guidPart = fileName.Substring(0, underscoreIndex);
                    string realFileNamePart = fileName.Substring(underscoreIndex + 1);

                    fileInfos.Add(new FileInfo
                    {
                        Guid = guidPart,
                        FileName = realFileNamePart
                    });

                }
            }

            return Ok(fileInfos);
        }
        public class FileUploadResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string FileUrl { get; set; }
        }

        private static string GenerateUniqueString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Shuffle the characters and take the first 'length' characters
            string shuffledChars = new string(chars.OrderBy(c => random.Next()).ToArray());
            return new string(shuffledChars.Take(length).ToArray());
        }
          public class FileInfo
        {
            public string Guid { get; set; }
            public string FileName { get; set; }
        }

        public async Task<string> CreateVectorDbFromPdf(string pdfUrl, string fileGuid, string fileName)
        {
            string openAiApiKey = Configuration["OPENAI_API_KEY"]; 
            var provider = new OpenAiProvider(openAiApiKey);
            var embeddingModel = new TextEmbeddingV3SmallModel(provider);

            string pdfId = fileGuid.Replace("-", "");

            using var vectorDatabase = new SqLiteVectorDatabase(dataSource: $"vectors.db");
         
            await vectorDatabase.AddDocumentsFromAsync<PdfPigPdfLoader>(
                embeddingModel,
                dimensions: 1536,
                dataSource: DataSource.FromUrl(pdfUrl),
                collectionName: pdfId,
                textSplitter: null);

            return fileName;
        }
    }
}
