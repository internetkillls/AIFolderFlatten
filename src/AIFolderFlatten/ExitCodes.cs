namespace AIFolderFlatten;

internal static class ExitCodes
{
    public const int Success = 0;
    public const int GeneralFailure = 1;
    public const int ArgumentError = 2;
    public const int ManifestVerificationFailure = 3;
    public const int GitFailure = 4;
    public const int ScanFailure = 5;
    public const int OutputWriteFailure = 6;
    public const int PrivacyLeakFailure = 7;
}
