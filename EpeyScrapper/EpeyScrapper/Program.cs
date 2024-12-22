using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
     
        string cookie = "_gid=GA1.2.1893669592.1734558928; renkler=; PHPSESSID=ece4f8a58fd6ad0060caacd9555ce68d; _gat=1; cf_clearance=1ClbLpfcZmsqzS1EfQYwHwpZMUqGlFFW8GJoEDwWki8-1734656408-1.2.1.1-qxAll5i1Mlx_LCQzWImUIP8WnD20E.cMIOQ_pkI.1LlO1aYky758iE7fWmYP9bQlytINzpJkTY36pfQwgXQyJhoRJIFemafx.7B6GGWYZOrdQ6ujC1G37n3wxMbYW3Ree4FGEvKI9nhgLW0iigNLHzLW34taAmdXz7a70Fvi1zjnrkWzb1Ex1nZ0wAD48955aTiLwr9iE3QAcBjoN4uH1LM97LNr99usWm.fYymloW8tJ07_foACAqHgMFdrMzjkOWSFCNm_oHxBuREk8sW7iZ0V9lHAvWI105MT_8anb502GeNUQ2_dyUnVP1s_yyQC0IY5K2HdAYaSItBsw9WkZ4iBxk2H_95V8rh.jsdC_c0CwhkYOng3fl8nlhk4blyM7sf3ICnHU8qS_m5cGFZ9JrfNtnemIxh2yszs5o46Cg0B1QJLDxj4h14AFgkmwJ9j; _ga=GA1.1.1930074104.1734558928; __gads=ID=9f9ebf79f1523849:T=1734558928:RT=1734656408:S=ALNI_Mbp8CnT5pB-jENthJuVcUpOTSmbsw; __gpi=UID=00000f71be9c09a2:T=1734558928:RT=1734656408:S=ALNI_MY65zEFwIY9af7Bb10erBFyZAphdg; __eoi=ID=2713b89900ec94e6:T=1734558928:RT=1734656408:S=AA-AfjaxmpmereZqgCkkOWwkTNKQ; _ga_MZXRL7TKL9=GS1.1.1734656403.11.1.1734656409.0.0.0";

        int totalPages = 72; // Toplam taranacak sayfa sayısı
        string outputFile = "phone_links.txt"; // Kaydedilecek linklerin dosya adı

        // Link toplama işlemi
        //Console.WriteLine("Link toplama işlemi başlıyor...");
        //var links = await GetLinkleriTopla(cookie, totalPages);

        //// Linkleri dosyaya kaydet
        //if (links.Count > 0)
        //{
        //    File.WriteAllLines(outputFile, links);
        //    Console.WriteLine($"Toplam {links.Count} link kaydedildi: {outputFile}");
        //}
        //else
        //{
        //    Console.WriteLine("Hiç link bulunamadı.");
        //}

        // Kaydedilen linkleri gezme işlemi
        Console.WriteLine("Kaydedilen linkler geziliyor...");
        await GetLinkleriGez(cookie, outputFile);

        Console.WriteLine("Linkleri gezme işlemi tamamlandı.");
    }

    static async Task<List<string>> GetLinkleriTopla(string cookie, int totalPages)
    {
        string urlBase = "https://www.epey.com/kat/listele/";
        string refererBase = "https://www.epey.com/akilli-telefonlar/";

        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Accept", "text/html, */*; q=0.01");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");

                var allLinks = new List<string>();

                for (int page = 1; page <= totalPages; page++)
                {
                    string referer = refererBase + (page == 1 ? "" : $"{page}/");
                    string postData = $"kategori_id=1&cerez=MjExNzI0&limit=31&sayfa={page}";

                    client.DefaultRequestHeaders.Remove("Referer");
                    client.DefaultRequestHeaders.Add("Referer", referer);

                    var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                    Console.WriteLine($"Taranıyor: Sayfa {page}");
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(urlBase, content);
                        if (response.IsSuccessStatusCode)
                        {
                            string html = await response.Content.ReadAsStringAsync();

                            // HTML Parsing
                            HtmlDocument document = new HtmlDocument();
                            document.LoadHtml(html);
                            // Marka adını al
                            var brandNode = document.DocumentNode.SelectSingleNode("//div[@class='yol']/a[last()]");
                            string brandName = brandNode != null ? brandNode.InnerText.Trim() : "Bilinmeyen Marka";

                            // Ürün ismini al
                            var productNode = document.DocumentNode.SelectSingleNode("//div[@class='baslik']/h1/a");
                            string productModel = productNode != null
                                ? productNode.GetAttributeValue("title", "Bilinmeyen Ürün").Trim()
                                : "Bilinmeyen Ürün";

                            // Proje dizininde markaya ait klasör kontrolü
                            string projectDirectory = Directory.GetCurrentDirectory() + "/uploads";
                            string brandDirectory = Path.Combine(projectDirectory, brandName);
                            string productDirectory = Path.Combine(brandDirectory, productModel);

                            // Marka klasörünü oluştur
                            if (!Directory.Exists(brandDirectory))
                            {
                                Directory.CreateDirectory(brandDirectory);
                                Console.WriteLine($"Marka klasörü oluşturuldu: {brandDirectory}");
                            }

                            // Model klasörünü oluştur
                            if (!Directory.Exists(productDirectory))
                            {
                                Directory.CreateDirectory(productDirectory);
                                Console.WriteLine($"Model klasörü oluşturuldu: {productDirectory}");
                            }
                        
                            // Telefon linklerini bul
                            var links = document.DocumentNode.SelectNodes("//div[@class='detay cell']//a[@class='urunadi']")
                                ?.Select(node => node.Attributes["href"].Value)
                                .Where(href => href.StartsWith("https://www.epey.com"))
                                .ToList();

                            if (links != null)
                            {
                                allLinks.AddRange(links);
                                Console.WriteLine($"{links.Count} link bulundu.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Hata: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Hata: {ex.Message}");
                    }
                }

                return allLinks;
            }
        }
    }

    static async Task GetLinkleriGez(string cookie, string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Hata: {inputFile} dosyası bulunamadı.");
            return;
        }

        var links = File.ReadAllLines(inputFile);
        if (links.Length == 0)
        {
            Console.WriteLine("Hata: Dosyada link bulunamadı.");
            return;
        }

        string logFilePath = "downloaded_images.txt";

        HashSet<string> processedLinks = new HashSet<string>();
        if (File.Exists(logFilePath))
        {
            var loggedUrls = File.ReadAllLines(logFilePath);
            processedLinks = new HashSet<string>(loggedUrls);
            Console.WriteLine($"Toplam {processedLinks.Count} link logdan yüklendi.");
        }
        else
        {
            Console.WriteLine("Log dosyası bulunamadı, yeni oluşturulacak.");
        }


        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");

                foreach (var link in links.OrderBy(o=> o.Length))
                {
                    //if (processedLinks.Contains(link))
                    //{
                    //    Console.WriteLine($"Link zaten işlendi: {link}");
                    //    continue; // İşlenmiş linki atla
                    //}

                    if(link.Contains("infinix"))
                    {
                        Console.WriteLine($"Taranıyor: {link}");

                        // İşlenen linki loga kaydet
                        processedLinks.Add(link);
                        await File.AppendAllTextAsync(logFilePath, link + Environment.NewLine);

                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(link);
                            if (response.IsSuccessStatusCode)
                            {
                                string html = await response.Content.ReadAsStringAsync();

                                // HTML içeriğini parse et
                                HtmlDocument document = new HtmlDocument();
                                document.LoadHtml(html);

                                // Marka ve model bilgilerini al
                                var brandNode = document.DocumentNode.SelectSingleNode("//div[@class='yol']/a[last()]");
                                string brandName = brandNode != null ? brandNode.InnerText.Trim() : "Bilinmeyen Marka";

                                var productNode = document.DocumentNode.SelectSingleNode("//div[@class='baslik']/h1/a");
                                string productModel = productNode != null
                                    ? productNode.GetAttributeValue("title", "Bilinmeyen Ürün").Trim()
                                    : "Bilinmeyen Ürün";

                                // Klasör oluşturma
                                string projectDirectory = Directory.GetCurrentDirectory() + "/uploads";
                                string brandDirectory = Path.Combine(projectDirectory, brandName);
                                string productDirectory = Path.Combine(brandDirectory, productModel);

                                if (!Directory.Exists(productDirectory))
                                    Directory.CreateDirectory(productDirectory);

                                Console.WriteLine($"Ürün: {productModel}");

                                // Görsel URL'leri al
                                var imgNodes = document.DocumentNode.SelectNodes("//div[@class='buyuk row']//img");
                                if (imgNodes != null)
                                {
                                    foreach (var imgNode in imgNodes)
                                    {
                                        string imgUrl = imgNode.GetAttributeValue("src", "");
                                        if (!string.IsNullOrEmpty(imgUrl))
                                        {
                                            // Ürün resimlerinin URL'lerini oluştur
                                            List<string> imageUrls = GenerateImageUrls(imgUrl, 35);
                                            foreach (var imageUrl in imageUrls)
                                            {
                                                Console.WriteLine($"Resim sorgulanıyor: {imageUrl}");
                                                await DownloadAndSaveImage(client, imageUrl, productDirectory, logFilePath);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Resim bulunamadı.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Hata: {response.StatusCode} - {link}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hata: {ex.Message} - {link}");
                        }
                    }

                  

                 
                }
            }
        }
    }

    static List<string> GenerateImageUrls(string baseUrl, int maxImages)
    {
        List<string> urls = new List<string>();

        // s_ veya m_ varsa b_ yap
        if (baseUrl.Contains("/s_"))
            baseUrl = baseUrl.Replace("/s_", "/b_");
        else if (baseUrl.Contains("/m_"))
            baseUrl = baseUrl.Replace("/m_", "/b_");

        string fileExtension = Path.GetExtension(baseUrl); // .png, .jpg, .jpeg gibi
        string baseWithoutExtension = baseUrl.Substring(0, baseUrl.LastIndexOf('-')); // Uzantıyı ve numarayı ayır

        for (int i = 1; i <= maxImages; i++)
        {
            string updatedUrl = $"{baseWithoutExtension}-{i}{fileExtension}";
            urls.Add(updatedUrl);
        }

        return urls;
    }

    static async Task DownloadAndSaveImage(HttpClient client, string imageUrl, string saveDirectory, string logFilePath)
    {
       

        try
        {
            HttpResponseMessage response = await client.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
            {
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                string fileName = Path.GetFileName(imageUrl);
                string filePath = Path.Combine(saveDirectory, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Console.WriteLine($"Görsel indirildi: {filePath}");

     
            }
            else
            {
                Console.WriteLine($"Görsel bulunamadı: {imageUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Görsel indirme hatası: {ex.Message} - {imageUrl}");
        }
    }




}
