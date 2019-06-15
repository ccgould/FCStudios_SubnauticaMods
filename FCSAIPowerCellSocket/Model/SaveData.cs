using System.Collections.Generic;

namespace FCSAIPowerCellSocket.Model
{
    internal class SaveData
    {
        public IEnumerable<PowercellData> PowercellDatas { get; set; } = new List<PowercellData>();
    }
}
