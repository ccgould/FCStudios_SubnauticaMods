# Runtime Unity Editor For Subnautica

### Subnautica Fork
This fork is to work with Subnautica in particular. This will be maintained to stay working with the latest Subnautica updates. It will not be updated to be in parity with ManlyMarco/RuntimeUnityEditor.

### Description
In-game inspector, editor and interactive console for Subnautica. It's designed for debugging and modding Subnautica.

### Features
- Works on Subnautica via the BepInEx framework.
- GameObject and component browser
- Object inspector that allows modifying values of objects in real time
- REPL C# console
- All parts are integrated together (e.g. REPL console can access inspected object, inspector can focus objects on GameObject list, etc.)
- Pin variables to the top left of your screen even while the editor is closed.

![preview](https://user-images.githubusercontent.com/39247311/64476158-ce1a4c00-d18b-11e9-97d6-084452cdbf0a.PNG)


### How to use
- Build the project as a class library with .net framework 4.7.2
- The mcs dependency is this https://github.com/kkdevs/mcs - basically a port of roslyn-level mcs with all new language features to .Net 2.0

---
##### Supporting Development
I really didn't do much, if you love this tool so much the original author had this message:
`You can support development of my plugins through my Patreon page: https://www.patreon.com/ManlyMarco`
