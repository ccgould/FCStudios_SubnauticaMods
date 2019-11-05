using System;
using System.IO;
using Dna;
using SemVersion;
using static FCSModdingUtility.DI;

namespace FCSModdingUtility
{
    internal static class ApplicationHelpers
    {
        internal static bool SafeDeleteDirectory(string location, bool notify = true)
        {
            try
            {
                if (Directory.Exists(location))
                {
                    Directory.Delete(location, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (notify)
                {
                    ViewModelApplication.DefaultError(e.Message);
                    //TODO report message to log and window
                }
            }
            return false;
        }

        internal static bool SafeDeleteFile(string location, bool notify = true)
        {
            try
            {
                if (File.Exists(location))
                {
                    File.Delete(location);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (notify)
                {
                    ViewModelApplication.DefaultError(e.Message);
                    //TODO report message to log and window
                }
            }
            return false;
        }

        internal static bool VersionCompare(string v1, string v2,string mod, out string message)
        {
            try
            {

                //SemanticVersion version = new SemVersion();

                if (v1.Length < 5 || v2.Length < 5)
                {
                    message = "Incorrect Version expecting (Semantic Version).";
                    return false;
                }

                var version1 = new Version(v1);
                var version2 = new Version(v2);

                v1 = VersionToSematic(version1);
                v2 = VersionToSematic(version2);

                version1 = new Version(v1);
                version2 = new Version(v2);
                
                var result = version1.CompareTo(version2);

                if (result > 0)
                {
                    message = "version 1 is greater";
                }
                else if (result < 0)
                {
                    message = "version 2 is greater";
                }
                else
                {
                    message = "versions are equal";
                    return true;
                }
            }
            catch (Exception e)
            {
                message = string.Empty;
                ViewModelApplication.DefaultError(e.Message);
                return false;
            }
            
            return false;
        }

        public static string VersionToSematic(Version version)
        {
            var major = version.Major == -1 ? 0 : version.Major;
            var minor = version.Minor == -1 ? 0 : version.Minor;
            var build = version.Build == -1 ? 0 : version.Build;

            return $"{major}.{minor}.{build}";
        }

        public static string VersionToSematic(string versionInfoProductVersion)
        {
            try
            {
                var version = new Version(versionInfoProductVersion);
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError(e.Message);
                return string.Empty;
            }
        }

        public static string CreateAssetDirectory(string modLocation)
        {
            var assetsFolder = Path.Combine(modLocation, "Assets");
            return SafeCreateDirectory(assetsFolder);
        }

        public static string SafeCreateDirectory(string location)
        {
            try
            {
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError(e.Message);
                return string.Empty;
            }

            return location;
        }

        public static string GetAssetFolder(string modLocation)
        {
            var directories = Directory.GetDirectories(modLocation);

            foreach (string directory in directories)
            {
                if (Path.GetFileName(directory).Equals("Assets", StringComparison.OrdinalIgnoreCase))
                {
                    return directory;
                }
            }

            return String.Empty;
        }

        public static string AddToAssetFolder(string modLocation, string fileLocation, bool replace = true)
        {
            try
            {
                var assetFolder = GetAssetFolder(modLocation);
                var modName = Path.GetFileName(modLocation);

                if (assetFolder.IsNullOrEmpty())
                {
                    ViewModelApplication.DefaultWarning($"No asset folder was found in the {modName} mod directory. One is now being created.");
                    assetFolder = CreateAssetDirectory(modLocation);
                }

                if (!Directory.Exists(assetFolder))
                {
                    ViewModelApplication.DefaultError($"No asset folder was found after attempting creating canceling operation.");
                    return String.Empty;
                }

                if(!File.Exists(fileLocation))
                {
                    ViewModelApplication.DefaultError($"The file {fileLocation} doesn't exist canceling operation.");
                    return String.Empty;
                }
  
                var result = SafeAddToModDirectory(Path.Combine(assetFolder, Path.GetFileName(fileLocation) ?? throw new InvalidOperationException()), fileLocation, replace);
                
                return result;
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError($"[{nameof(ApplicationHelpers)}]: {e.Message}");
            }

            return String.Empty;
        }

        private static string SafeAddToModDirectory(string newFileLocation, string fileLocation, bool replace = true)
        {
            try
            {
                File.Copy(fileLocation, newFileLocation, replace);

                return newFileLocation;
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError($"{nameof(ApplicationHelpers)}: {e.Message}");
            }

            return String.Empty;
        }

        public static void AddToModDirectory(string modLocation, string fileLocation)
        {
            try
            {
                if (string.IsNullOrEmpty(modLocation))
                {
                    ViewModelApplication.DefaultError($"[Add To Mod Directory]: Mod location is empty or null.");
                    return;
                }

                if (!Directory.Exists(modLocation))
                {
                    ViewModelApplication.DefaultError($"[Add To Mod Directory]: Mod directory doesn't exist.");
                    return;
                }

                if (string.IsNullOrEmpty(fileLocation))
                {
                    ViewModelApplication.DefaultError($"[Add To Mod Directory]: File location is empty or null.");
                    return;
                }

                if (!File.Exists(fileLocation))
                {
                    ViewModelApplication.DefaultError($"[Add To Mod Directory]: File doesn't exist.");
                    return;
                }

                var newFile = Path.Combine(modLocation, Path.GetFileName(fileLocation));

                SafeAddToModDirectory(newFile, fileLocation);

            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError($"{nameof(ApplicationHelpers)}: {e.Message}");
            }
        }
        public static void ReplaceModIcon(string iconName,string modLocation, string originalFile)
        {
            try
            {
                var origNewLocation = AddToAssetFolder(modLocation,originalFile);

                var assetFolder = GetAssetFolder(modLocation);

                var newFile = Path.Combine(assetFolder,$"{iconName}.png");

                SafeRenameFile(origNewLocation,newFile,true);
                
            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError($"{nameof(ApplicationHelpers)}: {e.Message}");
            }
        }

        public static void SafeRenameFile(string originalFile, string newFile, bool deleteOriginal = false)
        {
            try
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }

                File.Move(originalFile,newFile);

                if (deleteOriginal)
                {
                    if (File.Exists(originalFile))
                    {
                        File.Delete(originalFile);
                    }
                }

            }
            catch (Exception e)
            {
                ViewModelApplication.DefaultError($"{nameof(ApplicationHelpers)}: {e.Message}");
            }
        }

    }
}
