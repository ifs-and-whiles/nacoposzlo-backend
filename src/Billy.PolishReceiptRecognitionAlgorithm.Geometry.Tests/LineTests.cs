using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Geometry.Tests
{
    public class LineTests
    {
        [Theory]
        [InlineData(0,2,1,0, 1.414213562373095)]
        [InlineData(2,2,1,0,0)]
        public void can_measure_distance_to_point_from_not_vertical_line(
            int x, int y, int a, int b, double expectedDistance)
        {
            //given
            var point = new Point(x, y);
            var line = Line.FromParameters(a, b);

            //when
            var result = line.DistanceTo(point);

            //then
            result.Should().BeApproximately(expectedDistance, Double.Tolerance);
        }

        [Theory]
        [InlineData(0, 2, 0, 0)]
        [InlineData(2, 2, 4, 2)]
        public void can_measure_distance_to_point_from_vertical_line(
            int x, int y, int vx, double expectedDistance)
        {
            //given
            var point = new Point(x, y);
            var line = Line.Vertical(vx);

            //when
            var result = line.DistanceTo(point);

            //then
            result.Should().BeApproximately(expectedDistance, Double.Tolerance);
        }

        [Fact]
        public void can_intersect_vertical_and_horizontal_lines()
        {
            //given
            var vertical = Line.Vertical(0);
            var horizontal = Line.FromParameters(0, 0);

            //when
            var intersection = vertical.TryIntersect(horizontal);

            //then
            intersection.Match(
                point => point.Should().BeEquivalentTo(new Point(0, 0)),
                linesRelation => throw new WrongResultException(
                    "Lines should intersect"));
        }

        [Fact]
        public void can_intersect_two_lines()
        {
            //given
            var first = Line.FromParameters(-1, 1);
            var second = Line.FromParameters(1, 1);

            //when
            var intersection = first.TryIntersect(second);

            //then
            intersection.Match(
                point => point.Should().BeEquivalentTo(new Point(0, 1)),
                linesRelation => throw new WrongResultException(
                    "Lines should intersect"));
        }

        [Fact]
        public void two_different_vertical_lines_cannot_be_intersected_because_the_are_parallel()
        {
            //given
            var first = Line.Vertical(0);
            var second = Line.Vertical(1);

            //when
            var intersection = first.TryIntersect(second);

            //then
            intersection.Match(
                point => throw new WrongResultException(
                    "Lines should not intersect"),
                linesRelation => linesRelation.Should().Be(LinesRelation.ParallelLines));
        }

        [Fact]
        public void two_same_vertical_lines_does_not_intersect()
        {
            //given
            var first = Line.Vertical(0);
            var second = Line.Vertical(0);

            //when
            var intersection = first.TryIntersect(second);

            //then
            intersection.Match(
                point => throw new WrongResultException(
                    "Lines should not intersect"),
                linesRelation => linesRelation.Should().Be(LinesRelation.SameLine));
        }

        [Fact]
        public void two_parallel_lines_dont_intersect()
        {
            //given
            var first = Line.FromParameters(1, 0);
            var second = Line.FromParameters(1, 1);

            //when
            var intersection = first.TryIntersect(second);

            //then
            intersection.Match(
                point => throw new WrongResultException(
                    "Lines should not intersect"),
                linesRelation => linesRelation.Should().Be(LinesRelation.ParallelLines));
        }

        [Fact]
        public void two_lines_with_same_parameters_dont_intersect()
        {
            //given
            var first = Line.FromParameters(1,1);
            var second = Line.FromParameters(1,1);

            //when
            var intersection = first.TryIntersect(second);

            //then
            intersection.Match(
                point => throw new WrongResultException(
                    "Lines should not intersect"),
                linesRelation => linesRelation.Should().Be(LinesRelation.SameLine));
        }

        [Fact]
        public void can_get_perpendicular_line_to_not_vertical_line()
        {
            //given
            var line = Line.FromParameters(1, 0);
            var point = new Point(4, 2);

            //when
            var perpendicular = line.GetPerpendicularLine(point);
            
            //then
            perpendicular.Should().BeEquivalentTo(
                Line.FromParameters(-1, 6), 
                options => options.IncludingAllRuntimeProperties());
        }

        [Fact]
        public void can_get_perpendicular_line_to_horizontal_line()
        {
            //given
            var line = Line.FromParameters(0, 0);
            var point = new Point(4, 2);

            //when
            var perpendicular = line.GetPerpendicularLine(point);

            //then
            perpendicular.Should().BeEquivalentTo(
                Line.Vertical(4),
                options => options.IncludingAllRuntimeProperties());
        }

        [Fact]
        public void can_get_perpendicular_line_to_vertical_line()
        {
            //given
            var line = Line.Vertical(0);
            var point = new Point(4, 2);

            //when
            var perpendicular = line.GetPerpendicularLine(point);

            //then
            perpendicular.Should().BeEquivalentTo(
                Line.FromParameters(0, 2),
                options => options.IncludingAllRuntimeProperties());
        }

        [Fact]
        public void can_project_point_on_vertical_line()
        {
            //given
            var line = Line.Vertical(2);
            var point = new Point(3, 3);

            //when
            var projectedPoint = line.GetProjectionOf(point);

            //then
            projectedPoint.Should().Be(new Point(2, 3));
        }

        [Fact]
        public void can_project_point_on_not_vertical_line()
        {
            //given
            var line = Line.FromParameters(1, 0);
            var point = new Point(2, 0);

            //when
            var projectedPoint = line.GetProjectionOf(point);

            //then
            projectedPoint.Should().Be(new Point(1, 1));
        }

        [Fact]
        public void projection_of_the_point_which_lies_on_a_given_line_should_be_the_point_itself()
        {
            //given
            var line = Line.FromParameters(1, 0);
            var point = new Point(1, 1);

            //when
            var projectedPoint = line.GetProjectionOf(point);

            //then
            projectedPoint.Should().Be(point);
        }
    }
}
