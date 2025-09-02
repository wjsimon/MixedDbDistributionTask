using Grpc.Core;
using MixedDbDistributionTask.Data;
using MixedDbDistributionTask.Services;

namespace MixedDbDistributionTask.Classes
{
    internal static class ApiAllowance
    {
        public static bool AllowPublic(DatabaseCreationService dbcs, ServerCallContext context, out DbIndex? masterIndex)
        {
            if (dbcs.MasterAvailable)
            {
                masterIndex = dbcs.MasterIndex;
                return true;
            }
            else
            {
                masterIndex = null;
                return false;
            }
        }

        public static bool AllowGlobal(DatabaseCreationService dbcs, ServerCallContext context, out DbIndex? masterIndex) //much bigger check for the less oppressive policy?
        {
            var tenantId = context.UserState["tenant"]?.ToString() ?? string.Empty;
            if (tenantId != null && dbcs.MasterAvailable && dbcs.TenantValid(tenantId))
            {
                masterIndex = dbcs.MasterIndex;
                return true;
            }
            else
            {
                masterIndex = null;
                return false;
            }
        }

        public static bool AllowLocal(DatabaseCreationService dbcs, ServerCallContext context, out DbIndex dbIndex) //if db returns, api key was succesfully matched to a database
        {
            return dbcs.TryGetIndex(context.UserState["tenant"].ToString()!, out dbIndex);
        }
    }
}
