using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using Billy.IntegrationTests.Infrastructure;
using Billy.Users.Contracts;
using Billy.Users.Domain;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Commands = Billy.Users.Contracts.Commands.Users.V1;
namespace Billy.Users.Tests
{
    [Collection(FixtureHostsCollection.Name)]
    public class UsersApiTests : UsersApiTestFixture
    {
        [Fact]
        public async Task can_create_user()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 20,
                ReceiptsRecognitionCurrentPackageCounter = 1,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await wait_for_projection_to_process_events();

            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 1,
                    Limit = 20,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 1
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_create_user_with_default_receipts_recognition_usage()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await wait_for_projection_to_process_events();

            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = ReceiptsRecognitionCurrentPackageCounter.Default.Value,
                    Limit = ReceiptsRecognitionLimit.Default.Value,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 0
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_create_user_with_infinite_receipts_recognition_limit()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = -1,
                ReceiptsRecognitionCurrentPackageCounter = 0,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await wait_for_projection_to_process_events();

            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 0,
                    Limit = -1,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 0
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_increase_receipts_recognition_current_package_counter()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 20,
                ReceiptsRecognitionCurrentPackageCounter = 0,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await IncreaseReceiptsRecognitionCurrentPackageCounter(new Commands.IncreaseReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier
            });
            
            await wait_for_projection_to_process_events();

            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 1,
                    Limit = 20,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 1
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task should_display_ads_when_receipts_recognition_limit_reached()
        {            
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 9,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await IncreaseReceiptsRecognitionCurrentPackageCounter(new Commands.IncreaseReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier
            });
            
            await wait_for_projection_to_process_events();

            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = true,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 10,
                    Limit = 10,
                    LimitExceeded = true,
                    TotalUtilizationCounter = 10
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task should_throw_exception_when_recognition_limit_already_reached()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 10,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await wait_for_projection_to_process_events();
            
            Func<Task> result = async () => await IncreaseReceiptsRecognitionCurrentPackageCounter(
                new Commands.IncreaseReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier
            });
            
            //then
             result.Should().Throw<TestApiCallErrorException>()
                .And.ErrorDetails.Message.Should().Be(ErrorCodes.ReceiptsRecognitionLimitReached);
        }

        [Fact]
        public async Task can_reset_receipts_recognition_current_package_counter()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 9,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await IncreaseReceiptsRecognitionCurrentPackageCounter(new Commands.IncreaseReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier
            });
            
            await ResetReceiptsRecognitionCurrentPackageCounter(new Commands.ResetReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier
            });
            
            await wait_for_projection_to_process_events();
            
            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 0,
                    Limit = 10,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 10
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_reset_receipts_recognition_current_package_counter_for_all_users()
        {
            //given

            var createFirstUserCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-first-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 9,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            var createSecondUserCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-second-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 2,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUsers(createFirstUserCommand, createSecondUserCommand);
            
            //Check situation when the limit has been reached. Reset should back LimitExceeded flag to false.
            await IncreaseReceiptsRecognitionCurrentPackageCounter(new Commands.IncreaseReceiptsRecognitionCurrentPackageCounter()
            {
                GlobalUserIdentifier = createFirstUserCommand.GlobalUserIdentifier
            });
            
            await ResetReceiptsRecognitionCurrentPackageCounterForAllUsers();
            
            await wait_for_projection_to_process_events();
            
            //then

            await ValidateUsersCurrentPackageCounter(
                expectedCounter: 0,
                createFirstUserCommand.GlobalUserIdentifier,
                createSecondUserCommand.GlobalUserIdentifier);

            
            async Task ValidateUsersCurrentPackageCounter(int expectedCounter, params string[] userIdentifiers)
            {
                foreach (var userIdentifier in userIdentifiers)
                {
                    var result = await GetSingleUserFromApi(userIdentifier);
                    result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
                    {
                        DisplayAds = false,
                        GlobalUserIdentifier = userIdentifier,
                        ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                        {
                            CurrentPackageCounter = expectedCounter,
                            Limit = 10,
                            LimitExceeded = false
                        },
                        TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                        {
                            DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                            WasTermsAndPrivacyPolicyAccepted = true
                        }
                    }, config => config
                        .Excluding(e => e.Id)
                        .Excluding(e => e.ReceiptsRecognitionUsage.TotalUtilizationCounter));
                }
            }
        }
        
        [Fact]
        public async Task can_assign_receipts_recognition_limit()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 10,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await AssignReceiptsRecognitionLimit(new Commands.AssignReceiptsRecognitionLimit()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                Limit = 100
            });
            
            await wait_for_projection_to_process_events();
            
            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 10,
                    Limit = 100,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 10
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_assign_an_infinite_receipts_recognition_limit()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 10,
                ReceiptsRecognitionCurrentPackageCounter = 10,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await AssignReceiptsRecognitionLimit(new Commands.AssignReceiptsRecognitionLimit()
            {
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                Limit = -1
            });
            
            await wait_for_projection_to_process_events();
            
            var result = await GetSingleUserFromApi(createCommand.GlobalUserIdentifier);
            
            //then
            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.User()
            {
                DisplayAds = false,
                GlobalUserIdentifier = createCommand.GlobalUserIdentifier,
                ReceiptsRecognitionUsage = new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
                {
                    CurrentPackageCounter = 10,
                    Limit = -1,
                    LimitExceeded = false,
                    TotalUtilizationCounter = 10
                },
                TermsAndPrivacyPolicy = new Queries.Users.V1.ReadModels.TermsAndPrivacyPolicy()
                {
                    DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                    WasTermsAndPrivacyPolicyAccepted = true
                }
            }, config => config.Excluding(e => e.Id));
        }

        [Fact]
        public async Task can_return_receipts_recognition_usage_status()
        {
            //given
            var createCommand = new Commands.Create()
            {
                GlobalUserIdentifier = "test-user-identifier",
                ReceiptsRecognitionLimit = 20,
                ReceiptsRecognitionCurrentPackageCounter = 1,
                DateOfConsents = new DateTimeOffset(new DateTime(2020, 01, 01)),
                WasTermsAndPrivacyPolicyAccepted = true
            };
            
            //when
            await CreateUser(createCommand);

            await wait_for_projection_to_process_events();

            var result = await GetReceiptsRecognitionUsageStatus(createCommand.GlobalUserIdentifier);
            
            //then

            result.Should().BeEquivalentTo(new Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage()
            {
                LimitExceeded = false,
                TotalUtilizationCounter = 1,
                Limit = 20,
                CurrentPackageCounter = 1
            });
        }

        public UsersApiTests(HostFixture hostFixture, ITestOutputHelper output) : base(hostFixture, output)
        {
        }
    }

    public class UsersApiTestFixture : TestFixture
    {
        protected IMapper AutoMapper { get; }
        protected Fixture Fixture { get; }
        
        public UsersApiTestFixture(HostFixture hostFixture, ITestOutputHelper output) 
            : base(hostFixture, output)
        {
            Fixture = new Fixture();
        }
        
        protected async Task<Queries.Users.V1.ReadModels.User> GetSingleUserFromApi(string globalUserIdentifier)
        {
            return await get_from_sut_api<Queries.Users.V1.ReadModels.User>(
                "users-api/v1/users/queries/get-single",
                ("GlobalUserIdentifier", globalUserIdentifier));
        }

        protected async Task CreateUsers(params Commands.Create[] commands)
        {
            foreach (var command in commands)
            {
                await CreateUser(command);
            }
        } 
                
        
        protected async Task CreateUser(Commands.Create command) => 
            await post_to_sut_api("users-api/v1/users/commands/create", command);
        
        protected async Task AssignReceiptsRecognitionLimit(Commands.AssignReceiptsRecognitionLimit command) => 
            await post_to_sut_api("users-api/v1/users/commands/assign-receipts-recognition-limit", command);
        
        protected async Task ResetReceiptsRecognitionCurrentPackageCounter(Commands.ResetReceiptsRecognitionCurrentPackageCounter command) => 
            await post_to_sut_api("users-api/v1/users/commands/reset-receipts-recognition-current-package-counter", command);
        
        protected async Task ResetReceiptsRecognitionCurrentPackageCounterForAllUsers() => 
            await post_to_sut_api("users-api/v1/users/commands/reset-receipts-recognition-current-package-counter-for-all-users", 
                new Commands.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers());
        
        protected async Task IncreaseReceiptsRecognitionCurrentPackageCounter(
            Commands.IncreaseReceiptsRecognitionCurrentPackageCounter command) =>
            await post_to_sut_api("users-api/v1/users/commands/increase-receipts-recognition-current-package-counter", command);
        
        protected async Task<Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage> GetReceiptsRecognitionUsageStatus(string globalUserIdentifier) 
        {
            return await get_from_sut_api<Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage>(
                "users-api/v1/users/queries/get-receipts-recognition-usage-status",
                ("GlobalUserIdentifier", globalUserIdentifier));
        }
            
    }
}