namespace Linger.UnitTests.Extensions.Core
{
    public partial class StringExtensionsTests
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

        public static TheoryData<string?, bool, int?> TryToIntData()
        {
            return new TheoryData<string?, bool, int?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToIntData))]
        public void TryToInt_ShouldReturnExpectedResult(string? value, bool expectedSuccess, int? expectedResult)
        {
            var success = value.TryToInt(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, int?, int?> ToIntOrNullData()
        {
            return new TheoryData<string?, int?, int?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrNullData))]
        public void ToIntOrNull_ShouldReturnExpectedResult(string? value, int? defaultValue, int? expected)
        {
            var result = value.ToIntOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, int, int> ToIntData()
        {
            return new TheoryData<string?, int, int>
                {
                    { null, 0, 0 },
                    { " ", 0, 0 },
                    { "123", 0, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntData))]
        public void ToInt_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
            var result = value.ToInt(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<int?>?, int?> ToIntOrNullData2()
        {
            return new TheoryData<string?, Func<int?>?, int?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrNullData2))]
        public void ToIntOrNull_ShouldReturnExpectedResult2(string? value, Func<int?>? defaultValueFunc, int? expected)
        {
            var result = value.ToIntOrNull(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<int>?, int> ToIntData2()
        {
            return new TheoryData<string?, Func<int>?, int>
                {
                    { null, ()=>0, 0 },
                    { " ", ()=>0, 0 },
                    { "123",()=> 0, 123 },
                    { null,null, 0 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntData2))]
        public void ToInt_ShouldReturnExpectedResult2(string? value, Func<int>? defaultValueFunc, int expected)
        {
            var result = value.ToInt(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        // New ToIntOrDefault tests
        public static TheoryData<string?, int, int> ToIntOrDefaultData()
        {
            return new TheoryData<string?, int, int>
                {
                    { null, 0, 0 },
                    { " ", 0, 0 },
                    { "123", 0, 123 },
                    { "abc", 42, 42 },
                    { "123", 999, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToIntOrDefault_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
            var result = value.ToIntOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, Func<int>?, int> ToIntOrDefaultFuncData()
        {
            return new TheoryData<string?, Func<int>?, int>
                {
                    { null, null, 0 },
                    { " ", null, 0 },
                    { "123", null, 123 },
                    { "abc", () => 42, 42 },
                    { "123", () => 999, 123 }
                };
        }

        [Theory]
        [MemberData(nameof(ToIntOrDefaultFuncData))]
        public void ToIntOrDefault_WithFunction_ShouldReturnExpectedResult(string? value, Func<int>? defaultValueFunc, int expected)
        {
            var result = value.ToIntOrDefault(defaultValueFunc);
            Assert.Equal(expected, result);
        }

        // Test backward compatibility - ensure old method still works
        [Theory]
        [MemberData(nameof(ToIntOrDefaultData))]
        public void ToInt_BackwardCompatibility_ShouldReturnExpectedResult(string? value, int defaultValue, int expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToInt(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, long?> TryToLongData()
        {
            return new TheoryData<string?, bool, long?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123", true, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(TryToLongData))]
        public void TryToLong_ShouldReturnExpectedResult(string? value, bool expectedSuccess, long? expectedResult)
        {
            var success = value.TryToLong(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, long?, long?> ToLongOrNullData()
        {
            return new TheoryData<string?, long?, long?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "123", null, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongOrNullData))]
        public void ToLongOrNull_ShouldReturnExpectedResult(string? value, long? defaultValue, long? expected)
        {
            var result = value.ToLongOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, long, long> ToLongData()
        {
            return new TheoryData<string?, long, long>
                {
                    { null, 0L, 0L },
                    { " ", 0L, 0L },
                    { "123", 0L, 123L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongData))]
        public void ToLong_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
            var result = value.ToLong(defaultValue);
            Assert.Equal(expected, result);
        }

        // New ToLongOrDefault tests
        public static TheoryData<string?, long, long> ToLongOrDefaultData()
        {
            return new TheoryData<string?, long, long>
                {
                    { null, 0L, 0L },
                    { " ", 0L, 0L },
                    { "123", 0L, 123L },
                    { "abc", 42L, 42L },
                    { "9223372036854775807", 0L, 9223372036854775807L }
                };
        }

        [Theory]
        [MemberData(nameof(ToLongOrDefaultData))]
        public void ToLongOrDefault_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
            var result = value.ToLongOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ToLongOrDefaultData))]
        public void ToLong_BackwardCompatibility_ShouldReturnExpectedResult(string? value, long defaultValue, long expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToLong(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, decimal?> TryToDecimalData()
        {
            return new TheoryData<string?, bool, decimal?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45m }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDecimalData))]
        public void TryToDecimal_ShouldReturnExpectedResult(string? value, bool expectedSuccess, decimal? expectedResult)
        {
            var success = value.TryToDecimal(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, decimal?, int?, decimal?> ToDecimalOrNullData()
        {
            return new TheoryData<string?, decimal?, int?, decimal?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45m },
                    { "123.456", null, 2, 123.46m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalOrNullData))]
        public void ToDecimalOrNull_ShouldReturnExpectedResult(string? value, decimal? defaultValue, int? digits, decimal? expected)
        {
            var result = value.ToDecimalOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, decimal, int?, decimal> ToDecimalData()
        {
            return new TheoryData<string?, decimal, int?, decimal>
                {
                    { null, 0m, null, 0m },
                    { " ", 0m, null, 0m },
                    { "123.45", 0m, null, 123.45m },
                    { "123.456", 0m, 2, 123.46m }
                };
        }

        [Theory]
        [MemberData(nameof(ToDecimalData))]
        public void ToDecimal_ShouldReturnExpectedResult(string? value, decimal defaultValue, int? digits, decimal expected)
        {
            var result = value.ToDecimal(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, float?> TryToFloatData()
        {
            return new TheoryData<string?, bool, float?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45f }
                };
        }

        [Theory]
        [MemberData(nameof(TryToFloatData))]
        public void TryToFloat_ShouldReturnExpectedResult(string? value, bool expectedSuccess, float? expectedResult)
        {
            var success = value.TryToFloat(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, float?, int?, float?> ToFloatOrNullData()
        {
            return new TheoryData<string?, float?, int?, float?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45f },
                    { "123.456", null, 2, 123.46f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatOrNullData))]
        public void ToFloatOrNull_ShouldReturnExpectedResult(string? value, float? defaultValue, int? digits, float? expected)
        {
            var result = value.ToFloatOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, float, int?, float> ToFloatData()
        {
            return new TheoryData<string?, float, int?, float>
                {
                    { null, 0f, null, 0f },
                    { " ", 0f, null, 0f },
                    { "123.45", 0f, null, 123.45f },
                    { "123.456", 0f, 2, 123.46f }
                };
        }

        [Theory]
        [MemberData(nameof(ToFloatData))]
        public void ToFloat_ShouldReturnExpectedResult(string? value, float defaultValue, int? digits, float expected)
        {
            var result = value.ToFloat(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, double?> TryToDoubleData()
        {
            return new TheoryData<string?, bool, double?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "123.45", true, 123.45 }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDoubleData))]
        public void TryToDouble_ShouldReturnExpectedResult(string? value, bool expectedSuccess, double? expectedResult)
        {
            var success = value.TryToDouble(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, double?, int?, double?> ToDoubleOrNullData()
        {
            return new TheoryData<string?, double?, int?, double?>
                {
                    { null, null, null, null },
                    { " ", null, null, null },
                    { "123.45", null, null, 123.45 },
                    { "123.456", null, 2, 123.46 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleOrNullData))]
        public void ToDoubleOrNull_ShouldReturnExpectedResult(string? value, double? defaultValue, int? digits, double? expected)
        {
            var result = value.ToDoubleOrNull(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, double, int?, double> ToDoubleData()
        {
            return new TheoryData<string?, double, int?, double>
                {
                    { null, 0.0, null, 0.0 },
                    { " ", 0.0, null, 0.0 },
                    { "123.45", 0.0, null, 123.45 },
                    { "123.456", 0.0, 2, 123.46 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleData))]
        public void ToDouble_ShouldReturnExpectedResult(string? value, double defaultValue, int? digits, double expected)
        {
            var result = value.ToDouble(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        // New ToDoubleOrDefault tests
        public static TheoryData<string?, double, int?, double> ToDoubleOrDefaultData()
        {
            return new TheoryData<string?, double, int?, double>
                {
                    { null, 0.0, null, 0.0 },
                    { " ", 0.0, null, 0.0 },
                    { "123.456", 0.0, null, 123.456 },
                    { "123.456", 0.0, 2, 123.46 },
                    { "abc", 42.5, null, 42.5 }
                };
        }

        [Theory]
        [MemberData(nameof(ToDoubleOrDefaultData))]
        public void ToDoubleOrDefault_ShouldReturnExpectedResult(string? value, double defaultValue, int? digits, double expected)
        {
            var result = value.ToDoubleOrDefault(defaultValue, digits);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ToDoubleOrDefaultData))]
        public void ToDouble_BackwardCompatibility_ShouldReturnExpectedResult(string? value, double defaultValue, int? digits, double expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToDouble(defaultValue, digits);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, DateTime?> TryToDateTimeData()
        {
            return new TheoryData<string?, bool, DateTime?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "2023-01-01", true, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(TryToDateTimeData))]
        public void TryToDateTime_ShouldReturnExpectedResult(string? value, bool expectedSuccess, DateTime? expectedResult)
        {
            var success = value.TryToDateTime(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, DateTime?, DateTime?> ToDateTimeOrNullData()
        {
            return new TheoryData<string?, DateTime?, DateTime?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "2023-01-01", null, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeOrNullData))]
        public void ToDateTimeOrNull_ShouldReturnExpectedResult(string? value, DateTime? defaultValue, DateTime? expected)
        {
            var result = value.ToDateTimeOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, DateTime, DateTime> ToDateTimeData()
        {
            return new TheoryData<string?, DateTime, DateTime>
                {
                    { null, DateTime.MinValue, DateTime.MinValue },
                    { " ", DateTime.MinValue, DateTime.MinValue },
                    { "2023-01-01", DateTime.MinValue, new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData))]
        public void ToDateTime_ShouldReturnExpectedResult(string? value, DateTime defaultValue, DateTime expected)
        {
            var result = value.ToDateTime(defaultValue);
            Assert.Equal(expected, result);
        }


        public static TheoryData<string?, DateTime> ToDateTimeData2()
        {
            return new TheoryData<string?, DateTime>
                {
                    { null,  DateTime.MinValue },
                    { " ",  DateTime.MinValue },
                    { "2023-01-01",  new DateTime(2023, 1, 1) }
                };
        }

        [Theory]
        [MemberData(nameof(ToDateTimeData2))]
        public void ToDateTime_ShouldReturnExpectedResult2(string? value, DateTime expected)
        {
            var result = value.ToDateTime();
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, bool?> TryToBoolData()
        {
            return new TheoryData<string?, bool, bool?>
                {
                    { null, false, null },
                    { " ", false, null },
                    { "true", true, true },
                    { "false", true, false },
                    { "1", true, true },
                    { "0", true, false }
                };
        }

        [Theory]
        [MemberData(nameof(TryToBoolData))]
        public void TryToBool_ShouldReturnExpectedResult(string? value, bool expectedSuccess, bool? expectedResult)
        {
            var success = value.TryToBool(out var result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<string?, bool?, bool?> ToBoolOrNullData()
        {
            return new TheoryData<string?, bool?, bool?>
                {
                    { null, null, null },
                    { " ", null, null },
                    { "true", null, true },
                    { "false", null, false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolOrNullData))]
        public void ToBoolOrNull_ShouldReturnExpectedResult(string? value, bool? defaultValue, bool? expected)
        {
            var result = value.ToBoolOrNull(defaultValue);
            Assert.Equal(expected, result);
        }

        public static TheoryData<string?, bool, bool> ToBoolData()
        {
            return new TheoryData<string?, bool, bool>
                {
                    { null, false, false },
                    { " ", false, false },
                    { "true", false, true },
                    { "false", false, false }
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolData))]
        public void ToBool_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
            var result = value.ToBool(defaultValue);
            Assert.Equal(expected, result);
        }

        // New ToBoolOrDefault tests
        public static TheoryData<string?, bool, bool> ToBoolOrDefaultData()
        {
            return new TheoryData<string?, bool, bool>
                {
                    { null, false, false },
                    { " ", false, false },
                    { "true", false, true },
                    { "false", true, false },
                    { "1", false, true },
                    { "0", true, false },
                    { "yes", false, true },
                    { "no", true, false },
                    { "success", false, true },
                    { "fail", true, false },
                    { "abc", true, true } // Default when conversion fails
                };
        }

        [Theory]
        [MemberData(nameof(ToBoolOrDefaultData))]
        public void ToBoolOrDefault_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
            var result = value.ToBoolOrDefault(defaultValue);
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ToBoolOrDefaultData))]
        public void ToBool_BackwardCompatibility_ShouldReturnExpectedResult(string? value, bool defaultValue, bool expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = value.ToBool(defaultValue);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(expected, result);
        }
    }
}
