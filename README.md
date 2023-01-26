<!-- https://github.com/foxscore/vcc-user-repo-fix -->

# User Repository Fix for VCC

This is a MelonLoader mod that fixes 2 issues with the creator companion:
- Not caching user repositories properly
- Not keeping repositories in memory after they are loaded

Doing so severely improves the performance if any user repositories are used.



## Installation

> If you've installed the Creator Companion for all users (i.e. not just yourself), you'll need to replace `C:\Users\[USERNAME]\AppData\Local\Programs` with `C:\Program Files (x86)` in the instructions below

> Every time you update the Creator Companion, you'll need to reinstall this mod (including MelonLoader)

- Download the MelonLoader installer from [here](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe)
- Install MelonLoader
  - Click the "Select" button
  - Go to `C:\Users\[USERNAME]\AppData\Local\Programs\VRChat Creator Companion` and select `CreatorCompanion.exe`
  - Click the "Install" button
- Open the file explorer and go to `C:\Users\[USERNAME]\AppData\Local\Programs\VRChat Creator Companion`
- Create a folder called `Mods`, if it doesn't already exist
- Download the latest `VccUserRepoFix.dll` from the [releases page](https://github.com/foxscore/vcc-user-repo-fix/releases/latest) and move it to the `Mods` folder



## Building

> The built DLL will be located at `VccUserRepoFix\bin\Debug\VccUserRepoFix.dll`. You can safely ignore all the other files.

- Before you start:
  - Install the [.NET 4.7.2 SDK](https://dotnet.microsoft.com/download/dotnet-framework/net472)
  - Install the Creator Companion for yourself. If you've installed it for all users, you'll need to replace the dependency path in `VccUserRepoFix.csproj` with `C:\Program Files (x86)\VRChat Creator Companion\`
  - Install [MelonLoader](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe) to the Creator Companion
- Clone the repository
- Open the solution in Visual Studio (or your preferred IDE)
- Build the solution
