using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class FlowerGrowMenu : MonoBehaviour {

  public static FlowerGrowMenu instance;

  private static Button closeButton;

  private static JSONNode flowerData;
  private static JSONNode flowerDNA;

  private static Slider bushinessSlider;
  private static Slider sizeSlider;
  private static Slider ageSlider;

  private static bool isFlower = false;
  private static Button flowerButton;
  private static Button shuffleColorButton;

  private static Flower flower;

  void Awake() {
    instance = this;

    flower = instance.GetComponent<Flower>();
  }

  public static void Show() {
    flowerData = JSON.Parse("{}");
    flowerDNA = JSON.Parse("{}");

    bushinessSlider = SliderManager.CreateHorizontalSlider("bushinessSlider", -25, 100, 250, 25, "MR", changeSliderValue, 0, 20, PlayerPrefs.GetInt("fire_level"));
    sizeSlider = SliderManager.CreateHorizontalSlider("flowerSizeSlider", -25, 0, 250, 25, "MR", changeSliderValue, 0, 10, PlayerPrefs.GetInt("earth_level"));
    ageSlider = SliderManager.CreateHorizontalSlider("flowerAgeSlider", -25, -100, 250, 25, "MR", changeSliderValue, 1, 72, 72);

    flowerButton = ButtonManager.CreateButton("flowerButton", "", -25, -200, 70, 70, "ROUNDED", 24, "MR", true, toggleFlower);
    shuffleColorButton = ButtonManager.CreateButton("shuffleColorButton", "SHFFL", -100, -200, 70, 70, "ROUNDED", 24, "MR", true, shuffleColorsAndReload);

    isFlower = flower.GetFlowerState() != "FLOWER";
    shuffleColors();
    toggleFlower();  // Make opposite, then toggle

  }

  public static void Hide() {
    closeButton = ButtonManager.DestroyAndPool(closeButton);


    ageSlider = SliderManager.DestroyAndPool(ageSlider);
    sizeSlider = SliderManager.DestroyAndPool(sizeSlider);
    bushinessSlider = SliderManager.DestroyAndPool(bushinessSlider);

    shuffleColorButton = ButtonManager.DestroyAndPool(shuffleColorButton);
    flowerButton = ButtonManager.DestroyAndPool(flowerButton);
  }

  private static void toggleFlower() {
    isFlower = !isFlower;

    Text flowerButtonText = ButtonManager.GetButtonText(flowerButton.name);
    flowerButtonText.text = isFlower ? "X" : "";

    changeSliderValue();
  }

  private static void shuffleColorsAndReload() {
    shuffleColors();
    changeSliderValue();
  }
  private static void shuffleColors() {
    flowerDNA["c_0"] = randomScaledColor();
    flowerDNA["c_1"] = randomScaledColor();
    flowerDNA["c_2"] = randomScaledColor();
  }

  private static string randomScaledColor() {
    List<string> colorList = new List<string>(){"0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F"};

    bool hasF = false;
    StringBuilder sb = new StringBuilder();

    while (!hasF) {
      sb = new StringBuilder();
      sb.Append("#");
      string newChar;

      for (int i = 0 ; i < 3 ; i++) {
        newChar = colorList[Random.Range(0, colorList.Count)];
        if (newChar == "F") {
          hasF = true;
        }
        sb.Append(newChar);
      }
      sb.Append(colorList[Random.Range(0, colorList.Count)]);
    }

    return sb.ToString();
  }


  private static void changeSliderValue() {
    Debug.Log("SLIDING");
    float vegAgeMax = 72f;

    float vegAge = isFlower ? vegAgeMax : ageSlider.value;
    float flowerAge = ageSlider.value;

    float vegSize = sizeSlider.value * vegAge;
    float vegBushiness = bushinessSlider.value * vegAge;

    string flowerState = isFlower ? "FLOWER" : "VEGETATIVE";

    if (isFlower) {

      float flowerBrightness = bushinessSlider.value * vegBushiness * vegSize * 24;
      float flowerSize = sizeSlider.value * 24;

      flowerData["veg_age"] = "72.0";
      flowerData["flower_age"] = ageSlider.value.ToString();

      float fireValue = flowerBrightness / (24f * vegBushiness * vegSize);
      while (fireValue < 4) {
        flowerBrightness = Mathf.Floor(flowerBrightness * 0.9f);
        fireValue += 1;
      }
      flowerData["flower_petal_count"] = Mathf.Min(Mathf.Floor(fireValue), 30).ToString();
      flowerData["waters"] = "0";
      flowerData["sunshine"] = "0";


      flowerData["flower_size"] = flowerSize.ToString();
      flowerData["flower_brightness"] = flowerBrightness.ToString();

    }

    flowerData["fruit_type"] = "CHERRY";
    flowerData["size"] = vegSize.ToString();
    flowerData["health"] = 1f.ToString();
    flowerData["veg_age"] = vegAge.ToString();
    flowerData["bushiness"] = vegBushiness.ToString();

    flower.ShowFlower();

     // flower_data:
     //  {"size"=>143.0,
     //   "health"=>1.0,
     //   "waters"=>0,
     //   "veg_age"=>72,
     //   "maturity"=>39.75371576722217,
     //   "sunshine"=>638413.29233825,
     //   "bushiness"=>285.0,
     //   "flower_age"=>4.038091429166328,
     //   "fruit_size"=>78.44511283555556,
     //   "flower_size"=>48.0,
     //   "fruit_sweetness"=>7967183.613770327,
     //   "flower_brightness"=>3912480.0000000014,
     //   "flower_petal_count"=>4},
     // flower_snapshots: [],
     // dna: {"c_0"=>"#F43", "c_1"=>"#6F3", "c_2"=>"#07F", "bushiness"=>1}

  }



}
