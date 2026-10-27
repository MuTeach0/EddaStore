using EddaStore.Data;
using EddaStore.Models.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Xml;

namespace EddaStore.Controllers;

public sealed class SeoController(
    ApplicationDbContext context,
    IConfiguration configuration,
    IWebHostEnvironment environment) : Controller
{
    [HttpGet("robots.txt")]
    public IActionResult Robots()
    {
        var publicBaseUrl = ResolvePublicBaseUrl();
        if (publicBaseUrl is null)
        {
            return Problem(
                "A valid public base URL is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var sitemapUrl = $"{publicBaseUrl.Scheme}://{publicBaseUrl.Authority}/sitemap.xml";
        var content = $"""
            User-agent: *
            Disallow: /Admin/
            Disallow: /Account/
            Disallow: /Cart
            Disallow: /Wishlist
            Disallow: /Checkout

            Sitemap: {sitemapUrl}
            """;

        return Content(content, "text/plain; charset=utf-8");
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken cancellationToken)
    {
        var publicBaseUrl = ResolvePublicBaseUrl();
        if (publicBaseUrl is null)
        {
            return Problem(
                "A valid public base URL is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var scheme = publicBaseUrl.Scheme;
        var authority = publicBaseUrl.Authority;

        string Absolute(string path) => $"{scheme}://{authority}{path}";

        var slugs = await context.Products
            .AsNoTracking()
            .Where(product => product.IsActive
                && !product.IsDeleted
                && !string.IsNullOrWhiteSpace(product.Slug))
            .Select(product => product.Slug)
            .Distinct()
            .ToListAsync(cancellationToken);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            WriteUrl(writer, Absolute("/"));
            WriteUrl(writer, Absolute("/Store"));
            WriteUrl(writer, Absolute("/Home/Contact"));
            WriteUrl(writer, Absolute("/Home/Faq"));
            WriteUrl(writer, Absolute("/Home/Privacy"));

            foreach (var slug in slugs)
            {
                WriteUrl(writer, Absolute($"/Store/Details?slug={Uri.EscapeDataString(slug)}"));
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        return File(stream.ToArray(), "application/xml; charset=utf-8");
    }

    private Uri? ResolvePublicBaseUrl()
    {
        var configured = configuration["App:PublicBaseUrl"];
        if (Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri)
            && (configuredUri.Scheme == Uri.UriSchemeHttps
                || (environment.IsDevelopment()
                    && configuredUri.Scheme == Uri.UriSchemeHttp)))
        {
            if (!environment.IsDevelopment()
                && (string.Equals(configuredUri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(configuredUri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(configuredUri.Host, "::1", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            return configuredUri;
        }

        // Development-only fallback for local HTTPS testing. Production must
        // configure App:PublicBaseUrl so output never depends on a Host header.
        if (environment.IsDevelopment() && Request.IsHttps)
        {
            return new Uri($"{Request.Scheme}://{Request.Host}");
        }

        return null;
    }

    private static void WriteUrl(XmlWriter writer, string location)
    {
        writer.WriteStartElement("url");
        writer.WriteElementString("loc", location);
        writer.WriteEndElement();
    }
}