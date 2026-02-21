using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCP.Tools;

[McpServerToolType]
public static class ProductTools
{
    [McpServerTool, Description("Get product information for a symbol.")]
    public static async Task<string> GetProduct(HttpClient client, [Description("Product symbol, e.g., 00001B")] string symbol)
    {
        using var jsonDocument = await client.ReadJsonDocumentAsync($"/products/{symbol}");
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(jsonDocument.RootElement, options);
    }

    [McpServerTool, Description("Get list of products (max 20).")]
    public static async Task<string> GetProducts(HttpClient client, [Description("Maximum number of products to return")] int max = 20)
    {
        using var jsonDocument = await client.ReadJsonDocumentAsync("/products");
        var root = jsonDocument.RootElement;
        var options = new JsonSerializerOptions { WriteIndented = true };

        if (root.ValueKind == JsonValueKind.Array)
        {
            var items = root.EnumerateArray().Take(max).ToArray();
            return JsonSerializer.Serialize(items, options);
        }

        return JsonSerializer.Serialize(root, options);
    }

    [McpServerTool, Description("Get purchase price statistics for a specific product including TwZakupy data.")]
    public static async Task<string> GetPurchaseStatistics(HttpClient client, [Description("Product symbol, e.g., 00200B")] string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return "symbol is required.";
        }

        try
        {
            using var response = await client.GetAsync($"/products/{symbol}/purchase-statistics");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Product '{symbol}' not found (404).";
            }

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var jsonDocument = await JsonDocument.ParseAsync(stream);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch (System.Exception ex)
        {
            return $"Error fetching purchase statistics for '{symbol}': {ex.Message}";
        }
    }

    [McpServerTool, Description("Get list of warehouses.")]
    public static async Task<string> GetWarehouses(HttpClient client)
    {
        try
        {
            using var jsonDocument = await client.ReadJsonDocumentAsync("/warehouses");
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch (System.Exception ex)
        {
            return $"Error fetching warehouses: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get document by number.")]
    public static async Task<string> GetDocumentByNumber(HttpClient client, [Description("Document number")] string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return "number is required.";
        }

        try
        {
            using var jsonDocument = await client.ReadJsonDocumentAsync($"/documents/by-number?number={System.Uri.EscapeDataString(number)}");
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch (System.Exception ex)
        {
            return $"Error fetching document by number '{number}': {ex.Message}";
        }
    }

    [McpServerTool, Description("Get documents with pagination.")]
    public static async Task<string> GetDocuments(HttpClient client, [Description("Page number")] int pageNumber = 1, [Description("Page size (max 100)")] int pageSize = 100)
    {
        if (pageSize > 100) pageSize = 100;

        try
        {
            using var jsonDocument = await client.ReadJsonDocumentAsync($"/documents?pageNumber={pageNumber}&pageSize={pageSize}");
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch (System.Exception ex)
        {
            return $"Error fetching documents: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get warehouse information by id.")]
    public static async Task<string> GetWarehouseById(HttpClient client, [Description("Warehouse id (int)")] int id)
    {
        try
        {
            using var response = await client.GetAsync($"/warehouses/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return $"Warehouse '{id}' not found (404).";
            }

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var jsonDocument = await JsonDocument.ParseAsync(stream);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch (System.Exception ex)
        {
            return $"Error fetching warehouse '{id}': {ex.Message}";
        }
    }

    [McpServerTool, Description("Get sales statistics for a single product symbol by querying sales-statistics and filtering results.")]
    public static async Task<string> GetProductSalesStatistics(
        HttpClient client,
        [Description("Product symbol, e.g., 00200B")] string productSymbol,
        [Description("Start date for filtering (yyyy-MM-dd)")] string? dateFrom = null,
        [Description("End date for filtering (yyyy-MM-dd)")] string? dateTo = null,
        [Description("Document type filter: FS, PA, RO")] string? documentType = null,
        [Description("Optional product group id filter")] int? groupId = null,
        [Description("Page number (default: 1)")] int page = 1,
        [Description("Page size (default: 50, max: 100)")] int pageSize = 50,
        [Description("Sort by: TotalNetSales, TotalProfit, TotalQuantity, ProductSymbol, ProductName")] string sortBy = "TotalNetSales",
        [Description("Sort order: ASC or DESC")] string sortOrder = "DESC")
    {
        if (string.IsNullOrWhiteSpace(productSymbol))
        {
            return "productSymbol is required.";
        }

        if (pageSize > 100) pageSize = 100;

        try
        {
            var q = string.Concat(
                "dateFrom=", (dateFrom ?? "null"),
                "&dateTo=", (dateTo ?? "null"),
                "&documentType=", (documentType ?? "null"),
                "&groupId=", (groupId.HasValue ? groupId.Value.ToString() : "null"),
                "&page=", page.ToString(),
                "&pageSize=", pageSize.ToString(),
                "&sortBy=", System.Uri.EscapeDataString(sortBy ?? string.Empty),
                "&sortOrder=", System.Uri.EscapeDataString(sortOrder ?? string.Empty)
            );

            using var jsonDocument = await client.ReadJsonDocumentAsync($"/products/sales-statistics?{q}");
            var root = jsonDocument.RootElement;
            var options = new JsonSerializerOptions { WriteIndented = true };

            // Try to find product in items array
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("productSymbol", out var ps) && string.Equals(ps.GetString(), productSymbol, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return JsonSerializer.Serialize(item, options);
                    }
                }

                return $"No sales statistics found for product '{productSymbol}' in the returned page.";
            }

            // If the endpoint returned an array directly, search it
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("productSymbol", out var ps) && string.Equals(ps.GetString(), productSymbol, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return JsonSerializer.Serialize(item, options);
                    }
                }

                return $"No sales statistics found for product '{productSymbol}'.";
            }

            return "Unexpected response format from sales-statistics endpoint.";
        }
        catch (System.Exception ex)
        {
            return $"Error fetching sales statistics: {ex.Message}";
        }
    }
}
