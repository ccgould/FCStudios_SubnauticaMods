namespace FCSPowerStorage.Helpers
{
    class StorageContainerHelper : StorageContainer
    {

        public new ItemsContainer container { get; private set; }


        public override void Awake()
        {
            base.Awake();
        }

        private void CreateContainer()
        {
            if (this.container == null)
            {
                this.container = new ItemsContainer(8, 10, base.transform,
                    this.storageLabel, this.errorSound);
            }
        }
    }
}
