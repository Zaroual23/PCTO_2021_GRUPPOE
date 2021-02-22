using System;
using Xunit;


namespace XUnitTestProject1
{
    public class UnitTest1
    {
        public int somma(int x, int y)
        {
            return x + y;
        }
        [Fact]
        public void Testprova()
        {
            Assert.Equal(4, somma(3, 1));
        }
        [Fact]
        public void Testprova2()
        {
            Assert.Equal(5, somma(3, 1));
        }
        [Fact]
        public void TestAPI()
        {

        }
        [Fact]
        public void TestMappa()
        {

        }
        [Fact]
        public void TestDLL()
        {

        }
    }
}
