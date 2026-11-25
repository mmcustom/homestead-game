// An optional status shown above an interactable's key lines (InteractionPromptHud), with no key of its own: e.g. the Hide Frame's
// "Hide Frame: a Deer Hide, scraped 3 of 20 strokes / Equip the Knife, then hold click to scrape it". With a header, an empty
// InteractionPrompt or SecondaryPrompt just hides that key's line, so each key line says only what that key does (Mike, 2026-11-25).
public interface IPromptHeader
{
    // Empty or null shows no header.
    string PromptHeader { get; }
}
