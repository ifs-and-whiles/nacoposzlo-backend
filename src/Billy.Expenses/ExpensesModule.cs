using Autofac;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Expenses.API.Commands;
using Billy.Expenses.API.Queries;
using Billy.Expenses.Domain.Expenses;
using Commands = Billy.Expenses.Contracts.Commands.Expenses.V1;
using Queries = Billy.Expenses.Contracts.Queries.Expenses.V1;

namespace Billy.Expenses
{
    public class ExpensesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ApplicationService<Expense,ExpenseStreamId>>();
            
            //Message handlers
            builder
                .RegisterType<CreateExpenseCommandHandler>()
                .As<IHandler<Commands.Create, Commands.CreateResponse>>();
            
            builder
                .RegisterType<ChangeDescriptionCommandHandler>()
                .As<IHandler<Commands.ChangeDescription>>();
            
            builder
                .RegisterType<ChangeTagsCommandHandler>()
                .As<IHandler<Commands.ChangeTags>>();
            
            builder
                .RegisterType<DeleteExpenseCommandHandler>()
                .As<IHandler<Commands.Delete>>();

            //Query handlers

            builder
                .RegisterType<GetExpenseQueryHandler>()
                .As<IHandler<Queries.GetExpense, Queries.ReadModels.Expense>>();
            
            builder
                .RegisterType<GetExpensesQueryHandler>()
                .As<IHandler<Queries.GetExpenses, Queries.PagedResult<Queries.ReadModels.Expense>>>();
        }

    }
}
