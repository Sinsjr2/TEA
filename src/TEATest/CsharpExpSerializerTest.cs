using System.Collections.Generic;
using NUnit.Framework;
using TestTools;
using TEA.Diagnostics;

namespace TEATest {

    public class CsharpExpSerializerTest {
        public record SampleMessage();
        public record SampleMessageBool(bool Boolean);
        public record SampleMessageByte(byte ByteValue);
        public record SampleMessageSByte(sbyte sbyteValue);
        public record SampleMessageChar(char CharValue);
        public record SampleMessageDecimal(decimal DecimalValue);
        public record SampleMessageDouble(double DoubleValue);
        public record SampleMessageFloat(float FloatValue);
        public record SampleMessageInt(int Integer);
        public record SampleMessageUInt(uint UIntValue);
        public record SampleMessageNInt(nint NIntValue);
        public record SampleMessageNUInt(nuint NUIntValue);
        public record SampleMessageLong(long LongValue);
        public record SampleMessageULong(ulong ULongValue);
        public record SampleMessageShort(short ShortValue);
        public record SampleMessageUShort(short UShortValue);
        public record SampleMessageString(string Str);
        public record SampleMessageReadOnlyList(IReadOnlyList<int> XS);
        public record SampleMessageWrapper1(SampleMessage S1);
        public record SampleMessageWrapper2(SampleMessage S1, SampleMessage S2);

        static string nameSpace = $"{nameof(TEATest)}.{nameof(CsharpExpSerializerTest)}";

        public record SampleMessageMember {
            public string Getter { get; } = "123";
            public string PrivateSetter { get; private set; } = "123456";
            public int SetAndGet { get; set; }
            public bool Bool = false;
        }

        public record SampleMessageConstructorAndMember(int I) {
            public int Foo { get; init; }
        }


        public struct Struct {
            public bool B;
            public short S;
        }

        public struct StructConstructor {
            public string Str;

            public StructConstructor(string Str) {
                this.Str = Str;
            }
        }

        public record IsExpectedExpStringData(object Obj, string Expected);

        public static IsExpectedExpStringData[] GetIsExpectedExpStringData() {
            return new IsExpectedExpStringData[] {
                new((byte)5, "5"),
                new(5.6, "5.6"),
                new(9.96f, "9.96f"),
                new('A', "'A'"),
                new(new[] { 1, 2, 3 }, "new[] { 1, 2, 3 }"),
                new(new List<System.Int32> { 4, 5 }, "new List<System.Int32> { 4, 5 }"),
                new(new SampleMessage(), $"new {nameSpace}.SampleMessage()"),
                new(new SampleMessageBool(true), $"new {nameSpace}.SampleMessageBool(true)"),
                new(new SampleMessageWrapper1(new SampleMessage()), $"new {nameSpace}.SampleMessageWrapper1(new {nameSpace}.SampleMessage())"),
                new(new SampleMessageMember() { SetAndGet = 5, Bool = true }, $"new {nameSpace}.SampleMessageMember() {{ SetAndGet = 5, Bool = true }}"),
                new(new SampleMessageConstructorAndMember(100) { Foo = 4 }, $"new {nameSpace}.SampleMessageConstructorAndMember(100) {{ Foo = 4 }}"),
                new(new Struct() { B = true, S = 70 }, $"new {nameSpace}.Struct() {{ B = true, S = 70 }}"),
                new(new StructConstructor("str"), $"new {nameSpace}.StructConstructor(\"str\")"),
                new(new SampleMessageReadOnlyList(new[] { 5, 6, 7 }), $"new {nameSpace}.SampleMessageReadOnlyList(new[] {{ 5, 6, 7 }})"),
            };
        }

        [Test]
        [TestCaseSource(nameof(GetIsExpectedExpStringData))]
        public void IsExpectedExpString(IsExpectedExpStringData data) {
            data.Obj.GetExp().Is(data.Expected);
        }
    }
}
