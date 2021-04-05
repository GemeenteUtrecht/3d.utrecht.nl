# 3DUtrecht

3D Utrecht is a platform ( soon available at 3d.utrecht.nl ) where the city of Utrecht can be experienced interactively in 3D using a browser using WebGL technology.

The code of this project is forked from 3DAmsterdam https://github.com/Amsterdam/3DAmsterdam

This code is licensed under the Creative Common Attribution-NoDerivatives 4.0 International (CC BY-ND 4.0) https://creativecommons.org/licenses/by-nd/4.0/

The main goals are:
- providing information about the city;
- making communication and participation more accessible through visuals;
- viewing and sharing 3D models.

More and more information and data is embedded, allowing future features like running simulations, visualization of solar and wind studies and showing impact of spatial urban changes. It will improve public insight in decision making processes.

## Unity 2019.4. (LTS)
The project is using the latest LTS (long-term support) release of Unity: 2019.4.
We will stick to this version untill new engine feature updates are required for the sake of maximum stability.
## WebGL/Universal Render Pipeline
Currently the main target platform is a WebGL(2.0) application.
The project is set up to use the [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline), focussing on high performance for lower end machines. Please note that shaders/materials have specific requirements in order to work in this render pipeline.

## Code convention 
All the project code and comments should be written in English. Content text is written in Dutch.

For C#/.NET coding conventions please refer to the Microsoft conventions:
[https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
For variable naming please refer to:
[https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)

## Project Setup 

The files structure of the project looks like this

```
project
│   README.md
│   .gitattributes
│   .gitignore
│   sync.py
└───3DNetherlands
    └───Assets
      └───3DAmsterdam
          Config3DAmsterdam_production
      └───3DUtrecht
          Config3DUtrecht
      └───Netherlands3D
      └───WebGLTemplates
          └───Fullscreen3DAmsterdam
          └───Fullscreen3DUtrecht
```

The directory Netherlands3D contains all reusable code, whereas 3DAmsterdam and 3DUtrecht define the city specific implementations. This includes a config file that contains the relative center coordinate, the website URL and other settings.

The 3DUtrecht files are located in the 3Dutrecht directory and  the html template files located in Fullscreen3DUtrecht.

The 3DAmsterdam core files are synchronized using the python sync.py script. 
This will copy the files from the GIT directory of the 3DAmsterdam develop branch.

The python sync script was chosen as a simple solution to use a nested Git repository file structure ( 3DUtrecht & 3DAmsterdam) where solutions like Git SubModule or Git SubTree proofed to be too complex.  Also the python script allows to be more flexible.

## Tile System

The platform uses a tile based system consisting of 1km by 1km tiles. The handling of the tiles is initiated by the TileHandler which resides under the /Layers in the scene and needs to be configured with the active Layers that implement the Layer baseclass. 

We currently have 4 implemented Layers which are 

- Buildings
- Trees
- Sewerage
- Terrain

Each Layer script needs implement the HandleLayer method. This function receives a TileChange variable that provides the [RD](https://nl.wikipedia.org/wiki/Rijksdriehoeksco%C3%B6rdinaten)  X and Y coordinate and an action.

The available actions are

- Create (create the tile)
- Upgrade (the Level of detail increases)
- Downgrade (the Level of detail decreases)
- Remove (remove the tile)

## AssetBundles

To improve performance the platform uses prebuild assets that are downloaded from the webserver. 

To create assets the workflow is as follows

- Generate the assets from code using AssetDatabase.CreateAsset. 
  This will create the asset as text based YAML file
- Then build the asset for WebGL using BuildPipeline.BuildAssetBundles. 
  This will create binary files and needs to be copied to the webserver that will serve them.

Note, the generation these assets can take some time on your computer.



