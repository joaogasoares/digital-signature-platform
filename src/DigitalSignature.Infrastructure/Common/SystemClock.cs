using DigitalSignature.Application.Common.Interfaces;

namespace DigitalSignature.Infrastructure.Common;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
