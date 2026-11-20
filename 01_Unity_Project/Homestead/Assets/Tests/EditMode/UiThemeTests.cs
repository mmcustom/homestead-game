using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Wave 7 "Ledger" look (Mike chose it 2026-11-20): UiKit loads the Alegreya fonts and the parchment / wood sprites, gives panels
// the framed parchment, writes in brown ink on the screens, and keeps light text for anything on a dark background.
public class UiThemeTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static Type Kit => Find("UiKit");
    static object Get(string name)
    {
        FieldInfo field = Kit.GetField(name, BindingFlags.Public | BindingFlags.Static);
        return field != null ? field.GetValue(null) : Kit.GetProperty(name, BindingFlags.Public | BindingFlags.Static).GetValue(null);
    }
    static T Call<T>(string name, params object[] args) =>
        (T)Kit.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(null, args);

    GameObject root;
    [SetUp] public void SetUp() => root = new GameObject("UiThemeTests", typeof(RectTransform));
    [TearDown] public void TearDown() => UnityEngine.Object.DestroyImmediate(root);

    [TestCase("Font", "AlegreyaSans-Regular")]
    [TestCase("BoldFont", "AlegreyaSans-Bold")]
    [TestCase("TitleFont", "Alegreya-Bold")]
    public void TheLedgerFontsLoad(string property, string fontName)
    {
        Assert.That(((Font)Get(property)).name, Is.EqualTo(fontName));
    }

    [Test]
    public void TheFontsCarryTheirLicences()
    {
        Assert.That(Resources.Load<TextAsset>("Fonts/OFL_alegreya"), Is.Not.Null);
        Assert.That(Resources.Load<TextAsset>("Fonts/OFL_alegreyasans"), Is.Not.Null);
    }

    [TestCase("PanelSprite", 72f)]
    [TestCase("CardSprite", 14f)]
    [TestCase("ButtonSprite", 16f)]
    public void TheSpritesLoadAsNineSlices(string property, float border)
    {
        var sprite = (Sprite)Get(property);
        Assert.That(sprite, Is.Not.Null);
        Assert.That(sprite.border, Is.EqualTo(new Vector4(border, border, border, border)));
    }

    [Test]
    public void APanelGetsTheFramedParchment()
    {
        Image panel = Call<Image>("Image", root.transform, "Panel", (Color)Get("PanelColor"));
        Assert.That(panel.sprite, Is.SameAs(Get("PanelSprite")));
        Assert.That(panel.type, Is.EqualTo(Image.Type.Sliced));
        Assert.That(panel.color, Is.EqualTo(Color.white), "the art is drawn in its own colours");
    }

    [Test]
    public void ARowGetsATintedPaperCard()
    {
        Color raised = (Color)Get("RaisedColor");
        Image row = Call<Image>("Image", root.transform, "Row", raised);
        Assert.That(row.sprite, Is.SameAs(Get("CardSprite")));
        Assert.That(row.color, Is.EqualTo(raised));
    }

    [Test]
    public void OtherColoursStayPlainRectangles()
    {
        Image dim = Call<Image>("Image", root.transform, "Dim", new Color(0f, 0f, 0f, 0.6f));
        Assert.That(dim.sprite, Is.Null);
    }

    [Test]
    public void ButtonsAreInkOnPaper()
    {
        Button button = Call<Button>("Button", root.transform, "B", "Use", 20, null, TextAnchor.MiddleCenter);
        Text label = button.GetComponentInChildren<Text>();
        Assert.That(label.color, Is.EqualTo((Color)Get("Ink")));
        Assert.That(label.font, Is.SameAs(Get("BoldFont")));
        Assert.That(((Image)button.targetGraphic).sprite, Is.SameAs(Get("ButtonSprite")));
    }

    [Test]
    public void LabelsOnDarkButtonsAreCream()
    {
        Button button = Call<Button>("Button", root.transform, "B", "Cook", 20, null, TextAnchor.MiddleCenter);
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.36f, 0.25f, 0.13f, 1f);
        button.colors = colors;
        Assert.That(Call<Color>("LabelColor", button, true), Is.EqualTo((Color)Get("Cream")));
        Assert.That(Call<Color>("LabelColor", button, false), Is.EqualTo((Color)Get("Muted")));
    }

    [Test]
    public void HeadingsUseTheSerifAndTextIsEnlarged()
    {
        Text heading = Call<Text>("Heading", root.transform, "H", "Inventory", 34, TextAnchor.MiddleLeft);
        Assert.That(heading.font, Is.SameAs(Get("TitleFont")));
        Assert.That(heading.fontSize, Is.EqualTo(34), "large sizes are left as agreed");
        Assert.That(Call<Text>("Text", root.transform, "S", "x", 20, Color.black, TextAnchor.MiddleLeft, FontStyle.Normal).fontSize,
                    Is.EqualTo(22), "small text is raised by the 1.12 legibility scale");
        Assert.That(Call<Text>("Text", root.transform, "M", "x", 25, Color.black, TextAnchor.MiddleLeft, FontStyle.Normal).fontSize,
                    Is.EqualTo(26), "but never past the large size");
    }

    [Test]
    public void BoldTextUsesTheRealBoldFace()
    {
        Text text = Call<Text>("Text", root.transform, "T", "x", 20, Color.black, TextAnchor.MiddleLeft, FontStyle.Bold);
        Assert.That(text.font, Is.SameAs(Get("BoldFont")));
        Assert.That(text.fontStyle, Is.EqualTo(FontStyle.Normal), "no fake bold on top of the bold face");
    }

    [Test]
    public void InkReadsOnTheParchment()
    {
        // WCAG contrast of the ink colours on the darkest paper used (the selected card), at least 4.5:1 for body text.
        Color paper = (Color)Get("SelectedColor");
        foreach (string ink in new[] { "Ink", "InkMuted", "InkWarning", "HeadingColor" })
            Assert.That(Contrast((Color)Get(ink), paper), Is.GreaterThanOrEqualTo(ink == "InkMuted" || ink == "InkWarning" ? 3.0f : 4.5f), ink);
    }

    static float Lum(Color c)
    {
        float L(float v) => v <= 0.03928f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * L(c.r) + 0.7152f * L(c.g) + 0.0722f * L(c.b);
    }
    static float Contrast(Color a, Color b)
    {
        float la = Lum(a), lb = Lum(b);
        return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
    }

    [Test]
    public void FontsUseLiningFigures()
    {
        // The digits were remapped to Alegreya's lining tabular figures (one.tf ...), so numbers sit on the line and line up in columns.
        var font = (Font)Get("Font");
        font.RequestCharactersInTexture("0123456789", 40);
        font.GetCharacterInfo('1', out CharacterInfo one, 40);
        font.GetCharacterInfo('8', out CharacterInfo eight, 40);
        Assert.That(one.advance, Is.EqualTo(eight.advance), "tabular: every digit is the same width");
    }

    // Homestead/UI/Apply Ledger Look restyled the scene-built UI: every text in the HUD, Pause Menu and Main Menu uses the
    // Ledger fonts, and the menu buttons are paper with ink labels.
    [TestCase("Assets/Scenes/World.unity")]
    [TestCase("Assets/Scenes/MainMenu.unity")]
    public void TheSceneUiUsesTheLedgerLook(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            int texts = 0, buttons = 0;
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                foreach (Text text in go.GetComponentsInChildren<Text>(true))
                {
                    if (text.GetComponentInParent<Canvas>(true) == null) continue;
                    Assert.That(text.font.name, Does.StartWith("Alegreya"), text.name + " in " + text.transform.parent.name);
                    texts++;
                }
                foreach (Button button in go.GetComponentsInChildren<Button>(true))
                {
                    Assert.That(((Image)button.targetGraphic).sprite, Is.SameAs(Get("ButtonSprite")), button.name);
                    Assert.That(button.GetComponentInChildren<Text>(true).color, Is.EqualTo((Color)Get("Ink")), button.name);
                    buttons++;
                }
            }
            Assert.That(texts, Is.GreaterThan(5));
            Assert.That(buttons, Is.GreaterThanOrEqualTo(3));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    // Wave 7 / 10 pictures: the Loss and Loading backgrounds and the logo load; the Main Menu shows the logo over its picture and
    // the Loading scene builds its screen.
    [TestCase("Art/loss_background", 1920)]
    [TestCase("Art/title_background", 1920)]
    [TestCase("Art/logo_lockup", 2000)]
    public void TheTitleArtLoads(string path, int width)
    {
        var texture = Resources.Load<Texture2D>(path);
        Assert.That(texture, Is.Not.Null, path);
        Assert.That(texture.width, Is.EqualTo(width));
    }

    [Test]
    public void TheMainMenuShowsTheLogoOverItsPicture()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Additive);
        try
        {
            Image logo = null, background = null;
            foreach (GameObject go in scene.GetRootGameObjects())
                foreach (Image image in go.GetComponentsInChildren<Image>(true))
                {
                    if (image.name == "Logo") logo = image;
                    if (image.name == "Background") background = image;
                }
            Assert.That(logo, Is.Not.Null);
            Assert.That(logo.sprite.name, Is.EqualTo("logo_lockup"));
            Assert.That(background.sprite.name, Is.EqualTo("menu_background"));
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    [Test]
    public void TheLoadingSceneHasItsScreen()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity", OpenSceneMode.Additive);
        try
        {
            bool found = false;
            foreach (GameObject go in scene.GetRootGameObjects())
                found |= go.GetComponent("LoadingScreen") != null;
            Assert.That(found);
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }
}
