using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using Billy.Expenses.API.Commands;
using Billy.Expenses.Contracts;
using Billy.Expenses.Projections;
using Billy.IntegrationTests.Infrastructure;
using Billy.IntegrationTests.Infrastructure.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Billy.Expenses.Tests
{
    [Collection(FixtureHostsCollection.Name)]
    public class ExpensesApiTests : ExpensesApiTestFixture
    {
        [Theory, AutoData]
        public async Task can_create_expense(Commands.Expenses.V1.Create createCommand)
        {
            var id = await CreateExpense(createCommand);
            
            await wait_for_projection_to_process_events();

            var result = await GetSingleExpenseFromAPI(id, createCommand.GlobalUserIdentifier);

            result.Should().BeEquivalentTo(
                AutoMapper.Map<Queries.Expenses.V1.ReadModels.Expense>(createCommand), config => config
                    .Excluding(e => e.Id));
        }

        [Theory, AutoData]
        public async Task can_delete_expense(Commands.Expenses.V1.Create createCommand)
        {
            var id = await CreateExpense(createCommand);

            await DeleteExpense(new Commands.Expenses.V1.Delete()
            {
                ExpenseId = id
            });

            await wait_for_projection_to_process_events();

            Func<Task> result = async () => await GetSingleExpenseFromAPI(id, createCommand.GlobalUserIdentifier);

            result.Should().ThrowExactly<TestApiCallErrorException>()
                .And
                .ErrorDetails
                .StatusCode
                .Should().Be(404);
        }

        [Theory, AutoData]
        public async Task can_change_description(
            Commands.Expenses.V1.Create createCommand,
            Commands.Expenses.V1.ChangeDescription changeDescriptionCommand)
        {
            var id =await CreateExpense(createCommand);

            changeDescriptionCommand.ExpenseId = id;
          
            await ChangeDescription(changeDescriptionCommand);

            await wait_for_projection_to_process_events();

            var result = await GetSingleExpenseFromAPI(id, createCommand.GlobalUserIdentifier);

            result.Should().BeEquivalentTo(
                AutoMapper.Map<Queries.Expenses.V1.ReadModels.Expense>(changeDescriptionCommand), 
                config => config
                    .Excluding(e => e.OwnerId)
                    .Excluding(e => e.ReceiptId));

            result.OwnerId.Should().Be(createCommand.GlobalUserIdentifier);
            result.ReceiptId.Should().Be(createCommand.ReceiptId);
        }

        [Theory, AutoData]
        public async Task can_change_tags(
            Commands.Expenses.V1.Create createCommand,
            Commands.Expenses.V1.ChangeTags changeTagsCommand)
        {
            var id = await CreateExpense(createCommand);
            
            changeTagsCommand.ExpenseId = id;

            await ChangeTags(changeTagsCommand);
            
            await wait_for_projection_to_process_events();
            
            var result = await GetSingleExpenseFromAPI(id, createCommand.GlobalUserIdentifier);

            result.Should().BeEquivalentTo(
                AutoMapper.Map<Queries.Expenses.V1.ReadModels.Expense>(createCommand), 
                config => config
                    .Excluding(e => e.Id)
                    .Excluding(e => e.Tags));

            result.Tags.Should().BeEquivalentTo(changeTagsCommand.Tags);
        }
        
        [Fact]
        public async Task can_get_all_expenses_for_user()
        {
            //arrange
            
            var createCommandForExpectedUser = Fixture.Create<Commands.Expenses.V1.Create>()
                .ForUser(Guid.NewGuid().ToString());

            var createCommandForAnotherUser = Fixture.Create<Commands.Expenses.V1.Create>()
                .ForUser(Guid.NewGuid().ToString());

            await CreateExpense(createCommandForExpectedUser);

            await CreateExpense(createCommandForAnotherUser);

            await wait_for_projection_to_process_events();

            //act
            
            var result = await GetUserExpensesFromAPI(createCommandForExpectedUser.GlobalUserIdentifier, pageNumber: 1, pageSize: 100);

            //assert

            result.Items.Count.Should().Be(1);
            
            result.Items.First().Should().BeEquivalentTo(
                AutoMapper.Map<Queries.Expenses.V1.ReadModels.Expense>(createCommandForExpectedUser), config => config.Excluding(e=> e.Id));
        }

        [Theory]
        [InlineData(1, 1, "first-expense")]
        [InlineData(2, 1, "second-expense")]
        [InlineData(1, 100, "first-expense", "second-expense")]
        public async Task can_use_page_expenses_for_user_list(int pageNumber, int pageSize, params string[] expectedExpenses)
        {
            //arrange
            var expectedUserId = Guid.NewGuid().ToString();
            
            var createFirstExpenseCommandForExpectedUser = Fixture.Create<Commands.Expenses.V1.Create>()
                .WithComment("first-expense")
                .ForUser(expectedUserId);
            
            var createSecondExpenseCommandForExpectedUser = Fixture.Create<Commands.Expenses.V1.Create>()
                .WithComment("second-expense")
                .ForUser(expectedUserId);

            var createExpenseCommandForAnotherUser = Fixture.Create<Commands.Expenses.V1.Create>()
                .ForUser(Guid.NewGuid().ToString());
            
            await CreateExpenses(
                createFirstExpenseCommandForExpectedUser,
                createSecondExpenseCommandForExpectedUser,
                createExpenseCommandForAnotherUser);
            
            await wait_for_projection_to_process_events();

            //act
            
            var result = await GetUserExpensesFromAPI(expectedUserId, pageNumber: pageNumber, pageSize: pageSize);
            
            //assert
            result.TotalCount.Should().Be(2);
            result.Items.Count.Should().Be(expectedExpenses.Length);
            result.Items.Select(e => e.Comments).Should().BeEquivalentTo(expectedExpenses);
        }

        public ExpensesApiTests(HostFixture hostFixture, ITestOutputHelper output) : base(hostFixture, output) { }
    }

    public class ExpensesApiTestFixture : TestFixture
    {
        protected IMapper AutoMapper { get; }
        protected Fixture Fixture { get; }
        public ExpensesApiTestFixture(HostFixture hostFixture, ITestOutputHelper output) : base(hostFixture, output)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Commands.Expenses.V1.Create, Queries.Expenses.V1.ReadModels.Expense>();
                cfg.CreateMap<Commands.Expenses.V1.Seller, Queries.Expenses.V1.ReadModels.Seller>();
                cfg.CreateMap<Commands.Expenses.V1.ChangeDescription, Queries.Expenses.V1.ReadModels.Expense>();
            });
            AutoMapper = configuration.CreateMapper();
            Fixture = new Fixture();
        }

        protected async Task<Queries.Expenses.V1.ReadModels.Expense> GetSingleExpenseFromAPI(Guid expenseId, string ownerId) =>
            await get_from_sut_api<Queries.Expenses.V1.ReadModels.Expense>(
                "expenses-api/v1/expenses/queries/get-single",
                ("Id", expenseId),
                ("OwnerId", ownerId));

        protected async Task<Queries.Expenses.V1.PagedResult<Queries.Expenses.V1.ReadModels.Expense>> GetUserExpensesFromAPI(
            string ownerId, int pageNumber = 1, int pageSize = 100) =>
            await get_from_sut_api<Queries.Expenses.V1.PagedResult<Queries.Expenses.V1.ReadModels.Expense>>(
                "expenses-api/v1/expenses/queries/get-all-for-user",
                ("OwnerId", ownerId),
                ("PageNumber", pageNumber),
                ("PageSize", pageSize));

        protected async Task CreateExpenses(params Commands.Expenses.V1.Create[] commands)
        {
            foreach (var command in commands)
            {
                await CreateExpense(command);
            }
        }

        protected async Task<Guid> CreateExpense(Commands.Expenses.V1.Create command)
        {
            var response = await post_to_sut_api_with_response<Commands.Expenses.V1.Create, Commands.Expenses.V1.CreateResponse>(
                "expenses-api/v1/expenses/commands/create", command);
            return response.ExpenseId;
        }
            

        protected async Task DeleteExpense(Commands.Expenses.V1.Delete command) =>
            await post_to_sut_api("expenses-api/v1/expenses/commands/delete", command);

        protected async Task ChangeDescription(Commands.Expenses.V1.ChangeDescription command) =>
            await post_to_sut_api("expenses-api/v1/expenses/commands/change-description", command);
        
        protected async Task ChangeTags(Commands.Expenses.V1.ChangeTags command) =>
            await post_to_sut_api("expenses-api/v1/expenses/commands/change-tags", command);
    }

    public static class CommandsTestExtensions
    {
        public static Commands.Expenses.V1.Create ForUser(this Commands.Expenses.V1.Create command, string userId)
        {
            command.GlobalUserIdentifier = userId;
            return command;
        }
        public static Commands.Expenses.V1.Create WithComment(this Commands.Expenses.V1.Create command, string comments)
        {
            command.Comments = comments;
            return command;
        }
    }
}
