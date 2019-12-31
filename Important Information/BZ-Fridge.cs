using System;
using UnityEngine;

// Token: 0x0200054A RID: 1354
public class Fridge : MonoBehaviour
{
	// Token: 0x0600203C RID: 8252 RVA: 0x000AF314 File Offset: 0x000AD514
	private void OnEnable()
	{
		if (!this.subscribed)
		{
			this.storageContainer.enabled = true;
			this.storageContainer.container.containerType = ItemsContainerType.Trashcan;
			this.storageContainer.container.onAddItem += this.AddItem;
			this.storageContainer.container.onRemoveItem += this.RemoveItem;
			this.subscribed = true;
		}
	}

	// Token: 0x0600203D RID: 8253 RVA: 0x000AF388 File Offset: 0x000AD588
	private void OnDisable()
	{
		if (this.subscribed)
		{
			this.storageContainer.container.onAddItem -= this.AddItem;
			this.storageContainer.container.onRemoveItem -= this.RemoveItem;
			this.subscribed = false;
			this.storageContainer.enabled = false;
		}
	}

	// Token: 0x0600203E RID: 8254 RVA: 0x000AF3EC File Offset: 0x000AD5EC
	private void AddItem(InventoryItem item)
	{
		if (item == null || item.item == null)
		{
			return;
		}
		Eatable component = item.item.GetComponent<Eatable>();
		if (component != null && component.decomposes)
		{
			component.PauseDecay();
		}
	}

	// Token: 0x0600203F RID: 8255 RVA: 0x000AF43C File Offset: 0x000AD63C
	private void RemoveItem(InventoryItem item)
	{
		if (item == null || item.item == null)
		{
			return;
		}
		Eatable component = item.item.GetComponent<Eatable>();
		if (component != null && component.decomposes)
		{
			component.UnpauseDecay();
		}
	}

	// Token: 0x040025DA RID: 9690
	[SerializeField]
	[AssertNotNull]
	private StorageContainer storageContainer;

	// Token: 0x040025DB RID: 9691
	private bool subscribed;
}
