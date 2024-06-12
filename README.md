# Eppie — open protocol encrypted p2p email

[![Crowdin](https://badges.crowdin.net/e/8fee200a40ee70ffd3fa6b7d8d23deee/localized.svg)](https://eppie.crowdin.com/eppie)

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
WIP

## Download latest binaries
WIP

## Build from source
WIP

## Planned features
As the project matures more features will be added, including but not limited to
* Creating a decentralized Eppie account
* Encrypted p2p messaging
* Encrypted decentralized backup
* Connecting existing decentralized identities, e.g. [ENS](https://ens.domains/)

## Technology Stack
At launch Eppie will to store the data using [IPFS](https://github.com/ipfs/ipfs) infrastructure, and the transport layer will work through [SBBS](https://github.com/BeamMW/beam/wiki/Secure-bulletin-board-system-%28SBBS%29). With that being said, the architecture allows to easily plug in multiple storage and transport technologies. Eppie's e2e encryption is based on [Elliptic-curve](https://en.wikipedia.org/wiki/Elliptic-curve_cryptography) cryptography. GUI application is being written in C# with [Uno](https://github.com/unoplatform/uno), and [CLI](https://github.com/Eppie-io/Eppie-CLI) is pure C#. Eppie targets Windows, macOS, Linux, iOS, and Android platforms.

## Contribution
First of all this is a pretty ambitiois project and we are greateful beyond measure for every bit of help from our community. If you decide to contribute, please create an issue first, or find an existing one, unless it's a very minor fix, like a typo.

[Here](https://eppie.crowdin.com/eppie) you can help Eppie with localization.

Also, feel free to [subscripe](https://eppie.io) to our waiting list.

