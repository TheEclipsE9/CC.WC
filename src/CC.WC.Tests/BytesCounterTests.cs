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
    }
}