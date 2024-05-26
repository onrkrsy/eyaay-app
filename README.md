# eyaay: LangChain .NET ve OpenAI API ile PDF'lerle Sohbet (RAG with .Net Core )
Bu proje, LangChain .NET kütüphanesi ve OpenAI API kullanılarak geliştirilmiş bir .NET Core uygulamasıdır. Kullanıcıların PDF dosyalarını yükleyip bu dosyaların içeriği hakkında sorular sormasına olanak tanır. Cevaplar, dokümanın içeriğine dayanarak ve Türkçe olarak sağlanır.


![image](https://github.com/onrkrsy/eyaay-app/assets/11960564/82681fe5-bdd4-4c3a-9c1a-05a74cc21a19)

## Amaç

Bu proje, araştırma amaçlı olarak geliştirilmiştir ve .NET ile Retrieval-Augmented Generation (RAG) örneği oluşturmak için kullanılmıştır. RAG, bellekten bilgi getirme ve bu bilgileri doğal dilde yanıt üretmek için kullanma yöntemidir. Bu projede, RAG metodolojisi ile PDF dosyalarındaki bilgileri alıp kullanıcı sorularına anlamlı ve doğru yanıtlar üretilmiştir.

## Kullanılan Teknolojiler

- **TextEmbeddingV3SmallModel**: Metin Embeding modeli, PDF içeriğini anlamlandırmak için kullanılmıştır.
- **Gpt35TurboModel**: Soru-cevap işlemlerini gerçekleştirmek için kullanılmıştır.
- **LangChain .NET**: En sık kullanılan LangChain kütüphanesinin C# implemantasyonudur. 
- **OpenAI API**: TextEmbeddingV3Small ve GPT-3.5 modelini kullanmak için OpenAI'nin API'si kullanılmıştır.

## Özellikler

- **PDF Dosyası Yükleme:** Sol üst menüdeki "Dosya yükleme" seçeneği ile PDF dosyalarını yükleyin.
- **Yüklenen Dosyaları Listeleme ve Kaldırma:** Yüklenen dosyalar sol tarafta listelenir.
- **Doküman Seçme:** Listeden soru sorulacak dokümanı seçin.
- **Sorular Sorun:** Mesajlaşma alanını kullanarak seçilen doküman hakkında sorular sorun.
- **Cevapları Alın:** Seçilen dokümanın içeriğine dayanarak Türkçe cevaplar alın.

### Gereksinimler

- Makinenizde yüklü .NET Core SDK
- Bir OpenAI API anahtarı

### Kurulum

1. **Depoyu klonlayın:**

   ```sh
   git clone [https://github.com/kullanici-adi/eyaay.git](https://github.com/onrkrsy/eyaay-app.git)
   cd eyaay
   ```

2. **OpenAI API anahtarını ayarlayın:**

   `appsettings.json` dosyasını açın ve OpenAI API anahtarınızı ekleyin:

   ```json
   {
       "OPENAI_API_KEY": "YOUR_OPENAI_API_KEY"
   }
   ```

3. **Projeyi derleyin ve çalıştırın:**

   ```sh
   dotnet build
   dotnet run
   ```

### Kullanım

1. Web tarayıcınızdan uygulamaya gidin.
2. Sol üst menüdeki "Dosya yükleme" seçeneğini kullanarak bir PDF dosyası yükleyin.
3. Sol taraftaki listeden yüklenen dokümanı seçin.
4. Mesajlaşma alanında sorularınızı sorun.
5. Seçilen dokümanın içeriğine dayanarak Türkçe cevaplar alın.
 
