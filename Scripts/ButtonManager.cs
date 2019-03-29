using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonManager : MonoBehaviour {

  private static Dictionary<string, Button> buttons;
  private static Dictionary<string, Text> buttonTexts;

  private static Queue<GameObject> buttonObjectQueue;
  private static Queue<GameObject> buttonTextObjectQueue;

  private static Sprite playIcon;
  private static Sprite buttonBackground;
  private static Sprite roundedButtonBackground;
  private static Sprite flatButtonBackground;

  void Awake() {
    Initialize();
  }

  public static void Initialize() {

    buttons = new Dictionary<string, Button>();
    buttonTexts = new Dictionary<string, Text>();

    buttonObjectQueue = new Queue<GameObject>();
    buttonTextObjectQueue = new Queue<GameObject>();

    playIcon = Resources.Load<Sprite>("Sprites/play_icon");
    buttonBackground = Resources.Load<Sprite>("Sprites/circle");
    roundedButtonBackground = Resources.Load<Sprite>("Sprites/circle_70");
    flatButtonBackground = Resources.Load<Sprite>("Sprites/button_square");

  }

  public static Button CreateMenuIcon(string name, string setButtonText, string iconName, string side, int slot, bool setInteractable, UnityAction listenerFunction) {
    Vector2 position = Vector2.zero;
    string pivotLocation = "LL";

    if (side == "LEFT") {
      position.x = 25;
    } else {
      position.x = -25;
      pivotLocation = "LR";
    }
    position.y = 45 + 110f * slot;

    Button menuButton = CreateButton(name, setButtonText, position.x, position.y, 100, 100, "ICON", 20, pivotLocation, setInteractable, listenerFunction);
    ButtonManager.UpdateButtonImage(name, iconName);
    ButtonManager.GetButtonText(name).gameObject.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 50);
    ButtonManager.GetButtonText(name).gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(50, 0);
    return menuButton;
  }

  public static Button CreateButton(string name, string setButtonText, float posX, float posY, float width, float height, string style, int fontSize,
    string pivotLocation, bool setInteractable, UnityAction listenerFunction) {

    if (buttons.ContainsKey(name)) return buttons[name];

    GameObject buttonObject;
    GameObject buttonTextObject;
    if (buttonObjectQueue.Count > 0) {
      buttonObject = buttonObjectQueue.Dequeue();  // Dequeues must happen together
      buttonTextObject = buttonTextObjectQueue.Dequeue();  // Dequeues must happen together
      buttonObject.SetActive(true);
    } else {
      buttonObject = new GameObject(name);
      buttonObject.AddComponent<CanvasRenderer>();
      buttonObject.AddComponent<RectTransform>();
      buttonObject.AddComponent<Image>();
      buttonObject.AddComponent<Outline>().effectDistance = new Vector2(1,1);
      Button button = buttonObject.AddComponent<Button>();
      button.transform.SetParent(TextManager.canvas.transform, false);

      buttonTextObject = new GameObject("Text");
      buttonTextObject.transform.SetParent(buttonObject.transform, false);
      buttonTextObject.AddComponent<CanvasRenderer>();
      buttonTextObject.AddComponent<RectTransform>();
      Text buttonText = buttonTextObject.AddComponent<Text>();
      buttonText.font = TextManager.font;
      buttonText.color = Color.white;

    }
    buttonObject.GetComponent<Button>().enabled = true;
    buttons[name] = buttonObject.GetComponent<Button>();
    buttonTexts[name] = buttonTextObject.GetComponent<Text>();

    updateButton(buttonObject, buttonTextObject, name, setButtonText, posX, posY, width, height, style, fontSize, pivotLocation, setInteractable, listenerFunction);
    return buttonObject.GetComponent<Button>();
  }

  public static Button DestroyAndPool(Button button) {
    if (button != null) {
      button.onClick.RemoveAllListeners();
      GameObject buttonObject = button.gameObject;


      if (buttonTexts.ContainsKey(buttonObject.name)) {
        GameObject buttonTextObject = buttonTexts[buttonObject.name].gameObject;
        if (buttons.ContainsKey(buttonObject.name)) {
          buttons.Remove(buttonObject.name);
          buttonTexts.Remove(buttonObject.name);
        }
        buttonObject.name = "PooledButton";
        buttonObject.SetActive(false);
        buttonObjectQueue.Enqueue(buttonObject);  // Queues must happen together
        buttonTextObjectQueue.Enqueue(buttonTextObject);  // Queues must happen together
      } else {
        Destroy(buttonObject);  // Failsafe
      }
    }
    return null;  // Be sure to free references to pooled objects
  }

  private static Button updateButton(GameObject buttonObject, GameObject buttonTextObject, string name, string setButtonText, float posX, float posY, float width, float height, string style,
    int fontSize, string pivotLocation, bool setInteractable, UnityAction listenerFunction) {

    buttonObject.name = name;
    buttonTextObject.name = name + "Text";

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
    } else if (pivotLocation == "UC") {
      pivot = new Vector2(0.5f,1f);
    } else if (pivotLocation == "LC") {
      pivot = new Vector2(0.5f,0);
    } else if (pivotLocation == "CENTER") {
      pivot = new Vector2(0.5f,0.5f);
    }

    Image image = buttonObject.GetComponent<Image>();
    image.color = Color.white;
    image.type = Image.Type.Sliced;
    if (style == "PLAY_BUTTON") {
      image.sprite = playIcon;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    } else if (style == "BACK_BUTTON") {
      image.color = Color.white;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
      image.sprite = Resources.Load<Sprite>("Sprites/back_arrow");
      width = height = 100;
      setButtonText = "";
    } else if (style == "ICON") {
      image.sprite = null;
      image.color = Color.white;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    } else if (style == "SQUARE") {
      image.sprite = flatButtonBackground;
      image.color = Color.black;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    } else if (style == "ROUNDED") {
      image.sprite = roundedButtonBackground;
      image.color = Color.white;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    } else if (style == "BLANK") {
      image.sprite = null;
      image.color = Color.clear;
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    } else if (style == "CIRCLE_LARGE") {
      image.color = Color.black;
      image.sprite = buttonBackground;
      buttonObject.GetComponent<Outline>().effectColor = Color.white;
    } else {
      image.sprite = roundedButtonBackground;
      image.color = new Color(0.25f,0.25f,0.25f,0.5f);
      buttonObject.GetComponent<Outline>().effectColor = Color.clear;
    }

    buttonObject.GetComponent<RectTransform>().pivot = pivot;
    buttonObject.GetComponent<RectTransform>().anchorMin = pivot;
    buttonObject.GetComponent<RectTransform>().anchorMax = pivot;
    buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    buttonObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX + horizontalOffset, posY);

    if (style == "BACK_BUTTON") {
      buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    }

    Color setButtonNormalColor = new Color(1f,1f,1f,1f);
    Color setButtonActiveColor = setButtonNormalColor;  // Not applicable to mobile
    Color setButtonPressedColor = new Color(1f,1f,1f,0.75f);
    Color setButtonDisabledColor = new Color(0.4f, 0.4f, 0.4f);

    Button button = buttonObject.GetComponent<Button>();
    button.transition = Selectable.Transition.ColorTint;
    ColorBlock cb = button.colors;
    cb.normalColor = setButtonNormalColor;
    cb.highlightedColor = setButtonActiveColor;
    cb.pressedColor = setButtonPressedColor;
    cb.disabledColor = setButtonDisabledColor;
    cb.fadeDuration = 0;
    button.colors = cb;
    button.targetGraphic = image;
    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(listenerFunction);

    buttonTextObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    buttonTextObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    Text buttonText = buttonTextObject.GetComponent<Text>();
    buttonText.text = setButtonText;
    buttonText.raycastTarget = false;
    buttonText.alignment = TextAnchor.MiddleCenter;
    buttonText.fontSize = fontSize;

    SetInteractable(name, setInteractable);

    return button;
  }

  public static void UpdateButtonImage(string buttonName, string imagePath) {
    if (!ButtonExists(buttonName)) {
      Debug.Log("Error updating button image: button does not exist");
      return;
    }
    GetButton(buttonName).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + imagePath);
  }

  public static bool ButtonExists(string name) {
    return buttons.ContainsKey(name);
  }

  public static Button GetButton(string name) {
    return buttons[name];
  }

  public static Text GetButtonText(string name) {
    return buttonTexts[name];
  }

  public static void SetInteractable(string buttonName, bool setInteractable) {
    if (buttons.ContainsKey(buttonName) && buttonTexts.ContainsKey(buttonName)) {
      Button button = GetButton(buttonName);
      Text buttonText = GetButtonText(buttonName);
      if (button != null && buttonText != null) {
        button.interactable = setInteractable;
        buttonText.color = setInteractable ? button.colors.normalColor : button.colors.disabledColor;
      }
    }
  }
}