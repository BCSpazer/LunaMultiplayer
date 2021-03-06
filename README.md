<p align="center">
    <img src="../master/External/logo.png" alt="Luna multiplayer logo"/>
    <a href="https://www.youtube.com/watch?v=gf6xyLnpnoM"><img src="https://img.youtube.com/vi/gf6xyLnpnoM/0.jpg" alt="Video" height="187" width="250"/></a>
</p>

<p align="center">
  <a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg" alt="PayPal"/></a>
  <a href="https://discord.gg/S6bQR5q"><img src="https://img.shields.io/discord/378456662392045571.svg" alt="Chat on discord"/></a>
  <a href="../../wiki"><img src="https://img.shields.io/badge/documentation-Wiki-4BC51D.svg" alt="Documentation" /></a>
</p>

<p align="center">
  <a href="../../releases"><img src="https://img.shields.io/github/release/lunamultiplayer/lunamultiplayer.svg" alt="Latest release" /></a>
  <a href="../../releases"><img src="https://img.shields.io/github/downloads/lunamultiplayer/lunamultiplayer/total.svg" alt="Total downloads" />
  <a href="../../"><img src="https://img.shields.io/github/search/lunamultiplayer/lunamultiplayer/goto.svg" alt="Total hits" />
</p>

---

# Luna Multiplayer Mod (LMP)

*Multiplayer mod for [Kerbal Space Program (KSP)](https://kerbalspaceprogram.com)*

*[Official KSP forum post](https://forum.kerbalspaceprogram.com/index.php?/topic/168271-131-luna-multiplayer-lmp-alpha/)*

*[Official website](http://lunamultiplayer.com)*


### Main features:

- Clean and optimized code, based on systems and windows which makes it easier to read and modify.
- Multi threaded (as much as Unity allows)
- Settings saved as XML.
- Time synced between clients and the server using [NTP](https://en.wikipedia.org/wiki/Network_Time_Protocol) protocol.
- [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol) based using the [Lidgren](https://github.com/lidgren/lidgren-network-gen3) library for reliable UDP message handling.
- Uses interpolation so the vessels shouldn't jump from one place to another.
- [Nat-punchtrough](../../wiki/Master-server) feature so a server doesn't need to open ports on it's router.
- Servers are displayed within the mod.
- Better creation of network messages so they are easier to modify and serialize.
- Every network message is cached in order to reduce the garbage collector spikes
- Based on tasks instead of threads.
- [QuickLZ](http://www.quicklz.com) for fast compression

Please check the [wiki](../../wiki) to see how to [build](../../wiki/How-to-compile-LMP), [run](../../wiki/How-to-play-with-LMP.) or [debug](../../wiki/Debugging-in-Visual-studio) LMP

---
### Servers:

You can check the available servers in the official website or trough the website inside the repo [here](http://htmlpreview.github.io/?http://github.com/LunaMultiplayer/LunaMultiplayer/blob/master/WebDisplayServers/servers.html)

---

### Status:

|   Branch   |   Build  |   Tests  |  Last commit  |   Activity    |
| ---------- | -------- | -------- | ------------- | ------------- |
| **master** |[![AppVeyor](https://img.shields.io/appveyor/ci/gavazquez/lunamultiplayer/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master) | [![AppVeyor Tests](https://img.shields.io/appveyor/tests/gavazquez/lunamultiplayer/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/lunamultiplayer/branch/master/tests) | [![GitHub last commit](https://img.shields.io/github/last-commit/lunamultiplayer/lunamultiplayer/master.svg)](../../commits/master) | [![GitHub commit activity](https://img.shields.io/github/commit-activity/y/lunamultiplayer/lunamultiplayer.svg)](../../commits/master)

---

### Contributing:

Consider [donating trough paypal](https://paypal.me/gavazquez) if you like this project. 
It will encourage us to do future releases and add new features :star:

Please write the code as you were going to leave it, return after 1 year and you'd have to understand what you wrote.  
It's **very** important that the code is clean and documented so in case someone leaves, another programmer could take and maintain it. Bear in mind that **nobody** likes to take a project where it's code looks like a dumpster.

There's also a test project in case you want to add tests to your code.

---

### Troubleshooting:

Please visit [this page](../../wiki/Troubleshooting) in the wiki to solve the most common issues with LMP

---

<p align="center">
  <a href="mailto:gavazquez@gmail.com"><img src="https://img.shields.io/badge/email-gavazquez@gmail.com-blue.svg?style=flat" alt="Email: gavazquez@gmail.com" /></a>
  <a href="./LICENSE"><img src="https://img.shields.io/github/license/lunamultiplayer/LunaMultiPlayer.svg" alt="License" /></a>
</p>
