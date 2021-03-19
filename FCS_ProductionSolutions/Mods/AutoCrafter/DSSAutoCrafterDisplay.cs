using System;
using FCSCommon.Abstract;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class DSSAutoCrafterDisplay : AIDisplay
    {
        private DSSAutoCrafterController _mono;

        internal void Setup(DSSAutoCrafterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
                
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void RemoveCraftingItem(CraftingItem craftingItem)
        {
            throw new NotImplementedException();
        }
    }
}