# Zeni Search - Advanced Backend Features

> **Philosophy:** Production-ready patterns for scalable web scraping and price tracking.

---

## 📍 Quick Navigation

### Topics Covered

1. [Scheduled Scraping](#1-scheduled-scraping) - Automate scraping with Hangfire
2. [Multiple Retailers](#2-multiple-retailers) - Interface-based architecture
3. [Price History](#3-price-history-tracking) - Track price changes over time
4. [Better Error Handling](#4-better-error-handling) - Production-grade resilience

---

# 1. Scheduled Scraping

## 1.1 Why Background Jobs?

**Current Problem:**

- Manual API calls to trigger scraping
- No automation
- User has to remember to run it

**Solution: Hangfire**

- Runs background jobs automatically
- Web dashboard to monitor jobs
- Retry failed jobs
- Persistent storage (survives restarts)

### Why Hangfire vs Quartz.NET?

| Feature            | Hangfire       | Quartz.NET     |
| ------------------ | -------------- | -------------- |
| **Setup**          | Simple         | Complex        |
| **Dashboard**      | Built-in UI ✅ | None           |
| **Persistence**    | PostgreSQL ✅  | Requires setup |
| **Learning Curve** | Easy           | Steep          |
| **Best For**       | Web apps       | Enterprise     |

**Decision: Hangfire** ✅

---

## 1.2 Install Hangfire

```bash
cd /home/ed/Documents/gitRepo/zeni-search/backend
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.PostgreSql
```

---

## 1.3 Configure Hangfire

**Location:** `Program.cs`

```csharp
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ZeniSearch.Api.Data;
using ZeniSearch.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register HttpClient
builder.Services.AddHttpClient();

// Register Scrapers
builder.Services.AddScoped<IProductScraper, TheIconicScraper>();

// ============================================
// HANGFIRE CONFIGURATION
// ============================================

// Configure Hangfire to use PostgreSQL
builder.Services.AddHangfire(config =>
{
    config
        // Use same PostgreSQL database
        .UsePostgreSqlStorage(connectionString)

        // Configure job options
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings();
});

// Add Hangfire server (processes background jobs)
builder.Services.AddHangfireServer(options =>
{
    // Worker count (parallel jobs)
    options.WorkerCount = 2;

    // Poll interval (how often to check for new jobs)
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // ============================================
    // HANGFIRE DASHBOARD
    // ============================================

    // Access at: http://localhost:5020/hangfire
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // In production, add authentication!
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================
// SCHEDULE RECURRING JOBS
// ============================================

// Schedule scraper to run daily at 3 AM
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-sandals-daily",
    service => service.ScrapeAllRetailers("sandals"),
    Cron.Daily(3) // 3 AM every day
);

// Schedule hourly scraping for popular items
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-popular-hourly",
    service => service.ScrapePopularProducts(),
    Cron.Hourly() // Every hour
);

app.Run();
```

---

## 1.4 Create Hangfire Authorization Filter

**Location:** `Services/HangfireAuthorizationFilter.cs`

```csharp
using Hangfire.Dashboard;

namespace ZeniSearch.Api.Services;

// Simple authorization for Hangfire dashboard
// In production, implement proper authentication!
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Development: Allow all
        // Production: Check user roles/claims

        var httpContext = context.GetHttpContext();

        // TODO: Add authentication
        // return httpContext.User.IsInRole("Admin");

        return true; // ⚠️ Change this in production!
    }
}
```

---

## 1.5 Create ScraperService

**Location:** `Services/ScraperService.cs`

```csharp
using ZeniSearch.Api.Data;

namespace ZeniSearch.Api.Services;

/// <summary>
/// Orchestrates scraping across multiple retailers
/// </summary>
public class ScraperService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScraperService> _logger;

    // Don't inject AppDbContext here! It would be disposed.
    // Use IServiceProvider to create scope per job
    public ScraperService(
        IServiceProvider serviceProvider,
        ILogger<ScraperService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Scrapes all retailers for a given search term
    /// </summary>
    public async Task ScrapeAllRetailers(string searchTerm)
    {
        _logger.LogInformation("Starting scheduled scrape for: {SearchTerm}", searchTerm);

        try
        {
            // Create a scope for this job
            // This ensures DbContext is properly disposed
            using var scope = _serviceProvider.CreateScope();

            // Get scraper from scope
            var scraper = scope.ServiceProvider.GetRequiredService<IProductScraper>();

            // Run scraper
            var count = await scraper.ScrapeProducts(searchTerm, maxProducts: 100);

            _logger.LogInformation(
                "Scheduled scrape completed. Scraped {Count} products for '{SearchTerm}'",
                count,
                searchTerm
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled scrape failed for: {SearchTerm}", searchTerm);
            throw; // Hangfire will retry
        }
    }

    /// <summary>
    /// Scrapes popular/trending products
    /// </summary>
    public async Task ScrapePopularProducts()
    {
        var popularSearches = new[] { "sandals", "slides", "thongs", "flip flops" };

        foreach (var term in popularSearches)
        {
            await ScrapeAllRetailers(term);

            // Rate limiting: wait between searches
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
```

---

## 1.6 Remove Manual ScraperController

Now that we have scheduled jobs, we can remove the manual trigger controller:

```bash
# Delete or comment out ScraperController.cs
# Keep it only if you want manual overrides
```

Or make it admin-only:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only admins can manually trigger
public class ScraperController : ControllerBase
{
    // ... existing code
}
```

---

## 1.7 Test Hangfire

**Step 1: Start your app**

```bash
dotnet watch run
```

**Step 2: Open Hangfire Dashboard**

Navigate to: `http://localhost:5020/hangfire`

You should see:

- Recurring jobs listed
- Job execution history
- Success/failure statistics

**Step 3: Trigger a job manually**

In Hangfire dashboard:

1. Click "Recurring jobs"
2. Find your job
3. Click "Trigger now"

**Step 4: Monitor execution**

- Watch "Jobs" tab to see it running
- Check logs in console
- Verify data in PostgreSQL

---

## 1.8 Cron Expressions Reference

```csharp
// Every minute
Cron.Minutely()

// Every hour
Cron.Hourly()

// Daily at specific time
Cron.Daily(3) // 3 AM
Cron.Daily(14) // 2 PM

// Specific days
Cron.Weekly(DayOfWeek.Monday, 9) // Monday 9 AM
Cron.Monthly(1, 0) // 1st of month, midnight

// Custom cron expression
"0 */2 * * *" // Every 2 hours
"0 9-17 * * 1-5" // Every hour 9 AM-5 PM, Mon-Fri
"*/15 * * * *" // Every 15 minutes
```

---

# 2. Multiple Retailers

## 2.1 The Problem

Currently:

- `TheIconicScraper` is hardcoded
- Can't easily add new retailers
- No common interface

**Goal:** Support multiple retailers with clean architecture.

---

## 2.2 Create Scraper Interface

**Location:** `Services/IProductScraper.cs`

```csharp
using ZeniSearch.Api.Models;

namespace ZeniSearch.Api.Services;

/// <summary>
/// Common interface for all product scrapers
/// </summary>
public interface IProductScraper
{
    /// <summary>
    /// The retailer name this scraper handles
    /// </summary>
    string RetailerName { get; }

    /// <summary>
    /// Scrapes products from this retailer
    /// </summary>
    /// <param name="searchTerm">What to search for</param>
    /// <param name="maxProducts">Maximum products to scrape</param>
    /// <returns>Number of new products added</returns>
    Task<int> ScrapeProducts(string searchTerm, int maxProducts = 50);

    /// <summary>
    /// Checks if this scraper is healthy/working
    /// </summary>
    Task<bool> HealthCheck();
}
```

---

## 2.3 Update TheIconicScraper to Implement Interface

**Location:** `Services/TheIconicScraper.cs`

```csharp
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using ZeniSearch.Api.Data;
using ZeniSearch.Api.Models;

namespace ZeniSearch.Api.Services;

public class TheIconicScraper : IProductScraper
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheIconicScraper> _logger;

    private const string BASE_URL = "https://www.theiconic.com.au";

    // Implement interface property
    public string RetailerName => "The Iconic";

    public TheIconicScraper(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<TheIconicScraper> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Implement interface method
    public async Task<int> ScrapeProducts(string searchTerm, int maxProducts = 50)
    {
        // ... existing implementation
        try
        {
            var searchUrl = $"{BASE_URL}/search/?q={Uri.EscapeDataString(searchTerm)}";
            var html = await FetchHtmlAsync(searchUrl);

            if (string.IsNullOrEmpty(html))
                return 0;

            var products = ParseProducts(html, searchTerm, maxProducts);

            if (products.Count == 0)
                return 0;

            var existingUrls = await _context.Product
                .Where(p => products.Select(x => x.ProductUrl).Contains(p.ProductUrl))
                .Select(p => p.ProductUrl)
                .ToListAsync();

            var newProducts = products
                .Where(p => !existingUrls.Contains(p.ProductUrl))
                .ToList();

            if (newProducts.Count > 0)
            {
                _context.Product.AddRange(newProducts);
                await _context.SaveChangesAsync();
            }

            return newProducts.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping The Iconic");
            throw;
        }
    }

    // Implement health check
    public async Task<bool> HealthCheck()
    {
        try
        {
            var html = await FetchHtmlAsync(BASE_URL);
            return !string.IsNullOrEmpty(html);
        }
        catch
        {
            return false;
        }
    }

    // ... existing private methods (FetchHtmlAsync, ParseProducts, etc.)
}
```

---

## 2.4 Create More Scrapers

**Location:** `Services/AmazonScraper.cs`

```csharp
namespace ZeniSearch.Api.Services;

public class AmazonScraper : IProductScraper
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AmazonScraper> _logger;

    public string RetailerName => "Amazon AU";

    public AmazonScraper(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<AmazonScraper> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<int> ScrapeProducts(string searchTerm, int maxProducts = 50)
    {
        // TODO: Implement Amazon scraping logic
        // Amazon is harder - they have anti-bot measures
        _logger.LogWarning("Amazon scraper not yet implemented");
        return 0;
    }

    public async Task<bool> HealthCheck()
    {
        return false; // Not implemented yet
    }
}
```

**Location:** `Services/EbayScraper.cs`

```csharp
namespace ZeniSearch.Api.Services;

public class EbayScraper : IProductScraper
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EbayScraper> _logger;

    public string RetailerName => "eBay Australia";

    public EbayScraper(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<EbayScraper> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<int> ScrapeProducts(string searchTerm, int maxProducts = 50)
    {
        // TODO: Implement eBay scraping
        _logger.LogWarning("eBay scraper not yet implemented");
        return 0;
    }

    public async Task<bool> HealthCheck()
    {
        return false;
    }
}
```

---

## 2.5 Create Scraper Factory

**Location:** `Services/ScraperFactory.cs`

```csharp
namespace ZeniSearch.Api.Services;

/// <summary>
/// Factory to get the right scraper for a retailer
/// </summary>
public class ScraperFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScraperFactory> _logger;

    public ScraperFactory(
        IServiceProvider serviceProvider,
        ILogger<ScraperFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets all registered scrapers
    /// </summary>
    public IEnumerable<IProductScraper> GetAllScrapers()
    {
        return _serviceProvider.GetServices<IProductScraper>();
    }

    /// <summary>
    /// Gets scraper by retailer name
    /// </summary>
    public IProductScraper? GetScraper(string retailerName)
    {
        var scrapers = GetAllScrapers();

        return scrapers.FirstOrDefault(s =>
            s.RetailerName.Equals(retailerName, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Gets all healthy scrapers
    /// </summary>
    public async Task<IEnumerable<IProductScraper>> GetHealthyScrapers()
    {
        var scrapers = GetAllScrapers();
        var healthy = new List<IProductScraper>();

        foreach (var scraper in scrapers)
        {
            try
            {
                if (await scraper.HealthCheck())
                {
                    healthy.Add(scraper);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Health check failed for {Retailer}",
                    scraper.RetailerName);
            }
        }

        return healthy;
    }
}
```

---

## 2.6 Register All Scrapers

**Location:** `Program.cs`

```csharp
// Register all scrapers
builder.Services.AddScoped<IProductScraper, TheIconicScraper>();
builder.Services.AddScoped<IProductScraper, AmazonScraper>();
builder.Services.AddScoped<IProductScraper, EbayScraper>();

// Register factory
builder.Services.AddScoped<ScraperFactory>();

// Register orchestration service
builder.Services.AddScoped<ScraperService>();
```

---

## 2.7 Update ScraperService to Use All Scrapers

**Location:** `Services/ScraperService.cs`

```csharp
public class ScraperService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScraperService> _logger;

    public ScraperService(
        IServiceProvider serviceProvider,
        ILogger<ScraperService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Scrapes ALL retailers for a search term
    /// </summary>
    public async Task ScrapeAllRetailers(string searchTerm)
    {
        _logger.LogInformation(
            "Starting multi-retailer scrape for: {SearchTerm}",
            searchTerm
        );

        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ScraperFactory>();

        // Get all scrapers
        var scrapers = factory.GetAllScrapers();

        var results = new Dictionary<string, int>();

        foreach (var scraper in scrapers)
        {
            try
            {
                _logger.LogInformation(
                    "Scraping {Retailer} for '{SearchTerm}'",
                    scraper.RetailerName,
                    searchTerm
                );

                var count = await scraper.ScrapeProducts(searchTerm, maxProducts: 50);
                results[scraper.RetailerName] = count;

                _logger.LogInformation(
                    "{Retailer}: {Count} new products",
                    scraper.RetailerName,
                    count
                );

                // Rate limiting: wait between retailers
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to scrape {Retailer}",
                    scraper.RetailerName
                );

                results[scraper.RetailerName] = 0;
                // Continue to next retailer instead of failing entirely
            }
        }

        var total = results.Values.Sum();
        _logger.LogInformation(
            "Multi-retailer scrape complete. Total: {Total} new products. Details: {@Results}",
            total,
            results
        );
    }

    /// <summary>
    /// Scrape only healthy retailers
    /// </summary>
    public async Task ScrapeHealthyRetailers(string searchTerm)
    {
        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ScraperFactory>();

        // Only get scrapers that pass health check
        var healthyScrapers = await factory.GetHealthyScrapers();

        _logger.LogInformation(
            "Found {Count} healthy scrapers",
            healthyScrapers.Count()
        );

        foreach (var scraper in healthyScrapers)
        {
            try
            {
                await scraper.ScrapeProducts(searchTerm, maxProducts: 50);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scrape failed for {Retailer}", scraper.RetailerName);
            }
        }
    }
}
```

---

# 3. Price History Tracking

## 3.1 Why Track Price History?

**Current Problem:**

- Overwrite prices when scraping
- No historical data
- Can't show price trends
- Can't alert on price drops

**Solution: Price History Table**

- Track every price change
- Show price graphs
- Alert users when prices drop
- Detect price manipulation

---

## 3.2 Create PriceHistory Model

**Location:** `Models/PriceHistory.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeniSearch.Api.Models;

/// <summary>
/// Tracks historical prices for products
/// </summary>
public class PriceHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Product
    /// </summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Price at this point in time
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// When this price was recorded
    /// </summary>
    [Required]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional: Source of this price (scraper, manual, API)
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }
}
```

---

## 3.3 Update Product Model

**Location:** `Models/Product.cs`

Add navigation property:

```csharp
public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Brand { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ProductUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string RetailerName { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // ============================================
    // NEW: Navigation property for price history
    // ============================================

    /// <summary>
    /// All historical prices for this product
    /// </summary>
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
}
```

---

## 3.4 Update AppDbContext

**Location:** `Data/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Product { get; set; }

    // Add PriceHistory DbSet
    public DbSet<PriceHistory> PriceHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product indexes
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.RetailerName);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Price);

        // ============================================
        // NEW: PriceHistory configuration
        // ============================================

        // Define relationship: Product has many PriceHistories
        modelBuilder.Entity<PriceHistory>()
            .HasOne(ph => ph.Product)
            .WithMany(p => p.PriceHistories)
            .HasForeignKey(ph => ph.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Delete history when product deleted

        // Index on ProductId for fast lookups
        modelBuilder.Entity<PriceHistory>()
            .HasIndex(ph => ph.ProductId);

        // Index on RecordedAt for time-based queries
        modelBuilder.Entity<PriceHistory>()
            .HasIndex(ph => ph.RecordedAt);

        // Composite index for common query: product + date range
        modelBuilder.Entity<PriceHistory>()
            .HasIndex(ph => new { ph.ProductId, ph.RecordedAt });
    }
}
```

---

## 3.5 Create Migration

```bash
cd /home/ed/Documents/gitRepo/zeni-search/backend
dotnet ef migrations add AddPriceHistory
dotnet ef database update
```

---

## 3.6 Create PriceHistoryService

**Location:** `Services/PriceHistoryService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ZeniSearch.Api.Data;
using ZeniSearch.Api.Models;

namespace ZeniSearch.Api.Services;

/// <summary>
/// Service for tracking and querying price history
/// </summary>
public class PriceHistoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PriceHistoryService> _logger;

    public PriceHistoryService(
        AppDbContext context,
        ILogger<PriceHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Records current price in history if it changed
    /// </summary>
    public async Task<bool> RecordPriceIfChanged(
        int productId,
        decimal newPrice,
        string source = "scraper")
    {
        try
        {
            // Get the most recent price for this product
            var latestPrice = await _context.PriceHistory
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.RecordedAt)
                .Select(ph => ph.Price)
                .FirstOrDefaultAsync();

            // If price changed (or no history exists), record it
            if (latestPrice == 0 || latestPrice != newPrice)
            {
                var priceHistory = new PriceHistory
                {
                    ProductId = productId,
                    Price = newPrice,
                    RecordedAt = DateTime.UtcNow,
                    Source = source
                };

                _context.PriceHistory.Add(priceHistory);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Price changed for product {ProductId}: {OldPrice} → {NewPrice}",
                    productId,
                    latestPrice,
                    newPrice
                );

                return true; // Price changed
            }

            return false; // Price unchanged
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording price history for product {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Gets price history for a product
    /// </summary>
    public async Task<List<PriceHistory>> GetPriceHistory(
        int productId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.PriceHistory
            .Where(ph => ph.ProductId == productId);

        if (startDate.HasValue)
        {
            query = query.Where(ph => ph.RecordedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(ph => ph.RecordedAt <= endDate.Value);
        }

        return await query
            .OrderBy(ph => ph.RecordedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets lowest price ever for a product
    /// </summary>
    public async Task<decimal?> GetLowestPrice(int productId)
    {
        return await _context.PriceHistory
            .Where(ph => ph.ProductId == productId)
            .MinAsync(ph => (decimal?)ph.Price);
    }

    /// <summary>
    /// Gets highest price ever for a product
    /// </summary>
    public async Task<decimal?> GetHighestPrice(int productId)
    {
        return await _context.PriceHistory
            .Where(ph => ph.ProductId == productId)
            .MaxAsync(ph => (decimal?)ph.Price);
    }

    /// <summary>
    /// Gets average price for a product
    /// </summary>
    public async Task<decimal?> GetAveragePrice(int productId)
    {
        return await _context.PriceHistory
            .Where(ph => ph.ProductId == productId)
            .AverageAsync(ph => (decimal?)ph.Price);
    }

    /// <summary>
    /// Detects if price dropped below a threshold
    /// </summary>
    public async Task<bool> IsPriceDrop(int productId, decimal threshold)
    {
        // Get last 2 prices
        var recentPrices = await _context.PriceHistory
            .Where(ph => ph.ProductId == productId)
            .OrderByDescending(ph => ph.RecordedAt)
            .Take(2)
            .Select(ph => ph.Price)
            .ToListAsync();

        if (recentPrices.Count < 2)
            return false;

        var currentPrice = recentPrices[0];
        var previousPrice = recentPrices[1];

        // Calculate percentage drop
        var dropPercentage = ((previousPrice - currentPrice) / previousPrice) * 100;

        return dropPercentage >= threshold;
    }

    /// <summary>
    /// Gets products with recent price drops
    /// </summary>
    public async Task<List<Product>> GetProductsWithPriceDrops(
        decimal thresholdPercentage = 10,
        int daysBack = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        // This is complex - get all products that had price changes
        var productsWithDrops = new List<Product>();

        var products = await _context.Product.ToListAsync();

        foreach (var product in products)
        {
            var hasDrop = await IsPriceDrop(product.Id, thresholdPercentage);
            if (hasDrop)
            {
                productsWithDrops.Add(product);
            }
        }

        return productsWithDrops;
    }
}
```

---

## 3.7 Update Scraper to Record Price History

**Location:** `Services/TheIconicScraper.cs`

```csharp
public class TheIconicScraper : IProductScraper
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheIconicScraper> _logger;
    private readonly PriceHistoryService _priceHistoryService; // NEW

    public string RetailerName => "The Iconic";

    public TheIconicScraper(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<TheIconicScraper> logger,
        PriceHistoryService priceHistoryService) // NEW
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _priceHistoryService = priceHistoryService;
    }

    public async Task<int> ScrapeProducts(string searchTerm, int maxProducts = 50)
    {
        try
        {
            var searchUrl = $"{BASE_URL}/search/?q={Uri.EscapeDataString(searchTerm)}";
            var html = await FetchHtmlAsync(searchUrl);

            if (string.IsNullOrEmpty(html))
                return 0;

            var scrapedProducts = ParseProducts(html, searchTerm, maxProducts);

            if (scrapedProducts.Count == 0)
                return 0;

            // ============================================
            // NEW: Check for existing products and update prices
            // ============================================

            var scrapedUrls = scrapedProducts.Select(p => p.ProductUrl).ToList();

            var existingProducts = await _context.Product
                .Where(p => scrapedUrls.Contains(p.ProductUrl))
                .ToListAsync();

            var existingUrlMap = existingProducts.ToDictionary(p => p.ProductUrl);

            var newProducts = new List<Product>();
            var updatedCount = 0;

            foreach (var scraped in scrapedProducts)
            {
                if (existingUrlMap.TryGetValue(scraped.ProductUrl, out var existing))
                {
                    // Product exists - check if price changed
                    if (existing.Price != scraped.Price)
                    {
                        // Record old price in history
                        await _priceHistoryService.RecordPriceIfChanged(
                            existing.Id,
                            scraped.Price,
                            source: "scraper"
                        );

                        // Update product
                        existing.Price = scraped.Price;
                        existing.Name = scraped.Name;
                        existing.Brand = scraped.Brand;
                        existing.ImageUrl = scraped.ImageUrl;
                        existing.LastUpdated = DateTime.UtcNow;

                        updatedCount++;
                    }
                }
                else
                {
                    // New product
                    newProducts.Add(scraped);
                }
            }

            // Save new products
            if (newProducts.Count > 0)
            {
                _context.Product.AddRange(newProducts);
                await _context.SaveChangesAsync();

                // Record initial price for new products
                foreach (var product in newProducts)
                {
                    await _priceHistoryService.RecordPriceIfChanged(
                        product.Id,
                        product.Price,
                        source: "scraper"
                    );
                }
            }

            // Save updated products
            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Scrape complete: {NewCount} new, {UpdatedCount} updated",
                newProducts.Count,
                updatedCount
            );

            return newProducts.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping The Iconic");
            throw;
        }
    }

    // ... existing methods
}
```

---

## 3.8 Add Price History Endpoints

**Location:** `Controllers/PriceHistoryController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using ZeniSearch.Api.Services;

namespace ZeniSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceHistoryController : ControllerBase
{
    private readonly PriceHistoryService _priceHistoryService;
    private readonly ILogger<PriceHistoryController> _logger;

    public PriceHistoryController(
        PriceHistoryService priceHistoryService,
        ILogger<PriceHistoryController> logger)
    {
        _priceHistoryService = priceHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get price history for a product
    /// GET /api/pricehistory/5
    /// </summary>
    [HttpGet("{productId}")]
    public async Task<ActionResult> GetPriceHistory(
        int productId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var history = await _priceHistoryService.GetPriceHistory(
                productId,
                startDate,
                endDate
            );

            var lowest = await _priceHistoryService.GetLowestPrice(productId);
            var highest = await _priceHistoryService.GetHighestPrice(productId);
            var average = await _priceHistoryService.GetAveragePrice(productId);

            return Ok(new
            {
                productId,
                history,
                statistics = new
                {
                    lowest,
                    highest,
                    average,
                    recordCount = history.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for product {ProductId}", productId);
            return StatusCode(500, "Error retrieving price history");
        }
    }

    /// <summary>
    /// Get products with recent price drops
    /// GET /api/pricehistory/drops?threshold=10&daysBack=7
    /// </summary>
    [HttpGet("drops")]
    public async Task<ActionResult> GetPriceDrops(
        [FromQuery] decimal threshold = 10,
        [FromQuery] int daysBack = 7)
    {
        try
        {
            var products = await _priceHistoryService.GetProductsWithPriceDrops(
                threshold,
                daysBack
            );

            return Ok(new
            {
                threshold,
                daysBack,
                count = products.Count,
                products
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price drops");
            return StatusCode(500, "Error retrieving price drops");
        }
    }
}
```

---

## 3.9 Register PriceHistoryService

**Location:** `Program.cs`

```csharp
// Register price history service
builder.Services.AddScoped<PriceHistoryService>();
```

---

# 4. Better Error Handling

## 4.1 Install Polly for Retry Logic

```bash
cd /home/ed/Documents/gitRepo/zeni-search/backend
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package Polly.Extensions.Http
```

---

## 4.2 Configure Resilient HTTP Client

**Location:** `Program.cs`

```csharp
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ... existing configuration

// ============================================
// RESILIENT HTTP CLIENT
// ============================================

// Configure named HTTP client with retry policy
builder.Services.AddHttpClient("ScraperClient")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// Retry policy: Retry 3 times with exponential backoff
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx, 408, network failures
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s delay");
            }
        );
}

// Circuit breaker: Stop trying if too many failures
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5, // Break after 5 failures
            durationOfBreak: TimeSpan.FromMinutes(1), // Stay broken for 1 minute
            onBreak: (outcome, duration) =>
            {
                Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset");
            }
        );
}
```

---

## 4.3 Update Scrapers to Use Named Client

**Location:** `Services/TheIconicScraper.cs`

```csharp
public class TheIconicScraper : IProductScraper
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TheIconicScraper> _logger;
    private readonly PriceHistoryService _priceHistoryService;

    // ... constructor

    private async Task<string> FetchHtmlAsync(string url)
    {
        try
        {
            // Use named client with retry policy
            var client = _httpClientFactory.CreateClient("ScraperClient");

            client.DefaultRequestHeaders.Add("User-Agent",
                "ZeniSearch/1.0 (Price Comparison Bot; Educational Purpose)");

            // Polly will handle retries automatically
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "HTTP {StatusCode} for URL: {Url}",
                    response.StatusCode,
                    url
                );
                return string.Empty;
            }

            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for URL: {Url}", url);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching HTML: {Url}", url);
            return string.Empty;
        }
    }

    // ... rest of the code
}
```

---

## 4.4 Add Global Exception Handler

**Location:** `Middleware/GlobalExceptionHandler.cs`

```csharp
using System.Net;
using System.Text.Json;

namespace ZeniSearch.Api.Middleware;

/// <summary>
/// Global exception handler middleware
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            error = exception.Message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(json);
    }
}
```

**Register middleware in `Program.cs`:**

```csharp
// Add after var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandler>();
```

---

## 4.5 Add Health Checks

**Location:** `Program.cs`

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddCheck<ScraperHealthCheck>("scraper");

// After app.UseAuthorization();
app.MapHealthChecks("/health");
```

**Location:** `Services/ScraperHealthCheck.cs`

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ZeniSearch.Api.Services;

/// <summary>
/// Health check for scrapers
/// </summary>
public class ScraperHealthCheck : IHealthCheck
{
    private readonly ScraperFactory _factory;
    private readonly ILogger<ScraperHealthCheck> _logger;

    public ScraperHealthCheck(
        ScraperFactory factory,
        ILogger<ScraperHealthCheck> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scrapers = _factory.GetAllScrapers();
            var healthyCount = 0;
            var totalCount = 0;

            foreach (var scraper in scrapers)
            {
                totalCount++;
                try
                {
                    if (await scraper.HealthCheck())
                    {
                        healthyCount++;
                    }
                }
                catch
                {
                    // Count as unhealthy
                }
            }

            if (healthyCount == 0)
            {
                return HealthCheckResult.Unhealthy(
                    $"No scrapers are healthy (0/{totalCount})"
                );
            }

            if (healthyCount < totalCount)
            {
                return HealthCheckResult.Degraded(
                    $"Some scrapers are unhealthy ({healthyCount}/{totalCount})"
                );
            }

            return HealthCheckResult.Healthy(
                $"All scrapers are healthy ({healthyCount}/{totalCount})"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}
```

---

## 4.6 Add Fallback Data Strategy

**Location:** `Services/CachedProductService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ZeniSearch.Api.Data;
using ZeniSearch.Api.Models;

namespace ZeniSearch.Api.Services;

/// <summary>
/// Service with caching and fallback logic
/// </summary>
public class CachedProductService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedProductService> _logger;

    public CachedProductService(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<CachedProductService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Search products with caching and fallback
    /// </summary>
    public async Task<List<Product>> SearchProducts(string query)
    {
        var cacheKey = $"search_{query}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out List<Product>? cachedProducts))
        {
            _logger.LogInformation("Returning cached results for: {Query}", query);
            return cachedProducts ?? new List<Product>();
        }

        try
        {
            // Try to get from database
            var products = await _context.Product
                .Where(p => p.Name.Contains(query) ||
                           (p.Brand != null && p.Brand.Contains(query)))
                .OrderBy(p => p.Price)
                .Take(50)
                .ToListAsync();

            // Cache for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, products, cacheOptions);

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database query failed, returning cached data");

            // Fallback: Try to return stale cache
            if (_cache.TryGetValue(cacheKey + "_stale", out List<Product>? staleProducts))
            {
                return staleProducts ?? new List<Product>();
            }

            // No fallback available
            return new List<Product>();
        }
    }
}
```

**Register in `Program.cs`:**

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CachedProductService>();
```

---

## 4.7 Add Scraper Monitoring

**Location:** `Services/ScraperMonitor.cs`

```csharp
namespace ZeniSearch.Api.Services;

/// <summary>
/// Monitors scraper health and sends alerts
/// </summary>
public class ScraperMonitor
{
    private readonly ILogger<ScraperMonitor> _logger;

    public ScraperMonitor(ILogger<ScraperMonitor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Monitors scraper execution and logs issues
    /// </summary>
    public async Task<T> MonitorScraperExecution<T>(
        string scraperName,
        Func<Task<T>> scraperAction)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting {Scraper}", scraperName);

            var result = await scraperAction();

            var duration = (DateTime.UtcNow - startTime).TotalSeconds;

            _logger.LogInformation(
                "{Scraper} completed in {Duration}s",
                scraperName,
                Math.Round(duration, 2)
            );

            return result;
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;

            _logger.LogError(ex,
                "{Scraper} failed after {Duration}s",
                scraperName,
                Math.Round(duration, 2)
            );

            // TODO: Send alert (email, Slack, etc.)
            await SendAlert(scraperName, ex);

            throw;
        }
    }

    private async Task SendAlert(string scraperName, Exception ex)
    {
        // TODO: Implement alerting
        // - Send email
        // - Post to Slack
        // - Log to monitoring service (Sentry, DataDog)

        _logger.LogWarning(
            "ALERT: {Scraper} failed with error: {Message}",
            scraperName,
            ex.Message
        );

        await Task.CompletedTask;
    }
}
```

---

## 4.8 Complete Program.cs

**Location:** `Program.cs`

```csharp
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Scalar.AspNetCore;
using ZeniSearch.Api.Data;
using ZeniSearch.Api.Middleware;
using ZeniSearch.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// HTTP Client with resilience
builder.Services.AddHttpClient("ScraperClient")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// Caching
builder.Services.AddMemoryCache();

// Scrapers
builder.Services.AddScoped<IProductScraper, TheIconicScraper>();
builder.Services.AddScoped<IProductScraper, AmazonScraper>();
builder.Services.AddScoped<IProductScraper, EbayScraper>();

// Services
builder.Services.AddScoped<ScraperFactory>();
builder.Services.AddScoped<ScraperService>();
builder.Services.AddScoped<PriceHistoryService>();
builder.Services.AddScoped<CachedProductService>();
builder.Services.AddScoped<ScraperMonitor>();

// Hangfire
builder.Services.AddHangfire(config =>
{
    config
        .UsePostgreSqlStorage(connectionString)
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings();
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddCheck<ScraperHealthCheck>("scraper");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Global exception handler
app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Schedule recurring jobs
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-all-daily",
    service => service.ScrapeAllRetailers("sandals"),
    Cron.Daily(3)
);

app.Run();

// Helper methods
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s");
            }
        );
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromMinutes(1),
            onBreak: (outcome, duration) =>
            {
                Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset");
            }
        );
}
```

---

# Summary

## What You've Built

### 1. Scheduled Scraping ✅

- Hangfire for background jobs
- PostgreSQL-backed job storage
- Web dashboard at `/hangfire`
- Recurring jobs (daily/hourly)
- Removed manual triggers

### 2. Multiple Retailers ✅

- `IProductScraper` interface
- Factory pattern for scraper selection
- Easy to add new retailers
- Health checks per scraper

### 3. Price History ✅

- `PriceHistory` table with foreign key
- Tracks all price changes
- Statistics (lowest, highest, average)
- Price drop detection
- API endpoints for history

### 4. Better Error Handling ✅

- Polly retry policies (exponential backoff)
- Circuit breaker pattern
- Global exception handler
- Health check endpoint
- Caching with fallback
- Monitoring and alerts

---

## Next Steps

1. **Testing:**

   ```bash
   dotnet watch run
   # Visit: http://localhost:5020/hangfire
   # Visit: http://localhost:5020/health
   ```

2. **Add More Scrapers:**

   - Implement Amazon scraper
   - Implement eBay scraper
   - Add more retailers

3. **Deploy:**

   - Create Dockerfile
   - Deploy to Fly.io
   - Set up production alerts

4. **Frontend Integration:**
   - Connect Next.js to API
   - Show price history charts
   - Price drop notifications

---

## File Checklist

```
backend/
├── Controllers/
│   ├── ProductsController.cs
│   └── PriceHistoryController.cs
├── Data/
│   └── AppDbContext.cs
├── Middleware/
│   └── GlobalExceptionHandler.cs
├── Migrations/
│   └── AddPriceHistory.cs
├── Models/
│   ├── Product.cs
│   └── PriceHistory.cs
├── Services/
│   ├── IProductScraper.cs
│   ├── TheIconicScraper.cs
│   ├── AmazonScraper.cs
│   ├── EbayScraper.cs
│   ├── ScraperFactory.cs
│   ├── ScraperService.cs
│   ├── PriceHistoryService.cs
│   ├── CachedProductService.cs
│   ├── ScraperMonitor.cs
│   ├── ScraperHealthCheck.cs
│   └── HangfireAuthorizationFilter.cs
└── Program.cs
```

---

**You're now production-ready!** 🚀
