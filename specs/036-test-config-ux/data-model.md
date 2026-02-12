# Data Model: 036-test-config-ux

**Date**: 2026-02-12

## Overview

This feature is UI-only. No data model changes are required. The existing entities remain unchanged.

## Existing Entities (No Changes)

### TestDefinition
- Id: string
- Type: string
- DisplayName: string
- Description: string?
- Parameters: JsonObject
- FieldPolicy: Dictionary<string, FieldPolicyType>
- Timeout: int?
- RetryCount: int?
- RequiresAdmin: bool
- DependsOn: List<string>

### FieldPolicyType (Enum)
- Editable
- Locked
- Hidden
- PromptAtRun

### TestParameterViewModel (Nested in TestConfigViewModel)
- Name: string
- Value: string
- Policy: FieldPolicyType
- IsLocked: bool (computed)
- IsHidden: bool (computed)
- IsEditable: bool (computed)
- IsPromptAtRun: bool (computed)
- Label: string (computed from Name)

## State Transitions

No changes. The form continues to operate with:
- Load → Display (read-only + editable fields)
- Edit → Modified (dirty state)
- Save → Persisted (async save with IsSaving flag)
- Cancel → Reverted (original values restored)
