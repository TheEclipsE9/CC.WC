using System.Runtime.Intrinsics.X86;
using System.Xml.Serialization;
using CC.WC.App;

namespace CC.WC.Tests
{
    public class BytesCounterTests
    {
        [Fact]
        public void Empty_Buffer_Should_Return_Zero()
        {
            //Arranger
            var sut = new BytesCounter();

            byte[] buffer = new byte[0];
            int readedCount = 0;
            //Act
            var result = sut.Count(buffer, readedCount);

            //Assert
            Assert.Equal(readedCount, result);
        }

        [Theory]
        [InlineData("Test.txt", 342190)]
        public void Read_from_file_return_expected_count(string fileName, int expectedByteCount)
        {
            //Arrange
            var stream = File.OpenRead(fileName);
            
            var sut = new BytesCounter();
            var context = new Context();
            context.SetCounter(sut);

            //Act
            var result = context.Count(stream);

            //Assert
            Assert.Equal(expectedByteCount, result);
        }
    }
}