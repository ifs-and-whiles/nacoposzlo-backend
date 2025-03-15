using System;
using AutoFixture;
using Billy.CodeReadability;
using Billy.Domain;
using Billy.EventSourcing;
using FluentAssertions;
using Xunit;

namespace Billy.Users.Domain.Tests.Aggregates
{
    public class UserAggregateTests
    {
        public Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void can_create_new_user()
        {
            //given
            var userId= Fixture.Create<UserId>();
            var globalUserIdentifier = Fixture.Create<GlobalUserIdentifier>();
            var receiptsRecognitionLimit = ReceiptsRecognitionLimit.From(10);
            var receiptsRecognitionCurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.From(0);
            var wasTermsAndPrivacyPolicyAccepted = true;
            var dateOfConsents = DateTimeOffset.UtcNow;
            
            //when
            var user = User.Create(
                userId, 
                globalUserIdentifier, 
                ReceiptsRecognitionUsage.From(receiptsRecognitionLimit, receiptsRecognitionCurrentPackageCounter),
                TermsAndPrivacyPolicy.From(wasTermsAndPrivacyPolicyAccepted, dateOfConsents));
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.UserAdded(
                id: userId,
                globalUserIdentifier: globalUserIdentifier,
                receiptsRecognitionLimit: receiptsRecognitionLimit,
                receiptsRecognitionCurrentPackageCounter: receiptsRecognitionCurrentPackageCounter,
                termsAndPrivacyPolicy: new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = wasTermsAndPrivacyPolicyAccepted,
                    DateOfConsents = dateOfConsents
                }
            ));
        }

        [Fact]
        public void should_create_user_and_indicate_that_limit_has_been_reached_when_counter_is_equal_limit()
        {
            //given
            var userId= Fixture.Create<UserId>();
            var globalUserIdentifier = Fixture.Create<GlobalUserIdentifier>();
            var receiptsRecognitionLimit = ReceiptsRecognitionLimit.From(10);
            var receiptsRecognitionCurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.From(10);
            var wasTermsAndPrivacyPolicyAccepted = true;
            var dateOfConsents = DateTimeOffset.UtcNow;
            
            //when
            var user = User.Create(
                userId, 
                globalUserIdentifier, 
                ReceiptsRecognitionUsage.From(receiptsRecognitionLimit, receiptsRecognitionCurrentPackageCounter),
                TermsAndPrivacyPolicy.From(wasTermsAndPrivacyPolicyAccepted, dateOfConsents));
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.UserAdded(
                id: userId,
                globalUserIdentifier: globalUserIdentifier,
                receiptsRecognitionLimit: receiptsRecognitionLimit,
                receiptsRecognitionCurrentPackageCounter: receiptsRecognitionCurrentPackageCounter,
                termsAndPrivacyPolicy: new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = wasTermsAndPrivacyPolicyAccepted,
                    DateOfConsents = dateOfConsents
                }
            ),new Events.Users.V1.ReceiptsRecognitionLimitReached(userId));
        }

        [Fact]
        public void can_create_new_user_with_default_receipts_recognition_usage()
        {
            //given
            var userId= Fixture.Create<UserId>();
            var globalUserIdentifier = Fixture.Create<GlobalUserIdentifier>();
            var receiptsRecognitionLimit = ReceiptsRecognitionLimit.From(null);
            var receiptsRecognitionCurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.From(null);
            var wasTermsAndPrivacyPolicyAccepted = true;
            var dateOfConsents = DateTimeOffset.UtcNow;
            
            //when
            var user = User.Create(
                userId, 
                globalUserIdentifier, 
                ReceiptsRecognitionUsage.From(receiptsRecognitionLimit, receiptsRecognitionCurrentPackageCounter),
                TermsAndPrivacyPolicy.From(wasTermsAndPrivacyPolicyAccepted, dateOfConsents));
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.UserAdded(
                id: userId,
                globalUserIdentifier: globalUserIdentifier,
                receiptsRecognitionLimit: ReceiptsRecognitionLimit.Default,
                receiptsRecognitionCurrentPackageCounter: ReceiptsRecognitionCurrentPackageCounter.Default,
                termsAndPrivacyPolicy: new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = wasTermsAndPrivacyPolicyAccepted,
                    DateOfConsents = dateOfConsents
                }
            ));
        }

        [Fact]
        public void can_create_new_user_with_infinite_receipts_recognition_limit()
        {
            //given
            var userId= Fixture.Create<UserId>();
            var globalUserIdentifier = Fixture.Create<GlobalUserIdentifier>();
            var receiptsRecognitionLimit = ReceiptsRecognitionLimit.From(-1);
            var receiptsRecognitionCurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.From(10);
            var wasTermsAndPrivacyPolicyAccepted = true;
            var dateOfConsents = DateTimeOffset.UtcNow;
            
            //when
            var user = User.Create(
                userId, 
                globalUserIdentifier, 
                ReceiptsRecognitionUsage.From(receiptsRecognitionLimit, receiptsRecognitionCurrentPackageCounter),
                TermsAndPrivacyPolicy.From(wasTermsAndPrivacyPolicyAccepted, dateOfConsents));
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.UserAdded(
                id: userId,
                globalUserIdentifier: globalUserIdentifier,
                receiptsRecognitionLimit: -1,
                receiptsRecognitionCurrentPackageCounter: 10,
                termsAndPrivacyPolicy: new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = wasTermsAndPrivacyPolicyAccepted,
                    DateOfConsents = dateOfConsents
                }
            ));
            user.ReceiptsRecognitionUsage.Limit.IsUnlimited.Should().BeTrue();
        }

        [Fact]
        public void can_increase_receipts_recognition_current_package_counter()
        {
            //given

            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(0),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            
            //when

            user.IncreaseReceiptsRecognitionCurrentPackageCounter();
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased(
                id: user.UserId.Value
            ));

        }

        [Fact]
        public void should_throw_exception_when_recognition_limit_already_reached()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(10),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            //when

            var exception = Assert.Throws<DomainException>(() => user.IncreaseReceiptsRecognitionCurrentPackageCounter());
            exception.ErrorCode.Should().Be(ErrorCodes.ReceiptsRecognitionLimitReached);
        }

        [Fact]
        public void should_emit_recognition_limit_reached_event_when_counter_has_been_increased_to_limit_value()
        {
            //given
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(9),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            //when

            user.IncreaseReceiptsRecognitionCurrentPackageCounter();

            //then
            user.GetChanges().Should().BeEquivalentTo(
                new Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased(user.UserId.Value),
                new Events.Users.V1.ReceiptsRecognitionLimitReached(user.UserId.Value));
            user.ReceiptsRecognitionUsage.CurrentPackageCounter.Value.Should().Be(10);
        }

        [Fact]
        public void can_increase_counter_for_user_with_infinite_receipts_recognition_limit()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(-1), 
                ReceiptsRecognitionCurrentPackageCounter.From(0),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            
            //when

            user.IncreaseReceiptsRecognitionCurrentPackageCounter();
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased(
                id: user.UserId.Value
            ));
            user.ReceiptsRecognitionUsage.CurrentPackageCounter.Value.Should().Be(1);
        }

        [Fact]
        public void can_reset_receipts_recognition_current_package_counter()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(5),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            
            //when

            user.ResetReceiptsRecognitionCurrentPackageCounter();
            
            //then
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.ReceiptsRecognitionCounterZeroed(
                id: user.UserId.Value
            ));
            user.ReceiptsRecognitionUsage.CurrentPackageCounter.Value.Should().Be(0);
        }

        [Fact]
        public void can_assign_receipts_recognition_limit()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(5),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            
            //when

            user.AssignReceiptsRecognitionLimit(ReceiptsRecognitionLimit.From(20));
            
            //then
            
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.ReceiptsRecognitionLimitAssigned(
                id: user.UserId.Value,
                limit: 20
            ));
            
            user.ReceiptsRecognitionUsage.Limit.Value.Should().Be(20);
        }

        [Fact]
        public void can_assign_an_infinite_receipts_recognition_limit()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(5),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            
            //when

            user.AssignReceiptsRecognitionLimit(ReceiptsRecognitionLimit.From(-1));
            
            //then
            
            user.GetChanges().Should().BeEquivalentTo(new Events.Users.V1.ReceiptsRecognitionLimitAssigned(
                id: user.UserId.Value,
                limit: -1
            ));
            
            user.ReceiptsRecognitionUsage.Limit.Value.Should().Be(-1);
        }

        [Fact]
        public void should_throw_domain_exception_when_new_receipts_recognition_limit_is_less_than_current_counter()
        {
            //given
            
            var user = new User();
            var userCreated =  new Events.Users.V1.UserAdded(
                Fixture.Create<UserId>(),
                Fixture.Create<GlobalUserIdentifier>(),
                ReceiptsRecognitionLimit.From(10), 
                ReceiptsRecognitionCurrentPackageCounter.From(5),
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = true,
                    DateOfConsents = DateTimeOffset.UtcNow 
                }
            );
            
            user.Load(new[] {new StoredEvent(1, "test-stream", userCreated, DateTime.UtcNow) });

            //when

            var exception = Assert.Throws<DomainException>(() => user.AssignReceiptsRecognitionLimit(ReceiptsRecognitionLimit.From(1)));
            exception.ErrorCode.Should().Be(ErrorCodes.ReceiptsRecognitionLimitLessThatCurrentCounter);
        }
    }
}