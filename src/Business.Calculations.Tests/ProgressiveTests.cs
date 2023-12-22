using Business.Calculations;
using Fluent.Calculations.Primitives.BaseTypes;
using FluentAssertions;

namespace Fluent.Calculations.Business.Tests
{
    public class ProgressiveTests
    {
        [Fact]
        public void Test()
        {
            var progressive = new Progressive<decimal, decimal>()
                .UpTo(100).MultiplyBy(0.01m)
                .UpTo(200).MultiplyBy(0.02m)
                .UpTo(300).MultiplyBy(0.03m)
                .ReminderMultiplier(0.04m);

            var result = progressive.Calculate(400);

            result.Amount.Should().Be(10);
        }

        [Fact]
        public void Test2()
        {
            var progressive = new Progressive<Number, Number>()
                .UpTo(100).MultiplyBy(0.01m)
                .UpTo(200).MultiplyBy(0.02m)
                .UpTo(300).MultiplyBy(0.03m)
                .Capped();

            var result = progressive.Calculate(400);

            result.Amount.Primitive.Should().Be(6);
        }
    }
}
