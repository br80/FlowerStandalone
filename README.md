# FlowerStandalone

1. Create a new Unity project with default settings
2. In a Terminal, navigate to your project directory and clone this repo:

`git clone https://github.com/br80/FlowerStandalone`

3. Move everything in the repo into your project's `Assets` directory including hidden `.git` and `.gitignore` files.

```
rm -rf Assets
mv FlowerStandalone Assets
```
3. Return to your Unity project, double-click the SampleScene in `Assets/Scenes`. You should see a `Flower` GameObject in the inspector. Click `AddComponent` and search for `Flower` script to add.

4. Find the plant prefabs in `Assets/Resources/Prefabs`. You need to drag their meshes into their mesh filters in the inspector. The meshes can be found in `Assets/Meshes`.

5. Hit Play
