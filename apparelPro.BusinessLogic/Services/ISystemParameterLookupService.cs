namespace apparelPro.BusinessLogic.Services
{
    // Small read-only lookup any service can depend on instead of querying
    // ApparelProDbContext.SystemParameters directly - keeps a given parameter
    // key's string literal and fallback default declared once (see
    // SystemParameterKeys) rather than copy-pasted into every consumer.
    public interface ISystemParameterLookupService
    {
        Task<string> GetValueAsync(string parameterKey, string defaultValue);
    }
}
