// The weapon a New Game starts with, fixed by the difficulty (Mike, 2026-10-23): Pioneer, the hardest, carries the bow and arrows;
// Settler and Homesteader carry the rifle and rounds. There is no weapon choice at New Game. Pure and engine-free so edit-mode tests
// cover it. Difficulty is the game's enum as a number (Homesteader 0, Settler 1, Pioneer 2), because the enum lives in the main assembly.
public enum StartingWeapon { Bow, Rifle }

public static class StartingWeapons
{
    public const int Pioneer = 2;

    public const string BowId = "recurve_bow", ArrowsId = "arrows", RifleId = "bolt_action_rifle", RoundsId = "rifle_rounds";
    public const int Arrows = 20, Rounds = 10;

    public static StartingWeapon For(int difficulty) => difficulty == Pioneer ? StartingWeapon.Bow : StartingWeapon.Rifle;

    // The weapon and its ammunition, as kit lines (item id, count).
    public static (string item, int count)[] Items(StartingWeapon weapon) => weapon == StartingWeapon.Rifle
        ? new[] { (RifleId, 1), (RoundsId, Rounds) }
        : new[] { (BowId, 1), (ArrowsId, Arrows) };
}
