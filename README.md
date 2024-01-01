# This mod is now archived and deprecated in favor of [USTManager](https://github.com/ZedDevStuff/USTManager) which should release anywhere between January 1st and 3rd (the repo will be prvate until then so the link won't get you anywhere)

# Update: As of v0.8.0 (currently unreleased), no longer requires [UKUIHelper](https://github.com/ZedDevStuff/UKUIHelper/releases/latest) to work

# UKMusicReplacement

Ultrakill mod allowing users to use custom songs in regular (non boss or special) levels

# Installation

To install, just unzip the [release you downloaded](https://github.com/ZedDevStuff/UKMusicReplacement/releases/latest) in your BepInEx plugins folder (or "UKMM Mods" if you use UKMM). Download and install [UKUIHelper](https://github.com/ZedDevStuff/UKUIHelper/releases/latest) for the mod to work. The releases contain a custom song named "Stolen Heaven" for 4-1 and 4-2 by [triageGremlin](https://www.youtube.com/c/triageGremlin), but if you build the mod yourself, it won't come with any song.

# How to add custom songs

To add custom songs, open "CustomMusic" in the mod folder, create a folder with your song pack name then another one inside named with the number of the level you want to replace the music from (ie "4-1","1-2", nothing more, nothing less) and add your 2 songs, one must be named "clean" and the other "battle". They must be either an ogg, wav or mp3 file or they won't work. 

# Building

To build UKMusicReplacement, you can either use Visual Studio or Visual Studio Code and dotnet. When building is finished, you should have a dll file. To use it, just put it in a folder and put that folder in you BepInEx plugins folder or "UKMM Mods" folder if you use UKMM.
