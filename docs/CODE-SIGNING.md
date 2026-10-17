# Code signing policy

## Current status

DockForge is applying to the SignPath Foundation open-source program. **The application has not yet been accepted and current releases are unsigned.** Preparing configuration files does not provide a certificate or a trusted signature.

If accepted, the intended attribution is: Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org). The certificate's publisher will be SignPath Foundation, rather than the maintainer's personal name.

## Responsibilities

- Author and committer: [sidosgr](https://github.com/sidosgr).
- Reviewer: [sidosgr](https://github.com/sidosgr); outside contributions require maintainer review.
- Release and signing approver: [sidosgr](https://github.com/sidosgr).

Maintainers must use multi-factor authentication for GitHub and SignPath. Each release signing request must receive manual approval. Production signing must be restricted to this repository's approved release tags and a trusted GitHub Actions build. Changes to workflows, packaging and signing configuration require the same review as application changes.

## Signing scope

Only binaries built from DockForge source may be signed with the project's signing identity: `DockForge.exe`, `DockForge.dll` and `DockForge.Core.dll` for x64 and ARM64, and the final combined installer. Microsoft .NET runtime files and other third-party components must retain their original signatures and must never be signed as DockForge code.

Proposed artifact configurations are in [`.signpath`](../.signpath). They use explicit file names and product metadata restrictions. SignPath must review these configurations and bind them to origin-verifying signing policies before production use.

The prepared workflow signs the application binaries first, packages those signed binaries, and then signs the outer installer. Inno Setup's generated uninstaller is a separate signing step and is **not covered by that two-stage workflow**. Before claiming full installer lifecycle signing, configure a separately approved uninstaller-signing process with SignPath. A signed outer installer does not imply that its embedded files or uninstaller are signed.

The workflow is manual and disabled until `SIGNPATH_ENABLED` is explicitly set to `true` after approval. Signed outputs are checked for a valid Authenticode signature, a trusted timestamp and the SignPath Foundation publisher. The workflow never publishes releases automatically. Checksums are generated after signing, because signing changes file contents.

## Setting up after acceptance

1. Complete the SignPath application at <https://signpath.org/apply>. It requires maintainer name/email, project details, reputation evidence and agreement to the Foundation's terms. Do not claim existing adoption or approval that the project does not have.
2. Follow SignPath's onboarding instructions to connect `sidosgr/DockForge` as a trusted build source. Restrict release signing to maintainer-approved `v*` tags and require manual approval.
3. Create the `app-binaries` and `installer` artifact configurations from the proposed XML files. Confirm the metadata restrictions against an unsigned build artifact. Do not relax the restrictions to sign third-party DLLs.
4. Store the SignPath API token in the GitHub Actions secret `SIGNPATH_API_TOKEN`. It must belong to a signing-request submitter with only the required project/policy access. Never paste tokens, private keys or account credentials into source files or chat.
5. Add repository variables `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG` and `SIGNPATH_SIGNING_POLICY_SLUG` with the actual values assigned by SignPath. The policy must use the Foundation's production certificate, not a test certificate.
6. Set `SIGNPATH_ENABLED=true` and manually run **Sign DockForge release** against an approved version tag. Approve the two requests in SignPath. Download the `DockForge-signed-release` workflow artifact, verify it, and publish it only after release review.
7. Update the signing-status text in this policy, README and release notes only after a real signed artifact has passed verification. Existing unsigned releases do not become signed automatically.

SignPath acceptance is discretionary. The Foundation requires verifiable project reputation; DockForge is new and approval is not guaranteed. [Program conditions](https://signpath.org/terms), [GitHub Actions integration](https://github.com/SignPath/github-action-submit-signing-request), [artifact configuration](https://docs.signpath.io/artifact-configuration/).

## Privacy

See the [privacy policy](PRIVACY.md). Settings, logs and backups remain local. Update checks contact the official GitHub repository when enabled; they can be disabled in Settings. DockForge never uploads settings or diagnostic logs automatically.
