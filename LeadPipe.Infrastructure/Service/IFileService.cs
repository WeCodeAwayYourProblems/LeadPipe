using System.Runtime.CompilerServices;

namespace LeadPipe.Infrastructure.Service;

public interface IFileService
{
    string GetLocalFile(string projectContainingLocalFolder, string localFolderToFind, string fileName);
    string GetLocalFolder(string projectContainingLocalFolder, string localFolderToFind);
    string GetMemberName(object origin, [CallerMemberName] string memberName = "");
}