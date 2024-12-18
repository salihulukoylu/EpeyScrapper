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
        string cookie = "_gid=GA1.2.791491693.1734544251; PHPSESSID=537801c74fba6029c37ee29a2ed9e583; cf_clearance=hmxSmA7JFCBqQ3ihqdLzmKQ0CaC_J1.96pxvQROXNd4-1734545542-1.2.1.1-mo77epaCAnPfU3qPe.eVBJuHi2H_uV_uNrvbzZn6AYND8B0DYPKMesHfsxnLjzwjA3MX7t3X0j.kRNpY4QjZoDtLG8940nuhreYejWRdbj3goCYF0M7RRRmxBRGe5MRtmT.YCzQkO3wMX_9xJ0MqaRi87dRCIkvZspQicXNwxqtLAWVg7ClZ7xQC7VuPWH3WI8sAUlG1BuHSGfVL8SiwbRZSWFqSLJV2vPW06KKjRZ75MehjeQ6UayaWJVcSM8ye5d2T40hhnBpagNH5GK6HkD.deMDZT_INFz0IXXDqr.fK8627IN5CvmPiZAxMY7wG5BzVUAi.d8e0gt.SY5m86dnMINsk5EWH1HxK9cFgIdVob1nUOOCOzgF4othOiIDfvH0lrjLud6uHUeW9rbULBtAvhvmJKW_z3dROubWxBHgJxnX7OPXTmkZuXUWQ52gW";
        int totalPages = 72; // Kaç sayfa scrape edilecek
        string outputFile = "phone_links.txt";

        //Console.WriteLine("Link toplama işlemi başlıyor...");
        //var links = await GetLinkleriTopla(cookie, totalPages);

        //// Linkleri kaydet
        //if (links.Count > 0)
        //{
        //    File.WriteAllLines(outputFile, links);
        //    Console.WriteLine($"Toplam {links.Count} link kaydedildi: {outputFile}");
        //}
        //else
        //{
        //    Console.WriteLine("Hiç link bulunamadı.");
        //}

        Console.WriteLine("Kaydedilen linkler geziliyor...");
        await GetLinkleriGez(cookie, outputFile);

        Console.WriteLine("Linkleri gezme işlemi tamamlandı.");
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

        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                client.DefaultRequestHeaders.Add("Accept", "text/html, */*; q=0.01");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");

                foreach (var link in links)
                {
                    Console.WriteLine($"Taranıyor: {link}");
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(link);
                        if (response.IsSuccessStatusCode)
                        {
                            string html = await response.Content.ReadAsStringAsync();

                            // HTML içeriğini parse et
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
                            else
                            {
                                Console.WriteLine($"Model klasörü zaten mevcut: {productDirectory}");
                            }

                            Console.WriteLine($"Ürün: {productModel}");
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
}
