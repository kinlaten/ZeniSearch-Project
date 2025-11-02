1. Fix Scraper: use the sale price instead of original price. How frequent they should be updated?
2. Why we saved searched products into db, how long its should be in there?
3. Refine searchResult by matching keyword first, then limit, then sorted by price.

4. Add use to use for authorization in hangfire

5. Which ecommerce platform should go first

```
1. Why Amazon and Some Platforms Are Strict
Amazon heavily monitors bot activity using:

Advanced anti-bot systems (CloudFront + custom detection layers).

Rotating CAPTCHAs, JavaScript fingerprinting, and IP / ASN blacklists.

Legal terms explicitly banning scraping under the Amazon Associates Agreement and robots.txt file.

Other platforms that are similarly strict include:

Platform	Strictness	Notes
Amazon	Extremely high	Frequent IP bans, CAPTCHA prompts.
eBay	High	Actively detects scraper patterns; has an official API.
Walmart	High	AWS-level detection, throttling non-browser traffic.
AliExpress (Alibaba)	Medium–High	Geo/IP checks, frequent JavaScript verification.
Shopee, Lazada	Medium	Will block sustained scraping but tolerate low volume.
Best Buy, Target	Moderate	They employ bot filtering but are more lenient on small/intermittent requests.
2. Safe Alternatives to Start With (Less Strict)
If you’re building a price-comparison or niche product search engine, start with ecommerce sites that are:

Smaller, less restrictive, or

Offer open APIs, affiliate APIs, or RSS product feeds.

Examples:

Platform	Reason to Start Here
OpenCart / Shopify stores	Many small stores disable bot blocking by default.
eBay API	Official API supports full catalog querying.
Best Buy API	Public developer API (for limited use).
Etsy	Generally easier to scrape and has developer APIs.
Newegg / B&H / Walmart Developer API	Well-documented APIs.
These will allow you to:

Build your data model, indexing, and ranking system.

Test your scraper architecture (with proxies, delays, and parser logic).

Avoid wasting resources on anti-bot work too early.

3. Handling Anti-Bot Systems (When You’re Ready)
When you eventually go for stricter sites like Amazon:

Use official alternatives first:

Amazon Product Advertising API (though limited and requires affiliate approval).

If you must scrape ethically, implement:

Human-like delays and randomized user-agents.

Rotating proxies (residential preferred).

Headless browsers (e.g., Playwright, Puppeteer).

Load-balancing across distributed nodes to avoid rate flags.

Always obey robots.txt where applicable.
It tells you what can (and cannot) be crawled safely.

4. Smart Strategy for You
Yes — absolutely:

Build your search engine with less strict ecommerce sites first
then move to Amazon and major chains once you’ve proven the model.

Reason:

You can validate your idea, data pipeline, and UI early.

Avoid unnecessary IP bans, CAPTCHA costs, or legal hurdles.

Your scraper infrastructure (proxy rotation, user agents, error recovery) will improve over time.

5. Future-Proof and Legal Approach
Once you scale:

Use affiliate APIs wherever available (Amazon, eBay, Walmart).

Join affiliate programs — this gives you legal, stabilized data feeds.

Combine scraped and official data for completeness.

Summary
Step	Goal
Start with	eBay, Etsy, Shopify-based stores (low protection)
Phase 2	Include BestBuy, Walmart via APIs
Phase 3	Integrate Amazon via official API or controlled scraping
Long-term	Add affiliate partnerships for reliable data pipelines
```

# 6. Most loved online shopping site in Australia

https://www.birdsnest.com.au/
https://www.showpo.com/

# 7.Backend stack

```
[Crawler (Scrapy)] → [ASP.NET Core API] → [PostgreSQL + Lucene.NET]
                                      ↘ [ZoneTree or OpenSearch for scaling]
                                           ↘ [Redis Cache] → [Client/Search UI]
```

# 8. Normalize Search keywords to match better

# 9. Add scroll down to bottom of page to make sure all products rendered . Add click function to nav to next page

# 10. Filter from wbsite not work for birdnest: maybe because dynamic render
