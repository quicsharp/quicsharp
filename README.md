# Quicsharp

## Introduction

QUIC is a transport layer network protocol implemented on top of UDP. Amongst others, it is meant to provide lower latency and faster connection migration (e.g. when switching between cellular antennas) compared to TCP or plain UDP. As of late 2019, it is being standardized by the Internet Engineering Task Force (IETF).

**This project is a partial implementation of the [IETF QUIC Transport draft v23](https://datatracker.ietf.org/doc/draft-ietf-quic-transport/23/).**

It is an experimental school project. Due to time constraints, some parts of the implementation don't perfectly match the draft. Others, like QUIC-TLS, are completely ommitted. It is not actively maintained.

## Demo

To showcase the capabilities of this C# library, we created a sample QUIC client and server (in `sample/`).

Let's launch both and have the client initiate a connection to the server:

**TODO: super-short video**

Using Wireshark, we can see the details of what was transmitted under the hood:

**TODO: screen capture**

Two packets were transmitted: one from the client to the server to initiate the connection, and a response from the server.

### Client -> server initial packet

It contains a suggested source and destination connection IDs (SCID and DCID). Per the doc, connection IDs are used for the consistent routing of packets:

> The primary function of a connection ID is to ensure that changes in
> addressing at lower protocol layers (UDP, IP) don’t cause packets for
> a QUIC connection to be delivered to the wrong endpoint. Each
> endpoint selects connection IDs [...] which will allow packets with
> that connection ID to be routed back to the endpoint and identified
> by the endpoint upon receipt.

> During the initial handshake, packets [...] are
> used to establish the connection ID that each endpoint uses. Each
> endpoint uses the Source Connection ID field to specify the
> connection ID that is used in the Destination Connection ID field of
> packets being sent to them. Upon receiving a packet, each endpoint
> sets the Destination Connection ID it sends to match the value of the
> Source Connection ID that they receive.

For more details, see [7.2: Negotiating connection IDs](https://tools.ietf.org/html/draft-ietf-quic-transport-23#section-7.2).

In this initial client -> server packet, the client picks a random SCID and a random DCID:

**TODO: screen capture**

Apart from that, the packet contains a payload, consisting of one or more frames. A frame has a type (e.g. PADDING, CRYPTO, ACK) and type-dependant fields. For more details, see [12.4: Frames and Frame Types](https://tools.ietf.org/html/draft-ietf-quic-transport-23#section-12.4).

The spec requires that the Initial packet contains, in the payload, a CRYPTO frame initiating the TLS handshake. However, since we chose not to implement the cryptographic aspects of QUIC (i.e. QUIC-TLS), no CRYPTO frame is sent in our implementation. Instead, we include PADDING (`0x00`) frames. They have no semantic value and are merely to increase the size of a packet. This is to meet the minimum required (UDP datagram) size of 1200 bytes (see [14: Packet Size](https://tools.ietf.org/html/draft-ietf-quic-transport-23#section-14)) to make amplification attacks impractical.

### Server -> client initial packet

In its response, as the spec requires, the server kept the SCID provided by the client and used it as DCID.
It also picks a new, random SCID that the client will use in subsequent interactions.

**TODO: screen capture**

Again, because we did not implement QUIC-TLS, the server's answer does not include any CRYPTO frame, only PADDING frames.

### Back to the demo

An exchange of Initial packets should be followed by Handshake packets until the TLS handshake is complete. Then, actually useful information can be exchanged between the client and the server using post-handshake packets called _short header packets_.

To be able to communicate information between the client and the server without doing a TLS handshake, we used another feature of QUIC called _0-RTT packets_. Per the spec:

> [A 0-RTT packet] is used to carry "early"
> data from the client to the server as part of the first flight, prior
> to handshake completion.

Let's see this in practice:

**TODO: super-short video**

Here, our client sent a 0-RTT packet, with a StreamFrame containing the data entered by the user.

We can see this packet in Wireshark:

**TODO: screen capture**

The SCID and DCID are the same as previously defined.

<!-- The payload contains a StreamFrame (as seen in the byte visualization), but Wireshark cannot decode this part because o -->

## About using Wireshark

Only specific versions of Wireshark are capable of inspecting QUIC draft-v23 packets.

## State of the project

- A sample server-chat and a sample console based client chat were made to have an example on how to use the C# library.

- Packet factory that implements all the Long Header Packet types and the Short Header Packet type. They should match the draft.
- A few frames were implemented (Padding, Ack, Stream). They can be encoded and decoded inside every type of packet.
- Streams are implemented and can be used like the TCP Streams in C#. However, the stream type is not implemented (it is always bidirectionnal).
- The packets are acknowledged according to the draft. They are acknowledged more than necessary. We resend lost packets.
- TLS is not implemented.

## How to use

This project is a C# library. There is a client sample and a server sample in this repository.

### Limitations
