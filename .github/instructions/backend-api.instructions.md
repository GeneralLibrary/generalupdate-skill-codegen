---
name: "GeneralUpdate Server API"
description: "Backend API contract for GeneralUpdate version verification"
applyTo: "**/*.cs"
---

# GeneralUpdate Server API

## Verification Endpoint
POST /Upgrade/Verification
Request: appKey, appType, clientVersion, productId, platform, tenantId
Response: body[{ id, version, url, hash, size, name, appType, isCrossVersion }]

## Report Endpoint
POST /Upgrade/Report
Request: recordId, type(1=pull/2=push), status(0=updating/1=success/2=failed), message
