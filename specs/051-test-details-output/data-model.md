# Data Model: Test Details Output for All Test Types

**Date**: 2026-02-23 | **Feature**: 051-test-details-output

## Overview

This feature adds no new entities. It maps existing evidence dictionary keys (from `TestEvidence.ResponseData` JSON) to formatted display strings in the converter output. The mappings below define what each new section renders.

## Evidence Key → Section Mappings

### [Ping] Section
Detection: `successRate` AND `pingResults`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `host` | Host: | string | yes |
| `successfulCount` / `totalCount` | Success: | `{successfulCount}/{totalCount}` | yes |
| `successRate` | Rate: | string (already formatted, e.g., "100%") | yes |
| `averageRoundtripTime` | Avg RTT: | string (e.g., "12ms") | yes |
| `pingResults` | (sub-items) | JSON array → indented per-attempt lines | no |

### [DNS] Section
Detection: `hostname` AND `addresses`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `hostname` | Hostname: | string | yes |
| `addresses` | (sub-items) | JSON array → indented IP lines | yes |
| `addressCount` | Addresses: | integer | yes |
| `resolutionTimeMs` | Resolution: | `{value} ms` | no |

### [TCP] Section
Detection: `host` AND `port` AND `connected`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `host` | Host: | string | yes |
| `port` | Port: | integer | yes |
| `connected` | Connected: | boolean → yes/no | yes |
| `connectTimeMs` | Connect: | `{value} ms` | no |

### [UDP] Section
Detection: `responded` AND `payloadSentBytes`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `responded` | Responded: | boolean → yes/no | yes |
| `roundTripTimeMs` | RTT: | `{value} ms` | no |
| `payloadSentBytes` | Sent: | `{value} bytes` | no |
| `payloadReceivedBytes` | Received: | `{value} bytes` | no |
| `responseDataPreview` | Data: | string (truncated preview) | no |

### [Disk Space] Section
Detection: `totalSpaceGB` AND `freeSpaceGB`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `path` | Path: | string | yes |
| `totalSpaceGB` | Total: | `{value} GB` | yes |
| `freeSpaceGB` | Free: | `{value} GB` | yes |
| `percentFree` | Free: | `{value}%` (on same concept, separate line) | no |
| `minimumFreeGB` | Minimum: | `{value} GB` | no |
| `thresholdMet` | Threshold: | boolean → met/not met | no |

### [Service] Section
Detection: `serviceName` AND `expectedStatus`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `serviceName` | Service: | string | yes |
| `displayName` | Display: | string | no |
| `status` | Status: | string | yes |
| `expectedStatus` | Expected: | string | yes |
| `startType` | Start Type: | string | no |
| `statusMatch` | Match: | boolean → yes/no | no |

### [mTLS] Section
Detection: `certificateSubject` AND `certificateThumbprint`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `connected` | Connected: | boolean → yes/no | yes |
| `responseTimeMs` | Response: | `{value} ms` | no |
| `certificateSubject` | Subject: | string | yes |
| `certificateIssuer` | Issuer: | string | no |
| `certificateThumbprint` | Thumbprint: | string | yes |
| `certificateNotBefore` | Valid From: | date string | no |
| `certificateNotAfter` | Valid To: | date string | no |
| `certificateHasPrivateKey` | Private Key: | boolean → yes/no | no |

### [Certificate] Section
Detection: `daysUntilExpiry` AND `isExpired`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `host` | Host: | string | yes |
| `port` | Port: | integer | no |
| `subject` | Subject: | string | yes |
| `issuer` | Issuer: | string | no |
| `thumbprint` | Thumbprint: | string | no |
| `notAfter` | Expires: | date string | yes |
| `daysUntilExpiry` | Days Left: | integer | yes |
| `isExpired` | Expired: | boolean → yes/no | yes |
| `isNotYetValid` | Not Yet Valid: | boolean → yes/no (omit if false) | no |

### [File] Section
Detection: `path` AND `exists` AND `size`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `path` | Path: | string | yes |
| `exists` | Exists: | boolean → yes/no | yes |
| `shouldExist` | Expected: | boolean → yes/no | no |
| `size` | Size: | formatted bytes | no |
| `lastModified` | Modified: | date string | no |

### [Directory] Section
Detection: `path` AND `exists` AND `directoryCount`

| Evidence Key | Display Label | Format | Required |
|-------------|--------------|--------|----------|
| `path` | Path: | string | yes |
| `exists` | Exists: | boolean → yes/no | yes |
| `shouldExist` | Expected: | boolean → yes/no | no |
| `fileCount` | Files: | integer | no |
| `directoryCount` | Directories: | integer | no |
| `creationTime` | Created: | date string | no |
