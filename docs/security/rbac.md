# RBAC baseline

Step 5 introduces the first identity and authorization baseline for the industrial Dispatcher project.

## Scope

Implemented in this step:

- `UserAccount`
- `Role`
- `PermissionScope`
- `RoleAssignment`
- backend permission filter for protected identity endpoints
- development current user from HTTP headers
- seeded development administrator
- last-admin revoke guard

## Development current user

Until the real authentication provider is selected, the API uses a development header user.

Default behavior:

- user id: `dev-admin`
- display name: `Development Administrator`
- permissions: all baseline permissions plus wildcard

Useful smoke-test headers:

```powershell
@{ 'X-Dispatcher-Permissions' = 'identity.me.view' }
```

A route hidden in the UI is not security. Every protected endpoint uses backend permission checks.

## Known limitations

- No production authentication provider yet.
- No password or identity provider integration yet.
- No audit table yet; audit hooks become real in the audit step.
- UI routes are skeleton routes; backend is the source of authorization truth.
