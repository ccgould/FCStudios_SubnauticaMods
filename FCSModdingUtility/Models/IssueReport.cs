using System;
using System.Collections.Generic;

namespace FCSModdingUtility.Models
{
    public class IssueReport
    {
        private string _message;

        private IssueReportType _issueReportType;

        private readonly List<string> _missingDependencies = new List<string>();

        public IssueReportType IssueReportType
        {
            get => _issueReportType;
            set
            {
                _issueReportType = value;

                switch (value)
                {
                    case IssueReportType.VersionMissMatch:
                        SetMessage("The version of the DLL and the mod.config are not the same.");
                        break;
                    case IssueReportType.MissingDependancy:
                        SetMessage("There are dependencies that are missing for this mod");
                        break;
                    case IssueReportType.Unknown:
                        break;
                    case IssueReportType.ModDisabled:
                        SetMessage("Mod is disabled.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public void SetMessage(string message)
        {
            _message = message;
        }

        public string GetMessage()
        {
            return _message;
        }

        public void AddMissingDependency(string dependency)
        {
            _missingDependencies?.Add(dependency);
        }

        public List<string> GetMissingDependencies()
        {
            return _missingDependencies;
        }

        public string DLLVersion { get; set; }
        public override string ToString()
        {
            return IssueReportType.ToString();
        }

        public string ToStringFull()
        {
            return $"⦿ {GetMessage()}";
        }
    }
}
