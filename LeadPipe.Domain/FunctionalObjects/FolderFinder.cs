using System.Runtime.CompilerServices;
using System.Text;

namespace LeadPipe.Domain.FunctionalObjects;

public static class FolderFinder
{
    #region Public Members
    /// <summary>
    /// <para>The goal of this method is to find a specific folder contained within the project folder of our project. The folder will not be found if it is not contained in the solution, or if the file structure does not follow the following pattern: /[SolutionName]/[SolutionName.ProjectName]/[Parent]/[Of]/[Dir]/[...]/[Folder]. Note that this pattern does not require the folder's parent if and only if the destination folder is a direct child of the folder with the name [SolutionName.ProjectName] in order to function properly</para>
    /// <para>The <paramref name="projectContainingLocalFolder"/> should be the nameof(projectname). For example: "nameof(<see cref="Domain"/>)"</para>
    /// <para>The <paramref name="projectContainingLocalFolder"/> is malformed if the caller attempts to ref the project thus: nameof([SolutionName].[ProjectName])</para>
    /// <para>The <paramref name="localFolderToFind"/> should be the actual name of the local folder that the caller wishes to find. The folder should be located inside of the folder of the [ProjectName]</para>
    /// <para>Thus, <paramref name="localFolderToFind"/> is properly formed if the full file path is formatted as follows:</para>
    /// <para>C:/Various/Folders[...]/[SolutionName]/[SolutionName].[ProjectName]/[FolderToFind]</para>
    /// <para>The [FolderToFind] is legal and properly formed if it has a child folder that we're trying to find and is passed as follows: "[ParentDir]/[ChildDir]"</para>
    /// </summary>
    /// <param name="projectContainingLocalFolder"></param>
    /// <param name="localFolderToFind"></param>
    /// <returns></returns>
    public static string GetLocalFolder(string projectContainingLocalFolder, string localFolderToFind) =>
        GetLocalFolder(projectContainingLocalFolder, _relativePath, localFolderToFind);

    /// <summary>
    /// <para>The goal of this method is to find a specific folder contained within the project folder of our solution. The file will be found if and only if it is a direct child of <paramref name="localFolderToFind"/>. The folder will not be found if it is not contained in the solution, or if the file structure does not follow the following pattern: /[SolutionName]/[SolutionName.ProjectName]/[Parent]/[Of]/[Dir]/[...]/[Folder]. Note that this pattern does not require the folder's parent if and only if the destination folder is a direct child of the folder with the name [SolutionName.ProjectName] in order to function properly</para>
    /// <para>The <paramref name="projectContainingLocalFolder"/> should be the nameof(projectname). For example: "nameof(<see cref="Domain"/>)"</para>
    /// <para>The <paramref name="projectContainingLocalFolder"/> is malformed if the caller attempts to ref the project thus: nameof([SolutionName].[ProjectName])</para>
    /// <para>The <paramref name="localFolderToFind"/> should be the actual name of the local folder that the caller wishes to find. The folder should be located inside of the folder of the [ProjectName]</para>
    /// <para>Thus, <paramref name="localFolderToFind"/> is properly formed if the full file path is formatted as follows:</para>
    /// <para>C:/Various/Folders[...]/[SolutionName]/[SolutionName].[ProjectName]/[FolderToFind]</para>
    /// <para>The [FolderToFind] is legal and properly formed if it has a child folder that we're trying to find and is passed as follows: "[ParentDir]/[ChildDir]"</para>
    /// </summary>
    /// <param name="projectContainingLocalFolder"></param>
    /// <param name="localFolderToFind"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetLocalFile(string projectContainingLocalFolder, string localFolderToFind, string fileName) =>
        GetLocalFolder(projectContainingLocalFolder, _relativePath, localFolderToFind) + fileName;

    public static string GetMemberName(object origin, [CallerMemberName] string memberName = "")
    {
        string fullName = origin.GetType().FullName!;
        string result = memberName == "" ? fullName : fullName + "." + memberName;
        return result;
    }
    #endregion

    #region Private members
    readonly static string _solution = nameof(LeadPipe);
    const string _relativePath = @".\";

    static string GetLocalFolder(string projectContainingLocalFolder, string relativePath, string localFolderToFind)
    {
        const char slash = '\\';
        string fullPath = Path.GetFullPath(relativePath);
        string[] pathSplit = fullPath.Split(slash);
        int index = default;

        // Iterate through the full path split by slashes
        // Find the first directory that contains the solution name
        for (var i = pathSplit.Length - 1; i >= 0; --i)
        {
            if (pathSplit[i].Equals(_solution))
            {
                index = i;
                break;
            }
        }

        // Use the index found above to rebuild the string path
        StringBuilder builder = new();
        for (var i = 0; i < index + 1; ++i)
        {
            builder.Append(pathSplit[i]);
            builder.Append(slash);
        }

        // Check whether the parent folder is located in the solution folder
        string projFolder = builder.ToString() + _solution + '.' + projectContainingLocalFolder;
        if (Directory.Exists(projFolder))
        {
            builder.Append(_solution);
            builder.Append('.');
            builder.Append(projectContainingLocalFolder);
            builder.Append(slash);
        }

        // Check whether the folder we're trying to find is located in the solution folder or the parent folder
        string locFolder = builder.ToString() + localFolderToFind;
        if (Directory.Exists(locFolder))
        {
            builder.Append(localFolderToFind);
            builder.Append(slash);
        }

        // Ensure this folder exists 
        if (!Directory.Exists(builder.ToString()))
            Directory.CreateDirectory(builder.ToString());

        string returnString = builder.ToString();
        builder.Clear();

        return returnString;
    }
    #endregion
}
