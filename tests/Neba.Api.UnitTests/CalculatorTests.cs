using Neba.Api;

namespace Neba.Api.UnitTests;

public class CalculatorTests
    {
        [Fact]
        public void Add_ShouldReturnSumOfTwoNumbers()
        {
            var calculator = new Calculator();
            
            var result = calculator.Add(1, 2);
            
            result.Should().Be(3);
        }
    }