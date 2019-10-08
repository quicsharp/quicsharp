using System;

namespace quicsharp.tests
{
    public class SamplePacket
    {
        static string hex = "ffff00000efe1d757104a60f7d65146c7301feeb20b24c891b10fdbbfe99e57bb9b897e94f85cb49df0044b893baeadb436829aac79f0b679f1547907fcc72cf1aa3522bd8c7e5c6b744f44d8d14520699aabbab0e6d5bb36b9fab67b03318d1e771e97bc28ba4203dea5523ac7075a2ea841a5df786dec68319abf06e703fd43be37e72382de06dd3c0c3fc7ae54883b03d6b57bd61b2614e8741f4718da6d95d1c759f5d8329948a2e6f7ccbef189ab85eb6deeb8d14040c438bc53f2301dd1b66e849eabaace078c1bcd3c67cf2ded7b154bdfb7b3a34cdb1d56e6c25e3dc6bc32a0766f19d2af4a29db2445a4d2a5d66ec18f8f84f5aa1b1659c0704cc27721ccbb944f4e89ae2b64cc5ace04c6a6f92d37c292050ceea483ef210205d27930460c7e7daf84df2a70f2b38c40c39c5e05243662f9de18ee7c296f67bb08c621362b9010bd72b121697ac7837628e5f346af2052b4b14a4c12ef981ed8765308b718651d86569fe059759f9c788a8909680f0077e3e6c2a597b4406c8d25fdb7b08db482b8b7f0ba65239fee9997c124c03ad8855abf9e7c3b2a6e914c3b1aef76d31637705ca8d925a133aab718bd99395bb4f46709f36a624966709436c744a049d20579b15920361852c9214f49c807f4e5118d42181f24888040fed59e7f7b37bf643d3a54b070a839ff1a2e199d871349f63b288dc1b10d4ee591a575b18ce67f7c6acfa622fcc9581c283ef43265aec43cc4188f11170c18088694d8f46b6d6a78b458cf656165b25ee3622c595a82b2ffadfaeb1240d566fb399291a7503d061d9251afe0cf5be78e83f0428bd7f424a17cd0fa3b11903c5b8b1ee879f9031924f5bab0da26757d941466da27f67d287e02abfeedd135147ac9dba6ed555dfc43d548abe43044188e06b2f10c2413b869b8e1bfd7b538db36063d05bd5809ad279394b53b363f6ac9987350257d02758e598d5e1c47773ecf5c01e660570cc19fc0e4484760c24cee8e66d74b9fa302941056a8027aaa4c32fac483c50c89f4af899bac7c0d6b2e1046494e19453877a10c3d38effc01511533a19f3d656220a799791de3ad7cd8281051f198ac7305de999fbe59e87ee64638cda8d51e1022e05a92e4867adc5b8af0347a1165cf4e542db2fdeae05874e5c7b3d90246585bffce4241798e107a4ec2ddd354173dbdf7d26bc588ebd8c5dcf00fe4b385a74a05e2a9b7bc7c7363d56111d1c0bd6e2f3b450a12dbe44b56a39fe80d8a9dfa0af2441a4d39f950b064103e9f07caa91b5ca9ada0612430d800acf37872da0ae441d61a5e05b6394c425026c1626b1f32e14bf6bdb09812a22e8dc23e2cc42e9be4932ecfdb7ed402d7896a4f78ee803f8a217fd5239bfeafbc1ea7cc57aa39aa8cc6eb2c7f4143322ebdec14d3a3678198b700dd10c579b791083444ebaa8e5213f5a5ec83d121d7588ad50351c07518f49edc02c5973e87b815dde0e3c8a02a99a2001d9828095958dd641515e8a9607d69935085a7c6f51745ad9318d3b94953f2209ce113fc85b762b238688426744a56372a0e6efecdcefbb057f866c5783e8da4e84e6ef29cd3acbfa8b5b4426d3500bf6b98be8a529daad786acb8df1fdf64af20d3a47341e5c53640100f2276c021fa4747af37badf94f524ac52aa0a66a2fb40797745d28688435154ca09255e9fd84e82ddc844cbad7fb5bff00939a4a8671a78c215cbd2dd0ed4f54b7b9298fbc4214ddc56d78f857ae71211e66";

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public byte[] Bytes()
        {
            return StringToByteArray(hex);
        }
    }
}

// Sample packet details

// QUIC IETF
//     QUIC Connection information
//     1... .... = Header Form: Long Header(1)
//     ..11 .... = Packet Type: Retry(3)
//     .... 1111 = Original Destination Connection ID Length: 18 octets(15)
//     Version: draft-14 (0xff00000e)
//     1111 .... = Destination Connection ID Length: 18 octets(15)
//     .... 1110 = Source Connection ID Length: 17 octets(14)
//     Destination Connection ID: 1d757104a60f7d65146c7301feeb20b24c89
//     Source Connection ID: 1b10fdbbfe99e57bb9b897e94f85cb49df
//     Original Destination Connection ID: 0044b893baeadb436829aac79f0b679f1547
//     Retry Token: 907fcc72cf1aa3522bd8c7e5c6b744f44d8d14520699aabb…


// 0000   ff ff 00 00 0e fe 1d 75 71 04 a6 0f 7d 65 14 6c
// 0010   73 01 fe eb 20 b2 4c 89 1b 10 fd bb fe 99 e5 7b
// 0020   b9 b8 97 e9 4f 85 cb 49 df 00 44 b8 93 ba ea db
// 0030   43 68 29 aa c7 9f 0b 67 9f 15 47 90 7f cc 72 cf
// 0040   1a a3 52 2b d8 c7 e5 c6 b7 44 f4 4d 8d 14 52 06
// 0050   99 aa bb ab 0e 6d 5b b3 6b 9f ab 67 b0 33 18 d1
// 0060   e7 71 e9 7b c2 8b a4 20 3d ea 55 23 ac 70 75 a2
// 0070   ea 84 1a 5d f7 86 de c6 83 19 ab f0 6e 70 3f d4
// 0080   3b e3 7e 72 38 2d e0 6d d3 c0 c3 fc 7a e5 48 83
// 0090   b0 3d 6b 57 bd 61 b2 61 4e 87 41 f4 71 8d a6 d9
// 00a0   5d 1c 75 9f 5d 83 29 94 8a 2e 6f 7c cb ef 18 9a
// 00b0   b8 5e b6 de eb 8d 14 04 0c 43 8b c5 3f 23 01 dd
// 00c0   1b 66 e8 49 ea ba ac e0 78 c1 bc d3 c6 7c f2 de
// 00d0   d7 b1 54 bd fb 7b 3a 34 cd b1 d5 6e 6c 25 e3 dc
// 00e0   6b c3 2a 07 66 f1 9d 2a f4 a2 9d b2 44 5a 4d 2a
// 00f0   5d 66 ec 18 f8 f8 4f 5a a1 b1 65 9c 07 04 cc 27
// 0100   72 1c cb b9 44 f4 e8 9a e2 b6 4c c5 ac e0 4c 6a
// 0110   6f 92 d3 7c 29 20 50 ce ea 48 3e f2 10 20 5d 27
// 0120   93 04 60 c7 e7 da f8 4d f2 a7 0f 2b 38 c4 0c 39
// 0130   c5 e0 52 43 66 2f 9d e1 8e e7 c2 96 f6 7b b0 8c
// 0140   62 13 62 b9 01 0b d7 2b 12 16 97 ac 78 37 62 8e
// 0150   5f 34 6a f2 05 2b 4b 14 a4 c1 2e f9 81 ed 87 65
// 0160   30 8b 71 86 51 d8 65 69 fe 05 97 59 f9 c7 88 a8
// 0170   90 96 80 f0 07 7e 3e 6c 2a 59 7b 44 06 c8 d2 5f
// 0180   db 7b 08 db 48 2b 8b 7f 0b a6 52 39 fe e9 99 7c
// 0190   12 4c 03 ad 88 55 ab f9 e7 c3 b2 a6 e9 14 c3 b1
// 01a0   ae f7 6d 31 63 77 05 ca 8d 92 5a 13 3a ab 71 8b
// 01b0   d9 93 95 bb 4f 46 70 9f 36 a6 24 96 67 09 43 6c
// 01c0   74 4a 04 9d 20 57 9b 15 92 03 61 85 2c 92 14 f4
// 01d0   9c 80 7f 4e 51 18 d4 21 81 f2 48 88 04 0f ed 59
// 01e0   e7 f7 b3 7b f6 43 d3 a5 4b 07 0a 83 9f f1 a2 e1
// 01f0   99 d8 71 34 9f 63 b2 88 dc 1b 10 d4 ee 59 1a 57
// 0200   5b 18 ce 67 f7 c6 ac fa 62 2f cc 95 81 c2 83 ef
// 0210   43 26 5a ec 43 cc 41 88 f1 11 70 c1 80 88 69 4d
// 0220   8f 46 b6 d6 a7 8b 45 8c f6 56 16 5b 25 ee 36 22
// 0230   c5 95 a8 2b 2f fa df ae b1 24 0d 56 6f b3 99 29
// 0240   1a 75 03 d0 61 d9 25 1a fe 0c f5 be 78 e8 3f 04
// 0250   28 bd 7f 42 4a 17 cd 0f a3 b1 19 03 c5 b8 b1 ee
// 0260   87 9f 90 31 92 4f 5b ab 0d a2 67 57 d9 41 46 6d
// 0270   a2 7f 67 d2 87 e0 2a bf ee dd 13 51 47 ac 9d ba
// 0280   6e d5 55 df c4 3d 54 8a be 43 04 41 88 e0 6b 2f
// 0290   10 c2 41 3b 86 9b 8e 1b fd 7b 53 8d b3 60 63 d0
// 02a0   5b d5 80 9a d2 79 39 4b 53 b3 63 f6 ac 99 87 35
// 02b0   02 57 d0 27 58 e5 98 d5 e1 c4 77 73 ec f5 c0 1e
// 02c0   66 05 70 cc 19 fc 0e 44 84 76 0c 24 ce e8 e6 6d
// 02d0   74 b9 fa 30 29 41 05 6a 80 27 aa a4 c3 2f ac 48
// 02e0   3c 50 c8 9f 4a f8 99 ba c7 c0 d6 b2 e1 04 64 94
// 02f0   e1 94 53 87 7a 10 c3 d3 8e ff c0 15 11 53 3a 19
// 0300   f3 d6 56 22 0a 79 97 91 de 3a d7 cd 82 81 05 1f
// 0310   19 8a c7 30 5d e9 99 fb e5 9e 87 ee 64 63 8c da
// 0320   8d 51 e1 02 2e 05 a9 2e 48 67 ad c5 b8 af 03 47
// 0330   a1 16 5c f4 e5 42 db 2f de ae 05 87 4e 5c 7b 3d
// 0340   90 24 65 85 bf fc e4 24 17 98 e1 07 a4 ec 2d dd
// 0350   35 41 73 db df 7d 26 bc 58 8e bd 8c 5d cf 00 fe
// 0360   4b 38 5a 74 a0 5e 2a 9b 7b c7 c7 36 3d 56 11 1d
// 0370   1c 0b d6 e2 f3 b4 50 a1 2d be 44 b5 6a 39 fe 80
// 0380   d8 a9 df a0 af 24 41 a4 d3 9f 95 0b 06 41 03 e9
// 0390   f0 7c aa 91 b5 ca 9a da 06 12 43 0d 80 0a cf 37
// 03a0   87 2d a0 ae 44 1d 61 a5 e0 5b 63 94 c4 25 02 6c
// 03b0   16 26 b1 f3 2e 14 bf 6b db 09 81 2a 22 e8 dc 23
// 03c0   e2 cc 42 e9 be 49 32 ec fd b7 ed 40 2d 78 96 a4
// 03d0   f7 8e e8 03 f8 a2 17 fd 52 39 bf ea fb c1 ea 7c
// 03e0   c5 7a a3 9a a8 cc 6e b2 c7 f4 14 33 22 eb de c1
// 03f0   4d 3a 36 78 19 8b 70 0d d1 0c 57 9b 79 10 83 44
// 0400   4e ba a8 e5 21 3f 5a 5e c8 3d 12 1d 75 88 ad 50
// 0410   35 1c 07 51 8f 49 ed c0 2c 59 73 e8 7b 81 5d de
// 0420   0e 3c 8a 02 a9 9a 20 01 d9 82 80 95 95 8d d6 41
// 0430   51 5e 8a 96 07 d6 99 35 08 5a 7c 6f 51 74 5a d9
// 0440   31 8d 3b 94 95 3f 22 09 ce 11 3f c8 5b 76 2b 23
// 0450   86 88 42 67 44 a5 63 72 a0 e6 ef ec dc ef bb 05
// 0460   7f 86 6c 57 83 e8 da 4e 84 e6 ef 29 cd 3a cb fa
// 0470   8b 5b 44 26 d3 50 0b f6 b9 8b e8 a5 29 da ad 78
// 0480   6a cb 8d f1 fd f6 4a f2 0d 3a 47 34 1e 5c 53 64
// 0490   01 00 f2 27 6c 02 1f a4 74 7a f3 7b ad f9 4f 52
// 04a0   4a c5 2a a0 a6 6a 2f b4 07 97 74 5d 28 68 84 35
// 04b0   15 4c a0 92 55 e9 fd 84 e8 2d dc 84 4c ba d7 fb
// 04c0   5b ff 00 93 9a 4a 86 71 a7 8c 21 5c bd 2d d0 ed
// 04d0   4f 54 b7 b9 29 8f bc 42 14 dd c5 6d 78 f8 57 ae
// 04e0   71 21 1e 66
