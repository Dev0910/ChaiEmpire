# Game Design

## High Concept

Chai Empire is a portrait Android incremental clicker where the player starts with one small gali tapri and grows into a national chai brand.

The game begins with manual work: tap the kettle, serve customers, earn rupees. Over time, the player buys upgrades that make manual tapping stronger and unlocks automation that keeps the stall running by itself. The long-term fantasy is moving from hands-on tapri owner to operator of a growing chai network: better recipes, staff, UPI collection, snack counters, delivery, franchises, tea estate contracts, and export counters.

## Design Promise

The player should feel:

- "I started with nothing but a kettle."
- "Every upgrade made my stall more alive."
- "My early chores became automated because I earned that comfort."
- "Each new location changes the scale of my business."
- "When prestige arrives, I am not losing progress; I am preserving the secret masala that lets me rebuild faster."

## Target Audience

Primary audience:

- Casual Android players who enjoy incremental, idle, and tycoon games.
- Indian players who recognize tapri culture, UPI, railway platforms, college canteens, office chai breaks, snacks, and festival demand.
- Global players curious about a cozy Indian business fantasy.

Secondary audience:

- Players who like optimization and number growth.
- Players who play in short sessions throughout the day.
- Players who enjoy games that continue while they are away.

## Tone

The tone should be:

- Warm.
- Grounded.
- Cozy.
- Lightly funny.
- Business-growth focused.
- Indian without becoming caricature.

Avoid:

- Mythology-heavy combat framing for the base game.
- Gambling-like mechanics.
- Harsh failure states.
- Pushy monetization.
- Overly complex strategy in the first session.

## Core Fantasy

The fantasy is local entrepreneurship. The player is not a hero saving the world; they are building a beloved chai business through persistence, taste, timing, and smart automation.

The world starts small and specific:

- A kettle.
- A stove.
- A lane-side stall.
- A queue.
- A few rupees.

It expands into:

- Better ingredients.
- Helpers.
- Payment systems.
- Snacks.
- Branded stalls.
- More locations.
- Delivery and franchise systems.
- Supply contracts.
- Export counters.

## Theme Pillars

| Pillar | Meaning | Gameplay expression |
| --- | --- | --- |
| Chai is routine | Chai is part of daily rhythm. | Rush hours, commuters, office demand, exam nights. |
| Growth is visible | The stall should visibly and numerically grow. | Upgrade levels, higher tap value, higher production rate, locations. |
| Automation is earned | The player should graduate from repetitive tapping. | Helper, UPI cashier, bulk kettle, passive upgrades. |
| India is mechanical, not cosmetic | The theme should affect systems. | UPI collection, railway/platform demand, snack combos, festival events. |
| Cozy optimization | The game should invite checking in, not demand constant attention. | Offline progress, capped rewards, short session loops. |

## Player Journey

### Stage 1: Lone Tapri Owner

Player actions:

- Tap kettle.
- Serve queue.
- Buy first recipe upgrades.

Design goal:

- Teach that rupees come from direct action.
- Make the first upgrade arrive quickly.
- Show production rate even before automation, so the player understands where the game is going.

### Stage 2: First Automation

Player actions:

- Buy Helper Boy.
- Buy UPI Cashier.
- Buy Bulk Kettle.

Design goal:

- Convert clicking into management.
- Make passive income visibly tick upward.
- Make manual tapping still useful, but no longer the only path.

### Stage 3: Location Expansion

Player actions:

- Unlock Bus Stand, Railway Platform, College Canteen, and beyond.
- Choose between more automation and new demand multipliers.

Design goal:

- Give medium-term goals.
- Make expansion feel like growing the city footprint.
- Give the player a reason to save for large purchases.

### Stage 4: Brand And Supply

Player actions:

- Invest in signboard, influencer reel, delivery, franchise, tea estate contract, export counter.

Design goal:

- Move from stall operations into brand operations.
- Prepare the economy for prestige.

### Stage 5: Secret Masala Prestige

Current status: implemented with prestige confirmation, reset, skill points, skill tree rows, and skill effects.

Player actions:

- Reset current rupees/upgrades/locations.
- Keep Masala Legacy.
- Spend skill points in a tree.
- Rebuild faster and reach new thresholds.

Design goal:

- Let the first reset feel empowering.
- Give long-term structure after the airport/lounge tier.

## Current Feature Scope

Implemented:

- Rupees currency.
- Chai served counter.
- Manual kettle tap.
- Serve queue action.
- Passive production from upgrades.
- Upgrade purchases.
- Location unlocks.
- Demand multiplier from highest unlocked location.
- Global multiplier from brand upgrades.
- Rush Hour boost.
- Local save/load.
- Offline reward with cap and efficiency.
- Prestige reset, Masala Legacy, skill points, and skill effects.
- Optional rotating live events.
- Optional sponsor rewards and cosmetic selections.
- Android portrait runtime-created UI.
- Android APK build entrypoint.

Planned/future:

- Real ad SDK and billing integration.
- Analytics.
- Cloud save.
- Better art/audio.

## Success Criteria For The Game

Short-term success:

- Player understands how to earn rupees in under 10 seconds.
- Player buys first upgrade within the first minute.
- Player unlocks first automation quickly enough to feel progression.

Medium-term success:

- Player has meaningful choices between tap strength, automation, and location unlocks.
- Player returns after closing the app and receives a clear offline reward.
- Player can read all important numbers on mobile.

Long-term success:

- Player sees the airport/lounge tier and prestige preview as an aspirational target.
- Future prestige gives a strong reason to replay without feeling punitive.
