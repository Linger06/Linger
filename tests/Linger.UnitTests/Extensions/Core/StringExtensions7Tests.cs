namespace Linger.UnitTests.Extensions.Core
{
    public class StringExtensions7Tests
    {
        public static TheoryData<string?, string, string> ToSafeStringData()
        {
            return new TheoryData<string?, string, string>
                {
                    { null, "", "" },
                    { null, "default", "default" },
                    { "value", "default", "value" }
                };
        }

        [Theory]
        [MemberData(nameof(ToSafeStringData))]
        public void ToSafeString_ShouldReturnExpectedResult(string? value, string defaultValue, string expected)
        {
            var result = value.ToSafeString(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, string?, string?> ToStringOrNullData()
        {
            return new TheoryData<string?, string?, string?>
                {
                    { null, null, null },
                    { null, "default", "default" },
                    { "value", "default", "value" }
                };
        }

        [Theory]
        [MemberData(nameof(ToStringOrNullData))]
        public void ToStringOrNull_ShouldReturnExpectedResult(string? value, string? defaultValue, string? expected)
        {
            var result = value.ToStringOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, char?> TryToCharData()
        {
            return new TheoryData<string?, bool, char?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "a", true, 'a' }
                };
        }

        [Theory]
        [MemberData(nameof(TryToCharData))]
        public void TryToChar_ShouldReturnExpectedResult(string? value, bool expectedSuccess, char? expectedResult)
        {
            var success = value.TryToChar(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, char?, char?> ToCharOrNullData()
        {
            return new TheoryData<string?, char?, char?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "a", null, 'a' }
                };
        }

        [Theory]
        [MemberData(nameof(ToCharOrNullData))]
        public void ToCharOrNull_ShouldReturnExpectedResult(string? value, char? defaultValue, char? expected)
        {
            var result = value.ToCharOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, char, char> ToCharData()
        {
            return new TheoryData<string?, char, char>
                {
                    { null, 'a', 'a' },
                    { " ", 'a', 'a' },
                    { "b", 'a', 'b' }
                };
        }

        [Theory]
        [MemberData(nameof(ToCharData))]
        public void ToChar_ShouldReturnExpectedResult(string? value, char defaultValue, char expected)
        {
            var result = value.ToChar(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<char>?, char> ToCharData2()
        {
            return new TheoryData<string?, Func<char>?, char>
                {
                    { null, ()=>'a', 'a' },
                    { " ", ()=>'a', 'a' },
                    { "b", ()=>'a', 'b' },
                    { null, null, '\0' }
                };
        }

        [Theory]
        [MemberData(nameof(ToCharData2))]
        public void ToChar_ShouldReturnExpectedResult2(string? value, Func<char>? defaultValueFunc, char expected)
        {
            var result = value.ToChar(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, sbyte?> TryToSByteData()
        {
            return new TheoryData<string?, bool, sbyte?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToSByteData))]
        public void TryToSByte_ShouldReturnExpectedResult(string? value, bool expectedSuccess, sbyte? expectedResult)
        {
            var success = value.TryToSByte(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, sbyte?, sbyte?> ToSByteOrNullData()
        {
            return new TheoryData<string?, sbyte?, sbyte?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToSByteOrNullData))]
        public void ToSByteOrNull_ShouldReturnExpectedResult(string? value, sbyte? defaultValue, sbyte? expected)
        {
            var result = value.ToSByteOrNull(defaultValue);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, Func<sbyte?>?, sbyte?> ToSByteOrNullData2()
        {
            return new TheoryData<string?, Func<sbyte?>?, sbyte?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 },
                    {null,()=>0,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToSByteOrNullData2))]
        public void ToSByteOrNull_ShouldReturnExpectedResult2(string? value, Func<sbyte?>? defaultValueFunc, sbyte? expected)
        {
            var result = value.ToSByteOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, sbyte, sbyte> ToSByteData()
        {
            return new TheoryData<string?, sbyte, sbyte>
                {
                    { null, 1, 1 },
                    { " ", 1, 1 },
                    { "123", 1, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToSByteData))]
        public void ToSByte_ShouldReturnExpectedResult(string? value, sbyte defaultValue, sbyte expected)
        {
            var result = value.ToSByte(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<sbyte>?, sbyte> ToSByteData2()
        {
            return new TheoryData<string?, Func<sbyte>?, sbyte>
                {
                    { null, () =>1, 1 },
                    { " ", () =>1, 1 },
                    { "123", () =>1, 123 },
                    { null, null, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToSByteData2))]
        public void ToSByte_ShouldReturnExpectedResult2(string? value, Func<sbyte>? defaultValueFunc, sbyte expected)
        {
            var result = value.ToSByte(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, byte?> TryToByteData()
        {
            return new TheoryData<string?, bool, byte?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToByteData))]
        public void TryToByte_ShouldReturnExpectedResult(string? value, bool expectedSuccess, byte? expectedResult)
        {
            var success = value.TryToByte(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, byte?, byte?> ToByteOrNullData()
        {
            return new TheoryData<string?, byte?, byte?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToByteOrNullData))]
        public void ToByteOrNull_ShouldReturnExpectedResult(string? value, byte? defaultValue, byte? expected)
        {
            var result = value.ToByteOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<byte?>?, byte?> ToByteOrNullData2()
        {
            return new TheoryData<string?, Func<byte?>?, byte?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 },
                    {null,()=>0,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToByteOrNullData2))]
        public void ToByteOrNull_ShouldReturnExpectedResult2(string? value, Func<byte?>? defaultValueFunc, byte? expected)
        {
            var result = value.ToByteOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, byte, byte> ToByteData()
        {
            return new TheoryData<string?, byte, byte>
                {
                    { null, 1, 1 },
                    { " ", 1, 1 },
                    { "123", 1, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToByteData))]
        public void ToByte_ShouldReturnExpectedResult(string? value, byte defaultValue, byte expected)
        {
            var result = value.ToByte(defaultValue);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, Func<byte>?, byte> ToByteData2()
        {
            return new TheoryData<string?, Func<byte>?, byte>
                {
                    { null, () =>1, 1 },
                    { " ", () =>1, 1 },
                    { "123", () =>1, 123 },
                    { null, null, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToByteData2))]
        public void ToByte_ShouldReturnExpectedResult2(string? value, Func<byte>? defaultValueFunc, byte expected)
        {
            var result = value.ToByte(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, ushort?> TryToUShortData()
        {
            return new TheoryData<string?, bool, ushort?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToUShortData))]
        public void TryToUShort_ShouldReturnExpectedResult(string? value, bool expectedSuccess, ushort? expectedResult)
        {
            var success = value.TryToUShort(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, ushort?, ushort?> ToUShortOrNullData()
        {
            return new TheoryData<string?, ushort?, ushort?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToUShortOrNullData))]
        public void ToUShortOrNull_ShouldReturnExpectedResult(string? value, ushort? defaultValue, ushort? expected)
        {
            var result = value.ToUShortOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<ushort?>?, ushort?> ToUShortOrNullData2()
        {
            return new TheoryData<string?, Func<ushort?>?, ushort?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 },
                    {null,()=>0,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToUShortOrNullData2))]
        public void ToUShortOrNull_ShouldReturnExpectedResult2(string? value, Func<ushort?>? defaultValueFunc, ushort? expected)
        {
            var result = value.ToUShortOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, ushort, ushort> ToUShortData()
        {
            return new TheoryData<string?, ushort, ushort>
                {
                    { null, 1, 1 },
                    { " ", 1, 1 },
                    { "123", 1, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToUShortData))]
        public void ToUShort_ShouldReturnExpectedResult(string? value, ushort defaultValue, ushort expected)
        {
            var result = value.ToUShort(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<ushort>?, ushort> ToUShortData2()
        {
            return new TheoryData<string?, Func<ushort>?, ushort>
                {
                    { null, () => 1, 1 },
                    { " ", () => 1, 1 },
                    { "123", () => 1, 123 },
                    {null,null,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToUShortData2))]
        public void ToUShort_ShouldReturnExpectedResult2(string? value, Func<ushort>? defaultValueFunc, ushort expected)
        {
            var result = value.ToUShort(defaultValueFunc);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, bool, short?> TryToShortData()
        {
            return new TheoryData<string?, bool, short?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToShortData))]
        public void TryToShort_ShouldReturnExpectedResult(string? value, bool expectedSuccess, short? expectedResult)
        {
            var success = value.TryToShort(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, short?, short?> ToShortOrNullData()
        {
            return new TheoryData<string?, short?, short?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortOrNullData))]
        public void ToShortOrNull_ShouldReturnExpectedResult(string? value, short? defaultValue, short? expected)
        {
            var result = value.ToShortOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<short?>?, short?> ToShortOrNullData2()
        {
            return new TheoryData<string?, Func<short?>?, short?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 },
                    {null,()=>0,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortOrNullData2))]
        public void ToShortOrNull_ShouldReturnExpectedResult2(string? value, Func<short?>? defaultValueFunc, short? expected)
        {
            var result = value.ToShortOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, short, short> ToShortData()
        {
            return new TheoryData<string?, short, short>
                {
                    { null, 1, 1 },
                    { " ", 1, 1 },
                    { "123", 1, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortData))]
        public void ToShort_ShouldReturnExpectedResult(string? value, short defaultValue, short expected)
        {
            var result = value.ToShort(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<short>?, short> ToShortData2()
        {
            return new TheoryData<string?, Func<short>?, short>
                {
                    { null, () => 1, 1 },
                    { " ", () => 1, 1 },
                    { "123", () => 1, 123 },
                    {null,null,0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToShortData2))]
        public void ToShort_ShouldReturnExpectedResult2(string? value, Func<short>? defaultValue, short expected)
        {
            var result = value.ToShort(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, byte[]?> TryToBytesData()
        {
            return new TheoryData<string?, bool, byte[]?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "test", true, Encoding.UTF8.GetBytes("test") }
                };
        }

        [Theory]
        [MemberData(nameof(TryToBytesData))]
        public void TryToBytes_ShouldReturnExpectedResult(string? value, bool expectedSuccess, byte[]? expectedResult)
        {
            var success = value.TryToBytes(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, byte[]?, byte[]?> ToBytesOrNullData()
        {
            return new TheoryData<string?, byte[]?, byte[]?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "test", null, Encoding.UTF8.GetBytes("test") }
                };
        }

        [Theory]
        [MemberData(nameof(ToBytesOrNullData))]
        public void ToBytesOrNull_ShouldReturnExpectedResult(string? value, byte[]? defaultValue, byte[]? expected)
        {
            var result = value.ToBytesOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, byte[], byte[]> ToBytesData()
        {
            return new TheoryData<string?, byte[], byte[]>
                {
                    { null, Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("default") },
                    { " ", Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("default") },
                    { "test", Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("test") }
                };
        }

        [Theory]
        [MemberData(nameof(ToBytesData))]
        public void ToBytes_ShouldReturnExpectedResult(string? value, byte[] defaultValue, byte[] expected)
        {
            var result = value.ToBytes(defaultValue, Encoding.UTF8);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<byte[]?>?, byte[]?> ToBytesOrNullData2()
        {
            return new TheoryData<string?, Func<byte[]?>?, byte[]?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "test", null, Encoding.UTF8.GetBytes("test") }
                };
        }

        [Theory]
        [MemberData(nameof(ToBytesOrNullData2))]
        public void ToBytesOrNull_ShouldReturnExpectedResult2(string? value, Func<byte[]?>? defaultValueFunc, byte[]? expected)
        {
            var result = value.ToBytesOrNull(defaultValueFunc: defaultValueFunc, encoding: Encoding.UTF8);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<byte[]>?, byte[]> ToBytesData2()
        {
            return new TheoryData<string?, Func<byte[]>?, byte[]>
                {
                    { null,()=> Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("default") },
                    { " ", ()=>Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("default") },
                    { "test", ()=>Encoding.UTF8.GetBytes("default"), Encoding.UTF8.GetBytes("test") },
                    {null,null,[] }
                };
        }

        [Theory]
        [MemberData(nameof(ToBytesData2))]
        public void ToBytes_ShouldReturnExpectedResult2(string? value, Func<byte[]>? defaultValueFunc, byte[] expected)
        {
            var result = value.ToBytes(defaultValueFunc, Encoding.UTF8);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, byte[]> ToBytesData3()
        {
            return new TheoryData<string?, byte[]>
                {
                    { null, [] },
                    { " ",  [] },
                    { "test",  Encoding.UTF8.GetBytes("test") }
                };
        }

        [Theory]
        [MemberData(nameof(ToBytesData3))]
        public void ToBytes_ShouldReturnExpectedResult3(string? value, byte[] expected)
        {
            var result = value.ToBytes(Encoding.UTF8);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, Guid?> TryToGuidData()
        {
            return new TheoryData<string?, bool, Guid?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", true, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(TryToGuidData))]
        public void TryToGuid_ShouldReturnExpectedResult(string? value, bool expectedSuccess, Guid? expectedResult)
        {
            var success = value.TryToGuid(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, Guid?, Guid?> ToGuidOrNullData()
        {
            return new TheoryData<string?, Guid?, Guid?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", null, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidOrNullData))]
        public void ToGuidOrNull_ShouldReturnExpectedResult(string? value, Guid? defaultValue, Guid? expected)
        {
            var result = value.ToGuidOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Guid, Guid> ToGuidData()
        {
            return new TheoryData<string?, Guid, Guid>
                {
                    { null, Guid.Empty, Guid.Empty },
                    { " ", Guid.Empty, Guid.Empty },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", Guid.Empty, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData))]
        public void ToGuid_ShouldReturnExpectedResult(string? value, Guid defaultValue, Guid expected)
        {
            var result = value.ToGuid(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<Guid?>?, Guid?> ToGuidOrNullData2()
        {
            return new TheoryData<string?, Func<Guid?>?, Guid?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", null, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidOrNullData2))]
        public void ToGuidOrNull_ShouldReturnExpectedResult2(string? value, Func<Guid?>? defaultValueFunc, Guid? expected)
        {
            var result = value.ToGuidOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<Guid>?, Guid> ToGuidData2()
        {
            return new TheoryData<string?, Func<Guid>?, Guid>
                {
                    { null,()=> Guid.Empty, Guid.Empty },
                    { " ", ()=> Guid.Empty, Guid.Empty },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", ()=> Guid.Empty, Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") },
                {null,null, Guid.Empty}
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData2))]
        public void ToGuid_ShouldReturnExpectedResult2(string? value, Func<Guid>? defaultValueFunc, Guid expected)
        {
            var result = value.ToGuid(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Guid> ToGuidData3()
        {
            return new TheoryData<string?, Guid>
                {
                    { null, Guid.Empty },
                    { " ",  Guid.Empty },
                    { "d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", Guid.Parse("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a") }
                };
        }

        [Theory]
        [MemberData(nameof(ToGuidData3))]
        public void ToGuid_ShouldReturnExpectedResult3(string? value, Guid expected)
        {
            var result = value.ToGuid();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Stream> ToStreamData()
        {
            return new TheoryData<string?, Stream>
                {
                    { null, Stream.Null },
                    { " ", Stream.Null },
                    { "test", new MemoryStream(Encoding.UTF8.GetBytes("test")) }
                };
        }

        [Theory]
        [MemberData(nameof(ToStreamData))]
        public void ToStream_ShouldReturnExpectedResult(string? value, Stream expected)
        {
            var result = value.ToStream();
            Assert.True(StreamContentsAreEqual(expected, result));
        }

        private bool StreamContentsAreEqual(Stream expected, Stream actual)
        {
            if (expected.Length != actual.Length)
                return false;

            expected.Position = 0;
            actual.Position = 0;

            for (var i = 0; i < expected.Length; i++)
            {
                if (expected.ReadByte() != actual.ReadByte())
                    return false;
            }

            return true;
        }
    }
}
