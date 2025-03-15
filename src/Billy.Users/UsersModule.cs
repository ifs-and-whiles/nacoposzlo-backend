using Autofac;
using Billy.CQRS;
using Billy.EventSourcing;
using Billy.Users.API.Commands;
using Billy.Users.API.Queries;
using Billy.Users.Contracts;
using Billy.Users.Domain;

namespace Billy.Users
{
    public class UsersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ApplicationService<User, UserStreamId>>();
            
            //Message handlers
            builder
                .RegisterType<AssignReceiptsRecognitionLimitCommandHandler>()
                .As<IHandler<Contracts.Commands.Users.V1.AssignReceiptsRecognitionLimit>>();
            
            builder
                .RegisterType<CreateUserCommandHandler>()
                .As<IHandler<Contracts.Commands.Users.V1.Create>>();
            
            builder
                .RegisterType<IncreaseReceiptsRecognitionCurrentPackageCounterCommandHandler>()
                .As<IHandler<Contracts.Commands.Users.V1.IncreaseReceiptsRecognitionCurrentPackageCounter>>();
            
            builder
                .RegisterType<ResetReceiptsRecognitionCurrentPackageCounterCommandHandler>()
                .As<IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounter>>();
            
            //Query handlers
            builder
                .RegisterType<GetUserQueryHandler>()
                .As<IHandler<Contracts.Queries.Users.V1.GetUser, Contracts.Queries.Users.V1.ReadModels.User>>();
            
            builder
                .RegisterType<GetUserReceiptsRecognitionUsageStatusQueryHandler>()
                .As<IHandler<Queries.Users.V1.GetUserReceiptsRecognitionUsageStatus, Queries.Users.V1.ReadModels.ReceiptsRecognitionUsage>>();

            builder
                .RegisterType<ResetReceiptsRecognitionCurrentPackageCounterForAllUsersCommandHandler>()
                .As<IHandler<Contracts.Commands.Users.V1.ResetReceiptsRecognitionCurrentPackageCounterForAllUsers>>();
        }
    }
}