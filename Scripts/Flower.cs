using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class Flower : MonoBehaviour {

  JSONNode flowerData;
  JSONNode flowerDNA;
  GameObject[] nodeRoots;

  private float windWaveCoefficient = 1;
  private float timeCoefficient = 1;

  private float flowerHeight;
  private float flowerWidth;

  private bool isDehydrated = false;


  void Awake() {
    Initialize();
  }

  public void Initialize() {
    string flowerDataString = "{\"size\":\"10\",\"health\":\"1\",\"veg_age\":\"3\", \"maturity\":\"1\", \"sunshine\":\"0\", \"bushiness\":\"11.97\", \"flower_age\":\"21.7458293272222\", \"fruit_size\":\"0\", \"flower_size\":\"2\", \"fruit_sweetness\":\"573.6024\", \"flower_brightness\":\"286.8012\", \"flower_petal_count\":\"8\"}";
    string flowerDNAString = "{\"c_0\":\"#BFE\", \"c_1\":\"#49F\", \"c_2\":\"#0F8\"}";
    flowerData = JSON.Parse(flowerDataString);
    flowerDNA = JSON.Parse(flowerDNAString);
    UpdateWindVariables();
    ShowFlower();


    FlowerGrowMenu.Show();

  }

  public string GetFlowerState() {
    return "FLOWER";
  }

  public string GetFlowerName() {
    return "Flowey";
  }

  public void ShowFlower() {

    foreach (Transform child in transform) {
      GameObject.Destroy(child.gameObject);
    }

    drawCherry(GetFlowerState());
  }


  private void drawCherry(string flowerState) {
    Vector3 rootPosition = transform.position;
    float vegAge = flowerData["veg_age"].AsFloat;
    float flowerAge = flowerData["flower_age"].AsFloat;
    float sizeValue = flowerData["size"].AsFloat / vegAge;
    float bushiness = flowerData["bushiness"].AsFloat / vegAge;
    bool isFlower = flowerState == "BUD" || flowerState == "FLOWER" || flowerState == "FRUIT" || flowerState == "MATURE";

    float health = 1f;

    Debug.Log(flowerData);
    // Debug.Log(vegAge);

    Color unhealthyColor = new Color(0.6f, 0.4f, 0);
    Color stemColorBase;
    ColorUtility.TryParseHtmlString ("#42EE00", out stemColorBase);
    Color stemColorTip;
    ColorUtility.TryParseHtmlString ("#BBFF42", out stemColorTip);
    Color leafColor;
    ColorUtility.TryParseHtmlString ("#42FF42", out leafColor);

    stemColorBase = Color.Lerp(unhealthyColor, stemColorBase, health);
    stemColorTip = Color.Lerp(unhealthyColor, stemColorTip, health);
    leafColor = Color.Lerp(unhealthyColor, leafColor, health);

    bool dehydrated = isDehydrated;
    if (dehydrated) {
      leafColor = new Color(0.6f, 0.4f, 0);
    }

    float totalStemHeight = vegAge * (1 + sizeValue / 2) / 3f;
    float nodeHeight = 1f - bushiness * 0.025f;
    int numVegNodes = 1 + Mathf.FloorToInt(totalStemHeight / nodeHeight);
    int totalNodes = numVegNodes;
    if (isFlower) totalNodes++;  // Draw an extra node for the flower

    flowerHeight = totalStemHeight;

    float localStemHeight = 0;
    float previousStemLength = 0;
    GameObject nodeRoot = gameObject;
    GameObject previousNodeRoot;
    nodeRoots = new GameObject[totalNodes];

    // Debug.Log("AGE: " + vegAge);
    // Debug.Log("STEM HEIGHT: " + totalStemHeight);
    // Debug.Log("BUSHINESS: " + bushiness);
    // Debug.Log("NODE HEIGHT: " + nodeHeight);
    // Debug.Log("NUM NODES: " + numVegNodes);
    // Debug.Log("SIZE: " + sizeValue);
    // Debug.Log(totalStemHeight + ", " + nodeHeight + ", " + numVegNodes);


    for (int i = 0 ; i < totalNodes ; i++) {
      bool isFlowerNode = isFlower && i == totalNodes - 1;
      previousNodeRoot = nodeRoot;
      nodeRoot = new GameObject("nodeRoot_" + i);
      nodeRoot.transform.parent = previousNodeRoot.transform;
      nodeRoot.transform.localPosition = new Vector3(0, previousStemLength, 0f);
      nodeRoots[i] = nodeRoot;

      // Each node is nodeHeight long, except last node contains the remaining height
      float curNodeLength = i == numVegNodes - 1 ? totalStemHeight - (numVegNodes - 1) * nodeHeight : Mathf.Min(nodeHeight, totalStemHeight);
      float curNodeWidth = i == 0 ? curNodeLength / 5 : nodeHeight / 5;  // TODO: Scale width based on local height and bushiness
      previousStemLength = curNodeLength;

      if (isFlowerNode) {
        curNodeLength = (flowerState == "BUD") ? flowerAge : 0.5f;
        flowerHeight += curNodeLength;
      }

      // Debug.Log(sizeValue + ", " + bushiness + ", " + vegAge);
      // Debug.Log("TOTAL: " + totalStemHeight + ", " + i + ", " + curNodeLength + " , " + localStemHeight);

      float basePercentage = localStemHeight / totalStemHeight;
      float tipPercentage = (localStemHeight + curNodeLength) / totalStemHeight;
      Color localStemColorBase = Color.Lerp(stemColorBase, stemColorTip, basePercentage);
      Color localStemColorTip = Color.Lerp(stemColorBase, stemColorTip, tipPercentage);

      GameObject stemObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/StemPrefab"), rootPosition, Quaternion.identity);
      stemObject.name = "Stem_" + i;
      stemObject.transform.localScale = new Vector3(curNodeWidth, curNodeLength, 1);
      stemObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", localStemColorBase);
      stemObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", localStemColorTip);
      stemObject.transform.parent = nodeRoot.transform;

      localStemHeight += curNodeLength;
      rootPosition += new Vector3(0, curNodeLength, 0);

      if (!isFlowerNode) {
        // Draw Leaves
        float leafCoefficient = Mathf.Pow( Mathf.Max(0.01f, Mathf.Sin( (1f - tipPercentage) * Mathf.PI / 2 )), 0.5f);
        for (int leaves_i = 0 ; leaves_i < 2 ; leaves_i++) {
          float angle = 90 * leafCoefficient;
          float length = bushiness * leafCoefficient * curNodeLength / (4 * nodeHeight);
          float width = length / 3;

          // Debug.Log("LEAF[" + leaves_i + "]: " + length + ", " + width + ", " + angle + ", " + leafCoefficient + ", " + bushiness + ", " + curNodeLength + ", " + nodeHeight);

          if (dehydrated) {
            width /= 2;
            angle = 150;
          }

          angle *= (1 - 2 * leaves_i);  // second leaf has negative angle

          flowerWidth = Mathf.Max(length * 2, flowerWidth);

          GameObject leafObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/LeafPrefab"), rootPosition + new Vector3(0, 0, -0.01f), Quaternion.identity);
          leafObject.name = "Leaf_" + leaves_i + "_" + i;
          leafObject.transform.localScale = new Vector3(width, length, 1);
          leafObject.transform.localEulerAngles = new Vector3(0, 0, angle);
          leafObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", localStemColorTip);
          leafObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", leafColor);
          leafObject.transform.parent = nodeRoot.transform;
        }
      } else {
        // Draw Flower

        if (flowerState == "BUD") {
          // Draw Bud
          float length = flowerData["flower_size"].AsFloat * flowerAge / 3;
          float width = length / 2;

          flowerHeight += length;

          GameObject budObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/PetalPrefab"), rootPosition + new Vector3(0, 0, -0.01f), Quaternion.identity);
          budObject.name = "Bud";
          budObject.transform.localScale = new Vector3(width, length, 1);
          budObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", localStemColorTip);
          budObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", leafColor);
          budObject.transform.parent = nodeRoot.transform;
        } else {
          // Draw Flower
          Color stamenColor = Color.white;
          Color petalColor = Color.white;
          Color petalColorTip = Color.white;

          int stamenCount = 0;
          float stamenLength = 0;
          float stamenWidth = 0;

          int brightness = Mathf.FloorToInt(flowerData["flower_brightness"].AsFloat);
          int petalCount = flowerData["flower_petal_count"].AsInt;
          float petalLength = flowerData["flower_size"].AsFloat / 3;
          float petalWidth = petalLength / 2;

          flowerHeight += petalLength;
          flowerWidth = Mathf.Max(petalLength * 2, flowerWidth);

          if (brightness < 80000) {
            // Tip is duller version of main color
            float dullnessValue = (brightness / 80000f);
            ColorUtility.TryParseHtmlString (flowerDNA["c_0"].ToStringNoQuotes(), out petalColor);
            petalColorTip = Color.Lerp(unhealthyColor, petalColor, dullnessValue);

          } else if (brightness < 150000) {
            // Tip is same color as main color
            ColorUtility.TryParseHtmlString (flowerDNA["c_0"].ToStringNoQuotes(), out petalColor);
            petalColorTip = petalColor;

            // Start to grow stamens (color is tertiary color)
            stamenLength = 0.8f * petalLength;
            stamenWidth = stamenLength / 10;
            ColorUtility.TryParseHtmlString (flowerDNA["c_2"].ToStringNoQuotes(), out stamenColor);

          } else {
            // Tip starts to change to secondary color
            ColorUtility.TryParseHtmlString (flowerDNA["c_0"].ToStringNoQuotes(), out petalColor);
            ColorUtility.TryParseHtmlString (flowerDNA["c_1"].ToStringNoQuotes(), out petalColorTip);

            stamenLength = 0.8f * petalLength;
            stamenWidth = stamenLength / 10;
            ColorUtility.TryParseHtmlString (flowerDNA["c_2"].ToStringNoQuotes(), out stamenColor);
          }

          petalColorTip.a = 0.65f;

          // Adjust petals for fruit
          if (flowerState == "FRUIT" || flowerState == "MATURE") {
            float fruitDecayRatio = 1 - Mathf.Min(1, flowerData["maturity"].AsFloat / 24);
            petalWidth *= fruitDecayRatio;
            stamenWidth *= fruitDecayRatio;
            petalColor.a *= fruitDecayRatio;
            petalColorTip.a *= fruitDecayRatio;
          }

          float petalAngle = 360f / petalCount;
          float currentPetalAngle = 0;

          for (int petal_i = 0 ; petal_i < petalCount ; petal_i++) {
            GameObject petalObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/PetalPrefab"), rootPosition + new Vector3(0, 0, -0.15f), Quaternion.identity);
            petalObject.name = "Petal_" + petal_i;
            petalObject.transform.localScale = new Vector3(petalWidth, petalLength, 1);
            petalObject.transform.localEulerAngles = new Vector3(0, 0, currentPetalAngle);
            petalObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", petalColor);
            petalObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", petalColorTip);
            petalObject.transform.parent = nodeRoot.transform;
            currentPetalAngle += petalAngle;
          }

          // Stamen count
          if (petalCount <= 5) stamenCount = 3;
          else if (petalCount <= 8) stamenCount = 5;
          else if (petalCount <= 13) stamenCount = 8;
          else stamenCount = 13;

          float stamenAngle = 360f / stamenCount;
          for (int stamen_i = 0 ; stamen_i < stamenCount ; stamen_i++) {
            GameObject stamenObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/LeafPrefab"), rootPosition+ new Vector3(0, 0, -0.16f), Quaternion.identity);
            stamenObject.name = "Stamen_" + stamen_i;
            stamenObject.transform.localScale = new Vector3(stamenWidth, stamenLength, 1);
            stamenObject.transform.localEulerAngles = new Vector3(0, 0, currentPetalAngle);
            // stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", petalColor);
            // stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", stamenColor);
            stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", stamenColor);
            stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", petalColor);
            stamenObject.transform.parent = nodeRoot.transform;
            currentPetalAngle += stamenAngle;
          }


          // "fruit_size"=>42.720497540555556,   "fruit_sweetness"=>3482147.7545306836,   "maturity"=>21.360248770277778,

          // START HERE
          if (flowerState == "FRUIT" || flowerState == "MATURE") {
            Debug.Log("IS FRUIT");
            float fruitSize = Mathf.Log10(flowerData["fruit_size"].AsFloat) / 3;
            float fruitSweetness = Mathf.Log10(flowerData["fruit_sweetness"].AsFloat / vegAge);
            float maturity = flowerData["maturity"].AsFloat;

            float ageRatio = Mathf.Min(1f, maturity);

            float fruitStemLength = ageRatio * totalStemHeight / 3;

            int numFruit = 1;
            float angleBetweenFruit = 0;
            float currentFruitAngle = 180;

            Debug.Log(fruitSize);
            Debug.Log(totalStemHeight);

            float sweetnessLerpValue = (fruitSweetness / 100);
            Color fruitColorTip = Color.Lerp(stemColorTip, Color.Lerp(Color.red, Color.black, sweetnessLerpValue), sweetnessLerpValue);
            Color fruitColorBase = Color.Lerp(stemColorTip, Color.red, sweetnessLerpValue);

            GameObject fruitObject;
            GameObject fruitStemObject;
            for (int fruit_i = 0 ; fruit_i < numFruit ; fruit_i++) {
              fruitObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/CherryPrefab"), rootPosition + new Vector3(0, 0, -0.17f), Quaternion.identity);
              fruitObject.name = "Fruit_" + fruit_i;
              float currentFruitAngleRadians = (currentFruitAngle + 90) * Mathf.Deg2Rad;
              fruitStemObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/StemPrefab"), rootPosition + new Vector3(0,0,-0.18f), Quaternion.identity);
              fruitStemObject.name = "FruitStem_" + fruit_i;
              fruitStemObject.transform.localScale = new Vector3(Mathf.Max(fruitStemLength / 16, 0.1f), fruitStemLength, 1);
              fruitStemObject.transform.localEulerAngles = new Vector3(0, 0, currentFruitAngle);
              fruitStemObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", stemColorBase);
              fruitStemObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", stemColorTip);
              fruitStemObject.transform.parent = nodeRoot.transform;
              fruitObject.transform.localScale = Vector3.one * fruitSize;
              fruitObject.transform.localPosition = rootPosition + (new Vector3(Mathf.Cos(currentFruitAngleRadians), Mathf.Sin(currentFruitAngleRadians), -0.19f/fruitStemLength)) * fruitStemLength;
              fruitObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", fruitColorBase);
              fruitObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", fruitColorTip);
              fruitObject.transform.localEulerAngles = new Vector3(0, 0, currentFruitAngle + 180);
              fruitObject.transform.parent = nodeRoot.transform;
              currentFruitAngle += angleBetweenFruit;
            }
          }




          // if (dehydrated) {
          //   petalColorTip = Color.Lerp(petalColorTip, new Color(0.6f, 0.4f, 0), 0.5f);
          // }
          // petalColor = Color.Lerp(unhealthyColor, petalColor, health);
          // petalColorTip = Color.Lerp(unhealthyColor, petalColorTip, health);
          // stamenColor = Color.Lerp(unhealthyColor, stamenColor, health);
          // petalColorTip.a = 0.5f;
          // float petalCount = node["flower"]["petal_count"].AsInt;
          // float petalLength = node["flower"]["petal_length"].AsFloat;
          // float petalWidth = node["flower"]["petal_width"].AsFloat;

          // float petalAngle = 360f / petalCount;
          // float currentPetalAngle = 0;

          // for (int i = 0 ; i < petalCount ; i++) {
          //   GameObject petalObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/PetalPrefab"), rootPosition + new Vector3(0, 0, -0.15f), Quaternion.identity);
          //   petalObject.name = "Petal_" + i;
          //   petalObject.transform.localScale = new Vector3(petalWidth, petalLength, 1);
          //   petalObject.transform.localEulerAngles = new Vector3(0, 0, currentPetalAngle);
          //   petalObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", petalColor);
          //   petalObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", petalColorTip);
          //   petalObject.transform.parent = nodeRoot.transform;
          //   currentPetalAngle += petalAngle;
          // }

          // float stamenAngle = 360f / stamenCount;
          // float currentStamenAngle = 0;
          // for (int i = 0 ; i < stamenCount ; i++) {
          //   GameObject stamenObject = (GameObject) Instantiate(Resources.Load<GameObject>("Prefabs/LeafPrefab"), rootPosition+ new Vector3(0, 0, -0.16f), Quaternion.identity);
          //   stamenObject.name = "Stamen_" + i;
          //   stamenObject.transform.localScale = new Vector3(stamenWidth, stamenLength, 1);
          //   stamenObject.transform.localEulerAngles = new Vector3(0, 0, currentStamenAngle);
          //   stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorBase", petalColor);
          //   stamenObject.GetComponent<MeshRenderer>().material.SetColor("_ColorTip", stamenColor);
          //   stamenObject.transform.parent = nodeRoot.transform;
          //   currentStamenAngle += stamenAngle;
          // }

        }

      }
    }
  }

  public void UpdateWindVariables() {
    windWaveCoefficient = 0.1f;
    // windWaveCoefficient = (float) PlayerPrefs.GetInt("air_level") / 10;
    timeCoefficient = (10f + PlayerPrefs.GetInt("time_speed")) / 5;
  }

  private void doFlowerWave() {
    if (flowerData == null || nodeRoots == null || nodeRoots[0] == null) return;
    for (int nodes_i = 0 ; nodes_i < nodeRoots.Length ; nodes_i++ ) {
      JSONNode node = flowerData["nodes"][nodes_i];
      // TODO: Scale angle based on percentage instead of node number?
      float stemAngle = windWaveCoefficient * Mathf.Sin(timeCoefficient * Time.time + nodes_i / 4f) - windWaveCoefficient + Mathf.PI / 2;  // Angle test
      nodeRoots[nodes_i].transform.eulerAngles = new Vector3(0,0,stemAngle * Mathf.Rad2Deg - 90);
    }
  }

  void Update() {
    if (nodeRoots != null) {
      doFlowerWave();
    }
  }


}
