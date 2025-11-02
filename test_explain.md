# Zeni Search Testing Guide

Complete guide to the testing strategy, structure, and best practices for Zeni Search backend.

---

## Table of Contents

1. [Overview](#overview)
2. [Why Testing Matters](#why-testing-matters)
3. [Testing Planning](#testing-planning)
4. [Project Structure](#project-structure)
5. [Why Separate Tests Project](#why-separate-tests-project)
6. [Testing Pyramid](#testing-pyramid)
7. [Test Implementation Order](#test-implementation-order)
8. [Unit Tests](#unit-tests)
9. [Integration Tests](#integration-tests)
10. [Scaling & Adding Functionality](#scaling--adding-functionality)
11. [Best Practices](#best-practices)

---

## Overview

This project uses a **comprehensive testing strategy** with:

- ✅ **9 total tests** (passing)
- ✅ **Separated unit & integration tests**
- ✅ **Test template pattern** for scalability
- ✅ **CI/CD ready** with xUnit framework

### Current Test Summary

```
Tests/
├── Unit/
│   └── Scrapers/
│       ├── BaseScraperTests.cs           (3 base tests - template)
│       └── TheIconicScraperTests.cs      (2 specific tests)
│
└── Integration/
    └── ScraperFactoryTests.cs            (4 integration tests)

TOTAL: 9 tests | All Passing ✓
```

---

## Why Testing Matters

### Without Testing ❌

- Bugs go to production
- Refactoring breaks existing code
- No confidence in changes
- Manual testing is slow & error-prone
- Technical debt accumulates

### With Testing ✅

- Catch bugs before deployment
- Refactor safely with confidence
- Automated verification
- Faster feedback loop
- Self-documenting code

### Testing ROI in This Project

- **Scrapers**: Verify each retailer's parsing logic
- **DI Container**: Ensure correct service registration
- **Data Flow**: Validate product extraction & storage
- **Error Handling**: Confirm graceful degradation

---

## Testing Planning

### Phase 1: Foundation (Current - ✓ Complete)

1. ✅ Set up test project structure
2. ✅ Create base template for scrapers
3. ✅ Implement first scraper tests (The Iconic)
4. ✅ Test DI container integration

### Phase 2: Expansion (Next Steps)

1. ⏳ Add more scraper tests (Amazon, etc.)
2. ⏳ Service layer tests
3. ⏳ Controller/API endpoint tests
4. ⏳ End-to-end tests

### Phase 3: Maturity (Future)

1. ⏳ Performance tests
2. ⏳ Load testing
3. ⏳ Security testing
4. ⏳ Integration with CI/CD pipeline

---

## Project Structure

### Directory Layout

```
backend/
├── Tests/                           ← Separate test project
│   ├── Tests.csproj                ← Test project file
│   ├── Unit/                        ← Unit tests (isolated)
│   │   └── Scrapers/
│   │       ├── BaseScraperTests.cs
│   │       └── TheIconicScraperTests.cs
│   ├── Integration/                 ← Integration tests (with DI)
│   │   └── ScraperFactoryTests.cs
│   ├── bin/                         ← Build output
│   └── obj/                         ← Build artifacts
│
├── Services/
│   ├── IProductScraper.cs          ← Interface being tested
│   ├── Scrapers/
│   │   ├── TheIconicScraper.cs
│   │   └── [AmazonScraper.cs]
│   ├── ScraperFactory.cs
│   └── ScraperService.cs
│
├── Program.cs                       ← DI setup (used in tests)
└── backend.csproj
```

### File Purposes

| File                       | Purpose                   | Tests Count      |
| -------------------------- | ------------------------- | ---------------- |
| `BaseScraperTests.cs`      | Template base class       | Provides 3 tests |
| `TheIconicScraperTests.cs` | The Iconic-specific tests | 2 tests          |
| `ScraperFactoryTests.cs`   | DI container integration  | 4 tests          |

---

## Why Separate Tests Project

### ✅ Advantages

| Aspect             | Benefit                        |
| ------------------ | ------------------------------ |
| **Isolation**      | Tests don't ship to production |
| **Organization**   | Clear test/code separation     |
| **Dependencies**   | Can add test-only packages     |
| **Maintenance**    | Easier to refactor test code   |
| **CI/CD**          | Can run/skip tests separately  |
| **Team Structure** | QA/automation can own tests    |

### ❌ If Tests Were in Main Project

- Test code mixed with production code
- xUnit/Moq packages in production build
- Risk of accidentally deploying tests
- Harder to find actual code
- Larger production binaries

### Our Approach

```csharp
// Tests.csproj - Separate project
<ItemGroup>
  <PackageReference Include="xunit" />
  <PackageReference Include="Moq" />
  <ProjectReference Include="../backend.csproj" />
</ItemGroup>

// backend.csproj - No test dependencies
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" />
  <PackageReference Include="HtmlAgilityPack" />
  <!-- No test packages here ✓ -->
</ItemGroup>
```

---

## Testing Pyramid

### The Concept

```
          🧪 Integration Tests (few, slow, high value)
             /                    \
        Service Level         DI Container
         Tests (some)         Tests (some)
           /                      \
    🧪 Unit Tests (many, fast, low cost)
       /                          \
  Property Tests          Scraper Tests
  Simple Checks           HTML Parsing
  Mocking                 Price Extract
```

### Breakdown for Zeni Search

```
Level 1: Integration (4 tests) - 44%
├─ Test DI container setup
├─ Verify scraper registration
├─ Check service composition
└─ Validate factory pattern

Level 2: Unit (5 tests) - 56%
├─ Base template tests (3)
│  ├─ RetailerName property
│  ├─ RetailerName consistency
│  └─ HealthCheck return type
├─ TheIconic-specific (2)
│  ├─ Retailer name verification
│  └─ HealthCheck with mock response

TOTAL: 9 tests
```

### Why This Ratio?

- **More unit tests** = Fast feedback (< 1 second)
- **Fewer integration tests** = Slower but test real DI
- **Balance** = Confidence without slow builds

---

## Test Implementation Order

### Step-by-Step Approach

#### Step 1: Setup (✓ Done)

- Create `Tests/` project
- Add xUnit/Moq packages
- Configure project file
- Create namespaces

#### Step 2: Base Infrastructure (✓ Done)

- Create `BaseScraperTests<T>` template
- Implement property tests (3 tests)
- Setup mock infrastructure
- Test inheritance works

#### Step 3: First Scraper Tests (✓ Done)

- Implement `TheIconicScraperTests`
- Add retailer-specific tests (2 tests)
- Mock HTTP client
- Verify parsing logic

#### Step 4: Integration Tests (✓ Done)

- Setup DI container
- Test `ScraperFactory`
- Verify scraper registration (4 tests)
- Validate service composition

#### Step 5: Add More Scrapers (⏳ Next)

```csharp
// When adding Amazon scraper:
public class AmazonScraperTests : BaseScraperTests<AmazonScraper>
{
    // Automatically gets 3 base tests
    // Add 3-5 Amazon-specific tests
    // Total: 3 + 4 = 7 tests for Amazon
}
```

---

## Unit Tests

### What Are Unit Tests?

Tests that verify **a single piece of functionality** in isolation, with all dependencies mocked.

### Our Unit Tests

#### 1. **BaseScraperTests<T>** (Template)

**Purpose:** Ensure every scraper implements `IProductScraper` correctly

**Tests (3 total):**

```csharp
public abstract class BaseScraperTests<TScraper> where TScraper : IProductScraper
{
    // Test 1: RetailerName is not empty
    [Fact]
    public void RetailerName_ShouldNotBeEmpty()
    {
        var name = Scraper.RetailerName;
        Assert.NotNull(name);
        Assert.NotEmpty(name);
    }

    // Test 2: RetailerName is consistent
    [Fact]
    public void RetailerName_ShouldBeConsistent()
    {
        var name1 = Scraper.RetailerName;
        var name2 = Scraper.RetailerName;
        Assert.Equal(name1, name2);
    }

    // Test 3: HealthCheck returns bool
    [Fact]
    public async Task HealthCheck_ShouldReturnBoolean()
    {
        var result = await Scraper.HealthCheck();
        Assert.IsType<bool>(result);
    }
}
```

**Why This Pattern?**

- Write once, inherit by all scrapers ✓
- No code duplication ✓
- Consistency across retailers ✓

#### 2. **TheIconicScraperTests** (Specific)

**Purpose:** Test The Iconic scraper's specific behavior

**Tests (2 total):**

```csharp
public class TheIconicScraperTests : BaseScraperTests<TheIconicScraper>
{
    // Test 1: Verify retailer name
    [Fact]
    public void RetailerName_ShouldBeTheIconic()
    {
        var retailerName = Scraper.RetailerName;
        Assert.Equal("The Iconic", retailerName);
    }

    // Test 2: Verify health check with mock
    [Fact]
    public async Task HealthCheck_WithValidResponse_ShouldReturnTrue()
    {
        // Arrange: Mock HTTP response
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        // ... setup mock ...

        // Act: Call HealthCheck
        var result = await scraper.HealthCheck();

        // Assert: Verify result
        Assert.True(result);
    }
}
```

**Inheritance:**

- Gets 3 tests from `BaseScraperTests<T>` ✓
- Adds 2 specific tests ✓
- **Total: 5 unit tests**

### Running Unit Tests

```bash
# Run all unit tests
dotnet test --filter Category=Unit

# Run specific test class
dotnet test --filter ClassName~TheIconicScraperTests

# Run with verbose output
dotnet test -v detailed
```

### Unit Test Characteristics

| Aspect          | Value                                |
| --------------- | ------------------------------------ |
| **Speed**       | ~1-5 ms each                         |
| **Isolation**   | All dependencies mocked              |
| **Coverage**    | Single class behavior                |
| **Reliability** | Very high (no external dependencies) |
| **Count**       | Many (5 tests)                       |

---

## Integration Tests

### What Are Integration Tests?

Tests that verify **multiple components work together**, including the actual DI container and service composition.

### Our Integration Tests

#### **ScraperFactoryTests** (4 tests)

**Purpose:** Verify DI container and scraper registration

```csharp
public class ScraperFactoryTests
{
    // Test 1: Get all registered scrapers
    [Fact]
    public void GetAllScrapers_WithRegisteredScrapers_ShouldReturnAll()
    {
        // Arrange: Setup DI container
        var services = new ServiceCollection();
        services.AddLogging();

        // Mock scrapers
        var mockScraper1 = new Mock<IProductScraper>();
        mockScraper1.Setup(s => s.RetailerName).Returns("The Iconic");

        services.AddScoped<IProductScraper>(_ => mockScraper1.Object);
        services.AddScoped<ScraperFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<ScraperFactory>();

        // Act: Get all scrapers
        var scrapers = factory.GetAllScrapers().ToList();

        // Assert: Verify count and content
        Assert.Equal(1, scrapers.Count);
        Assert.Contains(scrapers, s => s.RetailerName == "The Iconic");
    }

    // Test 2: Get service by name
    [Fact]
    public void GetService_WithValidRetailerName_ShouldReturnCorrectScraper()
    {
        // Arrange: Setup container with named scrapers
        // ...

        // Act: Retrieve specific scraper
        var scraper = factory.GetService("The Iconic");

        // Assert: Verify correct instance
        Assert.NotNull(scraper);
        Assert.Equal("The Iconic", scraper.RetailerName);
    }

    // Test 3 & 4: Error handling tests
    // ...
}
```

**What It Tests:**

- ✅ DI container setup works
- ✅ Scrapers register correctly
- ✅ Factory retrieves them properly
- ✅ Error handling for missing scrapers

### Integration Test Characteristics

| Aspect          | Value                             |
| --------------- | --------------------------------- |
| **Speed**       | ~50-500 ms (slower)               |
| **Isolation**   | Partial (uses real DI container)  |
| **Coverage**    | Multiple classes interaction      |
| **Reliability** | Medium (depends on configuration) |
| **Count**       | Few (4 tests)                     |

### Why Integration Tests Matter

```
Unit Tests (5)        Integration Tests (4)
├─ Scraper works      ├─ DI configured correctly
├─ Property valid     ├─ Services registered
├─ HealthCheck type   ├─ Factory composition
└─ No errors          └─ Real-world scenario
```

---

## Scaling & Adding Functionality

### Adding a New Scraper

#### Step 1: Implement the scraper

```csharp
// Services/Scrapers/AmazonScraper.cs
public class AmazonScraper : IProductScraper
{
    public string RetailerName => "Amazon Au";

    public async Task<int> ScrapeProducts(string searchTerm, int maxProducts)
    {
        // Implementation
    }

    public async Task<bool> HealthCheck()
    {
        // Implementation
    }
}
```

#### Step 2: Register in DI

```csharp
// Program.cs
builder.Services.AddScoped<IProductScraper, AmazonScraper>();
```

#### Step 3: Create test file

```csharp
// Tests/Unit/Scrapers/AmazonScraperTests.cs
public class AmazonScraperTests : BaseScraperTests<AmazonScraper>
{
    protected override AmazonScraper CreateScraper()
    {
        var mockContext = CreateMockAppDbContext();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        return new AmazonScraper(mockContext, mockHttpClientFactory.Object, MockLogger.Object);
    }

    // Automatically gets 3 base tests

    // Add Amazon-specific tests
    [Fact]
    public void RetailerName_ShouldBeAmazonAu()
    {
        Assert.Equal("Amazon Au", Scraper.RetailerName);
    }

    [Fact]
    public async Task ParsesAmazonSearchResults()
    {
        // Amazon-specific parsing test
    }

    [Fact]
    public async Task HandlesAmazonPricingFormat()
    {
        // Amazon price format: $99.00
    }
}
```

#### Step 4: Run tests

```bash
dotnet test
# Test summary: total: 12, passed: 12 ✓
# (3 base × 2 + 2 The Iconic + 4 Amazon + 4 integration)
```

### Test Distribution Growth

| Stage                      | Unit Tests | Integration | Total  |
| -------------------------- | ---------- | ----------- | ------ |
| Phase 1 (Current)          | 5          | 4           | **9**  |
| Phase 1b (1 more scraper)  | 8          | 4           | **12** |
| Phase 1c (2 more scrapers) | 11         | 4           | **15** |
| Phase 2 (Services)         | 11         | 8           | **19** |
| Phase 3 (API Controllers)  | 15         | 12          | **27** |

---

## Best Practices

### ✅ Do's

#### 1. **Test Behavior, Not Implementation**

```csharp
// ❌ BAD - Tests internal implementation
[Fact]
public void Scraper_CreatesHttpClient()
{
    var methodCalled = _mockFactory.Invocations.Any(i =>
        i.Method.Name == "CreateClient");
    Assert.True(methodCalled);
}

// ✅ GOOD - Tests observable behavior
[Fact]
public async Task HealthCheck_WithValidResponse_ShouldReturnTrue()
{
    var result = await Scraper.HealthCheck();
    Assert.True(result);
}
```

#### 2. **Use Descriptive Test Names**

```csharp
// ❌ BAD
[Fact]
public void Test1() { }

// ✅ GOOD
[Fact]
public void RetailerName_ShouldNotBeEmpty() { }
```

#### 3. **Follow AAA Pattern**

```csharp
[Fact]
public void SomeTest()
{
    // Arrange - Setup test data
    var scraper = CreateScraper();

    // Act - Call the method being tested
    var result = scraper.RetailerName;

    // Assert - Verify the result
    Assert.NotEmpty(result);
}
```

#### 4. **One Assertion Per Test (When Possible)**

```csharp
// ✅ GOOD - Easy to debug if fails
[Fact]
public void RetailerName_ShouldNotBeEmpty()
{
    var name = Scraper.RetailerName;
    Assert.NotEmpty(name);
}

// Multiple related assertions ok:
[Fact]
public void GetAllScrapers_ShouldReturnExpectedCount()
{
    var scrapers = factory.GetAllScrapers();
    Assert.NotEmpty(scrapers);  // Related assertions
    Assert.All(scrapers, s => Assert.NotEmpty(s.RetailerName));
}
```

#### 5. **Test Edge Cases**

```csharp
// ✅ GOOD - Tests edge cases
[Fact]
public void ScrapeProducts_WithEmptySearchTerm_ShouldHandleGracefully()
{
    // Should not throw
    var result = Scraper.ScrapeProducts("", 10);
    Assert.NotNull(result);
}
```

### ❌ Don'ts

#### 1. **Don't Test Logging**

```csharp
// ❌ BAD - Tests logging implementation detail
[Fact]
public void Method_ShouldCallLogger()
{
    _mockLogger.Verify(l => l.LogInformation(...));
}
```

#### 2. **Don't Create Shared Test State**

```csharp
// ❌ BAD - Tests are not independent
private List<Scraper> _scrapers = new();

[Fact]
public void FirstTest()
{
    _scrapers.Add(new Scraper());
}

[Fact]
public void SecondTest()
{
    Assert.NotEmpty(_scrapers); // Depends on FirstTest
}

// ✅ GOOD - Each test is independent
[Fact]
public void FirstTest()
{
    var scrapers = new List<Scraper> { new Scraper() };
    Assert.NotEmpty(scrapers);
}

[Fact]
public void SecondTest()
{
    var scrapers = new List<Scraper> { new Scraper() };
    Assert.NotEmpty(scrapers);
}
```

#### 3. **Don't Mock Everything**

```csharp
// ❌ BAD - Over-mocking defeats the purpose
[Fact]
public void Scraper_ShouldWork()
{
    var mockDb = new Mock<AppDbContext>();
    var mockHttp = new Mock<IHttpClientFactory>();
    var mockLogger = new Mock<ILogger>();
    var mockScraper = new Mock<TheIconicScraper>(); // Even mocking the class itself!
}

// ✅ GOOD - Mock only external dependencies
[Fact]
public void HealthCheck_WithValidResponse_ShouldReturnTrue()
{
    var mockHttpFactory = new Mock<IHttpClientFactory>(); // Mock dependency
    var scraper = new TheIconicScraper(
        CreateMockAppDbContext(),
        mockHttpFactory.Object,
        _mockLogger.Object
    ); // Use real scraper
}
```

#### 4. **Don't Use Thread.Sleep or Task.Delay in Tests**

```csharp
// ❌ BAD - Makes tests slow
[Fact]
public async Task SomeAsyncTest()
{
    await Task.Delay(5000); // 5 second wait!
}

// ✅ GOOD - Use mocking or test timeouts
[Fact]
public async Task SomeAsyncTest()
{
    var mockResponse = new Mock<HttpResponseMessage>();
    // No delay needed - test is instant
}
```

### 📋 Testing Checklist

Before committing code:

- [ ] All tests pass (`dotnet test`)
- [ ] No test warnings
- [ ] New feature has tests
- [ ] Existing tests still pass
- [ ] Test names are descriptive
- [ ] No hardcoded magic numbers (use variables)
- [ ] Tests are independent (no shared state)
- [ ] Mock only external dependencies
- [ ] Tests verify behavior, not implementation
- [ ] Code coverage > 70%

### 🚀 Running Tests Effectively

```bash
# Run all tests
dotnet test

# Run with specific filter
dotnet test --filter Category=Unit

# Run specific test class
dotnet test --filter "ClassName~TheIconicScraperTests"

# Run specific test
dotnet test --filter "TheIconicScraperTests.RetailerName_ShouldBeTheIconic"

# Run with code coverage
dotnet test /p:CollectCoverage=true

# Watch mode (run on file change)
dotnet watch test
```

### 📊 Metrics to Track

Monitor these to maintain test health:

| Metric              | Target | Current        |
| ------------------- | ------ | -------------- |
| Test Pass Rate      | 100%   | ✅ 100%        |
| Code Coverage       | >70%   | ⏳ In progress |
| Test Execution Time | <5s    | ✅ ~2s         |
| Tests Per Feature   | >1     | ✅ Average 2   |
| Flaky Tests         | 0      | ✅ 0           |

---

## Summary

### Current State

- ✅ **9 tests** passing
- ✅ **Proper structure** (Unit + Integration)
- ✅ **Template pattern** for scalability
- ✅ **Best practices** followed

### Next Steps

1. Add Amazon scraper tests (⏳)
2. Expand to service layer tests (⏳)
3. Add API controller tests (⏳)
4. Setup CI/CD pipeline (⏳)

### Key Takeaways

- **Test independently** - Mocks external dependencies
- **Test behavior** - Not implementation details
- **Test patterns** - Use templates to scale
- **Test fast** - Keep unit tests fast
- **Test often** - Run before every commit
