namespace MixedDbDistributionTask.Dashboard.Classes
{
    public readonly record struct QueryInfo(string Id, string[] ParamIds, QueryInfoScope Scope);
    public enum QueryInfoScope
    {
        Master,
        Tenant
    }
}
