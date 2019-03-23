[![License](https://img.shields.io/apm/l/vim-mode.svg?style=flat-square)](LICENSE.txt) [![VS14](https://img.shields.io/badge/Visual%20Studio%20Marketplace%20%7C%20vs14-v3.0.0-orange.svg?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=GeorgeAleksandria.CoCo) [![VS15](https://img.shields.io/badge/Visual%20Studio%20Marketplace%20%7C%20vs15-v3.0.0-orange.svg?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=GeorgeAleksandria.CoCo-19226)

# CoCo
A Visual Studio [2015](https://marketplace.visualstudio.com/items?itemName=GeorgeAleksandria.CoCo) (vs14) and [2017](https://marketplace.visualstudio.com/items?itemName=GeorgeAleksandria.CoCo-19226) (vs15) extension that is analyzing source code to colorize and decorate appropriate syntax nodes to different colors and different styles. It makes easily to read and understand sources, to find sought entities and to navigate by code. 

## Features

Currently extension supports analyzing C#, VB\.Net source codes. It's colorize and decorate a many semantic and language elements such as **variables**, **methods**, **types**, **members**, **namespaces** and many others. For more detailed list of supported items, please see [Classifications.md](https://github.com/GeorgeAlexandria/CoCo/blob/dev/Classifications.md).

Extension can apply the following decorations to analyzed elements:
* Changing foreground and background
* Changing font family
* Changing font styles and stretches
* Using bold font weight
* Changing font rendering size
* Using overline, underline, strikethrough and baseline font

supports the following analyze options:
* Disable classification a current item
* Disable classification a current item inside a xml doc comments

and can apply the chosen settings to:
* Editor text
* Quick info

## Examples

The following screenshots show a different applying decorations and colors to the code:

![](https://user-images.githubusercontent.com/13402478/44617734-03017c80-a871-11e8-86ac-5cc4e0c4d73f.png)

![](https://user-images.githubusercontent.com/13402478/44617735-04cb4000-a871-11e8-9f69-52caf1210996.png)

![](https://user-images.githubusercontent.com/13402478/48673969-722cfb00-eb58-11e8-9896-6caa1188ee1d.png)

![](https://user-images.githubusercontent.com/13402478/54866283-d23fe200-4d82-11e9-84c0-e9eb4b3b4de2.png)

## How to use 
Use **CoCo/Classifications** option page in the Visual Studio options to change decorations of items or their analyze options:

![](https://user-images.githubusercontent.com/13402478/54866265-a6246100-4d82-11e9-9450-8e466c4c067e.png)


You can disable classification of the chosen item in the **Analyze settings** sections to allow the other similar extensions or Visual Studio classify this item.

use **CoCo/Presets** option page to save your current settings as preset and to apply or delete existing presets:

![](https://user-images.githubusercontent.com/13402478/54866264-a6246100-4d82-11e9-8d7a-6ba7ececfac8.png)

and use **CoCo/Quick Info** to choose the behavior of the quick info feature:

![](https://user-images.githubusercontent.com/13402478/54866263-a58bca80-4d82-11e9-91ee-3a2d40112771.png)

You need to click **OK** button to confirm your changes on the all CoCo options pages. If you will not click the **OK** button 
extension will use the previous (or default) settings for all of pages. Settings are stored under **%localappdata%\CoCo\CoCo.config**.


### Installation
Extension doesn't apply the one of **default presets** after the **first installation** on a system. Therefore, you need to set colors and decorations manually or **apply existing presets** as pointed out above. After **updating extension** to a new version it will use the stored settings from the config fie on your system if it exists otherwise you still need to apply some colors and decorations.

### Presets
You can look at the screenshot below where code was colorized and decorate after applying the **CoCo dark theme** preset:

<details>
<summary>Click to expand screenshot</summary>
  
![](https://georgealeksandria.gallerycdn.vsassets.io/extensions/georgealeksandria/coco-19226/1.0/1504035613003/277591/1/DarkExample.PNG)

</details>

And you can look at the similar screenshot where code was colorized and decorate after applying another preset - **CoCo light|blue theme**:

<details>
<summary>Click to expand screenshot</summary>
  
![](https://georgealeksandria.gallerycdn.vsassets.io/extensions/georgealeksandria/coco-19226/1.0/1504035613003/277592/1/LightExample.PNG)

</details>

## Changelog
For more information on project's changelog, please see [ReleaseNotes.md](https://github.com/GeorgeAlexandria/CoCo/blob/dev/ReleaseNotes.md)

## Contributing
For more information on contributing to the project, please see [CONTRIBUTING.md](https://github.com/GeorgeAlexandria/CoCo/blob/dev/CONTRIBUTING.md)

## License

CoCo is licensed under the [MIT license](https://github.com/GeorgeAlexandria/CoCo/blob/dev/LICENSE.txt)