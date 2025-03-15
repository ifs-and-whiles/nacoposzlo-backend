using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Billy.Api.Mobile.ExpensesService;
using Billy.Api.Mobile.Utils;
using Billy.Expenses.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billy.Api.Mobile.API
{
    [Authorize]
    [ApiController, Route("mobile-api/v1/expenses")]
    public class ExpenseMobileApiController : MobileApiControllerBase
    {
        private readonly ExpensesServiceClient _expensesServiceClient;

        public ExpenseMobileApiController(ExpensesServiceClient expensesServiceClient)
        {
            _expensesServiceClient = expensesServiceClient;
        }

        [HttpGet]
        [Route("get-single")]
        public async Task<Contract.Mobile.V1.Expenses.Expense> GetExpense(
            [FromQuery] Contract.Mobile.V1.Expenses.GetSingleExpenseRequest request)
        {
            var expense = await _expensesServiceClient.GetExpense(new Queries.Expenses.V1.GetExpense()
            {
                Id = request.Id,
                CorrelationId = GetRequestCorrelationId()
            });

            return new Contract.Mobile.V1.Expenses.Expense()
            {
                Id = expense.Id,
                Date = expense.Date,
                Comments = expense.Comments,
                Quantity = expense.Quantity,
                ReceiptId = expense.ReceiptId,
                Seller = new Contract.Mobile.V1.Expenses.Seller()
                {
                    Location = expense.Seller?.Location,
                    Name = expense.Seller?.Name,
                    PostalCode = expense.Seller?.PostalCode,
                    TaxNumber = expense.Seller?.TaxNumber
                },
                Tags = expense.Tags,
                Title = expense.Title,
                TotalAmount = expense.TotalAmount,
                UnitPrice = expense.UnitPrice
            };
        }

        [HttpPost, Route("create")]
        public async Task<Guid> CreateExpense(Contract.Mobile.V1.Expenses.CreateRequest request)
        {
            var expenseId = Guid.NewGuid();
            await _expensesServiceClient.CreateExpense(new Commands.Expenses.V1.Create()
            {
                Id = expenseId,
                //TODO: [FP] Temp solution
                OwnerId = Guid.NewGuid(),
                Date = request.Date,
                Comments = request.Comments,
                Quantity = request.Quantity,
                ReceiptId = request.ReceiptId,
                Seller = new Commands.Expenses.V1.Seller()
                {
                    Location = request.Seller?.Location,
                    Name = request.Seller?.Name,
                    PostalCode = request.Seller?.PostalCode,
                    TaxNumber = request.Seller?.TaxNumber
                },
                Tags = request.Tags,
                Title = request.Title,
                TotalAmount = request.TotalAmount,
                UnitPrice = request.UnitPrice
            });
            return expenseId;
        }

        [HttpPost, Route("delete")]
        public async Task DeleteExpense(Contract.Mobile.V1.Expenses.DeleteRequest request)
        {
            await _expensesServiceClient.DeleteExpense(new Commands.Expenses.V1.Delete()
            {
                CorrelationId = GetRequestCorrelationId(),
                Id = request.Id
            });
        }

        [HttpPost, Route("update")]
        public async Task ChangeExpenseDescription(Contract.Mobile.V1.Expenses.ChangeDescriptionRequest request)
        {
            await _expensesServiceClient.ChangeDescription(new Commands.Expenses.V1.ChangeDescription()
            {
                CorrelationId = GetRequestCorrelationId(),
                Id = request.Id,
                Date = request.Date,
                TotalAmount = request.TotalAmount,
                Tags = request.Tags,
                Comments = request.Comments,
                Quantity = request.Quantity,
                Seller = new Commands.Expenses.V1.Seller()
                {
                    Location = request.Seller?.Location,
                    Name = request.Seller?.Name,
                    PostalCode = request.Seller?.PostalCode,
                    TaxNumber = request.Seller?.TaxNumber
                },
                Title = request.Title,
                UnitPrice = request.UnitPrice
            });
        }
    }
}
