// A system that owns one block of save data (Save_Data_Model.md: "each system owns its own data block").
// Register with SaveManager.Instance.Register(this) once SaveManager exists, and unregister on destroy.
public interface ISaveable
{
    // Save file the block is written to — one of Unity_Architecture.md's files:
    // player, inventory, world, journal, discovery, livestock.
    string SaveFile { get; }

    // Unique name for this block within its file, e.g. "time" or "weather" inside world.json.
    string SaveKey { get; }

    // Returns a [Serializable] object that JsonUtility can write.
    object CaptureState();

    // Receives the JSON previously produced from CaptureState().
    void RestoreState(string json);
}
