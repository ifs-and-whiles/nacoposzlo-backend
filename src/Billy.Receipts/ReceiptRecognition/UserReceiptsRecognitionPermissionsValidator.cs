using System;
using System.Threading.Tasks;
using Billy.Receipts.Permissions;
using Billy.Receipts.UsersService;
using Serilog;

namespace Billy.Receipts.ReceiptRecognition
{
    public class UserReceiptsRecognitionPermissionsValidator
    {
        private readonly UsersServiceClient _usersServiceClient;
        private readonly ILogger _logger = Log.ForContext<UserReceiptsRecognitionPermissionsValidator>();

        public UserReceiptsRecognitionPermissionsValidator(UsersServiceClient usersServiceClient)
        {
            _usersServiceClient = usersServiceClient;
        }

        // It's not good place for it. In the future we should think about more generic solution.
        // Access should be granted by special service. Receipts domain should not have this responsibility
        // To simplify flow and solution logic has been put here.
        public async Task EnsureThatUserHasPermissionsToRecognizeReceiptOrThrow(
            string globalUserIdentifier,
            Guid correlationId)
        {
            var recognitionUsageStatus = await _usersServiceClient.GetReceiptsRecognitionUsageStatus(
                globalUserIdentifier,
                correlationId);

            _logger
                .ForContext("GlobalUserIdentifier", globalUserIdentifier)
                .ForContext("CorrelationId", correlationId)
                .Information("Recognition usage status checked. {@recognitionUsageStatus}", recognitionUsageStatus);

            if (recognitionUsageStatus.LimitExceeded)
            {
                throw new ReceiptsRecognitionLimitExceededException(
                    $"Receipts recognition limit has been reached for user {globalUserIdentifier}. Access denied to receipts recognition",
                    SecurityErrorCodes.ReceiptsRecognitionLimitExceeded);
            }
        }
    }

    public class ReceiptsRecognitionLimitExceededException : SecurityException
    {
        public ReceiptsRecognitionLimitExceededException(string message, string code) : base(message, code)
        {
            
        }
    }
    
    
    
}