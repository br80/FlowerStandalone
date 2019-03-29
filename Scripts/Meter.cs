using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Meter : MonoBehaviour {

  private GameObject meterObject;
  private GameObject meterMaskObject;

  private float percentage;

  private string type;

  private float moveSpeed;
  private float targetPercentage;

  void Awake() {
    gameObject.AddComponent<Image>().type = Image.Type.Sliced;
    gameObject.AddComponent<Outline>().effectDistance = new Vector2(1,1);
    gameObject.GetComponent<Outline>().effectColor = Color.white;

    meterMaskObject = new GameObject("MeterMask");
    meterMaskObject.AddComponent<Mask>().showMaskGraphic = false;
    meterMaskObject.AddComponent<Image>().type = Image.Type.Sliced;

    meterObject = new GameObject("Meter");
    meterObject.AddComponent<Image>().type = Image.Type.Sliced;

    meterMaskObject.transform.SetParent(transform);
    meterObject.transform.SetParent(meterMaskObject.transform);

  }

  public void Initialize(string setType, Color meterColor) {
    type = setType;

    if (type == "CIRCLE") {
      gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/circle");
      meterMaskObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/button_square");
      meterObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/circle");
      meterObject.GetComponent<Image>().color = meterColor;

      gameObject.GetComponent<Image>().color = new Color(0,0,0);
      meterObject.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
      meterMaskObject.GetComponent<RectTransform>().sizeDelta = meterObject.GetComponent<RectTransform>().sizeDelta;
    } else {
      // Default
      gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/button_square");
      meterMaskObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/button_square");
      meterObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/button_square");
      meterObject.GetComponent<Image>().color = meterColor;

      gameObject.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
      meterObject.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta;
      meterMaskObject.GetComponent<RectTransform>().sizeDelta = meterObject.GetComponent<RectTransform>().sizeDelta;
    }
  }

  public void SlidePercentage(float setPercentage, float setMoveSpeed) {
    Debug.Log("SLIDE PERCENTAGE: " + setPercentage + ", " + setMoveSpeed + ", " + targetPercentage);
    if (setMoveSpeed <= 0) {
      SetPercentage(Mathf.Min(1, setPercentage));
    } else {
      targetPercentage = setPercentage;
      moveSpeed = setMoveSpeed;
      StartCoroutine(tickMeterMove());
    }
  }

  private IEnumerator tickMeterMove() {
    yield return 0;
    percentage = Mathf.MoveTowards(percentage, targetPercentage, moveSpeed * Time.deltaTime);
    updatePercentage();
    if (percentage != targetPercentage) {
      StartCoroutine(tickMeterMove());
    }
  }


  public void SetPercentage(float setPercentage) {
    percentage = Mathf.Max(0f, Mathf.Min(1f, setPercentage));
    updatePercentage();
  }

  private void updatePercentage() {
    Debug.Log("PERCENTAGE: " + percentage);
    Vector2 maxSize = gameObject.GetComponent<RectTransform>().sizeDelta;
    if (type == "CIRCLE") {
      meterMaskObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -maxSize.y * (1f - percentage));
      meterObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, maxSize.y * (1f - percentage));
    } else {
      meterMaskObject.GetComponent<RectTransform>().sizeDelta = new Vector2(maxSize.x * percentage, maxSize.y);
      meterMaskObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
      meterObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
      meterMaskObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((-maxSize.x + maxSize.x * percentage) / 2, 0);
    }
  }




}
