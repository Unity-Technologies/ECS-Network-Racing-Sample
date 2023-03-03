# ECS Network Racing

![ECSNetworkRacingHeader](https://user-images.githubusercontent.com/3436237/209012220-f888baf2-568b-4c06-bda2-8146333c76d6.jpg)

* Requires LFS, please clone the repository using LFS, do not download as a ZIP.
* Requires Unity 2022.2.0b8 or later.

Current tested version 2022.2.8f1
https://unity.com/releases/editor/whats-new/2022.2.8#release-notes

# Unity Hub URL
unityhub://2022.2.8f1/996aee41dc57

# Running the demo

* Open Multiplayer > Window: PlayMode Tools
* Select PlayMode Type -> Client Server
* Open scene "Project/Assets/Scenes/MainMenu.unity"
* Choose your car for the race.
* When you are in the lobby you can wait for other players or just press the Left Menu button and then Start button to start the race.

# Known Issues

* When using the Editor as Client Only, and running the game from the MainMenu scene, the Auto Connect Address field should be left empty  to avoid a conflict with the MainMenu connect address.
* Build times are taking much longer than expected. Since all the Shader Variants are being compiled, and not only the ones being used in the project.
* For the Dedicated Server builds, it is taking shaders and models that are only used in the client, this increases the size of the build. We are working on optimizing the tools to create more optimal Dedicated Server builds.
* Mobile IL2CPP builds only works with Managed Stripping Level set to Minimal.


# Controls
Keyboard: 

  Up Arrow / W: Throttle
  Down Arrow / S: Brake / Reverse
  Left and Right Arrows / A and D: Car Steering

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
