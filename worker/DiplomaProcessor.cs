using Minio;
using Minio.DataModel.Args;

using PuppeteerSharp;
using PuppeteerSharp.Media;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class DiplomaProcessor
{
    private readonly DiplomaContext _context;
    private readonly IMinioClient _minioClient;

    public DiplomaProcessor(DiplomaContext context, IMinioClient minioClient)
    {
        _context = context;
        _minioClient = minioClient;
    }

    public async Task ProcessMessageAsync(string message)
    {
        var diplomaData = JsonSerializer.Deserialize<Diploma>(message);

        Console.WriteLine($"Processando diploma para {diplomaData.NomeAluno}");

        // Carregar template HTML e substituir placeholders
        var templatePath = "template-diploma.html";
        var templateHtml = await File.ReadAllTextAsync(templatePath);
        templateHtml = templateHtml.Replace("{{nome_aluno}}", diplomaData.NomeAluno)
                                   .Replace("{{nome_curso}}", diplomaData.NomeCurso)
                                   .Replace("{{data_conclusao}}", diplomaData.DataConclusao.ToString("dd/MM/yyyy"))
                                   .Replace("{{nacionalidade}}", diplomaData.Nacionalidade)
                                   .Replace("{{naturalidade}}", diplomaData.Naturalidade)
                                   .Replace("{{data_nascimento}}", diplomaData.DataNascimento.ToString("dd/MM/yyyy"))
                                   .Replace("{{numero_rg}}", diplomaData.NumeroRg)
                                   .Replace("{{data_emissao}}", diplomaData.DataEmissao.ToString("dd/MM/yyyy"));

        // Gerar PDF com PuppeteerSharp
        //await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            ExecutablePath = "/usr/bin/google-chrome-stable",
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        var page = await browser.NewPageAsync();
        await page.SetContentAsync(templateHtml);

        // Gerar o PDF a partir do conteúdo HTML
        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true
        });

        await browser.CloseAsync();

        // Salvar no MinIO
        var pdfFilePath = $"diplomas/{diplomaData.NomeAluno}_{diplomaData.DataConclusao:yyyyMMdd}.pdf";
        using var memoryStream = new MemoryStream(pdfBytes);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket("diplomas")
            .WithObject(pdfFilePath)
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length));

        // Persistir dados no Postgres
        var diploma = new Diploma
        {
            NomeAluno = diplomaData.NomeAluno,
            NomeCurso = diplomaData.NomeCurso,
            DataConclusao = diplomaData.DataConclusao,
            DataEmissao = DateTime.Now,
            DataNascimento = diplomaData.DataNascimento,
            Nacionalidade = diplomaData.Nacionalidade,
            Naturalidade = diplomaData.Naturalidade,
            NumeroRg = diplomaData.NumeroRg
        };

        //_context.Diplomas.Add(diploma);
        //await _context.SaveChangesAsync();

        Console.WriteLine($"Diploma gerado e salvo em {pdfFilePath}");
    }
}
