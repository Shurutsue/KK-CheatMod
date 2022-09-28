# KK-CheatMod
Cheat mod for KoboldKare. Still a mess, but I am learning.

### Features:
- A Kobold Gene Editor with the added ability to clear stomach/metabolism bar.<br>
It will also show a line from the window to the Kobold (You can configure it in the config folder after starting it once with the mod)
- A Spawn Menu, where you can choose between Prefabs and Reagents.
- A simple multiplayer protection, requiring the host to allow the usage of the mod for the user<br>
He can easily do so by typin `!togglemods <PlayerID>` where <PlayerID> would be the user to allow the mods for<br>
*(He can figure out the ID by typing `/list players`*

<br>

## Building the mod yourself
To build the project yourself, you'll have to copy 3 files from KoboldKare_Data/Managed:
- Assembly-CSharp.dll
- PhotonRealtime.dll
- PhotonUnityNetworking.dll

Shove them into the Libs folder and add as dependency in Visual Studio (if not already present)

<br>

## Installing BepInEx and getting the unstripped DLL's

This mod uses BepInEx 5, so this process is tested with the version 5.4.21, which you can find [here](https://github.com/BepInEx/BepInEx/releases/latest).

Extract the contents directly into the installation folder of KoboldKare (Where the executable is to start the game)

### Install Unity

Since the game comes with stripped dll's, you'll have to get the unstripped ones yourself.<br>
So download the Unity Version for the game (at the time of writing it's 2021.3.6f1)<br>
Then create a new 3D project and wait for unity to finish loading.<br>
When that's done, click on File -> Build Settings -> Build<br>
Wait for it to finish, again, and then go to built project location, into it's _Data/Managed folder and copy the following files:

- mscorlib.dll
- netstandard.dll
- MonoSecurity.dll
- All dll files with System at the beginning of it's name
- UnityEngine.dll
- UnityEngine.UI.dll
- UnityEngine.UIModule.dll
- UnityEngine.TextRenderingModule.dll
- UnityEngine.IMGUIModule.dll
- UnityEngine.CoreModule.dll

This should come to a total of 27 files. These you'll copy into a new folder in the Install location of KoboldKare (ex. Unstripped)<br>
In case you want to modify this mod, or make your own, depending on what you wish to use, you may have to copy other dll files as well.

Following this, you open up doorstop_config.ini and change the last line from:<br>
`dllSearchPathOverride=` to `dllSearchPathOverride=<NewFolderName>`<br>
Where <NewFolderName> would be the folder you copied the dll's into (ex. Unstripped)<br>

Now start the game once - and quit it when it finished loading.<br>
You should now have a Plugins folder in the BepInEx folder, there you place the Cheatmod.dll to use it.