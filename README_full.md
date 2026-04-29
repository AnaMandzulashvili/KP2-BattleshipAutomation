# BattleshipAutomation

An end-to-end test automation bot for [Battleship Game](http://en.battleship-game.org). It opens the site, places a fleet, finds a random opponent, plays the game autonomously, and asserts a victory.

---

## How it works

1. Opens the Battleship game page
2. Randomly places the fleet on the board
3. Finds a random online opponent and starts the game
4. Plays automatically — picks the best cell to fire at, shoots, remembers the result, repeats
5. Asserts the bot won

---

## Tech stack

| What for | Tool |
|---|---|
| Language | C# / .NET 8.0 |
| BDD | Reqnroll (Gherkin) |
| Browser automation | Aquality.Selenium + ChromeDriver |
| Test runner | NUnit |
| Logging | NLog |
| Reporting | ExtentReports |

---

## Project structure

```
src/BattleshipAutomation/
├── Features/          # Gherkin scenario
├── Steps/             # Step definitions
├── Screens/           # Page objects (SetupScreen, GameScreen)
├── Game/              # Game logic (service, strategy, shot processor)
├── Models/            # GameBoard, Cell, Ship
├── Enums/             # CellState, GameOutcome, HuntDirection
├── Config/            # Typed config classes
├── Utils/
│   ├── Hooks/         # Test lifecycle (browser, DI, reporting)
│   └── FrameworkAdditions/  # LogMethod AOP decorator
├── gameSettings.json  # URL, board size, fleet, timeouts
├── settings.json      # Aquality / Chrome config
└── nlog.config        # Logging config
```

---

## Running the test

```bash
git clone <repository-url>
cd BattleshipAutomation
dotnet restore src/BattleshipAutomation/BattleshipAutomation.csproj
dotnet test src/BattleshipAutomation/BattleshipAutomation.csproj
```

> An internet connection is required — the site matches you with a live opponent.

---

## Configuration

All settings are in `gameSettings.json`:

| Property | Value | Description |
|---|---|---|
| `BaseUrl` | `http://en.battleship-game.org` | Game URL |
| `BoardSize` | `10` | 10×10 grid |
| `Fleet` | `[5, 4, 3, 3, 2, 2, 1]` | Ship sizes |
| `MinRandomiseClicks` | `1` | Min clicks on Randomise |
| `MaxRandomiseClicks` | `15` | Max clicks on Randomise |

**Timeouts:**

| Property | Seconds | Description |
|---|---|---|
| `OpponentWaitSeconds` | `180` | Wait for opponent to join |
| `TurnWaitSeconds` | `120` | Wait for your turn |
| `ShotResultSeconds` | `15` | Wait for shot result |
| `PageLoadSeconds` | `60` | Wait for page to load |
| `ShotSettleSeconds` | `3` | Wait for board to settle after a hit |

---

## Bot strategy

The bot uses **Hunt & Target** with a **Probability Density** overlay:

- **Hunt phase** — builds a map of all possible ship placements and fires at the cell with the highest probability. A checkerboard bias reduces wasted shots (the smallest ship is size 2, so every other cell can be skipped).
- **Target phase** — after a hit, fires at adjacent cells. On the second hit it locks the direction (horizontal or vertical) and keeps firing along that axis until the ship sinks.

---

## Reports and logs

| Output | Location |
|---|---|
| HTML report | `bin/Debug/net8.0/Reports/index.html` |
| Log file | `bin/Debug/net8.0/Log/Debug.log` |
