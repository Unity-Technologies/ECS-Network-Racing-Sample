# DOTS Cars Demo (Name TBD)

* Uses dots repo as a Git Submodule. Packages manifest points to the submodule.
* Requires Unity 2022.2, whatever works with the latest dots master.
* Requires to be connected to Unity VPN.
* Current tested editor version is Unity: Unity.DOTS: 2022.2.0a12-dots (Hg: 5675e87fdb3e Git: 3f7fc94bd590f)

# Unity Downlaoder
https://beta.unity3d.com/download/5675e87fdb3e/download.htm

# Unity Hub URL
unityhub://2022.2.0a12-dots/5675e87fdb3e

# Cloning
The DOTS Cars repo uses git submodules (https://git-scm.com/book/en/v2/Git-Tools-Submodules).

Fresh clone:
```
git clone git@github.cds.internal.unity3d.com:unity/dots-demo.git --recurse-submodules
```

If you have a clone before submodules were added or you didn't use `--recurse-submodules`:
```
git submodule init
git submodule update
```

# Running the demo
* Connect to Unity VPN
* Open scene "Project/Assets/Scenes/MainMenu.unity"
* Choose your car for the race.
* Choose Local if you want to create Client and Server. 
* When you are in the lobby you can wait for other players or just press the Start button to start the race.

# Known Issues


# Controls
Keyboard: 

  Up Arrow / W: Throttle
  Down Arrow / S: Brake / Reverse
  Left and Right Arrows / A and D: Car Steering

Mobile:

  WIP
  
# Features

* Netcode for Entities
* Baking
* Idiomatic Foreach
* Aspects
* ISystem
* Unity Physics
* Jobs
* Burst
* Vivox

# Assets

* IMPORTANT: We are currently using temporal assets to have an idea of the feeling of the game. We don't have permissions or licences to publish the assets in the project.
