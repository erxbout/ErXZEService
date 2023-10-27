using ErXZEService.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ErXZEService.UnitTest
{
    [TestClass]
    public class RemainingChargeTimeCalculatorTest
    {
        [TestMethod]
        public void TestRemainingChargeTimeCalculator_Timespan()
        {
            var calculatedWithTargetAndCurrentKwhSame = RemainingChargeTimeCalculator.CalculateTimeSpan(10, 10, 1);
            var calculatedMostCaseScenario = RemainingChargeTimeCalculator.CalculateTimeSpan(10, 1, 10);

            var argumentExceptionThrown = false;

            try
            {
                var calculated = RemainingChargeTimeCalculator.CalculateTimeSpan(10, 1, 0);
            }
            catch (ArgumentException e)
            {
                argumentExceptionThrown = true;
            }
            catch (Exception e)
            {
                Assert.Fail("Unsupported Exception thrown: ", e);
            }

            calculatedWithTargetAndCurrentKwhSame.Should().Be(TimeSpan.Zero);
            calculatedMostCaseScenario.Should().Be(TimeSpan.FromMinutes(54));

            argumentExceptionThrown.Should().BeTrue();
        }

        [TestMethod]
        public void TestRemainingChargeTimeCalculator_Progress()
        {
            var regularProgress = RemainingChargeTimeCalculator.CalculateProgress(10, 2.6922m);
            var targetExceeded = RemainingChargeTimeCalculator.CalculateProgress(10, 11);
            var noTarget = RemainingChargeTimeCalculator.CalculateProgress(0, 11);
            var noCurrent = RemainingChargeTimeCalculator.CalculateProgress(11, 0);

            regularProgress.Should().Be(26.92M);
            targetExceeded.Should().Be(1);
            noTarget.Should().Be(1);
            noCurrent.Should().Be(0);
        }
    }
}
