# Zeni Search - Sandal Price Comparison Engine for Australian Market

## Project Overview

Building a search engine to find the best prices for sandals across Australian e-commerce retailers using Next.js (Frontend) and ASP.NET Core (Backend).

---

## 🚀 Quick Start for Solo Developers

**If you're building this alone, START SIMPLE!**

**✅ Deployment Choice: Fly.io + Docker** (chosen path)

This document contains a comprehensive roadmap with 11 phases, but for a solo developer, you should:

1. **Skip to**: [Ultra-Minimal MVP Timeline](#estimated-timeline) (2-4 weeks)
2. **Skip to**: [Fly.io Deployment Guide](#flyio-deployment-guide-your-chosen-path) for detailed setup
3. **Focus on**: 1 retailer, basic search, Docker deployment to Fly.io
4. **Skip initially**: User accounts, price alerts, multiple retailers, caching, background jobs

**Core Philosophy:**

- Build the smallest version that proves the concept works
- Get something live quickly (even with just 50 products from 1 retailer)
- Validate there's demand before investing months
- Add complexity only when needed

**Your First 2 Weeks Should Produce:**

- A working search for "sandals"
- Results showing products from ONE retailer
- Links that take users to buy the product
- Deployed and accessible online (free hosting)

**Then expand incrementally:**

- Week 3: Add a second retailer
- Week 4: Add price sorting and basic filters
- Week 5+: Automate scraping, add more features

The detailed phases below are your **reference guide** for when you're ready to scale up. Don't try to build everything at once!

---

## Phase 1: Research & Planning

### 1.1 Market Research

- **Identify Target Retailers**
  - Major Australian retailers: The Iconic, Catch.com.au, Myer, David Jones, Rebel Sport, Athlete's Foot
  - Amazon.com.au
  - Specialty footwear retailers: Platypus, Hype DC, Stylerunner
  - Budget retailers: Kmart, Target, Big W
- **Competitive Analysis**
  - Study existing price comparison sites: Google Shopping, Shopbot, PriceMe (NZ), GetPrice
  - Understand monetization models (affiliate links, sponsored listings, ads)

### 1.2 Legal & Ethical Considerations

- **Must Research:**
  - Robots.txt compliance for each target website
  - Terms of Service (ToS) for web scraping legality
  - Australian Consumer Law (ACL) requirements for price advertising
  - GDPR/Privacy Act compliance for user data
  - Copyright considerations for product images and descriptions
- **Resources:**
  - ACCC (Australian Competition & Consumer Commission) guidelines: https://www.accc.gov.au/
  - Web scraping legal guide: https://blog.apify.com/is-web-scraping-legal/
  - Robots.txt specification: https://www.robotstxt.org/

---

## Phase 2: System Architecture Design

### 2.1 High-Level Architecture

```
User Browser
    ↓
Next.js Frontend (SSR/SSG)
    ↓
ASP.NET Core Web API
    ↓
┌─────────────┬──────────────┬────────────────┐
│   Database  │  Cache Layer │  Queue System  │
│  (SQL/NoSQL)│    (Redis)   │  (RabbitMQ/    │
│             │              │   Hangfire)    │
└─────────────┴──────────────┴────────────────┘
    ↑
Scraping Workers/Services
```

### 2.2 Technology Stack Details

**Frontend (Next.js)**

- Next.js 14+ with App Router
- TypeScript for type safety
- UI Library: Tailwind CSS + shadcn/ui or Material-UI
- State Management: React Context or Zustand
- API Communication: Fetch API or Axios
- SEO optimization with Next.js metadata API

**Backend (ASP.NET Core)**

- ASP.NET Core 8.0+ Web API
- Entity Framework Core for ORM
- Authentication: JWT tokens
- Background jobs: Hangfire or Quartz.NET
- API Documentation: Swagger/OpenAPI

**Database**

- **Primary: PostgreSQL** (recommended - open-source, powerful, works everywhere)
- Alternative: SQL Server (if committed to Microsoft ecosystem)
- Search: Elasticsearch or Azure Cognitive Search (for fast full-text search) - **Skip for MVP, use PostgreSQL full-text**
- Cache: Redis (for frequently accessed data, rate limiting) - **Skip for Ultra-Minimal MVP**
- MongoDB: Could work but PostgreSQL is better for structured product data

**For Solo Developers:** Start with just PostgreSQL. It can handle search, caching (with indexes), and all your data needs for the first 10K-100K products.

**Scraping Infrastructure**

- C# with HtmlAgilityPack, AngleSharp, or Selenium WebDriver
- Alternatively: Python with Scrapy/BeautifulSoup called from ASP.NET
- Proxy rotation service (if needed): Bright Data, ScraperAPI
- User-agent rotation
- Rate limiting and respectful scraping

### 2.3 Key Resources

- **Next.js Documentation**: https://nextjs.org/docs
- **ASP.NET Core Documentation**: https://learn.microsoft.com/en-us/aspnet/core/
- **Web Scraping with C#**: https://scrapfly.io/blog/web-scraping-with-csharp/
- **Building APIs with ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Elasticsearch Guide**: https://www.elastic.co/guide/

---

## Phase 3: Database Design

### 3.1 Core Entities

- **Products**
  - ProductID, Name, Brand, Category, Gender, Sizes, Colors
  - Unique identifier (SKU or normalized name+brand)
- **Retailers**
  - RetailerID, Name, Domain, LogoURL, AffiliateProgram
- **Prices**
  - PriceID, ProductID, RetailerID, Price, Currency, DiscountPrice
  - Timestamp, ProductURL, InStock, ShippingCost
- **PriceHistory**
  - Historical price tracking for trend analysis
- **Users** (if implementing accounts)
  - UserID, Email, PasswordHash, Preferences
- **PriceAlerts**
  - AlertID, UserID, ProductID, TargetPrice
- **SearchQueries**
  - For analytics and autocomplete suggestions

### 3.2 Database Design Resources

- **Database Design Best Practices**: https://learn.microsoft.com/en-us/sql/relational-databases/database-design/database-design-best-practices
- **EF Core Documentation**: https://learn.microsoft.com/en-us/ef/core/
- **PostgreSQL vs SQL Server**: Compare based on needs and Azure/AWS hosting

---

## Phase 4: Data Collection (Web Scraping)

### 4.1 Scraping Strategy

1. **Identify Data Sources**
   - Product listing pages
   - Product detail pages
   - API endpoints (if publicly available)
2. **Scraping Methods**
   - Static HTML parsing (HtmlAgilityPack, AngleSharp)
   - Dynamic JavaScript sites (Selenium, Playwright, Puppeteer Sharp)
   - API interception (browser DevTools Network tab analysis)
3. **Data Extraction Points**

   - Product name, brand, price, images, sizes, colors
   - Product URL (for "Buy Now" redirects)
   - Availability status
   - Shipping information

4. **Scheduling**
   - High-demand products: scrape every 6-12 hours
   - Regular products: daily scraping
   - Use Hangfire for scheduled background jobs
5. **Error Handling & Monitoring**
   - Retry logic for failed requests
   - Logging (Serilog with ASP.NET Core)
   - Alert system for scraper failures
   - Track scraping success rates

### 4.2 Best Practices

- Respect robots.txt
- Implement rate limiting (delays between requests)
- Use appropriate User-Agent headers
- Implement caching to avoid redundant scraping
- Consider using official APIs or affiliate data feeds where available

### 4.3 Resources

- **Web Scraping Guide**: https://www.scrapehero.com/how-to-build-a-price-monitoring-and-comparison-web-app/
- **HtmlAgilityPack**: https://html-agility-pack.net/
- **Selenium with C#**: https://www.selenium.dev/documentation/webdriver/
- **Playwright for .NET**: https://playwright.dev/dotnet/
- **Hangfire Documentation**: https://www.hangfire.io/
- **Rate Limiting in ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit

---

## Phase 5: Backend API Development (ASP.NET Core)

### 5.1 Core API Endpoints

**Search & Product Endpoints**

- `GET /api/products/search?q={query}&brand={brand}&minPrice={}&maxPrice={}...`
- `GET /api/products/{id}`
- `GET /api/products/{id}/price-history`
- `GET /api/products/trending`
- `GET /api/products/deals` (best discounts)

**Filter & Aggregation**

- `GET /api/filters` (available brands, sizes, colors)
- `GET /api/categories`

**User Features** (if implementing accounts)

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/users/alerts`
- `POST /api/users/alerts`
- `DELETE /api/users/alerts/{id}`

**Analytics**

- `POST /api/analytics/click` (track user clicks for affiliate attribution)

### 5.2 Implementation Considerations

- **Pagination**: Implement cursor-based or offset pagination
- **Caching**: Cache frequently accessed data with Redis
- **Search Optimization**: Use Elasticsearch for fast full-text search
- **Response Compression**: Enable Gzip/Brotli compression
- **CORS Configuration**: Allow Next.js frontend domain
- **API Versioning**: Use URL versioning (/api/v1/...)
- **Rate Limiting**: Protect API from abuse

### 5.3 Resources

- **RESTful API Design**: https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design
- **ASP.NET Core Web API Tutorial**: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
- **Redis Caching**: https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview
- **API Rate Limiting**: https://github.com/stefanprodan/AspNetCoreRateLimit
- **JWT Authentication**: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/

---

## Phase 6: Search Engine Implementation

### 6.1 Search Features

- **Full-text search** across product names and descriptions
- **Autocomplete/Type-ahead** suggestions
- **Filters**:
  - Brand, Gender, Size, Color
  - Price range
  - Retailer
  - Discount percentage
  - Availability (in stock only)
- **Sorting**:
  - Lowest price, Highest discount, Newest, Popular

### 6.2 Search Technology Options

1. **Database Full-Text Search** (basic, good for MVP)
   - PostgreSQL: pg_trgm extension, tsvector
   - SQL Server: Full-Text Search
2. **Elasticsearch** (recommended for production)
   - Fast, scalable full-text search
   - Advanced filtering and aggregations
   - Fuzzy matching, synonyms
3. **Azure Cognitive Search / AWS CloudSearch**
   - Managed search services
   - AI-powered features

### 6.3 Search Optimization

- Implement search analytics to improve relevance
- Use stemming and lemmatization
- Handle typos with fuzzy matching
- Implement synonym mapping (e.g., "sandals" = "thongs" in Australian context)
- A/B test search ranking algorithms

### 6.4 Resources

- **Elasticsearch with .NET**: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html
- **PostgreSQL Full-Text Search**: https://www.postgresql.org/docs/current/textsearch.html
- **Search UX Best Practices**: https://baymard.com/blog/search-functionality
- **Building a Search Engine**: https://www.algolia.com/blog/engineering/how-to-build-a-search-engine/

---

## Phase 7: Frontend Development (Next.js)

### 7.1 Key Pages & Components

**Pages**

- Home page (search bar, featured deals, trending sandals)
- Search results page (with filters sidebar)
- Product detail page (price comparison, price history chart)
- About/How it works
- Privacy Policy, Terms of Service

**Components**

- Search bar with autocomplete
- Product card (image, name, price range, retailer count)
- Price comparison table
- Price history chart (Chart.js, Recharts)
- Filter sidebar (collapsible on mobile)
- Sort dropdown
- Pagination
- Breadcrumbs
- Loading skeletons

### 7.2 Frontend Features

- **SSR/SSG for SEO**
  - Pre-render popular search pages
  - Dynamic metadata for social sharing
- **Performance Optimization**
  - Image optimization (Next.js Image component)
  - Lazy loading
  - Code splitting
  - Prefetching links
- **Responsive Design**
  - Mobile-first approach
  - Touch-friendly UI
- **User Experience**
  - Loading states
  - Error boundaries
  - Skeleton screens
  - Infinite scroll or pagination
  - Filter chips showing active filters

### 7.3 SEO Optimization

- Semantic HTML
- Structured data (Schema.org Product markup)
- Sitemap generation
- Meta tags optimization
- Open Graph tags for social sharing
- Fast loading times (aim for < 3s)

### 7.4 Resources

- **Next.js App Router**: https://nextjs.org/docs/app
- **Next.js SEO**: https://nextjs.org/learn/seo/introduction-to-seo
- **Tailwind CSS**: https://tailwindcss.com/docs
- **shadcn/ui Components**: https://ui.shadcn.com/
- **React Query (TanStack Query)**: https://tanstack.com/query/latest/docs/react/overview
- **Chart.js**: https://www.chartjs.org/
- **Schema.org Product Schema**: https://schema.org/Product

---

## Phase 8: Additional Features (Post-MVP)

### 8.1 Price Alerts

- Users can set target prices for products
- Email/SMS notifications when price drops
- Implementation: Background job checks price changes, sends notifications

### 8.2 Price History & Analytics

- Historical price tracking with charts
- Best time to buy insights
- Price trend predictions

### 8.3 User Accounts & Wishlists

- Save favorite products
- Compare saved items
- Purchase history (external links tracking)

### 8.4 Advanced Search

- Image search (upload sandal photo to find similar)
- Visual similarity search
- Natural language queries ("beach sandals under $50")

### 8.5 Monetization

- Affiliate links integration (Commission Factory, Rakuten, Amazon Associates)
- Sponsored product listings
- Premium features (extended price history, advanced alerts)

### 8.6 Resources

- **Affiliate Marketing in Australia**: https://www.commissionfactory.com/
- **Email Services**: SendGrid, AWS SES, Mailgun
- **Image Recognition**: Azure Computer Vision, Google Cloud Vision API

---

## Phase 9: Deployment & DevOps

### 9.1 Hosting Options

**Frontend (Next.js)**

- Vercel (recommended, creators of Next.js)
- Netlify
- AWS Amplify
- Azure Static Web Apps
- Self-hosted on VPS (Docker)

**Backend (ASP.NET Core)**

- Azure App Service (recommended for .NET)
- AWS Elastic Beanstalk / ECS
- Docker containers on any cloud
- Self-hosted VPS (DigitalOcean, Linode)

**Database**

- Azure SQL Database / PostgreSQL
- AWS RDS
- Managed database services

**Search**

- Elasticsearch Cloud / Elastic Cloud
- Azure Cognitive Search
- Self-hosted Elasticsearch

### 9.2 DevOps Practices

- CI/CD pipelines (GitHub Actions, Azure DevOps, GitLab CI)
- Infrastructure as Code (Terraform, Azure ARM templates)
- Container orchestration (Kubernetes, Docker Compose)
- Monitoring & Logging (Application Insights, CloudWatch, Datadog)
- Error tracking (Sentry)
- Uptime monitoring (UptimeRobot, Pingdom)

### 9.3 Resources

- **Deploying Next.js**: https://nextjs.org/docs/deployment
- **Deploying ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/
- **Docker for .NET**: https://learn.microsoft.com/en-us/dotnet/core/docker/introduction
- **GitHub Actions**: https://docs.github.com/en/actions

---

## Phase 10: Testing & Quality Assurance

### 10.1 Testing Strategy

**Backend Testing**

- Unit tests (xUnit, NUnit)
- Integration tests (TestServer)
- API tests (Postman, REST Client)
- Load testing (k6, JMeter)

**Frontend Testing**

- Component tests (Jest, React Testing Library)
- E2E tests (Playwright, Cypress)
- Visual regression tests (Percy, Chromatic)

**Scraper Testing**

- Test with sample HTML pages
- Monitor for site structure changes
- Validate data extraction accuracy

### 10.2 Quality Metrics

- Code coverage (aim for >80%)
- Performance benchmarks
- Accessibility (WCAG 2.1 AA compliance)
- Browser compatibility
- Mobile responsiveness

### 10.3 Resources

- **xUnit Documentation**: https://xunit.net/
- **Testing ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/test/
- **React Testing Library**: https://testing-library.com/docs/react-testing-library/intro/
- **Playwright**: https://playwright.dev/

---

## Phase 11: Launch Preparation

### 11.1 Pre-Launch Checklist

- [ ] Legal compliance verified (ToS, Privacy Policy, scraping permissions)
- [ ] Database seeded with initial product data
- [ ] All critical bugs fixed
- [ ] Performance optimization completed
- [ ] SEO implementation verified
- [ ] Analytics setup (Google Analytics, Plausible)
- [ ] Error monitoring configured
- [ ] Backup strategy implemented
- [ ] Load testing completed
- [ ] Security audit performed
- [ ] Documentation completed

### 11.2 Launch Strategy

- Soft launch with limited product categories
- Gather user feedback
- Iterate based on analytics and feedback
- Gradual expansion to more retailers and products
- Marketing: SEO, social media, Australian deal forums (OzBargain)

### 11.3 Post-Launch Monitoring

- Monitor scraper health
- Track API performance
- Monitor database size growth
- Watch for errors and crashes
- Track user engagement metrics
- Monitor affiliate conversion rates

---

## Estimated Timeline

### ⚡ ULTRA-MINIMAL MVP (Solo Developer) - 2-4 weeks 🎯 **START HERE**

**Goal**: Validate the concept with absolute minimum features

**What to Build:**

1. **Backend (ASP.NET Core)** - 3-4 days

   - Simple Web API with 2-3 endpoints
   - PostgreSQL database (start with local, deploy to Railway)
   - ONE scraper for ONE retailer (e.g., The Iconic)
   - Manual scraping trigger (no background jobs yet)
   - Basic product search endpoint

2. **Frontend (Next.js)** - 4-5 days

   - Home page with search bar
   - Search results page (simple list view)
   - Product detail page showing prices from 1-2 retailers
   - Basic responsive design (mobile-friendly)
   - No user accounts, no authentication

3. **Data Collection** - 2-3 days

   - Write ONE scraper for one retailer's sandals category
   - Run it manually to populate initial data (~50-200 products)
   - Store: product name, brand, price, image URL, product link

4. **Deployment** - 1-2 days
   - Frontend: Vercel (free tier)
   - Backend: Fly.io (free tier - 3 VMs included)
   - Database: Fly.io PostgreSQL (via `fly postgres create`)
   - Docker: Create simple Dockerfile for ASP.NET backend

**What to SKIP in Ultra-Minimal MVP:**

- ❌ User accounts & authentication
- ❌ Price alerts
- ❌ Price history tracking
- ❌ Multiple retailers (start with 1, add 1 more if successful)
- ❌ Redis caching
- ❌ Elasticsearch (use SQL LIKE queries)
- ❌ Background job scheduling
- ❌ Email notifications
- ❌ Advanced filters (just search + basic price sorting)
- ❌ Affiliate links (add after validation)
- ❌ Analytics tracking
- ❌ Automated testing (manual test only)

**Tech Stack Simplified:**

- Frontend: Next.js + Tailwind CSS (no additional libraries)
- Backend: ASP.NET Core + Entity Framework + PostgreSQL
- Containerization: Docker (for backend deployment)
- Deployment: Vercel (frontend) + Fly.io (backend + database)
- That's it!

**Success Criteria:**

- ✅ Can search for "sandals"
- ✅ See results with prices
- ✅ Click through to retailer site
- ✅ Works on mobile and desktop

**After This Works:**

- Add ONE more retailer
- Add price sorting/filtering
- Automate scraping with Hangfire
- Then consider full MVP features

**Ultra-Minimal Database Schema:**

Just 2 tables to start:

```
Products Table:
- Id (int, primary key)
- Name (string)
- Brand (string)
- Price (decimal)
- ImageUrl (string)
- ProductUrl (string)
- RetailerName (string)
- LastUpdated (datetime)

(Optional) SearchQueries Table:
- Id (int, primary key)
- Query (string)
- SearchCount (int)
- LastSearched (datetime)
```

That's literally all you need to start! Add more tables later when needed.

---

### MVP (Minimum Viable Product) - 8-12 weeks

_Build this AFTER validating the Ultra-Minimal version above_

- Week 1-2: Setup, architecture, database design
- Week 3-4: Basic scrapers for 2-3 major retailers
- Week 5-6: Backend API development
- Week 7-8: Frontend development (search, results, detail pages)
- Week 9-10: Integration, testing, bug fixes
- Week 11-12: Deployment, monitoring setup, soft launch

### Full Product - 16-24 weeks

- MVP + advanced features (alerts, user accounts, analytics)
- More retailers integrated
- Mobile app consideration
- Marketing and user acquisition

---

## Key Success Metrics

1. **Data Quality**

   - Scraping success rate (aim for >95%)
   - Price accuracy
   - Product catalog size

2. **User Engagement**

   - Search queries per day
   - Click-through rate to retailers
   - Return user rate
   - Average session duration

3. **Technical Performance**

   - Page load time (<3 seconds)
   - API response time (<200ms)
   - Uptime (99.9%+)
   - Search result relevance

4. **Business Metrics**
   - Affiliate conversion rate
   - Revenue per click
   - User acquisition cost
   - Monthly active users

---

## Helpful Learning Resources

### General Architecture

- **Building Scalable Web Applications**: https://learn.microsoft.com/en-us/azure/architecture/
- **System Design Primer**: https://github.com/donnemartin/system-design-primer

### Australian-Specific

- **OzBargain** (study popular deal site): https://www.ozbargain.com.au/
- **Australian E-commerce Trends**: https://www.auspost.com.au/content/dam/auspost_corp/media/documents/inside-australian-online-shopping-ecommerce-report.pdf

### Communities & Forums

- **Reddit**: r/webdev, r/dotnet, r/nextjs
- **Stack Overflow**
- **Dev.to**
- **Australian Developer Community**: https://www.meetup.com/en-AU/find/?keywords=developer

---

## Important Notes

1. **Start SUPER Small (Solo Devs)**: Begin with just 1 retailer and ~50-200 products, validate the concept, then expand. Don't build for scale until you have users!
2. **Legal First**: Ensure compliance before scraping any site - check robots.txt and Terms of Service
3. **Data Quality**: Accurate prices are critical - implement validation
4. **User Trust**: Display last update time, be transparent about data freshness
5. **Respectful Scraping**: Don't overload retailer servers - add delays between requests
6. **Affiliate Disclosure**: Clearly disclose affiliate relationships (required by law)
7. **Mobile Focus**: Many Australians shop on mobile, prioritize mobile UX
8. **Consider Data Feeds**: Some retailers offer affiliate data feeds (easier and more legal than scraping)
9. **Validate Before Scaling**: Get 10-20 real users using your Ultra-Minimal version before adding complexity
10. **Free Hosting First**: Use free tiers (Vercel, Railway) until you have revenue - don't spend money prematurely

---

## Next Steps

### For Solo Developers (Ultra-Minimal Approach):

**Week 1 - Backend Setup:**

1. Install: Node.js, .NET SDK 8.0, Docker Desktop, PostgreSQL (local), VS Code
2. Create ASP.NET Core Web API project: `dotnet new webapi -n ZeniSearch.Api`
3. Add Entity Framework Core with PostgreSQL: `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`
4. Create simple Product model (just the 8 fields above)
5. Write ONE basic scraper for The Iconic sandals page (start with just the first page, ~50 products)
6. Create single API endpoint: `GET /api/products/search?q={query}`
7. Test locally with Swagger

**Week 2 - Frontend + Docker:**

8. Create Dockerfile for your ASP.NET backend (see example below)
9. Test Docker build locally: `docker build -t zeni-api .`
10. Create Next.js project: `npx create-next-app@latest zeni-search --typescript --tailwind --app`
11. Build 3 pages: Home (with search), Results, Product Detail
12. Connect frontend to your local API
13. Make it mobile-responsive
14. Test end-to-end flow

**Week 3 - Deploy:**

15. Install Fly.io CLI: `curl -L https://fly.io/install.sh | sh` (or `brew install flyctl` on Mac)
16. Deploy backend: `fly launch` in backend directory
17. Create PostgreSQL: `fly postgres create` and attach to app
18. Deploy frontend to Vercel: `vercel deploy`
19. Update frontend env vars to point to Fly.io backend
20. Share with 5-10 friends for feedback

**Week 4+ - Iterate Based on Feedback:**

21. Monitor: Do they actually use it? What do they search for?
22. If people use it: Add a second retailer
23. If search is the issue: Improve search relevance
24. If UI is confusing: Improve the interface
25. Only add complexity if there's demand

### For Team/Full MVP:

If you have a team or after validating the Ultra-Minimal version, follow the 11 phases outlined above starting from Phase 1.

---

## Railway vs AWS: Which Should You Use?

### What is Railway?

**Railway.app** is a modern, developer-friendly Platform-as-a-Service (PaaS) that makes deploying web applications incredibly simple. Think of it as "deployment made easy" - you connect your GitHub repo, and it automatically builds and deploys your app.

### Railway vs AWS Comparison

| Feature                   | Railway (Recommended for Solo MVP)        | AWS (For Production Scale)              |
| ------------------------- | ----------------------------------------- | --------------------------------------- |
| **Setup Time**            | 5-10 minutes                              | 1-2 hours (or more)                     |
| **Free Tier**             | $5 credit/month (covers small apps)       | Complex free tier with limits           |
| **Complexity**            | Very simple, beginner-friendly            | Steep learning curve                    |
| **Database Setup**        | One-click PostgreSQL                      | Manual setup (RDS) or EC2 config        |
| **Automatic Deploys**     | Built-in from GitHub                      | Requires setup (CodePipeline, etc.)     |
| **Environment Variables** | Simple web UI                             | Multiple places (Systems Manager, etc.) |
| **SSL/HTTPS**             | Automatic                                 | Manual setup or use Load Balancer       |
| **Monitoring**            | Built-in basic metrics                    | Powerful but requires CloudWatch setup  |
| **Pricing**               | Pay-as-you-go ($5-20/month for small app) | Can be $20-100+/month if not careful    |
| **Best For**              | MVPs, side projects, solo devs            | Large-scale production apps             |

### Why Use Railway for Your MVP?

**1. Time to Deployment:**

- **Railway**: Push to GitHub → Automatic deploy (5 mins)
- **AWS**: Set up EC2/ECS, configure security groups, load balancers, RDS, networking (hours/days)

**2. Cost Simplicity:**

- **Railway**: Clear pricing. $5 free credit covers ~500K requests/month
- **AWS**: Complex pricing. Easy to accidentally spend $50-100 on misconfigured resources

**3. Developer Experience:**

- **Railway**: One dashboard, everything visible
- **AWS**: 200+ services, overwhelming for beginners

**4. Database Management:**

- **Railway**: Click "Add PostgreSQL" → Done. Includes backups.
- **AWS**: Set up RDS, configure security groups, VPC, backups manually

**5. No DevOps Required:**

- **Railway**: Just push code
- **AWS**: Need to learn IAM, VPC, EC2, Security Groups, Load Balancers

### When to Switch from Railway to AWS?

Move to AWS when you have:

- **High traffic**: 10M+ requests/month
- **Complex infrastructure needs**: Multiple microservices, data pipelines
- **Enterprise requirements**: Compliance, advanced security, custom networking
- **Revenue**: Making $1000+/month and need to optimize costs at scale

### Cost Comparison Example (Small App)

**Your MVP (~100 users, 50K requests/month):**

| Service        | Railway         | AWS                                       |
| -------------- | --------------- | ----------------------------------------- |
| Backend API    | $5-10/month     | $15-25/month (EC2 t3.micro or App Runner) |
| PostgreSQL     | Included above  | $15-25/month (RDS t3.micro)               |
| SSL/HTTPS      | Free            | $0 (with ACM) but setup time              |
| **Total**      | **$5-10/month** | **$30-50/month**                          |
| **Setup Time** | **10 minutes**  | **2-4 hours**                             |

### Other Alternatives to Railway

**Similar to Railway (Good for MVPs):**

- **Render.com**: Very similar to Railway, also beginner-friendly
- **Fly.io**: Excellent for Docker containers, slightly more technical
- **Heroku**: Original PaaS, but more expensive now

**If you hate Railway, use:** Render.com (nearly identical experience)

---

## Docker Deployment: Railway vs Fly.io

### Should You Use Docker for Your MVP?

**Docker Benefits:**

- ✅ Consistent environment (dev = production)
- ✅ Easy to switch hosting providers later
- ✅ Better control over dependencies
- ✅ Can run anywhere (local, Railway, Fly.io, AWS, etc.)

**Docker Drawbacks (for beginners):**

- ❌ Additional learning curve (Dockerfile, docker-compose)
- ❌ More things to configure and debug
- ❌ Slightly slower iteration (rebuild image each time)

### Railway vs Fly.io for Docker

| Feature              | Railway                       | Fly.io                              |
| -------------------- | ----------------------------- | ----------------------------------- |
| **Docker Support**   | Yes (auto-detects Dockerfile) | Native Docker platform              |
| **Without Docker**   | ✅ Auto-detects .NET/Node.js  | ❌ Docker required                  |
| **Setup Complexity** | Simple (Docker optional)      | Moderate (Docker required)          |
| **Pricing**          | $5 credit/month               | Free allowance (3 VMs)              |
| **PostgreSQL**       | One-click add-on              | Built-in `fly postgres` command     |
| **Deployment Speed** | Fast (caches layers)          | Very fast (edge deployment)         |
| **Global Edge**      | No (single region)            | ✅ Yes (deploy to multiple regions) |
| **Learning Curve**   | Low                           | Moderate                            |
| **CLI Experience**   | Good                          | Excellent                           |
| **Dashboard UI**     | Better (more visual)          | Basic (CLI-focused)                 |
| **Best For**         | Beginners, quick MVPs         | Docker users, global apps           |

### Railway with Docker

**How it works:**

1. Add a `Dockerfile` to your ASP.NET project
2. Push to GitHub
3. Railway auto-detects and builds Docker image
4. Deploy automatically

**Pros:**

- Works with or without Docker (flexible)
- Simple setup
- Better dashboard
- Less configuration needed

**Example Dockerfile for ASP.NET:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YourProject.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourProject.dll"]
```

### Fly.io with Docker

**How it works:**

1. Install Fly CLI: `curl -L https://fly.io/install.sh | sh`
2. `fly launch` (generates Dockerfile + fly.toml)
3. `fly deploy` (builds and deploys)

**Pros:**

- Excellent Docker experience
- Free tier is generous (3 shared VMs)
- Global edge deployment (fast worldwide)
- Can deploy to multiple regions easily
- Great CLI tools
- Better for complex Docker setups

**Cons:**

- Docker is mandatory (can't just push .NET code)
- More CLI-focused (less visual dashboard)
- Slightly steeper learning curve

### Recommendation for Your Project

#### Choose **Railway** if:

- ✅ You're new to Docker (can start without it, add later)
- ✅ You want the simplest possible deployment
- ✅ You prefer visual dashboards
- ✅ You want to deploy NOW and learn Docker later
- ✅ Your app only needs single-region hosting

#### Choose **Fly.io** if:

- ✅ You're comfortable with Docker already
- ✅ You want to learn Docker (good learning opportunity)
- ✅ You prefer CLI tools
- ✅ You want global edge deployment (users worldwide)
- ✅ You need multiple regions (e.g., Sydney + Singapore + US)

### My Honest Recommendation for Your Sandal Search Project

**Week 1-2 (Learning & Building):**
Use **Railway WITHOUT Docker**

- Focus on learning ASP.NET + Next.js, not Docker
- Get something working faster
- Deploy by just pushing code

**Week 3-4 (MVP Live):**
If it's working, **add Docker** to Railway

- Create Dockerfile
- Railway auto-detects and uses it
- Now you have portability

**Month 2+ (If Successful):**
**Consider Fly.io** if:

- You need global edge (Australian + international users)
- You want better performance in multiple regions
- Railway costs exceed Fly.io's free tier

### Docker Learning Curve

**Without Docker (Railway native):**

```bash
# Just push code
git push origin main
# Railway builds and deploys automatically
```

**With Docker (Railway or Fly.io):**

```bash
# Create Dockerfile
# Create .dockerignore
# Test locally: docker build -t myapp .
# Test run: docker run -p 5000:80 myapp
# Push to deploy
```

**Time cost:**

- No Docker: 0 hours learning, deploy in 10 minutes
- With Docker: 2-4 hours learning, deploy in 30 minutes

### Best Approach for Solo Developer

**Phase 1: Ultra-Minimal MVP (Weeks 1-3)**
→ Use **Railway** WITHOUT Docker

- Why: Fastest path to validation
- Learn: ASP.NET, PostgreSQL, Next.js
- Skip: Docker complexity

**Phase 2: Working MVP (Weeks 4-6)**
→ Add **Docker** to Railway

- Why: Still simple, now portable
- Learn: Dockerfile basics
- Benefit: Can move to any platform later

**Phase 3: Scale (Month 3+)**
→ Consider **Fly.io** or **AWS**

- Why: Better performance/cost at scale
- Learn: Multi-region deployment
- Benefit: Global edge, lower latency

### Can You Switch Later?

**Yes, easily with Docker!**

If you use Docker from the start:

- Railway → Fly.io: 30 minutes (change deployment target)
- Railway → AWS ECS: 1-2 hours
- Fly.io → Railway: 30 minutes
- Any → Any: Just redeploy the same Docker image

If you don't use Docker:

- Railway → Fly.io: Need to create Dockerfile first (2-4 hours)

### Final Answer to Your Question

**Should you use Fly.io instead of Railway if using Docker?**

**Both are excellent choices. Here's the decision tree:**

```
Do you already know Docker well?
├─ YES → Fly.io (better Docker experience, free tier)
└─ NO
   └─ Do you want to learn Docker now?
      ├─ YES → Fly.io (great learning platform)
      └─ NO → Railway (easier, add Docker later)
```

**My recommendation for you:**

1. **Start with Railway WITHOUT Docker** (fastest validation)
2. **Week 3: Add Dockerfile** (now portable)
3. **Month 2: Try Fly.io** if you need global edge or want to compare

Don't let Docker choice block you from building. Railway lets you start simple and add Docker when ready. Fly.io forces Docker from day 1 (good for learning, but slower start).

**Resources:**

- **Railway Docker Guide**: https://docs.railway.app/deploy/dockerfiles
- **Fly.io .NET Guide**: https://fly.io/docs/languages-and-frameworks/dotnet/
- **Fly.io vs Railway**: https://community.fly.io/t/fly-io-vs-railway/
- **Docker Tutorial**: https://docker-curriculum.com/

---

## Fly.io Deployment Guide (Your Chosen Path)

Since you've chosen Fly.io + Docker, here's your step-by-step deployment guide:

### Prerequisites

```bash
# 1. Install Fly.io CLI
# On Mac:
brew install flyctl

# On Linux:
curl -L https://fly.io/install.sh | sh

# On Windows (PowerShell):
iwr https://fly.io/install.ps1 -useb | iex

# 2. Sign up and login (free, no credit card required initially)
fly auth signup
# Or if you have an account:
fly auth login

# 3. Verify installation
fly version
```

### Step 1: Create Dockerfile for ASP.NET Backend

Create a `Dockerfile` in your ASP.NET project root:

```dockerfile
# Use the official .NET 8 runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ZeniSearch.Api.csproj", "./"]
RUN dotnet restore "ZeniSearch.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "ZeniSearch.Api.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "ZeniSearch.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variable to listen on all interfaces
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ZeniSearch.Api.dll"]
```

### Step 2: Create .dockerignore

Create `.dockerignore` in the same directory:

```
**/bin/
**/obj/
**/out/
**/.vs/
**/.vscode/
**/*.user
*.md
.git/
.gitignore
Dockerfile
docker-compose.yml
```

### Step 3: Test Docker Build Locally

```bash
# Build the image
docker build -t zeni-api .

# Test run locally (make sure port 8080 is free)
docker run -p 8080:8080 zeni-api

# Test in browser: http://localhost:8080/swagger
# Stop with Ctrl+C
```

### Step 4: Deploy Backend to Fly.io

```bash
# In your ASP.NET project directory
cd ZeniSearch.Api

# Initialize Fly.io app (interactive)
fly launch

# It will ask you:
# - App name: zeni-search-api (or your choice)
# - Region: Sydney (syd) for Australian users
# - Setup PostgreSQL: NO (we'll do it separately)
# - Deploy now: NO (configure first)
```

This creates a `fly.toml` file. Edit it to ensure:

```toml
app = "zeni-search-api"
primary_region = "syd"

[build]
  dockerfile = "Dockerfile"

[env]
  ASPNETCORE_URLS = "http://+:8080"

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = true
  auto_start_machines = true
  min_machines_running = 0  # Scale to zero when idle (saves money)

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 256  # Start small, scale up if needed
```

### Step 5: Create PostgreSQL Database

```bash
# Create a Postgres cluster (use Sydney region)
fly postgres create --name zeni-search-db --region syd --vm-size shared-cpu-1x --initial-cluster-size 1

# Attach database to your app
fly postgres attach zeni-search-db --app zeni-search-api

# This automatically creates a DATABASE_URL secret
# Verify:
fly secrets list --app zeni-search-api
```

### Step 6: Update Connection String in Your App

In your `Program.cs` or startup, use the Fly.io connection string:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Get connection string from environment (Fly.io sets DATABASE_URL)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    // Fallback to local connection for development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Rest of your configuration...
```

### Step 7: Deploy!

```bash
# Deploy your app
fly deploy

# Monitor deployment
fly logs

# Once deployed, get your app URL
fly status

# Your API will be at: https://zeni-search-api.fly.dev
```

### Step 8: Run Database Migrations

```bash
# SSH into your Fly.io machine
fly ssh console --app zeni-search-api

# Inside the container, run migrations
# (You'll need to set up EF Core migrations first)

# Or, create a separate migration command in your app
# and run it via fly ssh
```

**Alternative: Run migrations via a one-off command:**

Add this to your `Program.cs`:

```csharp
// Check if we should run migrations on startup
if (Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

Then deploy with migrations:

```bash
# Set secret to run migrations
fly secrets set RUN_MIGRATIONS=true --app zeni-search-api

# Deploy (migrations run on startup)
fly deploy

# Unset after first deploy
fly secrets unset RUN_MIGRATIONS --app zeni-search-api
```

### Step 9: Deploy Frontend to Vercel

```bash
# In your Next.js project directory
cd zeni-search-frontend

# Install Vercel CLI
npm i -g vercel

# Login
vercel login

# Deploy
vercel

# Set environment variable for API URL
vercel env add NEXT_PUBLIC_API_URL
# Enter: https://zeni-search-api.fly.dev

# Deploy production
vercel --prod
```

### Step 10: Configure CORS in ASP.NET

Update your `Program.cs` to allow Vercel frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        policy.WithOrigins(
            "https://your-app.vercel.app",
            "http://localhost:3000" // For local development
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// After building app
app.UseCors("AllowVercel");
```

### Useful Fly.io Commands

```bash
# View logs (real-time)
fly logs --app zeni-search-api

# SSH into your machine
fly ssh console --app zeni-search-api

# Scale your app
fly scale vm shared-cpu-1x --memory 512 --app zeni-search-api
fly scale count 2 --app zeni-search-api  # Run 2 instances

# Check status
fly status --app zeni-search-api

# Open app in browser
fly open --app zeni-search-api

# List secrets
fly secrets list --app zeni-search-api

# Set a secret (like API keys)
fly secrets set SCRAPER_USER_AGENT="ZeniSearch/1.0" --app zeni-search-api

# View pricing/usage
fly dashboard

# Update to different region
fly regions add sin  # Singapore
fly regions list

# Destroy app (careful!)
fly apps destroy zeni-search-api
```

### Fly.io Pricing for Your MVP

**Free Tier Includes:**

- 3 shared-cpu-1x VMs (256MB RAM each)
- 160GB bandwidth/month
- 3GB persistent storage

**Your Expected Costs (MVP with <100 users):**

- Backend API: Free (1 VM with auto-stop)
- PostgreSQL: Free (1 small instance)
- **Total: $0/month for first few months**

**When you grow (1000+ users):**

- Backend: ~$5-10/month (keep VMs running)
- PostgreSQL: ~$5-10/month (larger instance)
- **Total: ~$10-20/month**

### Monitoring & Debugging

```bash
# Real-time logs
fly logs --app zeni-search-api

# Metrics
fly dashboard

# Check health
fly checks list --app zeni-search-api

# Restart app
fly apps restart zeni-search-api
```

### Tips for Success

1. **Start Small**: Use smallest VM size (256MB), scale up only when needed
2. **Auto-stop**: Enable in fly.toml to save money when idle
3. **Regions**: Start with Sydney (syd), add more later if needed
4. **Secrets**: Never commit secrets, use `fly secrets set`
5. **Database Backups**: Fly.io auto-backs up Postgres daily
6. **Logs**: Monitor logs regularly for errors
7. **Health Checks**: Add a `/health` endpoint to your API

### Dockerfile Best Practices

```dockerfile
# Use multi-stage builds (reduces final image size)
# Copy only necessary files
# Use specific .NET versions (not "latest")
# Don't run as root (add USER directive)
# Use .dockerignore to exclude unnecessary files
```

### Complete Example Project Structure

```
zeni-search/
├── backend/
│   ├── ZeniSearch.Api/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── ZeniSearch.Api.csproj
│   │   ├── Dockerfile
│   │   ├── .dockerignore
│   │   └── fly.toml
│   └── ...
├── frontend/
│   ├── app/
│   ├── components/
│   ├── public/
│   ├── package.json
│   ├── next.config.js
│   └── .env.local
└── README.md
```

### Next Steps After Deployment

1. Test your deployed API: `https://your-app.fly.dev/swagger`
2. Test frontend connection to backend
3. Seed database with initial sandal products
4. Share with friends and get feedback
5. Monitor logs for errors
6. Iterate based on feedback

**Resources:**

- **Fly.io .NET Docs**: https://fly.io/docs/languages-and-frameworks/dotnet/
- **Fly.io PostgreSQL**: https://fly.io/docs/postgres/
- **Fly.io Pricing**: https://fly.io/docs/about/pricing/
- **Community Forum**: https://community.fly.io/

---

### Quick Railway Setup Guide (Alternative)

```bash
# 1. Sign up at railway.app (free, no credit card required for trial)
# 2. Install Railway CLI
npm i -g @railway/cli

# 3. Login
railway login

# 4. In your backend project directory
railway init
railway add --plugin postgresql  # Adds PostgreSQL database

# 5. Deploy
railway up

# 6. Set environment variables in Railway dashboard
# 7. Connect your GitHub repo for auto-deploys
```

**Resources:**

- Railway Docs: https://docs.railway.app/
- Railway vs AWS: https://railway.app/
- Deployment Guide: https://docs.railway.app/deploy/deployments

---

## Final Thoughts for Solo Developers

**The biggest mistake** you can make is spending 3 months building a perfect, scalable system with 10 retailers, user accounts, price alerts, and beautiful charts... only to find out nobody wants to use it.

**The smart approach:**

- Week 1-2: Build the simplest version
- Week 3: Deploy it and share with real users
- Week 4: See if anyone actually uses it
- Then decide whether to continue or pivot

Your goal for the first month is to answer: **"Will people actually use a sandal price comparison site?"**

Not: "Can I build the most sophisticated price comparison architecture?"

Start simple. Validate. Then scale.

Good luck with your Zeni Search project! 🚀
