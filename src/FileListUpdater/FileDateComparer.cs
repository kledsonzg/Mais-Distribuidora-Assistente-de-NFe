using System.Collections;

namespace NFeAssistant.FileListUpdater
{
    internal class FileDateComparer : IComparer<FileInfo>
    {
        int IComparer<FileInfo>.Compare(FileInfo? x, FileInfo? y)
        {
            var fileDates = new DateTime[] {x.CreationTime > x.LastWriteTime ? x.LastWriteTime : x.CreationTime, 
                y.CreationTime > y.LastWriteTime ? y.LastWriteTime : y.CreationTime};

            if(fileDates[0] == fileDates[1] )
            {
                return new CaseInsensitiveComparer().Compare(y.FullName, x.FullName);
            }
            if(fileDates[0] > fileDates[1] )
                return -1;
            
            return 1;
        }
    }
}