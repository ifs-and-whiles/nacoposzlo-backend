using AutoFixture;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class ReceiptSectionsParserTests
    {
        public  Fixture Fixture { get; }

        public ReceiptSectionsParserTests()
        {
            Fixture = new Fixture();
        }

        [Fact]
        public void can_parse_sections_of_perfectly_scanned_receipt()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4]
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5]
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[6],
                    lines[7],
                    lines[8],
                }), 
                new ReceiptSections.AmountSection(
                    lines[9]), 
                new ReceiptSections.FooterSection(new[]
                {
                    lines[10]
                })));
        }

        [Fact]
        public void can_parse_sections_when_paragon_fiskalny_line_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[4]
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[5],
                    lines[6],
                    lines[7],
                }),
                new ReceiptSections.AmountSection(
                    lines[8]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[9]
                })));
        }

        [Fact]
        public void can_parse_sections_when_paragon_fiskalny_and_header_date_lines_are_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[3]
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[4],
                    lines[5],
                    lines[6],
                }),
                new ReceiptSections.AmountSection(
                    lines[7]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[8]
                })));
        }

        [Fact]
        public void can_parse_sections_when_paragon_fiskalny_and_header_date_and_tax_number_lines_are_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[1],
                    lines[2],
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[3],
                    lines[4],
                    lines[5],
                }),
                new ReceiptSections.AmountSection(
                    lines[6]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[7],
                })));
        }

        [Fact]
        public void can_parse_sections_when_receipt_amount_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[6],
                    lines[7],
                    lines[8],
                }),
                new ReceiptSections.AmountSection(
                    null),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[9],
                })));
        }

        [Fact]
        public void can_parse_sections_when_receipt_amount_and_tax_sum_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                }),
                new ReceiptSections.TaxesSection(
                    new ReceiptLine[0]),
                new ReceiptSections.AmountSection(
                    null),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[6],
                    lines[7],
                    lines[8],
                })));
        }

        [Fact]
        public void can_parse_sections_when_receipt_amount_and_tax_sum_and_tax_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Kwota A 23,00% 1,28"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                    lines[6],
                    lines[7],
                }),
                new ReceiptSections.TaxesSection(
                    new ReceiptLine[0]),
                new ReceiptSections.AmountSection(
                    null),
                new ReceiptSections.FooterSection(
                    new ReceiptLine[0])));
        }

        [Fact]
        public void can_parse_sections_when_tax_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Kwota A 23,00% 1,28"),
                Line("Podatek PTU 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                    lines[6],
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[7],
                }),
                new ReceiptSections.AmountSection(
                    lines[8]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[9],
                })));
        }

        [Fact]
        public void can_parse_sections_when_tax_and_tax_sum_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Kwota A 23,00% 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                    lines[6],
                }),
                new ReceiptSections.TaxesSection(
                    new ReceiptLine[0]),
                new ReceiptSections.AmountSection(
                    lines[7]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[8],
                })));
        }

        [Fact]
        public void can_parse_sections_when_tax_sum_is_missing()
        {
            //given
            var lines = new[]
            {
                Line("shop name"),
                Line("shop address"),
                Line("NIP 111-111-11-11"),
                Line("2019-12-08 nr wydr.111111"),
                Line("PARAGON FISKALNY"),
                Line("product 1szt * 4,60 = 4,60 C"),
                Line("Sprzed. opod. PTU A 6,85"),
                Line("Kwota A 23,00% 1,28"),
                Line("SUMA PLN 16,05"),
                Line("something in the footer")
            };

            //when
            var sections = ReceiptSectionsParser.GetSections(lines);

            //then
            sections.Should().BeEquivalentTo(new ReceiptSections(
                new ReceiptSections.HeaderSection(new[]
                {
                    lines[0],
                    lines[1],
                    lines[2],
                    lines[3],
                    lines[4],
                }),
                new ReceiptSections.ProductsSection(new[]
                {
                    lines[5],
                }),
                new ReceiptSections.TaxesSection(new[]
                {
                    lines[6],
                    lines[7],
                }),
                new ReceiptSections.AmountSection(
                    lines[8]),
                new ReceiptSections.FooterSection(new[]
                {
                    lines[9],
                })));
        }

        public ReceiptLine Line(string text)
        {
            return new ReceiptLine(
                text: text,
                //values of bounding box doesnt really matter in these tests
                boundingBox: Fixture.Create<BoundingBox>());
        }
    }
}