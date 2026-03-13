# MCP — Model Context Protocol Server

A lightweight **MCP server** built with **C# / .NET**, exposing a set of tools that allow AI assistants (like Claude) to interact with a backend API for products, documents, warehouses, and sales statistics.

---

## Tech Stack

- **.NET** (C#)
- **ModelContextProtocol.Server** — MCP tool registration & hosting
- **System.Net.Http** — HTTP client for backend API communication
- **System.Text.Json** — JSON serialization

---

## Project Structure

```
MCP/
├── Tools/
│   └── ProductTools.cs     # All MCP tool definitions
├── HttpClientExt.cs        # HttpClient extension helpers
├── Program.cs              # Entry point & server setup
├── MCP.csproj
└── MCP.sln
```

---

## Available Tools

| Tool | Description |
|------|-------------|
| `GetProduct` | Fetch product info by symbol (e.g. `00001B`) |
| `GetProducts` | List products, max 20 by default |
| `GetPurchaseStatistics` | Purchase price stats including TwZakupy data for a product |
| `GetWarehouses` | List all warehouses |
| `GetWarehouseById` | Fetch a single warehouse by ID |
| `GetDocumentByNumber` | Fetch a document by its number |
| `GetDocuments` | Paginated list of documents (max page size: 100) |
| `GetProductSalesStatistics` | Sales stats for a product with filtering, sorting, and pagination |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run the server

```bash
git clone https://github.com/redox132/MCP.git
cd MCP
dotnet run
```

---

## Connecting to an AI Client

Once running, point your MCP-compatible client (e.g. Claude Desktop) to this server. Example `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/MCP"]
    }
  }
}
```

---

## Tool Usage Examples

**Get a product by symbol:**
```
GetProduct("00001B")
```

**Get sales statistics with filters:**
```
GetProductSalesStatistics(
  productSymbol: "00200B",
  dateFrom: "2024-01-01",
  dateTo: "2024-12-31",
  documentType: "FS",
  sortBy: "TotalNetSales",
  sortOrder: "DESC"
)
```
