using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// HoverTip (2026-11-19): hovering an inventory icon shows the item's name beside the pointer; a click on the icon still does the row's action;
// the label goes away when the pointer leaves or the row is rebuilt.
public class HoverTipTests
{
    static Type Find(string name)
    {
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = a.GetType(name);
            if (t != null)
                return t;
        }
        throw new Exception(name + " not found");
    }

    static Transform Tip(GameObject canvas) => canvas.transform.Find("Hover Tip");

    [Test]
    public void HoveringShowsTheNameAndAClickStillReachesTheRow()
    {
        Type tipType = Find("HoverTip");
        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        try
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(canvas.transform, false);
            Image icon = iconGo.GetComponent<Image>();
            icon.raycastTarget = false;
            int clicks = 0;
            UnityAction onClick = () => clicks++;
            var tip = (Component)tipType.GetMethod("Add").Invoke(null, new object[] { icon, "Rough Stool", onClick });

            Assert.That(icon.raycastTarget, Is.True, "the icon answers the pointer");
            Assert.That(Tip(canvas), Is.Null, "nothing shows before the pointer arrives");

            var pointer = new PointerEventData(null) { position = new Vector2(100f, 100f), button = PointerEventData.InputButton.Left };
            ((IPointerEnterHandler)tip).OnPointerEnter(pointer);
            Transform box = Tip(canvas);
            Assert.That(box, Is.Not.Null);
            Assert.That(box.gameObject.activeSelf, Is.True);
            Assert.That(box.GetComponentInChildren<Text>().text, Is.EqualTo("Rough Stool"));
            Assert.That(box.GetSiblingIndex(), Is.EqualTo(canvas.transform.childCount - 1), "drawn on top");
            Assert.That(box.GetComponent<Image>().raycastTarget, Is.False, "the label never takes the pointer itself");

            ((IPointerClickHandler)tip).OnPointerClick(pointer);
            Assert.That(clicks, Is.EqualTo(1), "a click on the icon does the row's action");

            ((IPointerExitHandler)tip).OnPointerExit(pointer);
            Assert.That(box.gameObject.activeSelf, Is.False, "gone when the pointer leaves");

            ((IPointerEnterHandler)tip).OnPointerEnter(pointer);
            Assert.That(box.gameObject.activeSelf, Is.True);
            UnityEngine.Object.DestroyImmediate(iconGo);
            Assert.That(box.gameObject.activeSelf, Is.False, "gone when the row is rebuilt under the pointer");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(canvas);
        }
    }

    // The Trading Post and Storage rows (QuantityRowUi.SetIcon): a known item gets its picture down the left with its name on hover, and the
    // name, stepper and total move right of it; an id with no icon leaves the row exactly as it was.
    [Test]
    public void ATradeOrStorageRowShowsTheItemsIconAndNameOnHover()
    {
        Type rowType = Find("QuantityRowUi");
        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        try
        {
            var list = new GameObject("List", typeof(RectTransform)).GetComponent<RectTransform>();
            list.SetParent(canvas.transform, false);
            MethodInfo create = rowType.GetMethod("Create");
            MethodInfo setIcon = rowType.GetMethod("SetIcon");
            UnityAction<bool> none = shift => { };

            object row = create.Invoke(null, new object[] { list, "Buy", true, 1, 5, none });
            var rowRt = (RectTransform)rowType.GetProperty("Row").GetValue(row);
            var name = (Text)rowType.GetProperty("Name").GetValue(row);
            var total = (Text)rowType.GetProperty("Total").GetValue(row);
            float nameBefore = name.rectTransform.offsetMin.x, totalBefore = total.rectTransform.offsetMin.x;

            setIcon.Invoke(row, new object[] { "chair", "Chair" });
            Transform icon = rowRt.Find("Icon");
            Assert.That(icon, Is.Not.Null, "the chair has a picture");
            Assert.That(icon.GetComponent<Image>().sprite, Is.Not.Null);
            Component tip = icon.GetComponent(Find("HoverTip"));
            Assert.That(tip, Is.Not.Null, "hovering it shows the name");
            Assert.That(tip.GetType().GetProperty("Text").GetValue(tip), Is.EqualTo("Chair"));
            Assert.That(name.rectTransform.offsetMin.x, Is.GreaterThan(nameBefore + 70f), "the name moves right of the picture");
            Assert.That(total.rectTransform.offsetMin.x, Is.GreaterThan(totalBefore + 70f), "the total moves with it");

            object plain = create.Invoke(null, new object[] { list, "Store", false, 1, 5, none });
            var plainName = (Text)rowType.GetProperty("Name").GetValue(plain);
            float plainBefore = plainName.rectTransform.offsetMin.x;
            setIcon.Invoke(plain, new object[] { "no_such_item", "Nothing" });
            Assert.That(((RectTransform)rowType.GetProperty("Row").GetValue(plain)).Find("Icon"), Is.Null);
            Assert.That(plainName.rectTransform.offsetMin.x, Is.EqualTo(plainBefore), "no icon, no change");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(canvas);
        }
    }
}
