# Homestead
## Unity Architecture v1.0

Status: Approved

---

# Purpose

Unity project-specific conventions for Homestead — scene structure, manager architecture, save format, and script organization inside 01_Unity_Project. Originally written by Copilot 2026-09-22 at `05_Documentation/Technical_Design/Unity_Architecture.md`; relocated here by Claude 2026-09-23, since this folder's confirmed purpose is exactly this kind of Unity project convention. Content is unchanged from the original — only reformatted to match this doc set's house style.

---

# Engine

Unity 6.3 LTS, version 6000.3.24f1, with HDRP.

---

# Scene Architecture

Four scenes: Bootstrap, MainMenu, World, Loading.

---

# Bootstrap Scene

Purpose: initialize all game managers. Managers persist between scenes via `DontDestroyOnLoad()`. Bootstrap is loaded first and loads MainMenu.

---

# Manager Architecture

## GameManager

Coordinates all systems: application lifecycle, scene transitions, system initialization.

---

## SaveManager

Save, load, autosave. Storage format: JSON.

---

## TimeManager

Clock, day/night, calendar, seasons.

---

## DiscoveryManager

Discoveries, knowledge tracking, map markers.

---

## JournalManager

Journal entries, player notes, discovery records.

---

## InventoryManager

Player inventory, storage containers, equipment.

---

## WeatherManager

Weather, temperature, seasonal conditions.

---

## AudioManager

Music, ambient effects, sound effects.

---

# Save Architecture

Format: JSON. Structure: player.json, inventory.json, world.json, journal.json, discovery.json, livestock.json.

---

# Script Organization

Assets/Scripts, with subfolders: Core, Managers, Player, Survival, Discovery, Inventory, Wildlife, Saving, UI, World.

---

# Design Rules

1. All major game systems communicate through managers.
2. Managers are initialized from Bootstrap.
3. Only one active manager instance may exist at a time.
