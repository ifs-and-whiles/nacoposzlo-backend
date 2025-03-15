using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.Api.Mobile.Infrastructure.Configs;
using Billy.Api.Mobile.Utils;
using Flurl.Http.Configuration;
using Billy.Expenses.Contracts;
namespace Billy.Api.Mobile.ExpensesService
{
    public class ExpensesServiceClient
    {
        private readonly HttpServiceClient _httpServiceClient;
    
        public ExpensesServiceClient(
            IFlurlClientFactory flurlClientFactory,
            ExpensesServiceConfig expensesServiceConfig,
            RequestRetryPolicyConfig requestRetryPolicyConfig)
        {
            _httpServiceClient = new HttpServiceClient(
                expensesServiceConfig.HostUrl,
                flurlClientFactory,
                requestRetryPolicyConfig);
        }

        public async Task<Queries.Expenses.V1.ReadModels.Expense> GetExpense(Queries.Expenses.V1.GetExpense query) =>
            await _httpServiceClient.Get<Queries.Expenses.V1.ReadModels.Expense>(
                "api/v1/expenses/queries/get-single",
                query.CorrelationId,
                ("Id", query.Id));

        public async Task CreateExpense(Commands.Expenses.V1.Create command) =>
            await _httpServiceClient.Post(
                "api/v1/expenses/commands/create",
                command.CorrelationId,
                command);

        public async Task DeleteExpense(Commands.Expenses.V1.Delete command) => await _httpServiceClient.Post(
            "api/v1/expenses/commands/delete",
            command.CorrelationId,
            command);

        public async Task ChangeDescription(Commands.Expenses.V1.ChangeDescription command) =>
            await _httpServiceClient.Post(
                "api/v1/expenses/commands/change-description",
                command.CorrelationId,
                command);

    }
}
