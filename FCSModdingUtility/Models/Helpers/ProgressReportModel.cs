using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSModdingUtility
{
    internal class ProgressReportModel
    {
        public ObservableCollection<ModItemViewModel> ModsLoaded { get; set; }
        public int PercentageComplete { get; set; } = 0;
    }
}
