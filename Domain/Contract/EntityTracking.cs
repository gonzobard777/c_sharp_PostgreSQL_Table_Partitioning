namespace Domain.Contract;

public enum EntityTracking
{
    Enabled,
    Disabled, // .AsNoTracking()
    DisabledWithIdentityResolution, // .AsNoTrackingWithIdentityResolution()
}