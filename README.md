# Quicsharp

## Introduction

This project is based on the [IETF QUIC Transport draft v23](https://datatracker.ietf.org/doc/draft-ietf-quic-transport/23/).

It is an experimental project, and the goal is to implement QUIC over UDP as a C# library. This is a school project, so due to time constraints, some parts of the implementation don't perfectly match the draft. It is not actively maintained.

## State of the project

* A sample server-chat and a sample console based client chat were made to have an example on how to use the C# library.

* Packet factory that implements all the Long Header Packet types and the Short Header Packet type. They should match the draft.
* A few frames were implemented (Padding, Ack, Stream). They can be encoded and decoded inside every type of packet.
* Streams are implemented and can be used like the TCP Streams in C#. However, the stream type is not implemented (it is always bidirectionnal).
* The packets are acknowledged according to the draft. They are acknowledged more than necessary. However we don't send retry packet if a packet is lost.
* TLS is not implemented.

## How to use

This project is a C# library. There is a client sample and a server sample in this repository.
