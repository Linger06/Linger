namespace Linger.UnitTests.Helper
{
    public class GuidCodeTests
    {
        [Fact]
        public void NewId_ShouldReturn32CharacterString()
        {
            // Act
            var result = GuidCode.NewId;

            // Assert
            Assert.Equal(31, result.Length);
            Assert.Matches(@"^\d{21}[a-fA-F0-9]{10}$", result);
        }

        [Fact]
        public void NewDateTimeId_ShouldReturn23CharacterString()
        {
            // Act
            var result = GuidCode.NewDateTimeId;

            // Assert
            Assert.Equal(21, result.Length);
            Assert.Matches(@"^\d{21}$", result);
        }

        [Fact]
        public void NewDateGuid_ShouldReturn10CharacterString()
        {
            // Act
            var result = GuidCode.NewDateGuid;

            // Assert
            Assert.Equal(10, result.Length);
            Assert.Matches(@"^\d{6}[a-fA-F0-9]{4}$", result);
        }

        [Fact]
        public void NewGuid_ShouldReturnValidGuid()
        {
            // Act
            Guid result = GuidCode.NewGuid();

            // Assert
            Assert.NotEqual(Guid.Empty, result);
        }

#if NET9_0_OR_GREATER
        [Fact]
        public void CreateVersion7_ShouldReturnValidGuidV7()
        {
            // Act
            Guid result = GuidCode.CreateVersion7();

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.Equal(7, (result.ToByteArray()[7] & 0xF0) >> 4);
        }
#endif

        [Fact]
        public void GetInt64UniqueCode_ShouldReturnNonZeroValue()
        {
            // Act
            var result = GuidCode.GetInt64UniqueCode();

            // Assert
            Assert.NotEqual(0, result);
        }

        [Fact]
        public void GetInt32UniqueCode_ShouldReturnNonZeroValue()
        {
            // Act
            var result = GuidCode.GetInt32UniqueCode();

            // Assert
            Assert.NotEqual(0, result);
        }

        [Fact]
        public void MultipleNewIds_ShouldBeUnique()
        {
            // Act
            var id1 = GuidCode.NewId;
            var id2 = GuidCode.NewId;

            // Assert
            Assert.NotEqual(id1, id2);
        }
    }
}
