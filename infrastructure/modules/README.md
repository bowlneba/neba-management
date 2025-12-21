# Infrastructure Modules

This folder contains reusable Bicep modules used by higher-level deployment templates.

## `storageAccount` module

Purpose: provisions a StorageV2 account with recommended defaults for secure deployments.

Key security defaults and notes:

- `requireInfrastructureEncryption` (parameter, default: `true`)
  - Enforces infrastructure (double) encryption where supported. This prevents accidental omission of the setting and helps meet stricter compliance requirements.
  - Not all SKUs/regions support infrastructure encryption; validate target combinations before production rollout.

- Managed identity (`identity` block): NOT enabled by default
  - The module intentionally omits an `identity` block by default. This is safe if the storage account does not need to authenticate to other Azure resources.
  - Pros of omission: simpler deployment surface, no automatic principal to manage.
  - Risk: if you need the storage account to access Key Vault (for CMK) or perform identity-based operations, omission prevents identity-based RBAC workflows and may push consumers toward account keys or less secure patterns.

Recommendations

- If you do NOT need the storage account itself to act as an identity:
  - Keep the current behavior and ensure client resources (VMs, Functions, App Services) use Managed Identities to access storage via Azure AD.
  - Continue to set `allowSharedKeyAccess: false` and avoid emitting connection strings or keys from templates.

- If you DO need the storage account to have an identity (e.g., for CMK key access): enable an identity and assign roles. Two options:
  1. System-assigned identity (simple):

```bicep
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  // ...
  identity: {
    type: 'SystemAssigned'
  }
  // ...
}
```

After deployment, assign Key Vault access (or required RBAC) to the `principalId` of the storage account.

  2. User-assigned identity (recommended when you want lifecycle control / reuse):

```bicep
param userAssignedIdentityResourceId string

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  // ...
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityResourceId}': {}
    }
  }
  // ...
}
```

Then grant the identity permissions in Key Vault or via RBAC as appropriate.

Verification

Use the Azure CLI to confirm identity and encryption settings:

```bash
az storage account show --name <name> --resource-group <rg> --query "identity" -o json
az storage account show --name <name> --resource-group <rg> --query "properties.encryption" -o json
```

If you want, I can: (A) add an optional `identity` parameter to this module (system- or user-assigned) and example role-assignment guidance, or (B) keep the README-only change.``
