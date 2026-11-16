using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Applications.Service.Auditoria;

public class LeitorArquivosService
{
    private const long TamanhoMaximo = 5 * 1024 * 1024;          // por arquivo
    private const long ZipMaximo = 50 * 1024 * 1024;             // zip enviado
    private const long ZipDescompactadoMaximo = 200 * 1024 * 1024; // proteção contra zip bomb
    private const int ZipMaxEntradas = 20_000;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    // ---------- ZIP de XMLs (nunca grava em disco: sem risco de zip slip) ----------
    public async Task<List<(string Nome, byte[] Bytes)>> LerZipAsync(IFormFile? arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0) throw new DomainException("Envie o .zip com os XMLs.");
        if (arquivo.Length > ZipMaximo) throw new DomainException("O .zip excede 50 MB.");
        if (!Path.GetExtension(arquivo.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Os XMLs devem vir em um arquivo .zip.");

        try
        {
            using var zip = new ZipArchive(arquivo.OpenReadStream(), ZipArchiveMode.Read);

            var entradas = zip.Entries
                .Where(e => e.Length > 0
                         && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                         && !e.FullName.Contains("__MACOSX")
                         && !e.Name.StartsWith("._"))
                .ToList();

            if (entradas.Count == 0) throw new DomainException("O .zip não contém arquivos .xml.");
            if (entradas.Count > ZipMaxEntradas) throw new DomainException("Arquivos demais no .zip.");
            if (entradas.Sum(e => e.Length) > ZipDescompactadoMaximo)
                throw new DomainException("O conteúdo descompactado é grande demais.");

            var lista = new List<(string, byte[])>(entradas.Count);
            foreach (var e in entradas)
            {
                if (e.Length > TamanhoMaximo) { lista.Add((e.FullName, [])); continue; }
                using var s = e.Open();
                using var ms = new MemoryStream();
                await s.CopyToAsync(ms, ct);
                lista.Add((e.FullName, ms.ToArray()));
            }
            return lista;
        }
        catch (InvalidDataException) { throw new DomainException("O .zip está corrompido."); }
    }

    // ---------- XML: nota (nfeProc) ou evento (procEventoNFe) ----------
    public XmlLido LerXml(byte[] bytes)
    {
        if (bytes.Length == 0) throw new DomainException("Arquivo vazio ou grande demais.");

        XDocument doc;
        try
        {
            // Prohibit + XmlResolver nulo = protege contra XXE
            var cfg = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using var reader = XmlReader.Create(new MemoryStream(bytes), cfg);
            doc = XDocument.Load(reader);
        }
        catch (XmlException) { throw new DomainException("XML inválido."); }

        var raiz = doc.Root!;

        if (raiz.Name.LocalName == "procEventoNFe")
        {
            var ev = D(raiz, "infEvento") ?? throw new DomainException("Evento sem infEvento.");
            return new XmlLido(null, new EventoXml(
                Txt(ev, "chNFe") ?? throw new DomainException("Evento sem chNFe."),
                Txt(ev, "tpEvento") ?? ""));
        }

        var inf = D(raiz, "infNFe") ?? throw new DomainException("XML sem infNFe.");
        var chave = inf.Attribute("Id")?.Value.Replace("NFe", "");
        if (chave is null || chave.Length != 44 || !chave.All(char.IsDigit))
            throw new DomainException("Chave de acesso inválida.");

        var itens = inf.Elements().Where(e => e.Name.LocalName == "det").Select(det =>
        {
            var p = F(det, "prod") ?? throw new DomainException("Item sem 'prod'.");
            return new ItemXml(
                int.Parse(det.Attribute("nItem")?.Value ?? "0"),
                Txt(p, "cEAN") ?? "", Txt(p, "NCM"), Txt(p, "CFOP"), Txt(p, "uCom") ?? "UN",
                Dec(Txt(p, "qCom")), Dec(Txt(p, "vProd")));
        }).ToList();

        var nota = new NotaXml(
            chave, Convert.ToHexString(SHA256.HashData(bytes)),
            Txt(F(inf, "ide"), "mod") ?? "",
            Txt(F(inf, "emit"), "CNPJ"), Txt(F(inf, "dest"), "CNPJ"),
            D(raiz, "cStat")?.Value,
            Dec(D(inf, "vNF")?.Value), itens);

        return new XmlLido(nota, null);
    }

    // ---------- JSONs ----------
    public async Task<ArquivoErp> LerErpAsync(IFormFile arquivo, CancellationToken ct)
    {
        var bytes = await LerBytesAsync(arquivo, ".json", ct);
        try
        {
            var erp = JsonSerializer.Deserialize<ArquivoErp>(bytes, JsonOpts);
            return erp?.Movimentacoes is null
                ? throw new DomainException($"{arquivo.FileName}: JSON do ERP sem 'movimentacoes'.")
                : erp;
        }
        catch (JsonException) { throw new DomainException($"{arquivo.FileName}: JSON inválido."); }
    }

    public async Task<List<ProdutoCadastro>> LerProdutosAsync(IFormFile arquivo, CancellationToken ct)
    {
        var bytes = await LerBytesAsync(arquivo, ".json", ct);
        try
        {
            var lista = JsonSerializer.Deserialize<List<ProdutoCadastro>>(bytes, JsonOpts);
            return lista is { Count: > 0 } ? lista : throw new DomainException("produtos.json vazio.");
        }
        catch (JsonException) { throw new DomainException("produtos.json inválido."); }
    }

    // ---------- auxiliares ----------
    private static async Task<byte[]> LerBytesAsync(IFormFile? arquivo, string extensao, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0) throw new DomainException($"Envie o arquivo {extensao}.");
        if (arquivo.Length > TamanhoMaximo) throw new DomainException($"{arquivo.FileName}: maior que 5 MB.");
        if (!Path.GetExtension(arquivo.FileName).Equals(extensao, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"{arquivo.FileName}: deve ser {extensao}.");

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private static XElement? F(XElement? pai, string nome) =>   // filho direto
        pai?.Elements().FirstOrDefault(e => e.Name.LocalName == nome);

    private static XElement? D(XElement? pai, string nome) =>   // descendente
        pai?.Descendants().FirstOrDefault(e => e.Name.LocalName == nome);

    private static string? Txt(XElement? pai, string nome) => F(pai, nome)?.Value;

    private static decimal Dec(string? v) =>
        decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
            ? d : throw new DomainException("Valor numérico inválido no XML.");
}