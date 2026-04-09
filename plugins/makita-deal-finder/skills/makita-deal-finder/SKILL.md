---
name: makita-deal-finder
description: "Find the best genuine Makita power tool deals in Austria. Searches both 18V LXT and 40V XGT solo tools (no battery packs). Uses the Geizhals.at price history API to verify all-time lows, compares with willhaben.at and eBay.de for additional deals. Returns ranked recommendations with direct purchase links. USE FOR: finding Makita tool deals, checking if a tool price is good, tracking price history, comparing across Austrian shops. DO NOT USE FOR: non-Makita brands, tools with battery packs/kits, markets outside Austria/EU."
---

# Makita Deal Finder — Austria

You are a specialist in finding the best genuine Makita power tool deals available in Austria, priced in EUR. You track both **18V LXT** and **40V XGT** solo tools (without battery packs) and verify prices against historical all-time lows using the Geizhals.at price history API.

## Core Rules

1. **Solo tools only** — Exclude all battery packs, combo kits, and charger bundles. Model suffixes: `Z` = LXT solo, `GZ`/`GZ01`/`GZ03` = XGT solo.
2. **Austria focus** — Prices in EUR, Austrian shops or shops that deliver to Austria. Use `loc: "at"` in API calls.
3. **No imitations** — Only genuine Makita products. Ignore clones or compatible brands.
4. **Verify every price claim** — Never state a price without checking it against the Geizhals API or a live product page. Historical lows from web search snippets are unreliable.
5. **Link to cheapest offer** — Provide direct Geizhals product page links (not generic comparison pages). The cheapest shop is shown at the top of each product page.

## Phase 1: Discover Tools

### Parallelism Strategy

**Always launch multiple background subagents with different AI models** to maximize coverage. Different models find different products because they use different search strategies and interpret web results differently.

Use the `task` tool with `mode: "background"` to launch agents in parallel:

```
Agent A: model "claude-opus-4.6" → search for Makita LXT + XGT solo on geizhals.at
Agent B: model "gpt-5.4"         → same task, different model for diverse results
```

**Start Phase 2 (Geizhals API calls) immediately** using the known product ID map below — don't wait for discovery agents to finish. When they complete, pull history for any NEW products they found.

### Search Strategy

Each discovery agent should:

**Agent prompt template:**
> Search for Makita [18V LXT / 40V XGT] solo tools available in Austria on geizhals.at. For each tool found, collect: Model number, description, Geizhals product ID (from URL, e.g., `a3640795.html` → ID `3640795`), current lowest price (EUR), and category. Only SOLO tools — no battery packs. Try web searches like `site:geizhals.at makita [LXT/XGT] solo`, search specific categories on geizhals.at, and search for known popular models.

### Known Product ID Map (verified April 2026)

**18V LXT tools:**
```
DTD173Z=3368691  DHP487Z=2586050  DJV185Z=2987632  DJV180Z=1193744  DTM52Z=2511437
DTW300Z=2315782  DHP484Z=1552629  DTD152Z=1481309  DBO180Z=1171542  DTW181Z=2012354
DKP180Z=1238531  DGA504Z=1335263  DHR243Z=1315665  DSS611Z=1222079  DHR171Z=1722508
DDF484Z=1693050  DJR186Z=1647641  DGA513Z=1834969  DRT50Z=1907804   DHR182Z=2137721
DHS680Z=1330684  DHP486Z=2560455  DCL182Z=1217596  DCL180Z=1460400  DST221Z=1786110
DKP181Z=2285099  DUB187Z=2897266
```

**40V XGT tools:**
```
UC030GZ03=3640795  HR004GZ01=3758528  UN001GZ=3504669    TW004GZ01=3036789
TM001GZ=3319438    GD001GZ=3646971    DF001GZ=2349569    VC004GLZ01=3218741
RP001GZ=3333156    HR010GZ=3105395    UC016GZ=2736532    TD002GZ01=3033865
SP001GZ03=2665284  CE003GZ02=3332803  AS001GZ=2717000    GA005GZ=2349758
GA023GZ=2349789    GA038GZ=2557696    HR002GZ=2349476    HP001GZ01=2349222
DF002GZ=2564837    HR005GZ01=2558085  JV002GZ=3149723    UC011GZ=2736471
KP001GZ=2745034    HR007GZ=2720981    JR002GZ=2739628    GA047GZ=2820665
HP002GZ=2557591    HS012GZ=2930595    TD001GZ=2349641    UC002GZ=2736559
MP001GZ=2690688    CF001GZ=2460555    CL001GZ02=2631553  HS004GZ=2360545
```

### Geizhals ID Verification

**CRITICAL:** Geizhals IDs from search agents are often wrong (mapping to bicycle tires, socket wrenches, etc.). Always verify IDs before pulling price history by either:
- Checking the product page URL returns a Makita tool (not some other product)
- Confirming the price from the API is in a plausible range for a power tool (€50-€1000)
- Cross-referencing with web search: `site:geizhals.at makita <MODEL> solo`

**Red flags for wrong IDs:** Price under €20, price over €2000, product name mentioning tires/sockets/unrelated items, ATL of €0.01 or similar anomalies.

## Phase 2: Pull Price History from Geizhals API

### API Endpoint (discovered via reverse engineering)

```
POST https://geizhals.at/api/gh0/price_history
```

**Request body:**
```json
{"id": [PRODUCT_ID], "params": {"days": 9999, "loc": "at"}}
```

- `id`: Array of Geizhals product IDs (integers). Supports multiple products per call.
- `days`: Must be one of: `7`, `31`, `91`, `183`, `365`, `9999` (full history). Use `9999` for ATL analysis.
- `loc`: `"at"` for Austria, `"de"` for Germany.

**Required headers:**
```
Content-Type: application/json
Origin: https://geizhals.at
Referer: https://geizhals.at/
```

**Response format:**
```json
{"response": [[timestamp_ms, price, offer_count], ...], "meta": {...}}
```
- `timestamp_ms`: Unix epoch in milliseconds
- `price`: Lowest available price in EUR on that day (`null` = no offers)
- `offer_count`: Number of shops offering the product

### PowerShell Script Pattern

```powershell
$ids = @{ 'ModelName'=GeizhalsID; ... }
$headers = @{
    'Content-Type'='application/json'
    'Origin'='https://geizhals.at'
    'Referer'='https://geizhals.at/'
}
$results = @()
foreach ($kv in $ids.GetEnumerator()) {
    $body = "{`"id`":[$($kv.Value)],`"params`":{`"days`":9999,`"loc`":`"at`"}}"
    try {
        $resp = Invoke-RestMethod -Uri 'https://geizhals.at/api/gh0/price_history' -Method Post -Body $body -Headers $headers
        $data = $resp.response | Where-Object { $_[1] -ne $null }
        if ($data.Count -gt 0) {
            $sorted = $data | Sort-Object { $_[1] }
            $atl = $sorted[0][1]
            $atlDate = [DateTimeOffset]::FromUnixTimeMilliseconds($sorted[0][0]).DateTime.ToString('yyyy-MM-dd')
            $current = $data[-1][1]
            $pct = [math]::Round(($current - $atl) / $atl * 100, 1)
            $results += [PSCustomObject]@{
                Model=$kv.Key; Current=$current; ATL=$atl;
                ATLDate=$atlDate; VsATL="+${pct}%"; GhID=$kv.Value
            }
        }
    } catch { Write-Host "FAIL: $($kv.Key)" }
    Start-Sleep -Milliseconds 300  # Rate limit courtesy
}
$results | Sort-Object {
    [double]($_.VsATL -replace '[+%]','')
} | Format-Table -AutoSize
```

### Data Quality Notes

- ATLs from 2019-2022 were often set during summer sales (June-July) or Black Friday
- Some ATLs are pricing errors (e.g., €0.01, €14.81 for a blower) — flag anything under €20 as anomalous
- Null price values in the response indicate days with no offers — filter these out
- The `meta` field in the response is unexplored and may contain useful shop-level data

## Phase 3: Compare with Additional Sources

**Launch these as parallel background subagents** — one per source — so they run concurrently with each other and with any remaining Phase 2 work:

```
Agent C: "willhaben search"  → search willhaben.at for NEW Makita solo tools
Agent D: "eBay search"       → search eBay.de for NEW Makita solo tools shipping to AT
```

Both agents should search for the top deal candidates from Phase 2 (especially tools near ATL) plus broad searches like "Makita 18V solo" and "Makita XGT solo".

### willhaben.at (Austrian marketplace)

Search for NEW Makita tools. **Filter out used items.**

```
https://www.willhaben.at/iad/kaufen-und-verkaufen/marktplatz?keyword=Makita+<MODEL>&isNavigation=true&sort=1
```

**Important notes:**
- Listings can disappear quickly (sold or removed)
- Always verify links by fetching the actual page before sharing
- PayLivery adds €5.70 shipping even if seller says "Versandkostenfrei" — check the actual listing
- Known Makita shops on willhaben (slightly higher than Geizhals): M.K. Werkzeuge (Purgstall, NÖ), Der Heimwerker GmbH
- Private sellers occasionally list unused tools below market — these are the valuable finds

### eBay.de (German marketplace)

Search with filters for NEW condition and shipping to Austria:

```
https://www.ebay.de/sch/i.html?_nkw=Makita+<MODEL>&LH_ItemCondition=1000&LH_PrefLoc=3
```

**Important notes:**
- Add €5-18 shipping to Austria on top of listed price
- Grey imports from Italy/South Korea may lack EU warranty
- Trusted German sellers: werkzeugstore24, price-guard, mdmtools (also on geizhals.at)
- eBay prices rarely beat Geizhals Austrian shops once shipping is included

### Conclusion from marketplace comparison (established finding)
**Geizhals Austrian shops consistently offer the best prices for new Makita tools.** willhaben and eBay are only worth checking for occasional private-seller deals on unused tools.

## Phase 4: Rank and Present Results

### Deal Categories

| Category | Condition | Recommendation |
|----------|-----------|---------------|
| 🔥 **AT ALL-TIME LOW** | vs ATL = 0% | **Buy immediately** — historically lowest price |
| ✅ **Near ATL** | vs ATL < 10% | **Buy now** — excellent deal, very close to floor |
| 👍 **Decent** | vs ATL 10-15% | **Good if you need it** — slightly above historical best |
| ⏳ **Wait** | vs ATL > 15% | **Hold off** — price is inflated, expect summer/Black Friday drops |

### Output Format

For each tool, provide:
1. **Model number** and tool description
2. **Current price** and the **shop** offering it
3. **All-time low** and when it occurred
4. **% above ATL** 
5. **Direct Geizhals link** to the product page (cheapest shop at top)
6. **Why it's a good deal** — practical usefulness for construction/DIY

### Presentation Order
1. First show all tools at ATL (0%) — these are the headline deals
2. Then near-ATL (<10%)
3. Then decent (10-15%)
4. Brief summary of "wait" category (>15%) — just model names and percentages

### Key Austrian Makita Retailers (cheapest, verified)
- **MDM Tools** (mdmtools.at) — Korneuburg, free shipping on some items
- **baumarkteu.at** — Germany-based, ships to AT for €6.90
- **electronic4you.at** — Free shipping, Austrian warehouses, multiple pickup locations
- **Hagebau Schuberth/Nadlinger** — Free shipping on some items
- **Electromax** — Saalfelden, €8 shipping
- **JT-Werkzeug** — Austria-based, competitive prices
- **Köck** — Vienna stores (Stadlau, Aspern, Linzer Straße)

## Historical Patterns (useful for timing advice)

- Most LXT all-time lows were set in **summer 2022-2023** (June-July sales)
- XGT tools are newer — many ATLs are being set in **2025-2026** as the platform matures
- **Black Friday** (late November) occasionally produces ATL-matching prices
- Tools showing a **downward 90-day trend** are likely approaching a new ATL — flag these
- New model releases (e.g., HR004GZ01 replacing HR002GZ) often start at ATL and rise — buy early

## Error Prevention Checklist

Before sharing any deal with the user:
- [ ] Price verified via Geizhals API (not just web search snippet)
- [ ] Geizhals product ID confirmed to be the correct Makita tool (not a mismatched product)
- [ ] Link tested — fetched the URL and confirmed it shows the right product
- [ ] Shipping costs checked — "free shipping" claims verified on actual listing
- [ ] Solo tool confirmed — no battery packs included in the listing
- [ ] If willhaben/eBay link — confirmed listing is still live (not sold/removed)
