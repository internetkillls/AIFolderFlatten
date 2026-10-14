using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIFolderFlatten.Output;

internal class SelectionInputResolution
{
    public string CommonRoot { get; }
    public bool IsMultiSelect { get; }
    public List<string> SelectedAbsolutePaths { get; }

    public SelectionInputResolution(string commonRoot, bool isMultiSelect, List<string> selectedAbsolutePaths)
    {
        CommonRoot = commonRoot;
        IsMultiSelect = isMultiSelect;
        SelectedAbsolutePaths = selectedAbsolutePaths;
    }
}

internal static class SelectionInputResolver
{
    public static SelectionInputResolution Resolve(string rawInputString)
    {
        var rawPaths = InputListParser.Parse(rawInputString);
        
        if (rawPaths.Count == 0)
        {
            throw new ArgumentException("No input paths provided.");
        }

        var absolutePaths = rawPaths.Select(p => Path.GetFullPath(p)).ToList();
        bool isMultiSelect = absolutePaths.Count > 1;

        string commonRoot;
        if (isMultiSelect)
        {
            commonRoot = CommonRootResolver.Resolve(absolutePaths);
        }
        else
        {
            var singlePath = absolutePaths[0];
            commonRoot = File.Exists(singlePath) ? (Path.GetDirectoryName(singlePath) ?? singlePath) : singlePath;
        }

        return new SelectionInputResolution(commonRoot, isMultiSelect, absolutePaths);
    }
}
