using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Billy.CodeReadability.Tests
{
    public class EitherSerializationTests
    {
        public class Foo
        {
            public string StringProp { get; set; }
        }

        public class Bar
        {
            public int IntProp { get; set; }
        }

        [Fact]
        public void can_serialize_either_with_left_item()
        {
            //given
            var either = new Either<Foo, Bar>(new Foo
            {
                StringProp = "some value"
            });

            //when
            var serialized = JsonSerializer.Serialize(either, new JsonSerializerOptions
            {

                Converters =
                {
                    new EitherJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"isLeft\":true,\"left\":{\"StringProp\":\"some value\"},\"right\":null}");
        }

        [Fact]
        public void can_deserialize_either_with_left_item()
        {
            //given
            var json = "{\"isLeft\":true,\"left\":{\"StringProp\":\"some value\"},\"right\":null}";
            
            //when
            var deserialized = JsonSerializer.Deserialize<Either<Foo, Bar>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new EitherJsonConverterFactory()
                }
            });

            //then
            deserialized.Match(
                left => left.Should().BeEquivalentTo(new Foo
                {
                    StringProp = "some value"
                }),
                right => throw new InvalidOperationException(
                    "Right should not be deserialized"));
        }

        [Fact]
        public void can_serialize_either_with_right_item()
        {
            //given
            var either = new Either<Foo, Bar>(new Bar
            {
                IntProp = 10
            });

            //when
            var serialized = JsonSerializer.Serialize(either, new JsonSerializerOptions
            {

                Converters =
                {
                    new EitherJsonConverterFactory()
                }
            });

            //then
            serialized.Should()
                .Be("{\"isLeft\":false,\"left\":null,\"right\":{\"IntProp\":10}}");
        }

        [Fact]
        public void can_deserialize_either_with_right_item()
        {
            //given
            var json = "{\"isLeft\":false,\"left\":null,\"right\":{\"IntProp\":10}}";

            //when
            var deserialized = JsonSerializer.Deserialize<Either<Foo, Bar>>(json, new JsonSerializerOptions
            {

                Converters =
                {
                    new EitherJsonConverterFactory()
                }
            });

            //then
            deserialized.Match(
                left => throw new InvalidOperationException(
                    "Left should not be deserialized"),
                right => right.Should().BeEquivalentTo(new Bar
                {
                    IntProp = 10
                }));
        }
    }
}