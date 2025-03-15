using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.Sections;
using Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests
{
    public class ProductLineParsingTests
    {

        [Theory]
        [InlineData("Okular. P. Słoneczne 1 * 10 10A", "Okular. P. Słoneczne", 1, null, 10, 10, "A")]
        [InlineData("Dicoflor krop. doustne 5 ml .12097B 1.000 * 32.95 32.95B", "Dicoflor krop. doustne 5 ml .12097B", 1, null, 32.95, 32.95, "B")]
        [InlineData("Bebico 1.000 szt * 32.95 32.95B", "Bebico", 1, "szt", 32.95, 32.95, "B")]
        [InlineData("PARACETAMOL 80 MG 10 CZOP.DOODBYT.7190B\r\n1 op * 4,50 = 4,50 B", "PARACETAMOL 80 MG 10 CZOP.DOODBYT.7190B", 1, "op", 4.5, 4.5, "B")]
        public void can_parse_realistic_product_line(string line, string name, decimal quantity, string unit,
            decimal unitPrice, decimal amount, string taxTag)
        {
            //when
            var product = Parse(line).Single();

            //then
            product.IsRecognizedAnd(
                assert => assert.Name().ShouldBe(name),
                assert => assert.Quantity().ShouldBe(quantity),
                assert => assert.Unit().ShouldBe(unit),
                assert => assert.UnitPrice().ShouldBe(unitPrice),
                assert => assert.Amount().ShouldBe(amount),
                assert => assert.TaxTag().ShouldBe(taxTag)
            );
        }

        [Theory]
        [InlineData("Product 1 * 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 + 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 # 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1# 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1#10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 x 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 x 10= 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 x 10 = 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 x 10 =10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10= 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10 = 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10 =10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10 : 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 x 10 10", "Product", 1, null, 10, 10, null)]
        [InlineData("Product 1 10 10", "Product", 1, null, 10, 10, null)]
        [InlineData("Pro duct 1 * 10 10A", "Pro duct", 1, null, 10, 10, "A")]
        [InlineData("123 Pro duct 321 1 * 10 10A", "123 Pro duct 321", 1, null, 10, 10, "A")]
        [InlineData("123 Pro duct 321 1 10 10A", "123 Pro duct 321", 1, null, 10, 10, "A")]
        [InlineData("Product\r\n1 * 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product\r\n1 10 10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product\r\n1 10 10", "Product", 1, null, 10, 10, null)]
        [InlineData("Product 1 * 10\r\n10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10\r\n10A", "Product", 1, null, 10, 10, "A")]
        [InlineData("Product 1 10\r\n10", "Product", 1, null, 10, 10, null)]
        [InlineData("Product 2o * 120.00 240A", "Product", 2, "o", 120, 240, "A")]
        [InlineData("Product 2o 120.00 240A", "Product", 2, "o", 120, 240, "A")]
        [InlineData("TORTILLA FUN 4X25 2 x4,99 9,98A", "TORTILLA FUN 4X25", 2, null, 4.99, 9.98, "A")]
        [InlineData("TORTILLA FUN 4X25 2 x4 , 99 9  ,  98A", "TORTILLA FUN 4X25", 2, null, 4.99, 9.98, "A")]
        [InlineData("ferrerocollect172 1szt.*5.00 5.00A", "ferrerocollect172", 1, "szt.", 5.00, 5.00, "A")]
        [InlineData("Dr.Max Tymianek i Podbiał 24 pasty.4473B\r\n1 op * 4,00 = 4,00B", "Dr.Max Tymianek i Podbiał 24 pasty.4473B", 1, "op", 4.00, 4.00, "B")]
        [InlineData("No-Spa MAX 80mg 20 tabl. .1431 B\r\n1 op * 16,99 = 16,99B", "No-Spa MAX 80mg 20 tabl. .1431 B", 1, "op", 16.99, 16.99, "B")]
        [InlineData("Torba foliowa kosz A 1 * 0,65 zł. 0,65 A", "Torba foliowa kosz A", 1, null, 0.65, 0.65, "A")]
        [InlineData("Wafle ryzowe sonko 130G C\r\n1 * 3,80 zł. 3,80C", "Wafle ryzowe sonko 130G C", 1, null, 3.80, 3.80, "C")]
        [InlineData("Cwiartki wedzone K:509 0.725+9.90 7.18C", "Cwiartki wedzone K:509", 0.725, null, 9.90, 7.18, "C")]
        [InlineData("Product 2 * 12o.00 240A", "Product 2 *", 12, "o.", 0, 240, "A")]
        [InlineData("Product 2 * 12o.00 = 240A", "Product 2 *", 12, "o.", 0, 240, "A")]
        [InlineData("Product 2 * 12o.00 : 240A", "Product 2 *", 12, "o.", 0, 240, "A")]
        public void can_parse_product_line(string line, string name, decimal quantity, string unit, decimal unitPrice,
            decimal amount, string taxTag)
        {
            //when
            var product = Parse(line).Single();

            //then
            product.IsRecognizedAnd(
                assert => assert.Name().ShouldBe(name),
                assert => assert.Quantity().ShouldBe(quantity),
                assert => assert.Unit().ShouldBe(unit),
                assert => assert.UnitPrice().ShouldBe(unitPrice),
                assert => assert.Amount().ShouldBe(amount),
                assert => assert.TaxTag().ShouldBe(taxTag)
            );
        }

        [Theory]
        [InlineData("Product 2 * ol 240A", "Product 2 * ol", 240, "A")]
        [InlineData("rabat -0.80C", "rabat", -0.80, "C")]
        [InlineData("RAZEM: 7.52", "RAZEM", 7.52, null)]
        [InlineData("Bez recepty 4,50", "Bez recepty", 4.5, null)]
        [InlineData("ProductX 2 * ol 240A", "ProductX 2 * ol", 240, "A")]
        [InlineData("Product 2 X* ol 240A", "Product 2 X* ol", 240, "A")]
        [InlineData("Rabat Rossne _ cia basic -0, 33 B", "Rabat Rossne _ cia basic", -0.33, "B")]
        [InlineData("Rabat Rossne _ cia basic - 0, 33 B", "Rabat Rossne _ cia basic", -0.33, "B")]
        public void can_parse_product_with_missing_unit_price(
            string line,string name, decimal amount, string taxTag)
        {
            //when
            var product = Parse(line).Single();

            //then
            product.IsRecognizedAnd(
                assert => assert.Name().ShouldBe(name),
                assert => assert.Quantity().ShouldBeEmpty(),
                assert => assert.Unit().ShouldBeEmpty(),
                assert => assert.UnitPrice().ShouldBeEmpty(),
                assert => assert.Amount().ShouldBe(amount),
                assert => assert.TaxTag().ShouldBe(taxTag)
            );
        }

        [Theory]
        [InlineData("Product * 120.00 240A", "Product *", 120, 240, "A")]
        [InlineData("Product 120.00 240A", "Product", 120, 240, "A")]
        public void can_parse_product_with_missing_quantity(
            string line, string name, decimal unitPrice, decimal amount, string taxTag)
        {
            //when
            var product = Parse(line).Single();

            //then
            product.IsRecognizedAnd(
                assert => assert.Name().ShouldBe(name),
                assert => assert.Quantity().ShouldBeEmpty(),
                assert => assert.Unit().ShouldBeEmpty(),
                assert => assert.UnitPrice().ShouldBe(unitPrice),
                assert => assert.Amount().ShouldBe(amount),
                assert => assert.TaxTag().ShouldBe(taxTag)
            );
        }
        
        [Theory]
        [InlineData("89,99A")]
        [InlineData("##STRONO##")]
        public void can_parse_unrecognizable_lines(string line)
        {
            //when
            var product = Parse(line).Single();

            //then
            product.IsNotRecognizedAnd(
                rawText => rawText.Should().Be(line));
        }

        [Fact]
        public void multiline_product_with_broken_quantity_merges_two_lines_into_product_name()
        {
            //when
            var product = Parse("Product\r\no * 120 240A").Single();

            //then
            product.IsRecognizedAnd(
                assert => assert.Name().ShouldBe("Product o * "),
                assert => assert.UnitPrice().ShouldBe(120),
                assert => assert.Amount().ShouldBe(240),
                assert => assert.TaxTag().ShouldBe("A"));
        }

        [Fact]
        public void unrecognized_line_and_single_line_should_precedence_over_broken_multiline()
        {
            //when
            var products = Parse("##STRONO##\r\nKMINKOWA -1.000*0.19 -0.19C");

            //then
            products.Length.Should().Be(2);

            products[0].IsNotRecognizedAnd(
                rawText => rawText.Should().Be("##STRONO##"));

            products[1].IsRecognizedAnd(
                assert => assert.Name().ShouldBe("KMINKOWA"),
                assert => assert.Quantity().ShouldBe(-1),
                assert => assert.UnitPrice().ShouldBe(0.19M),
                assert => assert.Amount().ShouldBe(-0.19M),
                assert => assert.TaxTag().ShouldBe("C"));
        }

        [Fact]
        public void single_line_with_missing_quantity_and_unit_price_should_precedence_over_unrecognized_line_and_single_line()
        {
            //when
            var products = Parse("rabat -10A\r\nKMINKOWA -1.000*0.19 -0.19C");

            //then
            products.Length.Should().Be(2);

            products[0].IsRecognizedAnd(
                assert => assert.Name().ShouldBe("rabat"),
                assert => assert.Quantity().ShouldBeEmpty(),
                assert => assert.Unit().ShouldBeEmpty(),
                assert => assert.UnitPrice().ShouldBeEmpty(),
                assert => assert.Amount().ShouldBe(-10M),
                assert => assert.TaxTag().ShouldBe("A"));

            products[1].IsRecognizedAnd(
                assert => assert.Name().ShouldBe("KMINKOWA"),
                assert => assert.Quantity().ShouldBe(-1),
                assert => assert.Unit().ShouldBeEmpty(),
                assert => assert.UnitPrice().ShouldBe(0.19M),
                assert => assert.Amount().ShouldBe(-0.19M),
                assert => assert.TaxTag().ShouldBe("C"));
        }

        [Fact]
        public void multiline_product_with_broken_unit_price_creates_one_unrecognized_record_and_one_recognized()
        {
            //when
            var products = Parse("Product\r\n2 * 12o 240A");

            //then
            products.Length.Should().Be(2);

            products[0].IsNotRecognizedAnd(
                rawText => rawText.Should().Be("Product"));

            products[1].IsRecognizedAnd(
                assert => assert.Name().ShouldBe("2 * 12o"),
                assert => assert.Amount().ShouldBe(240),
                assert => assert.TaxTag().ShouldBe("A"));
        }

        [Theory]
        [InlineData("##STRONO##\r\nProduct 1 x 1.00 = 1.00A", "##STRONO##", "Product", 1, 1, 1, "A")]
        public void can_recognize_correct_product_after_unrecognized_one( string line,
            string unrecognized, string recognizedName, 
            decimal recognizedQuantity, decimal recognizedUnitPrice,
            decimal recognizedAmount, string recognizedTaxTag)
        {
            //when
            var products = Parse(line);

            //then
            products.Length.Should().Be(2);

            products[0].IsNotRecognizedAnd(
                rawText => rawText.Should().Be(unrecognized));

            products[1].IsRecognizedAnd(
                assert => assert.Name().ShouldBe(recognizedName),
                assert => assert.Quantity().ShouldBe(recognizedQuantity),
                assert => assert.UnitPrice().ShouldBe(recognizedUnitPrice),
                assert => assert.Amount().ShouldBe(recognizedAmount),
                assert => assert.TaxTag().ShouldBe(recognizedTaxTag));
        }

        [Fact]
        public void can_recognize_product_with_broken_tax_tag()
        {
            //when
            var products = Parse("badanie usg ciaza 1*300.00 300. 0QE");

            //then
            products.Length.Should().Be(1);

            products[0].IsRecognizedAnd(
                assert => assert.Name().ShouldBe("badanie usg ciaza"),
                assert => assert.Quantity().ShouldBe(1),
                assert => assert.UnitPrice().ShouldBe(300),
                assert => assert.Amount().ShouldBe(300),
                assert => assert.TaxTag().ShouldBe("QE"));
        }

        public IReceiptProduct[] Parse(string line)
        {
            var lines = line.Split("\r\n");

            return ReceiptProductsParser.ParseProducts(
                new ReceiptSections.ProductsSection(
                    lines.Select(x => new ReceiptLine(x, new BoundingBox(
                        new Point(0, 0),
                        new Point(0, 0),
                        new Point(0, 0),
                        new Point(0, 0)))).ToArray()
                ));
        }
    }
}