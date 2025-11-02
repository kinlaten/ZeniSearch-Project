// ==================================================================================
// TEMPLATE: Use this as a starting point when creating tests for a new scraper
// ==================================================================================
//
// STEP 1: Create a new file in the Scrapers directory, e.g., YourScraperTests.cs
//
// STEP 2: Copy the code below and replace "YourScraper" with your scraper name
//
// STEP 3: Implement CreateScraper() to instantiate your scraper
//
// STEP 4: Add scraper-specific tests
//
// STEP 5: Run: dotnet test
//
// ==================================================================================

/*

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ZeniSearch.Api.Services.Scrapers;
using ZeniSearch.Api.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZeniSearch.Api.Tests.Unit.Scrapers;

/// <summary>
/// Tests specific to YourScraper implementation.
/// 
/// Automatically inherits 6 common tests from BaseScraperTests:
/// ✓ RetailerName_ShouldNotBeEmpty
/// ✓ RetailerName_ShouldBeConsistent
/// ✓ HealthCheck_ShouldReturnBoolean
/// ✓ ScrapeProducts_WithValidSearchTerm_ShouldReturnInt
/// ✓ ScrapeProducts_WithEmptySearchTerm_ShouldHandleGracefully
/// ✓ ScrapeProducts_WithNegativeMaxProducts_ShouldHandleGracefully
/// 
/// Plus your scraper-specific tests (add below).
/// </summary>
public class YourScraperTests : BaseScraperTests<YourScraper>
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public YourScraperTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    /// <summary>
    /// REQUIRED: Implement this method to create an instance of your scraper.
    /// This is called by the BaseScraperTests constructor.
    /// </summary>
    protected override YourScraper CreateScraper()
    {
        var mockContext = CreateMockAppDbContext();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        return new YourScraper(mockContext, mockHttpClientFactory.Object, MockLogger.Object);
    }

    // ========================
    // SPECIFIC TESTS (YourScraper Only)
    // ========================

    /// <summary>
    /// Test that your scraper identifies itself correctly.
    /// </summary>
    [Fact]
    public void RetailerName_ShouldBeYourRetailer()
    {
        // Act
        var retailerName = Scraper.RetailerName;

        // Assert
        Assert.Equal("Your Retailer Name", retailerName);
    }

    /// <summary>
    /// Test your scraper's specific HTML parsing logic.
    /// Include a sample HTML snippet and verify extraction works.
    /// </summary>
    [Fact]
    public async Task ParsesYourRetailersHtmlStructureCorrectly()
    {
        // TODO: Implement
        // Arrange: Create sample HTML from your retailer
        // Act: Call scraper methods
        // Assert: Verify products extracted correctly
    }

    /// <summary>
    /// Test your scraper handles your retailer's price format.
    /// E.g., $99.00, €49,99, ¥1000, etc.
    /// </summary>
    [Fact]
    public async Task HandlesYourRetailersPricingFormat()
    {
        // TODO: Implement
        // Test price parsing with your retailer's format
    }

    /// <summary>
    /// Test your scraper handles your retailer's URL structure.
    /// E.g., product URLs, search URLs, pagination, etc.
    /// </summary>
    [Fact]
    public async Task HandlesYourRetailersUrlStructure()
    {
        // TODO: Implement
        // Test URL parsing/normalization
    }

    // Add more tests as needed for your specific retailer's edge cases
}

*/

// ==================================================================================
// EXAMPLE: How this looks in practice (Amazon)
// ==================================================================================

/*

public class AmazonScraperTests : BaseScraperTests<AmazonScraper>
{
    protected override AmazonScraper CreateScraper()
    {
        var mockContext = CreateMockAppDbContext();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        return new AmazonScraper(mockContext, mockHttpClientFactory.Object, MockLogger.Object);
    }

    [Fact]
    public void RetailerName_ShouldBeAmazonAu()
    {
        Assert.Equal("Amazon Au", Scraper.RetailerName);
    }

    [Fact]
    public async Task ParsesAmazonSearchResultsCorrectly()
    {
        // Amazon HTML: //div[@role='listitem']
        // Test extraction of products from Amazon's structure
    }

    [Fact]
    public async Task HandlesAmazonPricingFormat()
    {
        // Amazon prices: $99.00, with RRP
        // Test price extraction and parsing
    }

    [Fact]
    public async Task HandlesAmazonProductUrls()
    {
        // Amazon URLs: /dp/B08B48M51T/ref=sr_1_5...
        // Test URL normalization
    }
}

*/

// ==================================================================================
// TEST DISTRIBUTION
// ==================================================================================
//
// With 2 scrapers (The Iconic + Amazon):
//
//   Base Tests:              6 tests × 2 = 12 tests
//   ├─ TheIconic:            2 specific tests
//   └─ Amazon:               4 specific tests
//   
//   TOTAL: 12 + 2 + 4 = 18 tests
//
// With 3 scrapers (add Ebay):
//
//   Base Tests:              6 tests × 3 = 18 tests
//   ├─ TheIconic:            2 specific tests
//   ├─ Amazon:               4 specific tests
//   └─ Ebay:                 3 specific tests
//
//   TOTAL: 18 + 2 + 4 + 3 = 27 tests
//
// ==================================================================================
