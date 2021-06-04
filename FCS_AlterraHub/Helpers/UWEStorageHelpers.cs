using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class UWEStorageHelpers
    {
        /// <summary>
        /// Creates a default UWE <see cref="StorageContainer"/>
        /// </summary>
        /// <param name="prefab">The object to add a storage container component.</param>
        /// <param name="storageRoot">The storage root.This is were items will be held.</param>
        /// <param name="classID">The class id of the prefab this helps the <see cref="ChildObjectIdentifier"/> to save and load the items.</param>
        /// <param name="storageLabel">The label that displays with the storage is open.</param>
        /// <param name="width">The width of the storage container</param>
        /// <param name="height">The height of the storage container</param>
        public static StorageContainer CreateStorageContainer(GameObject prefab,GameObject storageRoot, string classID, string storageLabel, int width,int height)
        {
            if (storageRoot == null)
            {
                storageRoot = new GameObject("StorageRoot");
                storageRoot.transform.parent = prefab.transform;
                UWE.Utils.ZeroTransform(storageRoot);
            }

            //We are disabling the gameObject so the settings can be set before Awake() is called.
            prefab.SetActive(false);

            //Create the storage container
            var storage = prefab.EnsureComponent<StorageContainer>();
            storage.prefabRoot = prefab;
            storage.width = width;
            storage.height = height;
            storage.storageLabel = storageLabel;

            //Make sure a ChildObjectIdentifier component is attached for saving and loading children.
            var childObjectIdentifier = storageRoot.EnsureComponent<ChildObjectIdentifier>();
            childObjectIdentifier.classId = classID;
            storage.storageRoot = childObjectIdentifier;

            //everything is set so re-enable the object
            prefab.SetActive(true);

            return storage;
        }
    }
}
