# Debug Cash Keys Checklist

**Goal:** check the two new testing keys on a real keyboard. Editor and development builds only.
**Keys:** PageUp adds $1,000 cash per press. Shift+PageUp sets cash to $0. (F11 is taken by the starting kit for older saves, F12 is the full kit.)
**Before you start:** back up your Saves folder. Use a throwaway game, not one you are measuring income in.
**How to report:** item number and pass or fail, with a short note on any fail.

---

1. In game (Playing state), press PageUp once. Cash goes up by exactly $1,000.00 and a HUD message like "Debug: +$1,000.00 (cash $X)" shows with the usual date and time.
2. Press PageUp five more times. Cash is up exactly $5,000.00 more, one step per press, no skips.
3. Press Shift+PageUp. Cash becomes $0 and the HUD shows "Debug: cash set to $0".
4. With cash at $0, press PageUp. It adds $1,000.00 again (the zero did not break the add key).
5. Press L and look at the current season. Income and "Income per real hour" are unchanged by the debug presses (no income appears from them). The difference shows only on the "Other (not in the ledger)" line.
6. The Trading Post, Inventory and Finances screens show the new cash amount right away (open each one after pressing the key).
7. Press PageUp while the Trading Post is open (or any menu). Note what happens. The keys are meant to work only while playing, so nothing should change, and nothing should break.
8. Save, press Shift+PageUp, then Continue or load that save. Cash comes back to the saved amount.
9. F12 (full kit) and F11 still behave as before.
10. Zero your cash and skip past a due day (use your time keys). The missed-payment fee and the missed-payment count still behave normally for a broke player (this is the check Claude Code did not run).
11. Press End to export the CSV. The "Cash at end" column includes the debug cash; every other number matches the Statement. (Expected, not a bug.)
12. Console is clean (only the known MCP "WebSocket is not initialised" message).

---

**Known and accepted:** debug cash is not recorded as income or expense, so it only shows on the Statement's "Other" line, and mixes with any other untracked money. Do not use these keys in a game you are measuring income in; the "Cash at end" figure would be off.
