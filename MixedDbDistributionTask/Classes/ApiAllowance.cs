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
            string[] grants = (string[])context.UserState["grants"];
            if (grants != null && dbcs.MasterAvailable && grants.Any(dbcs.TenantValid))
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

        public static bool AllowLocal(DatabaseCreationService dbcs, ServerCallContext context, string requestedTenant, out DbIndex? dbIndex) //if db returns, api key was succesfully matched to a database
        {
            if (((string[])context.UserState["grants"]).Contains(requestedTenant))
            {
                var success = dbcs.TryGetIndex(requestedTenant, out DbIndex found);
                
                if (success) { dbIndex = found; }
                else { dbIndex = null; }

                return success;
            }
            else
            {
                dbIndex = null;
                return false;
            }
        }
    }
}
