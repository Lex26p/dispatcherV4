# Tag API

This document contains local examples for the tag endpoints.

## Endpoints

```text
GET  /api/tags/{id}
GET  /api/devices/{deviceId}/tags
POST /api/tags/modbus
POST /api/tags/snmp
POST /api/tags/{id}/enable
POST /api/tags/{id}/disable
```

## Example: create Modbus tag

```powershell
$body = @{
    deviceId = "00000000-0000-0000-0000-000000000000"
    name = "Temperature"
    code = "TEMP_001"
    registerType = 3
    address = 0
    unitId = 1
    dataType = 2
    unit = "°C"
    scale = 0.1
    offset = 0
    pollIntervalMs = 1000
    historyEnabled = $true
    byteOrder = 0
    wordOrder = 0
    description = "Demo Modbus temperature tag"
} | ConvertTo-Json

Invoke-RestMethod http://localhost:5076/api/tags/modbus -Method Post -Body $body -ContentType "application/json"
```

## Example: create SNMP tag

```powershell
$body = @{
    deviceId = "00000000-0000-0000-0000-000000000000"
    name = "System uptime"
    code = "SNMP_SYS_UPTIME"
    oid = "1.3.6.1.2.1.1.3.0"
    dataType = 6
    unit = "ticks"
    scale = 1
    offset = 0
    pollIntervalMs = 5000
    historyEnabled = $true
    description = "Demo SNMP uptime tag"
} | ConvertTo-Json

Invoke-RestMethod http://localhost:5076/api/tags/snmp -Method Post -Body $body -ContentType "application/json"
```

## Notes

The tag endpoints require a working PostgreSQL database and existing device rows. If PostgreSQL is not ready yet, the project can still be built, but data endpoints will fail at runtime when they try to access the database.
