# Content Catalog

This catalog lists current implemented content and planned future content ideas.

## Upgrade Definitions

Current upgrades are authored in `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
`ChaiContent.CreateDefault()` loads this catalog at runtime and falls back to the built-in defaults from `ChaiContentData.CreateBuiltInDefault()` if the resource is missing or invalid.

| ID | Name | Description | Category | Kind | Base cost | Cost multiplier | Value |
| --- | --- | --- | --- | --- | ---: | ---: | ---: |
| `strong-tea` | Strong Tea Leaves | Each kettle tap brews one extra paid cup. | Brew Craft | TapFlat | 10 | 1.55 | +1 tap flat |
| `adrak-kick` | Adrak Kick | Ginger aroma makes every cup worth more. | Brew Craft | TapMultiplier | 45 | 1.6 | +15% tap |
| `elaichi-aroma` | Elaichi Aroma | Cardamom pulls people in from the next lane. | Brew Craft | TapMultiplier | 120 | 1.66 | +20% tap |
| `helper-boy` | Helper Boy | Auto-serves the queue while you focus on brewing. | Automation | PassiveFlat | 50 | 1.65 | +0.5/sec |
| `upi-cashier` | UPI Cashier | A QR stand collects payments automatically. | Automation | PassiveFlat | 95 | 1.68 | +0.35/sec |
| `bulk-kettle` | Bulk Kettle | A larger kettle keeps chai flowing by itself. | Automation | PassiveFlat | 180 | 1.7 | +2/sec |
| `samosa-counter` | Samosa Counter | Snacks raise average order value through the day. | Add-ons | PassiveFlat | 500 | 1.72 | +5/sec |
| `bun-maska-tray` | Bun Maska Tray | Buttery side orders keep office crowds lingering. | Add-ons | PassiveFlat | 1,250 | 1.75 | +12/sec |
| `kulhad-stack` | Kulhad Stack | Clay cups make every location feel memorable. | Add-ons | PassiveFlat | 3,500 | 1.78 | +28/sec |
| `painted-signboard` | Painted Signboard | A bright board improves demand everywhere. | Brand | GlobalMultiplier | 1,000 | 1.85 | +10% all |
| `influencer-reel` | Influencer Reel | A local food reel brings new queues overnight. | Brand | GlobalMultiplier | 5,000 | 1.9 | +35% all |
| `delivery-partner` | Delivery Partner | Thermos runs send chai to offices nearby. | Expansion | PassiveFlat | 25,000 | 1.9 | +80/sec |
| `franchise-kit` | Franchise Kit | A repeatable stall setup spreads your recipe. | Expansion | PassiveFlat | 250,000 | 1.95 | +600/sec |
| `tea-estate-contract` | Tea Estate Contract | Direct supply stabilizes the national rollout. | Supply | PassiveFlat | 2,000,000 | 2.0 | +3,000/sec |
| `export-counter` | Export Counter | Packaged masala chai starts leaving the country. | Late Game | PassiveFlat | 20,000,000 | 2.05 | +25,000/sec |

## Upgrade Categories

| Category | Current role | Future expansion |
| --- | --- | --- |
| Brew Craft | Improves manual tapping. | Recipes, regional chai styles, active-play skills. |
| Automation | Adds early passive income. | Staff roles, training, queue management. |
| Add-ons | Adds passive income through snacks/cups. | Combos, margins, festival specials. |
| Brand | Multiplies all income. | Reputation, loyalty, social campaigns. |
| Expansion | Adds larger passive systems. | Delivery routes, franchise tiers. |
| Supply | Adds late passive scale. | Ingredient contracts, price stability. |
| Late Game | Adds very high production. | Export markets, packaged products. |

## Location Definitions

Current locations are authored in `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
The JSON catalog is validated before conversion into immutable `LocationDefinition` values.

| ID | Name | Description | Unlock cost | Demand multiplier | Default |
| --- | --- | --- | ---: | ---: | --- |
| `gali-tapri` | Gali Tapri | Your first loyal lane-side queue. | 0 | 1 | Yes |
| `bus-stand` | Bus Stand | Morning commuters create steady demand. | 250 | 1.25 | No |
| `railway-platform` | Railway Platform | Peak-hour trains turn chai into a rhythm game. | 2,000 | 1.65 | No |
| `college-canteen` | College Canteen | Exam nights and gossip tables stretch demand. | 12,000 | 2 | No |
| `it-park` | IT Park | Sprint planning runs on cutting chai. | 80,000 | 2.75 | No |
| `highway-dhaba` | Highway Dhaba | Truckers and families keep the stove hot. | 500,000 | 3.7 | No |
| `mall-kiosk` | Mall Kiosk | Premium cups meet weekend footfall. | 5,000,000 | 5.2 | No |
| `airport-lounge` | Airport Lounge | Your brand is now national enough to prestige later. | 100,000,000 | 8 | No |

## Current UI Text

The runtime UI currently includes:

- Title: `Chai Empire`
- Primary action: `Tap Kettle`
- Secondary action: `Serve Queue`
- Burst action: `Rush Hour`
- Upgrade section title: `Upgrades`
- Location section title: `Locations`
- Prestige section title: `Secret Masala`
- Startup status if no offline reward: `Gali Tapri is open`
- Offline status: `Welcome back: Rs <amount>`
- Tap feedback: `Fresh cutting chai`
- Queue feedback: `Queue served`
- Rush feedback: `Rush hour: 2x for 20 sec`

## Planned Content Ideas

These are future ideas, not implemented.

### Recipes

| Recipe | Role |
| --- | --- |
| Cutting Chai | Early default recipe. |
| Adrak Chai | Active-tap multiplier. |
| Elaichi Chai | Demand or customer multiplier. |
| Masala Chai | Global income multiplier. |
| Irani Chai | Late location specialty. |
| Kulhad Chai | Offline or brand multiplier. |
| Gur Chai | Seasonal winter event recipe. |

### Snacks

| Snack | Role |
| --- | --- |
| Rusk | Cheap early add-on. |
| Biscuit Plate | Early passive boost. |
| Bun Maska | Mid passive boost. |
| Samosa | Strong add-on production. |
| Vada Pav | Location synergy with bus/rail. |
| Poha | Morning rush modifier. |
| Pakora | Monsoon event modifier. |

### Events

| Event | Possible mechanic |
| --- | --- |
| Monsoon Chai Rush | Higher tap value and snack demand for a timed window. |
| Diwali Sweet Combo | Add temporary sweet counters and gift box upgrades. |
| Cricket Match Night | Long active session bonus with crowd waves. |
| Exam Season | College Canteen production bonus. |
| Office Deadline Week | IT Park demand bonus. |
| Winter Morning | Adrak/Gur recipes gain bonuses. |

### Future Locations

| Location | Possible role |
| --- | --- |
| Tourist Ghat | Regional specialty recipes. |
| Metro Station | High-frequency commuter demand. |
| Film Studio | Brand/influencer synergy. |
| Corporate Campus | Delivery and subscription mechanics. |
| Tea Expo | Prestige-adjacent global demand. |

## Content Authoring Rules

- Edit the default catalog at `Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json`.
- Use `Chai Empire > Content > Validate Default Catalog` before committing content changes.
- Use `Chai Empire > Content > Export Built-In Catalog JSON` to regenerate JSON from the deterministic built-in fallback.
- Every upgrade must have a stable ID.
- IDs should be lowercase kebab case.
- Display names should be short enough for mobile buttons.
- Flavor text should explain the Indian theme and the gameplay role.
- Each upgrade must declare one primary mechanical role.
- Avoid adding several multipliers at the same early tier.
- Data-driven content should preserve the same fields as `UpgradeDefinition` and `LocationDefinition`.
