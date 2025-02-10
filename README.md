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

At its core, the users’ confidence that their data is accessible to them, and not to anyone else, relies on trust in the service. Privacy based on trust is weak. In Eppie mailbox belongs exclusively to the owner of the private key. Eppie operates autonomously in a p2p networks There are no servers or other authorities ‘providing the service’ and therefore controlling the data. Nobody has access to data, even us, the developers. The system relies solely on strong cryptography and the decetralized architecture.

## Features

Eppie is early in development. The p2p part is not publicly available at the moment. For now it works as a conventional email client with additional security features:

- Compatibility with Gmail, Microsoft Outlook and other major email providers.
- Eppie can authentication at Proton Mail servers (which no other native desktop clients can do, as far as we are aware).
- PGP encryption is supported.
- Local account created with [BIP39 standard](https://bitcoinwiki.org/wiki/mnemonic-phrase) Seed-Phrase.
- Encrypted local backup.

## Install from Microsoft Store, App Store or Google Play

There's a preview version currently available at Microsoft Store.
<p align="left">
  <a href="https://apps.microsoft.com/detail/Eppie%20Mail%20Preview/9n3r8xkz16c5?mode=direct&cid=github">
    <img src="https://get.microsoft.com/images/en-us%20light.svg" width="200" alt="Download" />
  </a>
</p>

WIP

## Downloads

You may download the latest release for your system:

### Windows Installer

- [Eppie.App-x86-x64-ARM64.msixbundle](https://github.com/Eppie-io/Eppie-App/releases/latest/download/Eppie.App-x86-x64-ARM64.msixbundle)

### Linux Binaries

WIP

### macOS Binaries

WIP

### Android Binaries

WIP

## Build from Source Code

### Windows

#### Prerequisites

- OS: Windows 10 or later
- IDE: Visual Studio 2022 with installed workloads and components:
  - .Net Multi-platform App UI development
    - Android SDK setup (with Android SDK Platform 31)
    - .NET profiling tools
    - Xamarin
  - Windows application development
    - Universal Windows Platform tools
    - Windows 11 SDK (10.0.22621.0)
    - Windows 10 SDK (10.0.19041.0)
    - Windows App SDK

> [!NOTE]
> You can add the **Android SDK Platform 31** api in the **Platforms** tab in the **Android SDK Manager**  
> **Visual Studio Menu**: **Tools** &#10148; **Android** &#10148; **Android SDK Manager**

#### To Clone

```console
git clone --recursive https://github.com/Eppie-io/Eppie-App.git eppie-app
```

#### To Build and Launch

1. Open the `src/Eppie.App/Eppie.App.sln` file in Visual Studio.
2. Set the appropriate project as your startup project:
   - **UWP**: Select `Eppie.App.UWP`.
   - **Windows App SDK**: Select `Eppie.App`.
3. Select the **x64** platform.
   - For **Windows App SDK**, also set the target framework to `net9.0-windows10.0.22621`.
4. Build the solution:
   - **Visual Studio Menu**: `Build ➤ Build Solution`
5. Launch the project:
   - **UWP**: Run with debugging using `Debug ➤ Start Debugging (F5)` or without debugging using `Debug ➤ Start Without Debugging (Ctrl + F5)`.
   - **Windows App SDK**: Run with debugging using `Debug ➤ Start Debugging (F5)` or without debugging using `Debug ➤ Start Without Debugging (Ctrl + F5)`.

For instructions on creating an app package, refer to [this guide](https://platform.uno/docs/articles/uno-publishing-windows-packaged-signed.html).


### Linux

WIP

### macOS

WIP

## Planned Features

As the project matures more features will be added, including but not limited to

- Creating a decentralized Eppie account
- Encrypted p2p messaging
- Encrypted decentralized backup
- Connecting existing decentralized identities, e.g. [ENS](https://ens.domains/)

## Technology Stack

At launch Eppie will to store the data using [IPFS](https://github.com/ipfs/ipfs) infrastructure, and the transport layer will work through [SBBS](https://github.com/BeamMW/beam/wiki/Secure-bulletin-board-system-%28SBBS%29). With that being said, the architecture allows to easily plug in multiple storage and transport technologies. Eppie's e2e encryption is based on [Elliptic-curve](https://en.wikipedia.org/wiki/Elliptic-curve_cryptography) cryptography. GUI application is being written in C# with [Uno](https://github.com/unoplatform/uno), and [CLI](https://github.com/Eppie-io/Eppie-CLI) is pure C#. Eppie targets Windows, macOS, Linux, iOS, and Android platforms.

## Contribution

First of all this is a pretty ambitiois project and we are greateful beyond measure for every bit of help from our community. If you decide to contribute, please create an issue first, or find an existing one, unless it's a very minor fix, like a typo.

[Here](https://eppie.crowdin.com/eppie) you can help Eppie with localization.

Also, feel free to [subscripe](https://eppie.io) to our waiting list. We might invite you for an interview or beta testing.
