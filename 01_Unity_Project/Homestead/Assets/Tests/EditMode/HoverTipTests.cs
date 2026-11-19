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
}
