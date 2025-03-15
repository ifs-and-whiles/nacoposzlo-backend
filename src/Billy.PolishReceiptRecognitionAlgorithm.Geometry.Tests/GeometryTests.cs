using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry.Tests
{
    public class GeometryTests
    {
        [Theory]
        [InlineData(1.0, 1.0, 3.0, 2.0, 2.0, 1.0)]
        [InlineData(1.0, 0, -1.0, 3.0, -2.0, 3.0)]
        public void can_get_vector_between_two_points(
            float ax, float ay, float bx, float by, float expectedX, float expectedY)
        {
            Geometry
                .Vector(
                    @from: new Point(ax, ay),
                    to: new Point(bx, by))
                .Should()
                .Be(new Vector2(expectedX, expectedY));
        }

        [Theory]
        [InlineData(3.0, 2.0, 1.0, 1.0, 2.0, 1.5)]
        [InlineData(1.0, 1.0, 3.0, 2.0, 2.0, 1.5)]
        [InlineData(-1.0, 3.0, 1.0, 0, 0, 1.5)]
        [InlineData(1.0, 0, -1.0, 3.0, 0, 1.5)]
        public void can_get_point_in_the_middle(
            float ax, float ay, float bx, float by, float expectedX, float expectedY)
        {
            Geometry
                .PointInTheMiddle(
                    first: new Point(ax, ay),
                    second: new Point(bx, by))
                .Should()
                .Be(new Point(expectedX, expectedY));
        }
    }
}