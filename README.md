# UKMusicReplacement

Ultrakill mod allowing users to use custom songs in regular (non boss or special) levels

# Installation

To install, just unzip the release in your BepInEx plugins folder (or "UKMM Mods" if you use UKMM). Requires [UKUIHelper](https://github.com/ZedDevStuff/UKUIHelper) to work. The releases contain a custom song named "Stolen Heaven" for 4-1 and 4-2 by [triageGremlin](https://www.youtube.com/c/triageGremlin), but if you build the mod yourself, it won't come with any song.

# How to add custom songs

To add custom songs, open "CustomMusic" in the mod folder, create a folder named with the number of the level you want to replace the music from (ie "4-1","1-2", nothing more, nothing less) and add your 2 songs, one must be named "clean" and the other "battle". They must be either an ogg, wav or mp3 file. 

# Building

To build UKMusicReplacement, you can either use Visual Studio or Visual Studio Code and dotnet. When building is finished, you should have a dll file. To use it, just put it in a folder and put that folder in you BepInEx plugins folder or "UKMM Mods" folder if you use UKMM.
