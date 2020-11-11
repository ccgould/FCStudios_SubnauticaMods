using System.Collections.Generic;
using System.Linq;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class FCSAquarium : MonoBehaviour
    {
		public void Initialize(GameObject storageRoot,TechType[] creatures)
        {
			if(_isInitialized) return;
            
            _itemsContainer = new ItemsContainer(4,4,storageRoot.transform,"FCSAquarium",null);

            var fishTankTracks = GameObjectHelpers.FindGameObject(gameObject, "FishTankTracks");

            trackObjects = fishTankTracks.GetChildren();

			fishRoot = storageRoot;

            this.tracks = new List<FishTrack>();
            for (int i = 0; i < this.trackObjects.Length; i++)
            {
                _itemsContainer.AddItem(creatures[i].ToPickupable());
				this.tracks.Add(new FishTrack(trackObjects[i]));
            }
            Invoke("InitFishDelayed", 0f);
            _isInitialized = true;
        }

        private void OnDisable()
        {
            this.Subscribe(false);
        }

        private void LateUpdate()
        {
            this.Subscribe(true);
        }

        private void Subscribe(bool state)
        {
			if(!_isInitialized) return;

            if (this.subscribed == state)
            {
                return;
            }
            if (_itemsContainer == null)
            {
                QuickLogger.Debug("Aquarium.Subscribe(): container null; will retry next frame");
                return;
            }
            if (this.subscribed)
            {
                _itemsContainer.onAddItem -= this.AddItem;
                _itemsContainer.onRemoveItem -= this.RemoveItem;
                _itemsContainer.isAllowedToAdd = null;
                this.fishRoot.SetActive(false);
            }
            else
            {
                _itemsContainer.onAddItem += this.AddItem;
                _itemsContainer.onRemoveItem += this.RemoveItem;
                _itemsContainer.isAllowedToAdd = new IsAllowedToAdd(this.IsAllowedToAdd);
                this.fishRoot.SetActive(true);
            }
            this.subscribed = state;
        }

        public void Destroy()
        {
			for (int i = _itemsContainer.count - 1; i > -1; i--)
            {
                _itemsContainer.RemoveItem(_itemsContainer.ElementAt(0).item);
            }
		}

		private void InitFishDelayed()
		{
            if (_itemsContainer != null)
			{
				foreach (InventoryItem item in _itemsContainer)
				{
					this.AddItem(item);
				}
			}
		}
		
		private void Update()
		{
			double timePassed = DayNightCycle.main.timePassed;
			if (this.timeNextInfectionSpread > 0.0 && timePassed > this.timeNextInfectionSpread)
			{
				if (this.InfectCreature())
				{
					this.timeNextInfectionSpread = timePassed + (double)this.spreadInfectionInterval;
					return;
				}
				this.timeNextInfectionSpread = -1.0;
			}
		}

		private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
		{
			return pickupable.GetComponent<AquariumFish>() != null;
		}

		private void AddItem(InventoryItem item)
		{
			if(item == null) return;
			GameObject fish = item.item.gameObject;
			AquariumFish aquariumFish = fish.GetComponent<AquariumFish>();
			
            if (aquariumFish == null) return;

			TechType techType = item.item.GetTechType();
			FishTrack freeTrack = this.GetFreeTrack();
            GameObject fishClone = Instantiate(aquariumFish.model, Vector3.zero, Quaternion.identity);
			fishClone.transform.SetParent(freeTrack.track.transform, false);
			fishClone.transform.localScale *= fishScale;
			this.SetupRenderers(fishClone);
			AnimateByVelocity animateByVelocity = fishClone.GetComponentInChildren<AnimateByVelocity>();
			animateByVelocity.rootGameObject = fishClone;
			animateByVelocity.animationMoveMaxSpeed = fishMaxSpeed;

			freeTrack.fishType = techType;
			freeTrack.fish = fishClone;
			freeTrack.item = fish;
            freeTrack.animateComponent = animateByVelocity;

			InfectedMixin component2 = fish.GetComponent<InfectedMixin>();
			
            if (component2 != null)
			{
				InfectedMixin infectedMixin = fishClone.AddComponent<InfectedMixin>();
				infectedMixin.renderers = GetMarmosetRenderers(fishClone).ToArray();
				infectedMixin.SetInfectedAmount(component2.GetInfectedAmount());
				freeTrack.infectedMixin = infectedMixin;
			}
			
            fishClone.SetActive(true);
			this.UpdateInfectionSpreading();
		}

		private void RemoveItem(InventoryItem item)
		{
			GameObject gameObject = item.item.gameObject;
			FishTrack trackByItem = this.GetTrackByItem(gameObject);
			if (trackByItem == null)
			{
				return;
			}
			InfectedMixin infectedMixin = trackByItem.infectedMixin;
			if (infectedMixin != null)
			{
				InfectedMixin component = gameObject.GetComponent<InfectedMixin>();
				if (component != null)
				{
					component.SetInfectedAmount(infectedMixin.GetInfectedAmount());
				}
			}

            trackByItem.Clear();
            this.UpdateInfectionSpreading();
            DestroyImmediate(gameObject);
		}

		private FishTrack GetFreeTrack()
		{
			return this.GetTrackByFishType(TechType.None);
		}

		private FishTrack GetTrackByFishType(TechType fishType)
		{
			for (int i = 0; i < this.tracks.Count; i++)
			{
				if (this.tracks[i].fishType == fishType)
				{
					return this.tracks[i];
				}
			}
			return null;
		}

		private FishTrack GetTrackByItem(GameObject item)
		{
			for (int i = 0; i < this.tracks.Count; i++)
			{
				if (this.tracks[i].item == item)
				{
					return this.tracks[i];
				}
			}
			return null;
		}

		private void SetupRenderers(GameObject gameObject)
		{
			int newLayer = LayerMask.NameToLayer("Viewmodel");
			Utils.SetLayerRecursively(gameObject, newLayer);
		}

		private bool ContainsHeroPeepers()
		{
			for (int i = 0; i < this.tracks.Count; i++)
			{
				if (this.tracks[i].fishType == TechType.Peeper)
				{
					Peeper component = this.tracks[i].item.GetComponent<Peeper>();
					if (component != null && component.isHero)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool ContainsInfectedCreature()
		{
			for (int i = 0; i < this.tracks.Count; i++)
			{
				if (this.tracks[i].infectedMixin != null && this.tracks[i].infectedMixin.GetInfectedAmount() > 0.25f)
				{
					return true;
				}
			}
			return false;
		}

		private bool InfectCreature()
		{
			bool result = false;
			for (int i = 0; i < this.tracks.Count; i++)
			{
				if (this.tracks[i].infectedMixin != null && this.tracks[i].infectedMixin.GetInfectedAmount() < 1f)
				{
					this.tracks[i].infectedMixin.SetInfectedAmount(1f);
					result = true;
					break;
				}
			}
			return result;
		}

		private void CureAllCreatures()
		{
			for (int i = 0; i < this.tracks.Count; i++)
			{
				InfectedMixin infectedMixin = this.tracks[i].infectedMixin;
				if (infectedMixin != null && infectedMixin.GetInfectedAmount() > 0.1f)
				{
					infectedMixin.SetInfectedAmount(0.1f);
				}
			}
		}

		private void UpdateInfectionSpreading()
		{
			if (this.ContainsHeroPeepers())
			{
				this.CureAllCreatures();
				this.timeNextInfectionSpread = -1.0;
				return;
			}
			if (this.timeNextInfectionSpread < 0.0 && this.ContainsInfectedCreature())
			{
				this.timeNextInfectionSpread = DayNightCycle.main.timePassed + (double)this.spreadInfectionInterval;
			}
		}

		private static List<Renderer> GetMarmosetRenderers(GameObject gameObject)
		{
			List<Renderer> list = new List<Renderer>();
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
			{
				bool flag = false;
				for (int j = 0; j < renderer.sharedMaterials.Length; j++)
				{
					Material material = renderer.sharedMaterials[j];
					if (material != null && material.shader != null && material.shader.name.Contains("Marmoset"))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.Add(renderer);
				}
			}
			return list;
		}
		
		public GameObject fishRoot;

		public GameObject[] trackObjects;

		public float spreadInfectionInterval = 10f;

		private List<FishTrack> tracks;

		private bool subscribed;

		private double timeNextInfectionSpread = -1.0;
        private ItemsContainer _itemsContainer;
        private bool _isInitialized;

        private const float fishScale = 0.3f;

		private const float fishMaxSpeed = 0.5f;

		private class FishTrack
		{
			public void Clear()
			{
				this.fishType = TechType.None;
				this.item = null;
				this.infectedMixin = null;
                Object.Destroy(animateComponent);
				Destroy(fish);
			}

			public FishTrack(GameObject track)
			{
				this.track = track;
			}

			public GameObject track;

			public TechType fishType;

			public GameObject fish;

			public GameObject item;

			public InfectedMixin infectedMixin;
            public AnimateByVelocity animateComponent;
        }
	}
}
