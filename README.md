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

The files structure of the project looks like this. 

```
3DAmsterdam.fork
└───3DAmsterdam
    
3d.utrecht.nl
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

To setup the file structure the following steps should be performed

1. Fork https://github.com/Amsterdam/3DAmsterdam and clone to 3DAmsterdam.fork

2. Create directory 3d.utrecht.nl. This is also the location to place the README.MD file

3. Copy the files from 3DAmsterdam.fork\3DAmsterdam to 3d.utrecht.nl\3DNetherlands

4. Remove directory 3d.utrecht.nl\3DNetherlands\Assets\3DAmsterdam

5. Remove directory 3d.utrecht.nl\3DNetherlands\Assets\Netherlands3D

6. Add symbolic links as shown below, make sure the drive supports symbolic links. For Windows this only works on NTFS volumes

   (to create the symbolic links you need to open a command prompt and run it as Administrator)
```
cd \3d.utrecht.nl\3DNetherlands\Assets
mklink /d 3DAmsterdam \3DAmsterdam.fork\3DAmsterdam\Assets\3DAmsterdam
mklink /d Netherlands3D \3DAmsterdam.fork\3DAmsterdam\Assets\Netherlands3D
```

This completes the setup of the file structure. 
The Webgl directory Fullscreen3DAmsterdam can be copied and made custom

## Unity Input System

The project uses the new Unity input system called Input Action Assets. The input mappings are defined in 
`\Assets\Netherlands3D\Input\3DNetherlands.inputactions`

This will then generate a c# class that will be used in 
`\Assets\Netherlands3D\Input\ActionHandler.cs`

## Local Test WebGl build

In order to locally test a build you start your own http server. If you have npm installed you can use the [http-server](https://www.npmjs.com/package/http-server) package and run it in the build directory

```
http.server
```
Or if you have [Python](https://www.google.com)  installed you can run


```
python -m http.server
```

You also need to install a CORS plugin to make it work

https://microsoftedge.microsoft.com/addons/detail/allow-cors-accesscontro/bhjepjpgngghppolkjdhckmnfphffdag

https://chrome.google.com/webstore/detail/allow-cors-access-control/lhobafahddgcelffkeicbaginigeejlf



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


