using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TextManager : MonoBehaviour {

  public static float gutterOffset;  // For super-wide iPhoneX

  public static Canvas canvas;
  private static GameObject canvasObject;

  public static Font font;
  public static Color fontColor;
  public static Color menuFontColor;

  private static Dictionary<string, bool> textIsMenu;

  private static Dictionary<string, Text> texts;

  private static Dictionary<string, InputField> inputFields;

  private static Sprite inputFieldSprite;

  private static Queue<GameObject> textObjectQueue;
  private static Queue<GameObject> inputFieldObjectQueue;

  public static string HighlightColorString = "#00C3EC";
  public static string InvalidColorString = "#00C3EC";


  void Awake() {
    Initialize();
  }

  public static void Initialize() {
    texts = new Dictionary<string, Text>();

    inputFields = new Dictionary<string, InputField>();

    textIsMenu = new Dictionary<string, bool>();

    textObjectQueue = new Queue<GameObject>();
    inputFieldObjectQueue = new Queue<GameObject>();

    initializeCanvas();
    initializeFont();

    inputFieldSprite = Resources.Load<Sprite>("Sprites/text_field_underline");
  }


  public static Text CreateText(string name, Vector2 position, Vector2 fieldSize, TextAnchor anchor, string pivotLocation, int fontSize, bool isMenu) {

    if (texts.ContainsKey(name)) return texts[name];

    GameObject textObject;
    if (textObjectQueue.Count > 0) {
      textObject = textObjectQueue.Dequeue();
      textObject.SetActive(true);
    } else {
      textObject = new GameObject(name);
      textObject.AddComponent<CanvasRenderer>();
      Text text = textObject.AddComponent<Text>();
      text.transform.SetParent(canvasObject.transform, false);
      text.raycastTarget = false;
      // Outline outline = textObject.AddComponent<Outline>();
      // outline.effectColor = new Color(1,1,1,0.5f);
    }
    updateText(textObject, name, position, fieldSize, anchor, pivotLocation, fontSize, isMenu);
    return textObject.GetComponent<Text>();
  }

  public static Text DestroyAndPool(Text text) {
    if (text != null) {
      text.text = "";
      GameObject textObject = text.gameObject;
      if (texts.ContainsKey(textObject.name)) {
        texts.Remove(textObject.name);
        textIsMenu.Remove(textObject.name);
      }
      textObject.name = "PooledText";
      textObject.SetActive(false);
      textObjectQueue.Enqueue(textObject);
    }
    return null;  // Be sure to free references to pooled objects
  }

  private static Text updateText(GameObject textObject, string name, Vector2 position, Vector2 fieldSize, TextAnchor anchor, string pivotLocation, int fontSize, bool isMenu) {

    textObject.name = name;
    float horizontalOffset = 0;

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
    } else if (pivotLocation == "UC") {
      pivot = new Vector2(0.5f,1f);
    } else if (pivotLocation == "LC") {
      pivot = new Vector2(0.5f,0);
    } else if (pivotLocation == "CENTER") {
      pivot = new Vector2(0.5f,0.5f);
    }

    Text text = textObject.GetComponent<Text>();
    text.rectTransform.anchoredPosition = position + new Vector2(horizontalOffset, 0);
    text.rectTransform.sizeDelta = fieldSize;
    text.rectTransform.pivot = pivot;
    text.rectTransform.anchorMin = pivot;
    text.rectTransform.anchorMax = pivot;

    text.font = font;
    if (isMenu) {
      text.color = menuFontColor;
    } else {
      text.color = fontColor;
    }
    text.fontSize = Mathf.FloorToInt(fontSize * 1f);
    text.alignment = anchor;
    text.verticalOverflow = VerticalWrapMode.Overflow;
    text.lineSpacing = 1;

    texts[name] = textObject.GetComponent<Text>();
    textIsMenu[name] = isMenu;

    return text;
  }

  public static Text GetText(string textKey) {
    if (texts.ContainsKey(textKey)) {
      return texts[textKey];
    } else {
      return null;
    }
  }

  public static void ChangeFontColor(Color newFontColor) {
    fontColor = newFontColor;
    foreach(var item in texts)
    {
      if (!textIsMenu[item.Key]) { // Don't change color of menu texts
        item.Value.color = fontColor;
      }
    }
  }

  public static void SetTextToMenuColor(string textName) {
    texts[textName].color = menuFontColor;
  }

  public static void SetTextToGameColor(string textName) {
    texts[textName].color = fontColor;
  }

  // HACKY
  public static string GetFontColorString() {
    if (fontColor == Color.black) return "#000";
    else return "#fff";
  }

  public static void ChangeHighlightColor(string colorString) {
    HighlightColorString = colorString;
  }

  public static void ChangeInvalidColor(string colorString) {
    InvalidColorString = colorString;
  }

  private static void initializeCanvas() {
    canvasObject = new GameObject("Canvas");
    canvas = canvasObject.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    canvasObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
    canvas.gameObject.AddComponent<GraphicRaycaster>();

    Vector2 referenceResolution = canvasObject.GetComponent<CanvasScaler>().referenceResolution;
    gutterOffset = Mathf.Max(0, referenceResolution.y * ( ((float)Screen.width) / ((float)Screen.height) - ((float)16) / 9) / 2);
  }

  private static void initializeFont() {
    // font = Resources.Load<Font>("DefaultAssets/Quicksand-Regular");
    // font = Resources.Load<Font>("DefaultAssets/Roboto-Regular");
    font = Resources.Load<Font>("DefaultAssets/SanFrancisco");
    fontColor = Color.white;
    menuFontColor = Color.white;
  }

  public static GameObject GetCanvasObject() {
    return canvasObject;
  }

  public static InputField DestroyAndPool(InputField inputField) {
    if (inputField != null) {
      inputField.text = "";  // Clear passwords
      GameObject inputFieldObject = inputField.gameObject;
      if (inputFields.ContainsKey(inputFieldObject.name)) {
        inputFields.Remove(inputFieldObject.name);
      }
      inputFieldObject.name = "PooledInputField";
      inputFieldObject.SetActive(false);
      inputFieldObjectQueue.Enqueue(inputFieldObject);
    }
    return null;  // Be sure to free references to pooled objects
  }

  public static InputField CreateInputField(string name, string setPlaceholderText, InputField.ContentType setContentType, float posX, float posY, float width, float height, string pivotLocation, bool setInteractable) {
    return CreateInputField(name, setPlaceholderText, setContentType, "DEFAULT", posX, posY, width, height, pivotLocation, setInteractable);
  }
  public static InputField CreateInputField(string name, string setPlaceholderText, InputField.ContentType setContentType, string style, float posX, float posY, float width, float height, string pivotLocation, bool setInteractable) {

    if (inputFields.ContainsKey(name)) return inputFields[name];

    GameObject inputFieldObject;
    if (inputFieldObjectQueue.Count > 0) {
      inputFieldObject = inputFieldObjectQueue.Dequeue();
      inputFieldObject.SetActive(true);
    } else {
      inputFieldObject = new GameObject(name);
      inputFieldObject.AddComponent<RectTransform>();
      inputFieldObject.AddComponent<Image>();
      InputField inputField = inputFieldObject.AddComponent<InputField>();
      inputField.transform.SetParent(canvasObject.transform, false);

      // create placeholder
      GameObject placeholderObject = new GameObject("InputFieldPlaceholder");
      placeholderObject.transform.SetParent(inputFieldObject.transform, false);
      placeholderObject.AddComponent<CanvasRenderer>();
      placeholderObject.AddComponent<RectTransform>();

      Text placeholderText = placeholderObject.AddComponent<Text>();
      placeholderText.font = font;
      placeholderText.fontSize = 30;
      placeholderText.alignment = TextAnchor.MiddleLeft;
      placeholderText.color = Color.grey;
      inputField.placeholder = placeholderText;

      // create text
      GameObject textObject = new GameObject("InputFieldText");
      Text text = textObject.AddComponent<Text>();
      textObject.transform.SetParent(inputFieldObject.transform, false);
      text.font = font;
      text.fontStyle = FontStyle.Normal;
      text.alignment = TextAnchor.MiddleLeft;
      text.color = Color.white;
      inputField.textComponent = text;
    }
    updateInputField(inputFieldObject, name, setPlaceholderText, setContentType, style, posX, posY, width, height, pivotLocation, setInteractable);
    return inputFieldObject.GetComponent<InputField>();

  }


  private static InputField updateInputField(GameObject inputFieldObject, string name, string setPlaceholderText, InputField.ContentType setContentType, string style, float posX, float posY, float width, float height, string pivotLocation, bool setInteractable) {

    inputFieldObject.name = name;

    float horizontalOffset = 0;

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
    } else if (pivotLocation == "UC") {
      pivot = new Vector2(0.5f,1f);
    } else if (pivotLocation == "LC") {
      pivot = new Vector2(0.5f,0);
    } else if (pivotLocation == "CENTER") {
      pivot = new Vector2(0.5f,0.5f);
    }

    RectTransform rectTransform = inputFieldObject.GetComponent<RectTransform>();
    rectTransform.pivot = pivot;
    rectTransform.anchorMin = pivot;
    rectTransform.anchorMax = pivot;
    rectTransform.anchoredPosition = new Vector2(posX + horizontalOffset, posY);
    rectTransform.sizeDelta = new Vector2(width, height);
    Image image = inputFieldObject.GetComponent<Image>();
    Sprite sprite = inputFieldSprite;
    image.sprite = sprite;
    image.type = Image.Type.Sliced;
    image.color = Color.white;
    InputField inputField = inputFieldObject.GetComponent<InputField>();
    inputField.interactable = true;
    inputField.transition = Selectable.Transition.ColorTint;
    inputField.targetGraphic = image;
    inputField.interactable = setInteractable;
    inputField.contentType = setContentType;

    GameObject placeholderObject = inputField.placeholder.gameObject;
    placeholderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 20, height);
    placeholderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
    Text placeholderText = placeholderObject.GetComponent<Text>();
    placeholderText.text = setPlaceholderText;

    // create text
    Text text = inputField.textComponent;
    text.color = Color.white;
    text.fontSize = 30;
    text.alignment = TextAnchor.MiddleLeft;
    GameObject textObject = text.gameObject;
    textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 20, height);
    textObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);
    text.text = "";

    if (style == "BLANK") {
      image.sprite = null;
      // inputField.targetGraphic = image;
      image.color = Color.clear;
      text.color = Color.white;
      text.fontSize = 40;
      text.alignment = TextAnchor.MiddleCenter;
    }

    inputFields[name] = inputField;

    return inputField;
  }

  public static InputField GetInputField(string name) {
    return inputFields[name];
  }


}
