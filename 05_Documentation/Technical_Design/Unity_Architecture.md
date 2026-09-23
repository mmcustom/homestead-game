# Homestead
## Unity Architecture v1.0

Status: Approved

---

# Engine

Unity 6.3 LTS

Version:

6000.3.24f1

HDRP

---

# Scene Architecture

Bootstrap

MainMenu

World

Loading

---

# Bootstrap Scene

Purpose:

Initialize all game managers.

Managers persist between scenes using:

DontDestroyOnLoad()

Bootstrap is loaded first.

Bootstrap loads MainMenu.

---

# Manager Architecture

## GameManager

Coordinates all systems.

Responsibilities:

- Application Lifecycle
- Scene Transitions
- System Initialization

---

## SaveManager

Responsibilities:

- Save
- Load
- Autosave

Storage Format:

JSON

---

## TimeManager

Responsibilities:

- Clock
- Day/Night
- Calendar
- Seasons

---

## DiscoveryManager

Responsibilities:

- Discoveries
- Knowledge Tracking
- Map Markers

---

## JournalManager

Responsibilities:

- Journal Entries
- Player Notes
- Discovery Records

---

## InventoryManager

Responsibilities:

- Player Inventory
- Storage Containers
- Equipment

---

## WeatherManager

Responsibilities:

- Weather
- Temperature
- Seasonal Conditions

---

## AudioManager

Responsibilities:

- Music
- Ambient Effects
- Sound Effects

---

# Save Architecture

Format:

JSON

Structure:

player.json

inventory.json

world.json

journal.json

discovery.json

livestock.json

---

# Script Organization

Assets

└── Scripts

    ├── Core

    ├── Managers

    ├── Player

    ├── Survival

    ├── Discovery

    ├── Inventory

    ├── Wildlife

    ├── Saving

    ├── UI

    └── World

---

# Design Rule

All major game systems communicate through managers.

Managers are initialized from Bootstrap.

Only one active manager instance may exist at a time.