# Territorial GMI

Territorial income for Frontline Logistics: Isarian Warfare.
The mod replaces the daily accommodation payment with income from player-controlled territory and infrastructure.

## Requirements

- Frontline Logistics: Isarian Warfare, Windows x64, Mono runtime.
- Game version 0.8.706 is the documented compatibility baseline. Other versions are not verified.
- [BepInEx 5.4.23.6, Windows x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).

The release does not include BepInEx, game assemblies, or game files.

## Installation

Back up your saves before you install an economy mod.

1. Close the game.
2. Install BepInEx in the game directory if necessary.
3. Download `TerritorialGmi-v1.2.1-win-x64.zip` from [Releases](https://github.com/biohazardous-man/territorial-gmi/releases).
4. Extract the archive into the game directory.
5. Start the game through Steam.

The installed files must have these paths:

```text
BepInEx/plugins/TerritorialGmi/FrontlineLogistics.TerritorialGmi.dll
BepInEx/plugins/TerritorialGmi/Localization/TerritorialGmi.csv
```

To remove the mod, close the game and remove `BepInEx/plugins/TerritorialGmi`.
Money already paid remains in the save.
The normal accommodation payment returns unless another mod replaces it.

## Campaign settings

Open **New Campaign → Territorial GMI → Configure**.
The checkbox enables the mod for that campaign.
The panel accepts whole-number rates from 0 through 1,000,000.

- **Apply** stores the rates for the new campaign.
- **Defaults** resets the panel fields only.
- **Cancel** or **Escape** closes the panel without new changes.
- The panel is not available during a campaign. F6 does not open it.

Each save stores its own enabled state, rates, and last payment date.
New campaigns receive the default rates.
Existing saves without mod settings use the legacy configuration when available.
Otherwise, they use the default rates with the mod enabled.
The legacy file is `BepInEx/config/local.frontlinelogistics.territorial-gmi.cfg`.
The mod reads this file for migration only. The panel does not change it.

## Income rules

Income appears in the normal **Daily Report** at the supply arrival hour.
Only tiles owned by the player's side qualify. Visibility and base range do not establish ownership.
The mod counts territory at payment time.
It does not pay for missed dates or dates before installation.
Repeated reports and save reloads cannot repeat a paid date.
An existing save receives its first payment on the next game date.

| Component | Default daily GMI |
| --- | ---: |
| Controlled tile | 5 |
| Minimum land payment, with at least one controlled tile | 10,000 |
| Road tile bonus | 20 |
| Rail tile bonus | 40 |
| Bridge tile bonus | 150 |
| Building tile bonus | 75 |

```text
income = max(10000, tiles * 5) + roads * 20 + rails * 40 + bridges * 150 + buildings * 75
```

Income is zero without controlled tiles.
Infrastructure bonuses remain above the land minimum.
Different categories can apply to one tile. Each category counts once per tile; floors do not multiply bonuses.
These defaults are generous, not the result of a long campaign balance study.

The mod leaves food, rest, morale, combat rewards, evacuation, sales, and refunds unchanged.
It does not require Comfort Morale or Rear Supplies.

The interface and daily report follow the game language, with English as the fallback.
The translation table supports the game's 16 language columns.

## Build from source

Install the .NET 10 SDK to build the mod and run the included regression scenarios.
Game assemblies and BepInEx must exist in a local game installation.
They are not distributed in this repository.

From the repository root, run this PowerShell command with your actual game directory:

```powershell
dotnet build TerritorialGmi/TerritorialGmi.csproj -c Release '-p:GameRoot=D:/Games/FrontlineLogistics/'
```

Keep the trailing slash in `GameRoot`.
The output is `TerritorialGmi/bin/Release/netstandard2.1/`.
A normal build does not install the mod.
To build and install it, close the game and add `-p:DeployPlugin=true` to the build command.

Run the income and payment-date regression scenarios:

```powershell
dotnet run --project TerritorialGmi.Tests/TerritorialGmi.Tests.csproj -c Release
```

## Compatibility limits

Game updates can change the internal methods used by this mod.
The mod does not reconstruct saved building-cell damage for map objects that have not been created in memory.
Logs are in `BepInEx/LogOutput.log`.
