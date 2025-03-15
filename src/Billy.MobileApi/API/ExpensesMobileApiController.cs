using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpensesMobileContract = Billy.MobileApi.Contracts.Expenses.Contract.Mobile.V1.Expenses;
using Commands = Billy.Expenses.Contracts.Commands.Expenses.V1;
using Queries = Billy.Expenses.Contracts.Queries.Expenses.V1;

namespace Billy.MobileApi.API
{
    [Authorize(Policy = "ExternalApiUsers")]
    [ApiController, Route(MobileApiRouteConsts.Prefix + "/v1/expenses")]
    public class ExpensesMobileApiController : MobileApiControllerBase
    {
        private readonly IHandler<Queries.GetExpenses, Queries.PagedResult<Queries.ReadModels.Expense>> _getExpensesQueryHandler;
        private readonly IHandler<Queries.GetExpense, Queries.ReadModels.Expense> _getExpenseQueryHandler;
        private readonly IHandler<Commands.Create, Commands.CreateResponse> _createExpenseCommandHandler;
        private readonly IHandler<Commands.Delete> _deleteExpenseCommandHandler;
        private readonly IHandler<Commands.ChangeDescription> _changeDescriptionCommandHandler;
        private readonly IHandler<Commands.ChangeTags> _changeExpenseTagsCommandHandler;

        public ExpensesMobileApiController(
            IHandler<Queries.GetExpenses, Queries.PagedResult<Queries.ReadModels.Expense>> getExpensesQueryHandler,
            IHandler<Queries.GetExpense, Queries.ReadModels.Expense> getExpenseQueryHandler,
            IHandler<Commands.Create, Commands.CreateResponse> createExpenseCommandHandler,
            IHandler<Commands.Delete> deleteExpenseCommandHandler,
            IHandler<Commands.ChangeDescription> changeDescriptionCommandHandler,
            IHandler<Commands.ChangeTags> changeExpenseTagsCommandHandler)
        {
            _getExpensesQueryHandler = getExpensesQueryHandler;
            _getExpenseQueryHandler = getExpenseQueryHandler;
            _createExpenseCommandHandler = createExpenseCommandHandler;
            _deleteExpenseCommandHandler = deleteExpenseCommandHandler;
            _changeDescriptionCommandHandler = changeDescriptionCommandHandler;
            _changeExpenseTagsCommandHandler = changeExpenseTagsCommandHandler;
        }
        
        [HttpGet]
        [Route("get-all-for-user")]
        public async Task<ExpensesMobileContract.PagedResult<ExpensesMobileContract.Expense>> GetAllForUser(
            [FromQuery]ExpensesMobileContract.GetAllExpensesForUser request)
        {
            var expensesPagedResult = await _getExpensesQueryHandler
                .Handle(new MessageContext<Queries.GetExpenses>
                {
                    Message = new Queries.GetExpenses
                    {
                        GlobalUserIdentifier = GetUserId(),
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    },
                    CorrelationId = GetRequestCorrelationId(),
                });
            
            return new ExpensesMobileContract.PagedResult<ExpensesMobileContract.Expense>()
            {
                Items = expensesPagedResult.Items.Select(MapExpense).ToList(),
                TotalCount = expensesPagedResult.TotalCount
            };
        }
        
        [HttpGet]
        [Route("get-single")]
        public async Task<ExpensesMobileContract.Expense> GetExpense(
            [FromQuery] ExpensesMobileContract.GetSingleExpenseRequest request)
        {
            var expense = await _getExpenseQueryHandler
                .Handle(new MessageContext<Queries.GetExpense>
                {
                    Message = new Queries.GetExpense()
                    {
                        ExpenseId = request.Id,
                        GlobalUserIdentifier = GetUserId()
                    },
                    CorrelationId = GetRequestCorrelationId()
                });

            return MapExpense(expense);
        }

        private ExpensesMobileContract.Expense MapExpense(
            Queries.ReadModels.Expense serviceExpense)
        {
            return new ExpensesMobileContract.Expense()
            {
                Id = serviceExpense.Id,
                Date = serviceExpense.Date,
                Comments = serviceExpense.Comments,
                Quantity = serviceExpense.Quantity,
                ReceiptId = serviceExpense.ReceiptId,
                Seller = new ExpensesMobileContract.Seller()
                {
                    Location = serviceExpense.Seller?.Location,
                    Name = serviceExpense.Seller?.Name,
                    PostalCode = serviceExpense.Seller?.PostalCode,
                    TaxNumber = serviceExpense.Seller?.TaxNumber
                },
                Tags = serviceExpense.Tags,
                Title = serviceExpense.Title,
                TotalAmount = serviceExpense.TotalAmount,
                UnitPrice = serviceExpense.UnitPrice
            };
        }

        [HttpPost, Route("create")]
        public async Task<Guid> CreateExpense(ExpensesMobileContract.CreateSingleRequest request)
        {
            var response = await _createExpenseCommandHandler
                .Handle(new MessageContext<Commands.Create>
                {
                    Message = new Commands.Create()
                    {
                        GlobalUserIdentifier = GetUserId(),
                        Date = request.Date,
                        Comments = request.Comments,
                        Quantity = request.Quantity,
                        ReceiptId = request.ReceiptId,
                        Seller = new Commands.Seller()
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
                    },
                    CorrelationId = GetRequestCorrelationId()
                });

            return response.ExpenseId;
        }


        [HttpPost, Route("bulk-create")]
        public async Task<List<ExpensesMobileContract.BulkOperationItemResult<Guid>>> CreateExpense(
            ExpensesMobileContract.BulkCreateRequest request)
        {
            var tasks = request.Items.Select(async bulkOperationItem =>
            {
                try
                {
                    var expenseId = await CreateExpense(bulkOperationItem.Entity);
                    return new ExpensesMobileContract.BulkOperationItemResult<Guid>()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.Ok,
                        OperationId = bulkOperationItem.OperationId,
                        EntityId = expenseId
                    };
                }
                catch (Exception)
                {
                    return new ExpensesMobileContract.BulkOperationItemResult<Guid>()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.UnknownError,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
                
            }).ToList();
            
            var operations = await Task.WhenAll(tasks);

            return operations.ToList();
        }


        [HttpPost, Route("delete")]
        public async Task DeleteExpense(ExpensesMobileContract.DeleteSingleRequest request)
        {
            await _deleteExpenseCommandHandler
                .Handle(new MessageContext<Commands.Delete>
                {
                    Message = new Commands.Delete()
                    {
                        GlobalUserIdentifier = GetUserId(),
                        ExpenseId = request.Id
                    },
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("bulk-delete")]
        public async Task<List<ExpensesMobileContract.BulkOperationItemResult>> DeleteExpense(
            ExpensesMobileContract.BulkDeleteRequest request)
        {
            var tasks = request.Items.Select(async bulkOperationItem =>
            {
                try
                {
                    await DeleteExpense(bulkOperationItem.Entity);
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.Ok,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
                catch (Exception)
                {
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.UnknownError,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
            }).ToList();

            var operations = await Task.WhenAll(tasks);

            return operations.ToList();
        }

        [HttpPost, Route("update")]
        public async Task ChangeExpenseDescription(ExpensesMobileContract.ChangeDescriptionRequest request)
        {
            await _changeDescriptionCommandHandler
                .Handle(new MessageContext<Commands.ChangeDescription>
                {
                    Message = new Commands.ChangeDescription()
                    {
                        GlobalUserIdentifier = GetUserId(),
                        ExpenseId = request.Id,
                        Date = request.Date,
                        TotalAmount = request.TotalAmount,
                        Tags = request.Tags,
                        Comments = request.Comments,
                        Quantity = request.Quantity,
                        Seller = new Commands.Seller()
                        {
                            Location = request.Seller?.Location,
                            Name = request.Seller?.Name,
                            PostalCode = request.Seller?.PostalCode,
                            TaxNumber = request.Seller?.TaxNumber
                        },
                        Title = request.Title,
                        UnitPrice = request.UnitPrice
                    },
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("bulk-update")]
        public async Task<List<ExpensesMobileContract.BulkOperationItemResult>> BulkChangeExpenseDescriptions(
            ExpensesMobileContract.BulkChangeDescriptionsRequest request)
        {
            var tasks = request.Items.Select(async bulkOperationItem =>
            {
                try
                {
                    await ChangeExpenseDescription(bulkOperationItem.Entity);
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.Ok,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
                catch (Exception)
                {
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.UnknownError,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
            }).ToList();

            var operations = await Task.WhenAll(tasks);

            return operations.ToList();
        }

        [HttpPost, Route("change-tags")]
        public async Task ChangeExpenseTags(ExpensesMobileContract.ChangeTagsRequest request)
        {
            await _changeExpenseTagsCommandHandler
                .Handle(new MessageContext<Commands.ChangeTags>
                {
                    Message = new Commands.ChangeTags()
                    {
                        GlobalUserIdentifier = GetUserId(),
                        ExpenseId = request.Id,
                        Tags = request.Tags
                    },
                    CorrelationId = GetRequestCorrelationId()
                });
        }

        [HttpPost, Route("bulk-change-tags")]
        public async Task<List<ExpensesMobileContract.BulkOperationItemResult>> BulkChangeExpenseTags(
            ExpensesMobileContract.BulkChangeTagsRequest request)
        {
            var tasks = request.Items.Select(async bulkOperationItem =>
            {
                try
                {
                    await ChangeExpenseTags(bulkOperationItem.Entity);
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.Ok,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
                catch (Exception)
                {
                    return new ExpensesMobileContract.BulkOperationItemResult()
                    {
                        Status = ExpensesMobileContract.BulkOperationStatus.UnknownError,
                        OperationId = bulkOperationItem.OperationId
                    };
                }
            }).ToList();

            var operations = await Task.WhenAll(tasks);

            return operations.ToList();
        }
    }
}