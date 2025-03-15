using System;
using Billy.CodeReadability;
using Billy.Domain;
using Billy.EventSourcing;

namespace Billy.Users.Domain
{
    public class User : AggregateRoot<UserStreamId>
    {
        public UserId UserId { get; private set; }
        public GlobalUserIdentifier GlobalUserIdentifier { get; private set; }
        public ReceiptsRecognitionUsage ReceiptsRecognitionUsage { get; private set; }

        public static User Create(
            UserId userId, 
            GlobalUserIdentifier globalUserIdentifier,
            ReceiptsRecognitionUsage receiptsRecognitionUsage,
            TermsAndPrivacyPolicy termsAndPrivacyPolicy)
        {
            var user = new User();
            
            user.Apply(new Events.Users.V1.UserAdded(
                userId,
                globalUserIdentifier,
                receiptsRecognitionUsage.Limit,
                receiptsRecognitionUsage.CurrentPackageCounter,
                new Events.Users.V1.TermsAndPrivacyPolicy()
                {
                    WasTermsAndPrivacyPolicyAccepted = termsAndPrivacyPolicy.WasTermsAndPrivacyPolicyAccepted,
                    DateOfConsents = termsAndPrivacyPolicy.DateOfConsents
                }));

            if(receiptsRecognitionUsage.LimitReached)
                user.Apply(new Events.Users.V1.ReceiptsRecognitionLimitReached(userId));
            
            return user;
        }

        public void IncreaseReceiptsRecognitionCurrentPackageCounter()
        {
            if(IsReceiptsRecognitionLimitAlreadyReached())
                throw new DomainException(
                    $"Receipts recognition limit has been reached. " +
                    $"Limit: {ReceiptsRecognitionUsage.Limit}, " +
                    $"current counter: {ReceiptsRecognitionUsage.CurrentPackageCounter}", 
                    ErrorCodes.ReceiptsRecognitionLimitReached);
            
            Apply(new Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased(UserId));
            
            if(HasTheLimitBeenJustReached())
                Apply(new Events.Users.V1.ReceiptsRecognitionLimitReached(UserId));
        }

        private bool HasTheLimitBeenJustReached() => 
            ReceiptsRecognitionUsage.LimitReached;

        private bool IsReceiptsRecognitionLimitAlreadyReached() => 
            ReceiptsRecognitionUsage.LimitReached;

        public void ResetReceiptsRecognitionCurrentPackageCounter()
        {
            Apply(new Events.Users.V1.ReceiptsRecognitionCounterZeroed(UserId));
        }

        public void AssignReceiptsRecognitionLimit(ReceiptsRecognitionLimit receiptsRecognitionLimit)
        {
            if(receiptsRecognitionLimit < ReceiptsRecognitionUsage.CurrentPackageCounter 
               && !receiptsRecognitionLimit.IsUnlimited)
                throw new DomainException(
                    $"New receipts recognition limit {receiptsRecognitionLimit} " +
                    $"can not be less than current package receipts recognition " +
                    $"counter {ReceiptsRecognitionUsage.CurrentPackageCounter}.",
                    ErrorCodes.ReceiptsRecognitionLimitLessThatCurrentCounter);
            
            Apply(new Events.Users.V1.ReceiptsRecognitionLimitAssigned(UserId, receiptsRecognitionLimit));
        }
        
        protected override void When(object @event)
        {
            switch (@event)
            {
                case Events.Users.V1.UserAdded e:
                    Id = UserStreamId.From(GlobalUserIdentifier.From(e.GlobalUserIdentifier));
                    UserId = UserId.From(e.Id);
                    GlobalUserIdentifier = GlobalUserIdentifier.From(e.GlobalUserIdentifier);
                    ReceiptsRecognitionUsage = ReceiptsRecognitionUsage.From(
                        ReceiptsRecognitionLimit.From(e.ReceiptsRecognitionLimit),
                        ReceiptsRecognitionCurrentPackageCounter.From(e.ReceiptsRecognitionCurrentPackageCounter));
                    break;
                case Events.Users.V1.ReceiptsRecognitionCurrentPackageCounterIncreased e:
                    ReceiptsRecognitionUsage.IncreaseCounter();
                    break;
                case Events.Users.V1.ReceiptsRecognitionCounterZeroed e:
                    ReceiptsRecognitionUsage.ResetCounter();
                    break;
                case Events.Users.V1.ReceiptsRecognitionLimitAssigned e:
                    ReceiptsRecognitionUsage.AssignLimit(e.Limit);
                    break;
            }
        }

        protected override void EnsureValidState()
        {

        }
    }
}