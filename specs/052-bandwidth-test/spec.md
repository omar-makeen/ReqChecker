# Feature Specification: Bandwidth Test Type

**Feature Branch**: `052-bandwidth-test`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "I need to add new test Bandwidth | Minimum download throughput check | url, minimumMbps, durationSeconds"

## Clarifications

### Session 2026-02-23

- Q: How many decimal places should measured throughput display in the `[Bandwidth]` details section? → A: Two decimal places (e.g., `25.47 Mbps`).
- Q: What label names and order should the `[Bandwidth]` details section use? → A: Concise labels with aligned padding: `URL`, `Speed`, `Minimum`, `Downloaded`, `Duration`, `Threshold` (value: `met` / `not met`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Download Throughput Check (Priority: P1)

A network engineer configures a Bandwidth test in a profile to verify that the download throughput to a given URL meets a minimum threshold. The test downloads data from the specified URL for up to `durationSeconds`, measures the actual throughput in megabits per second (Mbps), and compares it against the `minimumMbps` threshold. The result shows whether the throughput requirement was met along with the measured speed.

**Why this priority**: The core purpose of this test type is to validate minimum download bandwidth. Without throughput measurement and threshold comparison, the test has no value.

**Independent Test**: Can be fully tested by configuring a Bandwidth test with a publicly accessible URL hosting a large file (e.g., a speed test file), a `minimumMbps` of 1, and a `durationSeconds` of 5, running it, and verifying the result shows pass/fail with measured throughput in the evidence.

**Acceptance Scenarios**:

1. **Given** a profile with a Bandwidth test specifying a reachable `url`, `minimumMbps` of 10, and `durationSeconds` of 10, **When** the test runs and actual throughput is 25 Mbps, **Then** the result is Pass with evidence showing the measured throughput, minimum threshold, download size, and duration.
2. **Given** a profile with a Bandwidth test specifying a reachable `url` and `minimumMbps` of 100, **When** the test runs and actual throughput is 45 Mbps, **Then** the result is Fail with evidence showing the measured throughput fell below the minimum threshold.
3. **Given** a profile with a Bandwidth test specifying a `url` that is unreachable, **When** the test runs, **Then** the result is Fail with an error message indicating the URL could not be reached.

---

### User Story 2 - Duration-Bounded Measurement (Priority: P1)

An IT administrator wants bandwidth measurements to complete within a bounded time window to keep overall test runs predictable. They set `durationSeconds` to control how long the download runs. The test stops downloading after the duration elapses and calculates throughput based on the data received up to that point, even if the download did not complete.

**Why this priority**: Without a time bound, a slow connection could cause the test to run indefinitely (or until the file is fully downloaded). Duration control is essential for a predictable, automatable test.

**Independent Test**: Can be tested by configuring a Bandwidth test with `durationSeconds` set to 3 against a large file URL, running it, and verifying the test completes within approximately 3 seconds and reports measured throughput based on the data downloaded in that window.

**Acceptance Scenarios**:

1. **Given** a profile with a Bandwidth test where `durationSeconds` is 5, **When** the test runs against a URL serving a large file, **Then** the test stops downloading after approximately 5 seconds and reports throughput based on the bytes received during that window.
2. **Given** a profile with a Bandwidth test where the file is smaller than what could be downloaded in `durationSeconds`, **When** the test runs and the file completes before the duration elapses, **Then** the test calculates throughput based on the total file size and the actual elapsed time.
3. **Given** a profile with a Bandwidth test where `durationSeconds` is omitted, **When** the test runs, **Then** a default duration of 10 seconds is used.

---

### User Story 3 - Configuration Validation (Priority: P2)

A user misconfigures a Bandwidth test with missing or invalid parameters. The test detects the configuration error before attempting any network activity and reports a clear, actionable error message identifying what is wrong.

**Why this priority**: Early validation with clear error messages prevents confusing test failures and speeds up profile debugging.

**Independent Test**: Can be tested by configuring a Bandwidth test with missing `url`, zero `minimumMbps`, or negative `durationSeconds`, running it, and verifying each produces a specific configuration error without any network calls.

**Acceptance Scenarios**:

1. **Given** a profile with a Bandwidth test where `url` is empty or missing, **When** the test runs, **Then** the result is Fail with a configuration error stating the url parameter is required.
2. **Given** a profile with a Bandwidth test where `minimumMbps` is zero or negative, **When** the test runs, **Then** the result is Fail with a configuration error stating minimumMbps must be a positive number.
3. **Given** a profile with a Bandwidth test where `durationSeconds` is zero or negative, **When** the test runs, **Then** the result is Fail with a configuration error stating durationSeconds must be a positive number.
4. **Given** a profile with a Bandwidth test where `url` does not start with `http://` or `https://`, **When** the test runs, **Then** the result is Fail with a configuration error stating the URL must use HTTP or HTTPS.

---

### Edge Cases

- What happens when the server returns an HTTP error (e.g., 403, 404, 500)? The test fails with an error message including the HTTP status code, before any throughput calculation.
- What happens when the connection is dropped mid-download? The test calculates throughput based on the bytes successfully received up to the disconnection point. If zero bytes were received, the test fails with a connection error.
- What happens when the server throttles or rate-limits the download? The test reports the actual measured throughput (which will reflect the throttled rate) and passes or fails based on the threshold comparison.
- What happens when the URL redirects? The test follows HTTP redirects (up to a reasonable limit) and measures throughput from the final URL.
- What happens when `minimumMbps` is omitted? The test defaults to 0, meaning it passes as long as any data is downloaded (effectively a connectivity check).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `Bandwidth` test type that downloads data from a specified URL, measures download throughput in megabits per second (Mbps), and compares it against a minimum threshold.
- **FR-002**: System MUST accept a required `url` parameter specifying the HTTP or HTTPS URL to download from.
- **FR-003**: System MUST accept an optional `minimumMbps` parameter (positive number) specifying the minimum acceptable throughput in Mbps, defaulting to 0.
- **FR-004**: System MUST accept an optional `durationSeconds` parameter (positive integer) specifying the maximum download duration, defaulting to 10.
- **FR-005**: System MUST stop downloading after `durationSeconds` elapses and calculate throughput based on the bytes received in that window.
- **FR-006**: System MUST calculate throughput as: (total bytes downloaded * 8) / (elapsed seconds * 1,000,000), expressed in Mbps.
- **FR-007**: System MUST report Pass when the measured throughput meets or exceeds `minimumMbps`, and Fail when it does not.
- **FR-008**: System MUST capture evidence including: URL, measured throughput (Mbps), minimum threshold (Mbps), total bytes downloaded, elapsed time, and whether the threshold was met.
- **FR-009**: System MUST validate that `url` starts with `http://` or `https://` and fail with a configuration error otherwise.
- **FR-010**: System MUST validate that `url` is not empty and fail with a configuration error if missing.
- **FR-011**: System MUST validate that `minimumMbps` is non-negative and `durationSeconds` is positive, failing with descriptive configuration errors for invalid values.
- **FR-012**: System MUST report distinct error messages for: missing URL, invalid URL scheme, unreachable host, HTTP error responses, and throughput below threshold.
- **FR-013**: System MUST follow HTTP redirects when downloading.
- **FR-014**: System MUST display bandwidth test evidence in the results details view under a `[Bandwidth]` section with concise aligned labels in this order: `URL` (target URL), `Speed` (measured throughput to two decimal places, e.g., `25.47 Mbps`), `Minimum` (threshold to two decimal places, e.g., `10.00 Mbps`), `Downloaded` (human-readable byte format, e.g., `31.84 MB`), `Duration` (elapsed seconds to two decimal places, e.g., `10.02 s`), `Threshold` (`met` or `not met`).
- **FR-015**: System MUST include the Bandwidth test type in the conditional build manifest so it can be included or excluded via the `IncludeTests` build parameter.
- **FR-016**: System MUST update the README to document the Bandwidth test type, its parameters, and usage examples.
- **FR-017**: System MUST update the built-in test type count across the README and TestManifest.props comment to reflect the addition.

### Key Entities

- **Bandwidth Test Parameters**: Configuration for the test including the target download URL, minimum throughput threshold in Mbps, and maximum download duration in seconds.
- **Bandwidth Evidence**: Runtime data captured during test execution including the target URL, measured throughput (Mbps), minimum threshold (Mbps), total bytes downloaded, elapsed time in seconds, and whether the threshold was met.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can validate download bandwidth by configuring and running a Bandwidth test, receiving a clear pass/fail result with measured throughput within the configured duration.
- **SC-002**: Test evidence displays all relevant bandwidth details (URL, measured Mbps, minimum threshold, bytes downloaded, elapsed time) in the results view.
- **SC-003**: Error messages clearly distinguish between configuration errors (missing URL, invalid parameters), connection failures (host unreachable, HTTP errors), and threshold failures (throughput below minimum).
- **SC-004**: The test integrates consistently with existing application features: test selection, dependency chaining, retry logic, result history, and PDF export all work with Bandwidth results.

## Assumptions

- The test measures download throughput only (not upload). The URL should point to a resource that returns a reasonably large response body (e.g., a test file or any large downloadable resource). Small responses (e.g., a few KB HTML page) will yield inaccurate throughput measurements.
- Throughput is calculated over the actual elapsed wall-clock time, not just transfer time. This means connection setup overhead is included in the measurement, reflecting real-world user experience.
- The test uses a single HTTP GET request. It does not use multiple parallel connections (as some speed test tools do). The measured throughput reflects single-stream download performance.
- The test does not validate the content of the response body. Any bytes received count toward the throughput calculation.
- Evidence keys follow the project's camelCase convention (e.g., `url`, `measuredMbps`, `minimumMbps`, `bytesDownloaded`, `elapsedSeconds`, `thresholdMet`).
- The HTTP request uses standard headers without any custom User-Agent or Accept headers. Some CDNs or speed test servers may behave differently based on request headers.
- `durationSeconds` is a soft cap: the test cancels the download after the duration but may overshoot by a small margin due to in-flight data buffers; throughput is calculated using actual elapsed time.
