using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Billy.CodeReadability.Tests
{
    public class OptionSerializationTests
    {
        public class BarWithObjectFoo
        {
            public Option<Foo> Foo { get; set; }
        }

        public class Foo
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
        }

        public class BarWithStructFoo
        {
            public Option<int> Foo { get; set; }
        }

        [Fact]
        public void can_serialize_object_option()
        {
            //given
            var option = Option<Foo>.Some(new Foo
            {
                IntProp = 10,
                StringProp = "some value"
            });

            //when
            var serialized = JsonSerializer.Serialize(option, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"hasItem\":true,\"item\":{\"StringProp\":\"some value\",\"IntProp\":10}}");
        }

        [Fact]
        public void can_deserialize_object_option()
        {
            //given
            var json = "{\"hasItem\":true,\"item\":{\"StringProp\":\"some value\",\"IntProp\":10}}";

            //when
            var deserialized = JsonSerializer.Deserialize<Option<Foo>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Match(
                value => value.Should().BeEquivalentTo(new Foo
                {
                    IntProp = 10,
                    StringProp = "some value"
                }),
                () => throw new InvalidOperationException(
                    "Should deserialize option correctly"));
        }

        [Fact]
        public void can_serialize_struct_option()
        {
            //given
            var option = Option<int>.Some(10);

            //when
            var serialized = JsonSerializer.Serialize(option, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"hasItem\":true,\"item\":10}");
        }

        [Fact]
        public void can_deserialize_struct_option()
        {
            //given
            var json = "{\"hasItem\":true,\"item\":10}";

            //when
            var deserialized = JsonSerializer.Deserialize<Option<int>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(Option<int>.Some(10));
        }

        [Fact]
        public void can_serialize_object_with_object_option()
        {
            //given
            var bar = new BarWithObjectFoo
            {
                Foo = Option<Foo>.Some(new Foo
                {
                    IntProp = 10,
                    StringProp = "some value"
                })
            };

            //when
            var serialized = JsonSerializer.Serialize(bar, new JsonSerializerOptions
            {
                
                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"Foo\":{\"hasItem\":true,\"item\":{\"StringProp\":\"some value\",\"IntProp\":10}}}");
        }

        [Fact]
        public void can_deserialize_object_with_object_option()
        {
            //given
            var json = "{\"Foo\":{\"hasItem\":true,\"item\":{\"StringProp\":\"some value\",\"IntProp\":10}}}";

            //when
            var deserialized = JsonSerializer.Deserialize<BarWithObjectFoo>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Foo.Match(
                value => value.Should().BeEquivalentTo(new Foo
                {
                    IntProp = 10,
                    StringProp = "some value"
                }),
                () => throw new InvalidOperationException(
                    "Should deserialize option correctly"));
        }

        [Fact]
        public void can_serialize_object_with_struct_option()
        {
            //given
            var bar = new BarWithStructFoo
            {
                Foo = Option<int>.Some(10)
            };

            //when
            var serialized = JsonSerializer.Serialize(bar, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"Foo\":{\"hasItem\":true,\"item\":10}}");
        }

        [Fact]
        public void can_deserialize_object_with_struct_option()
        {
            //given
            var json = "{\"Foo\":{\"hasItem\":true,\"item\":10}}";

            //when
            var deserialized = JsonSerializer.Deserialize<BarWithStructFoo>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(new BarWithStructFoo
            {
                Foo = Option<int>.Some(10)
            });
        }

        [Fact]
        public void can_serialize_empty_object_option()
        {
            //given
            var option = Option<Foo>.None;

            //when
            var serialized = JsonSerializer.Serialize(option, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"hasItem\":false,\"item\":null}");
        }

        [Fact]
        public void can_deserialize_empty_object_option()
        {
            //given
            var json = "{\"hasItem\":false,\"item\":null}";

            //when
            var deserialized = JsonSerializer.Deserialize<Option<Foo>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(Option<Foo>.None);
        }

        [Fact]
        public void can_serialize_empty_struct_option()
        {
            //given
            var option = Option<int>.None;

            //when
            var serialized = JsonSerializer.Serialize(option, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"hasItem\":false,\"item\":null}");
        }

        [Fact]
        public void can_deserialize_empty_struct_option()
        {
            //given
            var json = "{\"hasItem\":false,\"item\":null}";

            //when
            var deserialized = JsonSerializer.Deserialize<Option<int>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(Option<int>.None);
        }
        
        [Fact]
        public void can_serialize_object_with_empty_object_option()
        {
            //given
            var bar = new BarWithObjectFoo
            {
                Foo = Option<Foo>.None
            };

            //when
            var serialized = JsonSerializer.Serialize(bar, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"Foo\":{\"hasItem\":false,\"item\":null}}");
        }

        [Fact]
        public void can_deserialize_object_with_empty_object_option()
        {
            //given
            var json = "{\"Foo\":{\"hasItem\":false,\"item\":null}}";

            //when
            var deserialized = JsonSerializer.Deserialize<BarWithObjectFoo>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(new BarWithObjectFoo
            {
                Foo = Option<Foo>.None
            });
        }

        [Fact]
        public void can_serialize_object_with_empty_struct_option()
        {
            //given
            var bar = new BarWithStructFoo
            {
                Foo = Option<int>.None
            };

            //when
            var serialized = JsonSerializer.Serialize(bar, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"Foo\":{\"hasItem\":false,\"item\":null}}");
        }

        [Fact]
        public void can_deserialize_object_with_empty_struct_option()
        {
            //given
            var json = "{\"Foo\":{\"hasItem\":false,\"item\":null}}";

            //when
            var deserialized = JsonSerializer.Deserialize<BarWithStructFoo>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(new BarWithStructFoo
            {
                Foo = Option<int>.None
            });
        }

        [Fact]
        public void item_should_be_ignored_when_ignoring_null_values_is_set()
        {
            //given
            var bar = new BarWithStructFoo
            {
                Foo = Option<int>.None
            };

            //when
            var serialized = JsonSerializer.Serialize(bar, new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"Foo\":{\"hasItem\":false}}");
        }

        [Fact]
        public void can_deserialize_json_with_ignored_null_item()
        {
            //given
            var json = "{\"Foo\":{\"hasItem\":false}}";

            //when
            var deserialized = JsonSerializer.Deserialize<BarWithStructFoo>(json, new JsonSerializerOptions
            {
                Converters =
                {
                    new OptionJsonConverterFactory()
                }
            });

            //then
            deserialized.Should().BeEquivalentTo(new BarWithStructFoo
            {
                Foo = Option<int>.None
            });
        }
    }
}
