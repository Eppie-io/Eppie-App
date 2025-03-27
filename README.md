# Eppie — open protocol encrypted p2p email

[![LicenseBadge](https://img.shields.io/github/license/Eppie-io/Eppie-App.svg)](https://raw.githubusercontent.com/Eppie-io/Eppie-App/main/LICENSE)
[![Build and Test](https://img.shields.io/github/actions/workflow/status/Eppie-io/Eppie-App/build.yml?logo=github&branch=main&event=push)](https://github.com/Eppie-io/Eppie-App/actions/workflows/build.yml?query=branch%3Amain+event%3Apush)
[![Crowdin](https://badges.crowdin.net/e/8fee200a40ee70ffd3fa6b7d8d23deee/localized.svg)](https://eppie.crowdin.com/eppie)
[![Release](https://img.shields.io/github/v/release/Eppie-io/Eppie-App)](https://github.com/Eppie-io/Eppie-App/releases/latest)

## Intro

[Eppie](https://eppie.io) is a next-gen _providerless_ email that allows its users to own their accounts, addresses and data. In addition to its own p2p network Eppie can communicate with other popular decentralized networks, like Ethereum, and is capable of interacting with conventional IMAP/SMTP email. It is beautiful and easy to use, just like a normal email client.

## Motivation

Out of 4+ billion email accounts in the world, about 0 belong to users.

A typical mailbox contains all sorts of important private information of our business and social activity, finance, health, consumer behaviour etc. Furthermore email is the primary identity provider in the modern Internet. We use our email address to log in to hundreds of other services. Email is the core of our digital identity. Yet we do not own it. It is controlled by a server and therefore belongs to the technology provider. The server decides whether to allow us to use our identity or not. This is a privacy violation by design. Identity naturally belongs to human, it should not be a service.

At its core, the users' confidence that their data is accessible to them, and not to anyone else, relies on trust in the service. Privacy based on trust is weak. In Eppie mailbox belongs exclusively to the owner of the private key. Eppie operates autonomously in a p2p networks There are no servers or other authorities 'providing the service' and therefore controlling the data. Nobody has access to data, even us, the developers. The system relies solely on strong cryptography and the decetralized architecture.

## Features

Eppie is early in development. The p2p part is not publicly available at the moment. For now it works as a conventional email client with additional security features:

- Compatible with Gmail, Microsoft Outlook and other major email providers.
- Eppie can authenticate at Proton Mail servers (which no other native desktop client can do, as far as we are aware).
- PGP encryption is supported.
- Local account created with [BIP39 standard](https://bitcoinwiki.org/wiki/mnemonic-phrase) Seed-Phrase.
- Encrypted local backup.

## Screenshots

<details>
  <summary>Main Screen</summary>
  <img src="https://github.com/user-attachments/assets/30301949-4b25-4ad8-be72-e954d9211aeb" alt="Main Screen"/>
</details>

<details>
  <summary>Settings Page</summary>
  <img src="https://github.com/user-attachments/assets/fb183b2f-766b-4910-9119-c7b8ab66781a" alt="Settings Page"/>
</details>

## Install from Microsoft Store, App Store or Google Play

There's a preview version currently available at Microsoft Store.
<p align="left">
  <a href="https://apps.microsoft.com/detail/Eppie%20Mail%20Preview/9n3r8xkz16c5?mode=direct&cid=github">
    <img src="https://get.microsoft.com/images/en-us%20light.svg" width="200" alt="Download" />
  </a>
</p>

App Store and Google Play: WIP

## Downloads

You may download the latest release for your system:

### Windows

- [eppie.desktop-win-x64.zip](https://github.com/Eppie-io/Eppie-App/releases/download/v1.0.0-preview.1/eppie.desktop-win-x64.zip) (Binaries)
- [Eppie.App-x86-x64-ARM64.msixbundle](https://github.com/Eppie-io/Eppie-App/releases/latest/download/Eppie.App-x86-x64-ARM64.msixbundle) (UWP)
- [Eppie.App.WinAppSDK-x86-x64-ARM64.msixbundle](https://github.com/Eppie-io/Eppie-App/releases/latest/download/Eppie.App.WinAppSDK-x86-x64-ARM64.msixbundle) (Windows App SDK)

### Linux

- [eppie.desktop-linux-x64.tar.gz](https://github.com/Eppie-io/Eppie-App/releases/latest/download/eppie.desktop-linux-x64.tar.gz) (Binaries)

### macOS

- [eppie.desktop-osx-x64.tar.gz](https://github.com/Eppie-io/Eppie-App/releases/latest/download/eppie.desktop-osx-x64.tar.gz) (Binaries)

### Android

WIP

## Build from Source Code

### Clone

```console
git clone --recursive https://github.com/Eppie-io/Eppie-App.git eppie-app
```

### Setting up the environment

Use the following [guide](https://platform.uno/docs/articles/get-started-vscode.html) to set up an environment for building Eppie in **VS Code** under Windows, Linux or macOS.  
For **Visual Studio 2022**, use [this guide](https://platform.uno/docs/articles/get-started-vs-2022.html)  
To build the UWP project, use [Visual Studio 2022](https://platform.uno/docs/articles/get-started-vs-2022.html) with installed component **Windows application development &#10148; Universal Windows Platform tools**

### Build and Launch

To [debug Eppie](https://platform.uno/docs/articles/create-an-app-vscode.html?tabs=skia#debug-the-app) on Windows, macOS, and Linux.  
To [debug Eppie](https://platform.uno/docs/articles/create-an-app-vs2022.html?tabs=desktop#debug-the-app) with **Visual Studio 2022**.

To run the UWP project, open `src/Eppie.App/Eppie.App.sln` file in **Visual Studio 2022** and select `Eppie.App.UWP` as your startup project.

To create Eppie packages, refer to [these instructions](https://platform.uno/docs/articles/uno-publishing-overview.html).

## Planned Features

As the project matures more features will be added, including but not limited to

- Creating a decentralized Eppie account
- Encrypted p2p messaging
- Encrypted decentralized backup
- Connecting existing decentralized identities, e.g. [ENS](https://ens.domains/)

## Technology Stack

At launch Eppie will store the data using [IPFS](https://github.com/ipfs/ipfs) infrastructure, and the transport layer will work through [SBBS](https://github.com/BeamMW/beam/wiki/Secure-bulletin-board-system-%28SBBS%29). With that being said, the architecture allows to easily plug in multiple storage and transport technologies. Eppie's e2e encryption is based on [Elliptic-curve](https://en.wikipedia.org/wiki/Elliptic-curve_cryptography) cryptography. GUI application is being written in C# with [Uno](https://github.com/unoplatform/uno), and [CLI](https://github.com/Eppie-io/Eppie-CLI) is pure C#. Eppie targets Windows, macOS, Linux, iOS, and Android platforms.

## Contribution

First of all this is a pretty ambitiois project and we are greateful beyond measure for every bit of help from our community. If you decide to contribute, please create an issue first, or find an existing one, unless it's a very minor fix, like a typo.

[Here](https://eppie.crowdin.com/eppie) you can help Eppie with localization.

Also, feel free to [subscripe](https://eppie.io) to our waiting list. We might invite you for an interview or beta testing.
