using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class SpatialAnalysisTests
    {
        [Fact]
        public void when_there_are_no_claims_each_detection_is_a_separate_line()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();
            
            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
            };

            var spatialClaims = SpatialClaims(
                new RowClaim<TestDetection>[0],
                new Column<TestDetection>[0]);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1),
                Line(2),
                Line(3)
            });
        }

        [Fact]
        public void detections_with_row_claim_are_merged_into_one_line()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();
            
            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
            };

            var rowClaims = new[]
            {
                RowClaim(1, 2, 10)
            };

            var spatialClaims = SpatialClaims(
                rowClaims,
                new Column<TestDetection>[0]);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1, 2),
                Line(3)
            });
        }

        [Fact]
        public void more_than_two_detections_with_row_claim_are_merged_into_one_line()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();

            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
            };

            var rowClaims = new[]
            {
                RowClaim(1, 2, 10),
                RowClaim(1, 3, 10),
                RowClaim(2, 3, 10),
            };

            var spatialClaims = SpatialClaims(
                rowClaims,
                new Column<TestDetection>[0]);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1, 2, 3)
            });
        }

        [Fact]
        public void when_all_items_are_in_different_columns_lines_can_be_merged_only_by_a_link_of_one_element()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();

            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
                Detection(4),
                Detection(5),
                Detection(6),
            };

            var rowClaims = new[]
            {
                RowClaim(1, 3, 10),
                RowClaim(2, 3, 15),
                RowClaim(4, 6, 15),
                RowClaim(5, 6, 10),
            };

            var spatialClaims = SpatialClaims(
                rowClaims,
                new Column<TestDetection>[0]);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1, 2, 3),
                Line(4, 5, 6)
            });
        }

        [Fact]
        public void when_some_of_items_are_in_the_same_columns_conflicted_claims_are_resolved_by_strength()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();

            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
                Detection(4),
                Detection(5),
                Detection(6),
            };

            var rowClaims = new[]
            {
                RowClaim(1, 3, 10),
                RowClaim(2, 3, 15),
                RowClaim(4, 6, 15),
                RowClaim(5, 6, 10),
            };

            var columns = new[]
            {
                Column(1, 2),
                Column(4, 5)
            };

            var spatialClaims = SpatialClaims(
                rowClaims,
                columns);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1),
                Line(2, 3),
                Line(4, 6),
                Line(5)
            });
        }

        //[Fact] //todo not sure how to solve this problem - but big enough margin for too weak claims seems to solve it in real life scenarios
        public void conflicts_are_propagated_through_dependent_elements()
        {
            //given
            var resolver = new SpatialClaimsResolver<TestDetection>();

            var detections = new[]
            {
                Detection(1),
                Detection(2),
                Detection(3),
                Detection(4),
                Detection(5),
                Detection(6),
            };

            var rowClaims = new[]
            {
                RowClaim(1, 2, 10),
                RowClaim(1, 4, 5),
                RowClaim(1, 5, 5),
                RowClaim(1, 6, 5),
                RowClaim(2, 3, 10),
                RowClaim(2, 4, 5),
                RowClaim(2, 5, 5),
                RowClaim(2, 6, 5),
                RowClaim(4, 5, 10),
                RowClaim(4, 1, 5),
                RowClaim(4, 2, 5),
                RowClaim(4, 3, 5),
                RowClaim(5, 6, 10),
                RowClaim(5, 1, 5),
                RowClaim(5, 2, 5),
                RowClaim(5, 3, 5),
            };

            var columns = new[]
            {
                Column(1, 4),
                Column(2, 5),
                Column(3, 6)
            };

            var spatialClaims = SpatialClaims(
                rowClaims,
                columns);

            //when
            var lines = resolver.Resolve(
                detections,
                spatialClaims);

            //then
            lines.Should().BeEquivalentTo(new[]
            {
                Line(1, 2, 3),
                Line(4, 5, 6),
            });
        }

        private static SpatialClaims<TestDetection> SpatialClaims(
            RowClaim<TestDetection>[] rowClaims,
            Column<TestDetection>[] columns)
        {
            return new SpatialClaims<TestDetection>(rowClaims, columns);
        }

        private static Column<TestDetection> Column(params int[] items)
        {
            return new Column<TestDetection>(
                items.Select(Detection).ToArray());
        }

        private static RowClaim<TestDetection> RowClaim(int first, int second, int strenght)
        {
            return new RowClaim<TestDetection>(
                new Pair<TestDetection>(
                    Detection(first),
                    Detection(second)),
                strenght);
        }

        private static Line<TestDetection> Line(params int[] ids)
        {
            return new Line<TestDetection>(
                ids.Select(Detection).ToArray());
        }

        private static TestDetection Detection(int id)
        {
            return new TestDetection
            {
                Id = id
            };
        }
        
        public class TestDetection: IIdentifiable
        {
            public int Id { get; set; }

            public override string ToString()
            {
                return Id.ToString();
            }
        }
    }
}
