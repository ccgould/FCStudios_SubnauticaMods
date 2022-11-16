using System.IO;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Systems
{
    public static class FileManager
    {
        public static bool Delete(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                QuickLogger.Info($"[FilesManager] Deleted File: {file}");
                return true;
            }

            QuickLogger.Warning($"[FilesManager] Failed to find file: {file}");
            return false;
        }

        public static bool CreateFile(string file,string contents,bool overwrite = false)
        {
            var fileExist = File.Exists(file);

            if (fileExist  && !overwrite)
            {
                QuickLogger.Warning($"[FilesManager] File was not created because the files exist and overwrite was set to false");
                return false;
            }
            
            if (!string.IsNullOrEmpty(contents))
            {
                File.WriteAllText(file,contents);
            }
            else
            {
                File.Create(file);
            }


            QuickLogger.Info($"[FilesManager] Created file: {file}");
            return false;
        }
    }
}
