using System;

namespace AIFolderFlatten.ExpertSystem;

/// <summary>
/// Compile-time constants and runtime guard for the expert-system-only boundary.
/// AI Seeder core must never call LLMs, embeddings, cloud APIs, or agents.
/// </summary>
internal static class ExpertSystemBoundary
{
    // Compile-time invariants — these must NEVER be changed to true in core
    public const bool AiCore = false;
    public const bool NetworkUsed = false;
    public const bool LlmUsed = false;
    public const bool EmbeddingsUsed = false;
    public const bool ExpertSystemOnly = true;

    public static void AssertExpertSystemMode(
        bool expertSystemOnly,
        bool allowLlm,
        bool allowNetwork,
        bool allowEmbeddings)
    {
        if (!expertSystemOnly)
            throw new InvalidOperationException(
                "Expert-System boundary violation: expertSystemOnly=false is forbidden. " +
                "AIFolderFlatten has no AI-runtime mode.");
        if (allowLlm)
            throw new InvalidOperationException(
                "Expert-System boundary violation: allowLlm=true is forbidden in core mode. " +
                "The core pipeline must never use LLMs.");
        if (allowNetwork)
            throw new InvalidOperationException(
                "Expert-System boundary violation: allowNetwork=true is forbidden in core mode. " +
                "Default network=false.");
        if (allowEmbeddings)
            throw new InvalidOperationException(
                "Expert-System boundary violation: allowEmbeddings=true is forbidden in core mode. " +
                "Embeddings are not part of the deterministic core.");
    }
}
