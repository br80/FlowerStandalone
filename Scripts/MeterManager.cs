using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeterManager : MonoBehaviour {

  private static Dictionary<string, Meter> meters;

  private static Queue<GameObject> meterObjectQueue;

  void Awake() {
    Initialize();
  }

  public static void Initialize() {
    meters = new Dictionary<string, Meter>();
    meterObjectQueue = new Queue<GameObject>();
  }

  public static Meter DestroyAndPool(Meter meter) {
    if (meter != null) {
      GameObject meterObject = meter.gameObject;

      Transform meterTextTransform = meterObject.transform.Find(meterObject.name + "Text");
      if (meterTextTransform != null) {
        meterTextTransform.SetParent(TextManager.canvas.transform);
        TextManager.DestroyAndPool(meterTextTransform.GetComponent<Text>());
      }

      if (meters.ContainsKey(meterObject.name)) {
        meters.Remove(meterObject.name);
      }
      meterObject.name = "PooledMeter";
      meterObject.SetActive(false);
      meterObjectQueue.Enqueue(meterObject);
    }
    return null;  // Be sure to free references to pooled objects
  }

  public static Meter CreateMeter(string name, float posX, float posY, float width, float height, string type, string pivotLocation, Color meterColor) {

    if (meters.ContainsKey(name)) return meters[name];

    GameObject meterObject;
    if (meterObjectQueue.Count > 0) {
      meterObject = meterObjectQueue.Dequeue();
      meterObject.SetActive(true);
    } else {
      meterObject = new GameObject(name);
      Meter meter = meterObject.AddComponent<Meter>();
      meter.transform.SetParent(TextManager.canvas.transform, false);
    }

    updateMeter(meterObject, name, posX, posY, width, height, type, pivotLocation, meterColor);
    return meterObject.GetComponent<Meter>();
  }

  private static Meter updateMeter(GameObject meterObject, string name, float posX, float posY, float width, float height, string type, string pivotLocation,
                                  Color meterColor) {

    meterObject.name = name;

    float horizontalOffset = 0;
    float gutterOffset = TextManager.gutterOffset;

    Vector2 pivot = new Vector2(0.5f, 0.5f);
    if (pivotLocation == "LL") {
      pivot = new Vector2(0,0);
      horizontalOffset = gutterOffset;
    } else if (pivotLocation == "UL") {
      pivot = new Vector2(0,1);
      horizontalOffset = gutterOffset;
    } else if (pivotLocation == "LR") {
      pivot = new Vector2(1,0);
      horizontalOffset = -gutterOffset;
    } else if (pivotLocation == "UR") {
      pivot = new Vector2(1,1);
      horizontalOffset = -gutterOffset;
    } else if (pivotLocation == "MR") {
      pivot = new Vector2(1,0.5f);
      horizontalOffset = -gutterOffset;
    } else if (pivotLocation == "ML") {
      pivot = new Vector2(0,0.5f);
      horizontalOffset = gutterOffset;
    } else if (pivotLocation == "LC") {
      pivot = new Vector2(0.5f,0);
    } else if (pivotLocation == "UC") {
      pivot = new Vector2(0.5f,1f);
    } else if (pivotLocation == "CENTER") {
      pivot = new Vector2(0.5f,0.5f);
    }

    meterObject.GetComponent<RectTransform>().pivot = pivot;
    meterObject.GetComponent<RectTransform>().anchorMin = pivot;
    meterObject.GetComponent<RectTransform>().anchorMax = pivot;
    meterObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    meterObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX + horizontalOffset, posY);

    Meter meter = meterObject.GetComponent<Meter>();
    meter.Initialize(type, meterColor);

    meters[name] = meter;

    return meter;
  }

  public static Text AddTextToMeter(string meterName, int fontSize, Color textColor) {
    if (!meters.ContainsKey(meterName)) {
      Debug.Log("Error adding text to meter: meter does not exist");
      return null;
    }

    GameObject meterObject = GetMeter(meterName).gameObject;
    RectTransform meterTransform = meterObject.GetComponent<RectTransform>();
    Text meterText = TextManager.CreateText(meterName + "Text", Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, "CENTER", fontSize, true);
    GameObject meterTextObject = meterText.gameObject;
    meterTextObject.transform.SetParent(meterObject.transform);
    meterTextObject.transform.SetSiblingIndex(1);
    meterTextObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    meterTextObject.GetComponent<RectTransform>().sizeDelta = meterTransform.sizeDelta;

    meterText.color = textColor;

    return meterText;
  }


  public static Meter GetMeter(string name) {
    return meters[name];
  }

}
