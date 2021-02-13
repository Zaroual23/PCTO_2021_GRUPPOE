using System;
using Xunit;

namespace XUnitTestProject
{
    public class UnitTest1
    {
        public int somma(int x,int y)
        {
            return x + y;
        }

        [Fact]
        public void Testprova()
        {
            Assert.Equal(4, somma(1, 3));
        }

        [Fact]
        public void TestMappa()
        {

        }

        [Fact]
        public void TestAPI()
        {

        }

        [Fact]
        public void TestDLL()
        {

        }
    }
}
