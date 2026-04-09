# Makita Deal Finder

Find the best genuine Makita power tool deals available in Austria. Tracks both **18V LXT** and **40V XGT** solo tools (no battery packs) against verified all-time low prices using the Geizhals.at price history API.

## What It Does

1. **Discovers** all Makita solo tools listed on Geizhals.at (62+ tracked products)
2. **Pulls real price history** via the Geizhals.at API to determine all-time lows
3. **Compares** with willhaben.at and eBay.de for additional marketplace deals
4. **Ranks** every tool by proximity to its historical lowest price
5. **Provides** direct purchase links to the cheapest Austrian shops

## How to Use

### Invoke the Agent (recommended)

The **agent** is the best entry point — it orchestrates the full workflow automatically:

```
/agent makita-deal-finder
```

Then ask naturally:

- *"Find me the best Makita deals right now"*
- *"Is the Makita HR004GZ01 at a good price?"*
- *"Refresh all prices and show what's at ATL"*

The agent will launch parallel searches, pull API data, verify prices, and present a ranked report.

### Invoke the Skill

The **skill** is a reference document containing the full methodology, API details, and product ID maps. It's loaded as context when the agent runs, but you can also invoke it directly:

```
/skills makita-deal-finder
```

Use the skill directly when you already know what you want and just need the API details or product IDs — for example, to write a custom price-checking script.

### Agent vs Skill — When to Use Which

| Use Case | Use |
|----------|-----|
| Full deal analysis with ranked results | **Agent** |
| Check if a specific tool is at a good price | **Agent** |
| Refresh all prices across the tracked catalog | **Agent** |
| Look up a Geizhals product ID | Skill |
| Reference the API endpoint/parameters | Skill |
| Build your own price-tracking script | Skill |

## Key Technical Details

- **Geizhals API**: `POST https://geizhals.at/api/gh0/price_history` — reverse-engineered endpoint that returns full price history for any tracked product
- **62+ tracked products**: 27 LXT + 35 XGT tools with verified Geizhals IDs
- **Multi-model search**: Uses parallel agents on different AI models (e.g., Opus + GPT) to maximize product discovery
- **Deal categories**: 🔥 At ATL (0%) → ✅ Near ATL (<10%) → 👍 Decent (10-15%) → ⏳ Wait (>15%)

## Example Output

```
🔥 AT ALL-TIME LOW RIGHT NOW
┌─────────────┬──────────────────────────────────┬─────────┐
│ Model       │ Tool                             │ Price   │
├─────────────┼──────────────────────────────────┼─────────┤
│ UC030GZ03   │ XGT Chainsaw 50cm BL             │ €439.90 │
│ TW004GZ01   │ XGT Impact Wrench 1/2" 350Nm     │ €190.71 │
│ TM001GZ     │ XGT Multitool BL (Starlock Max)  │ €167.33 │
│ DF001GZ     │ XGT Drill/Driver BL              │ €130.08 │
└─────────────┴──────────────────────────────────┴─────────┘
```
