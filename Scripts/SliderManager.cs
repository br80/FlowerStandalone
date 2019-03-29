using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SliderManager : MonoBehaviour {

  private static Dictionary<string, Slider> sliders;
  private static Queue<GameObject> sliderObjectQueue;

  void Awake() {
    Initialize();
  }

  public static void Initialize() {
    sliders = new Dictionary<string, Slider>();
    sliderObjectQueue = new Queue<GameObject>();
  }

  public static Slider CreateHorizontalSlider(string name, float posX, float posY, float width, float height, string pivotLocation, UnityAction listenerFunction, int minValue, int maxValue, int defaultValue) {
    return CreateSlider(name, posX, posY, width, height, pivotLocation, listenerFunction, minValue, maxValue, defaultValue, false);
  }
  public static Slider CreateVerticalSlider(string name, float posX, float posY, float width, float height, string pivotLocation, UnityAction listenerFunction, int minValue, int maxValue, int defaultValue) {
    return CreateSlider(name, posX, posY, width, height, pivotLocation, listenerFunction, minValue, maxValue, defaultValue, true);
  }

  public static Slider CreateSlider(string name, float posX, float posY, float width, float height, string pivotLocation, UnityAction listenerFunction, int minValue, int maxValue, int defaultValue, bool isVertical) {
    if (sliders.ContainsKey(name)) return sliders[name];

    GameObject sliderObject;
    if (sliderObjectQueue.Count > 0) {
      sliderObject = sliderObjectQueue.Dequeue();
      sliderObject.SetActive(true);
    } else {
      sliderObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/SliderPrefab"), Vector3.zero, Quaternion.identity);
      sliderObject.name = name;
      Slider slider = sliderObject.GetComponent<Slider>();
      slider.transform.SetParent(TextManager.canvas.transform, false);
    }

    updateSlider(sliderObject, name, posX, posY, width, height, pivotLocation, listenerFunction, minValue, maxValue, defaultValue, isVertical);
    return sliderObject.GetComponent<Slider>();
  }

  private static Slider updateSlider(GameObject sliderObject, string name, float posX, float posY, float width, float height, string pivotLocation, UnityAction listenerFunction, int minValue, int maxValue, int defaultValue, bool isVertical) {

    Slider slider = sliderObject.GetComponent<Slider>();

    RectTransform handleTransform = sliderObject.transform.Find("Handle Slide Area").Find("Handle").GetComponent<RectTransform>();
    handleTransform.localScale = new Vector3(4.5f, 4.5f, 1f);
    handleTransform.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/circle_padded");

    if (isVertical) {
      slider.SetDirection(Slider.Direction.BottomToTop, true);
      handleTransform.sizeDelta = new Vector2(10,25);
    } else {
      slider.SetDirection(Slider.Direction.LeftToRight, true);
      handleTransform.sizeDelta = new Vector2(height, 0);
    }

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
    } else if (pivotLocation == "CENTER") {
      pivot = new Vector2(0.5f,0.5f);
    }

    sliderObject.name = name;
    sliderObject.transform.SetParent(TextManager.canvas.transform, false);
    sliderObject.GetComponent<RectTransform>().pivot = pivot;
    sliderObject.GetComponent<RectTransform>().anchorMin = pivot;
    sliderObject.GetComponent<RectTransform>().anchorMax = pivot;
    sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    sliderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX + horizontalOffset, posY);

    slider.minValue = minValue;
    slider.maxValue = maxValue;
    slider.wholeNumbers = true;
    slider.value = defaultValue;
    slider.onValueChanged.RemoveAllListeners();
    slider.onValueChanged.AddListener(delegate {listenerFunction();});

    sliders[name] = slider;

    return slider;
  }

  public static Slider DestroyAndPool(Slider slider) {
    if (slider != null) {
      slider.onValueChanged.RemoveAllListeners();
      GameObject sliderObject = slider.gameObject;
      if (sliders.ContainsKey(sliderObject.name)) {
        sliders.Remove(sliderObject.name);
      }
      sliderObject.name = "PooledSlider";
      sliderObject.SetActive(false);
      sliderObjectQueue.Enqueue(sliderObject);
    }
    return null;  // Be sure to free references to pooled objects
  }

}
