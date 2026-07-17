using Dispatcher.Domain.Common;

namespace Dispatcher.Domain.Tags;

public sealed class SnmpTagAddress
{
    public string Oid { get; private set; } = string.Empty;

    private SnmpTagAddress()
    {
    }

    public SnmpTagAddress(string oid)
    {
        Oid = oid;
        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Oid))
        {
            throw new DomainException("SNMP OID is required.");
        }

        var parts = Oid.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length < 2 || parts.Any(part => !int.TryParse(part, out var number) || number < 0))
        {
            throw new DomainException("SNMP OID must contain dot-separated non-negative numbers.");
        }
    }
}
