# 聊天核心 ChatCore

聊天核心 ChatCore是用.NET Standard 2.0编写的共享聊天客户端库。该项目的主要目的是减少多程序对同一个聊天服务进行交互的开销（这对于游戏中有多个模块需要聊天服务来说非常有用）。

# 基本设置 (节奏光剑mod用户)

1. 从[这里](https://github.com/baoziii/ChatCore-v2)下载最新的压缩包并解压到游戏根目录
2. 在安装了任何调用聊天核心的mod以后，设置项网页将会在启动游戏后在你的默认浏览器中打开。你可以在里面配置你的Twitch和Bilibili设置项。根据你的浏览器语言会自动切换语言包。如果语言种类不对，可以到标题右侧的翻译按钮中切换翻译。


# 项目基本设置 (面向开发者)

[这里](https://github.com/baoziii/ChatCore-v2/tree/master/ChatCoreTester)包含了一个测试器。你可以查看示例来了解如何使用聊天核心ChatCore服务。

# 本地化

English: [baoziii](https://github.com/baoziii)

中文(简体): [baoziii](https://github.com/baoziii)

日本語: [乾杯君Sennke](twitter.com/SyLfc0knjpCd9QR) [๑Misaki๑](https://twitter.com/misakilwd0526)

# ChatCore
ChatCore is a shared chat client library written in .NET Standard 2.0. The main objective behind this project is to reduce overhead in situations where multiple assemblies may want to interact with the same chat services (this is most useful with game modifications that have several significant chat integrations).

# Basic Configuration (for Beat Saber mod users)
1. Grab the latest ChatCore.dll and ChatCore.manifest from https://github.com/brian91292/ChatCore/releases
2. Copy ChatCore.dll into the `Libs` folder inside your `Beat Saber` directory.
3. Copy ChatCore.manifest into the `Plugins` folder inside your `Beat Saber` directory.
4. After installing any mod that utilizes ChatCore, a settings web app will be launched upon starting the game. Use this to login, join/leave channels, and configure various settings. The application language dependes on your browser language. If the language detection failed, you can manually change the language by click the button on the right of title.


# Basic Project Setup (for devs)
Check out the included [Test Project](https://github.com/brian91292/ChatCore/blob/develop/ChatCoreTester/) for a basic example of how to start the ChatCore services.

# Localization

English: [baoziii](https://github.com/baoziii)

中文(简体): [baoziii](https://github.com/baoziii)

日本語: [乾杯君Sennke](twitter.com/SyLfc0knjpCd9QR) [๑Misaki๑](https://twitter.com/misakilwd0526)