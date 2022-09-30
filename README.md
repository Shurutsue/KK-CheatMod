# KK-CheatMod
Cheat mod for KoboldKare. Still a mess, but I am learning.

### Features:
- **Kobold Editor**  
  Allows for editing the genes or stomach content of the Kobold selected.  
  Includes an indication line from the window to the selected Kobold, to easily be able to distinguish which Kobold is currently selected.
- **Spawn Menu**  
Simplifies spawning in reagents or prefabs with a menu and allows to spawn in reagents in either a bucket, trough or watering can instead of only a bucket.  
*Only available when host or in singleplayer.
- **Simple multiplayer protection**  
  The host will have to allow the use of the mod via the chat command  
  `!togglemods <playerid>` where `<playerid>`is the actor number of the user to allow the mods for.  
 The host can type `/list players` to see current players with their actor ids.
- **Grab-One feature**  
 Includes an indiactor on the right side and a simple hotkey to toggle between grabbing only one object, or however much you would otherwise be able to. (Default H Key)  
*Does not require the host to accept

After the first start with the mod, you can change the configuration in `BepInEx/config/Cheat_Mod.cfg`.  
*Requires a restart for changes to take effect.

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

Since the game comes with stripped dll's, you'll have to get the unstripped ones yourself.  
So download the Unity Version for the game (at the time of writing it's 2021.3.6f1)  
Then create a new 3D project and wait for unity to finish loading.  
When that's done, click on File -> Build Settings -> Build  
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

This should come to a total of 27 files. These you'll copy into a new folder in the Install location of KoboldKare (ex. Unstripped)  
In case you want to modify this mod, or make your own, depending on what you wish to use, you may have to copy other dll files as well.

Following this, you open up doorstop_config.ini and change the last line from:  
`dllSearchPathOverride=` to `dllSearchPathOverride=<NewFolderName>`  
Where `<NewFolderName>` would be the folder you copied the dll's into (ex. `dllSearchPathOverride=Unstripped`)

Now start the game once - and quit it when it finished loading.  
You should now have a Plugins folder in the BepInEx folder, there you place the Cheatmod.dll to use it.
