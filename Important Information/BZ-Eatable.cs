using System;
using ProtoBuf;
using UnityEngine;

// Token: 0x020006B5 RID: 1717
[DisallowMultipleComponent]
[ProtoContract]
public class Eatable : MonoBehaviour, ISecondaryTooltip
{
	// Token: 0x060029CE RID: 10702 RVA: 0x000DF854 File Offset: 0x000DDA54
	private void Awake()
	{
		if (this.despawns && this.decomposes)
		{
			this.StartDespawnInvoke();
		}
		this.SetDecomposes(this.decomposes);
	}

	// Token: 0x060029CF RID: 10703 RVA: 0x000DF880 File Offset: 0x000DDA80
	public float GetFoodValue()
	{
		float result = this.foodValue;
		if (this.decomposes)
		{
			result = Mathf.Max(this.foodValue - this.GetDecayValue(), -25f);
		}
		return result;
	}

	// Token: 0x060029D0 RID: 10704 RVA: 0x000DF8B8 File Offset: 0x000DDAB8
	public float GetWaterValue()
	{
		float result = this.waterValue;
		if (this.decomposes)
		{
			result = Mathf.Max(this.waterValue - this.GetDecayValue(), -25f);
		}
		return result;
	}

	// Token: 0x060029D1 RID: 10705 RVA: 0x000DF8F0 File Offset: 0x000DDAF0
	public float GetHealthValue()
	{
		float result = this.healthValue;
		if (this.decomposes)
		{
			result = Mathf.Max(this.healthValue - this.GetDecayValue(), -25f);
		}
		return result;
	}

	// Token: 0x060029D2 RID: 10706 RVA: 0x000DF928 File Offset: 0x000DDB28
	public float GetColdResistanceTime()
	{
		return this.warmthTime;
	}

	// Token: 0x060029D3 RID: 10707 RVA: 0x000DF930 File Offset: 0x000DDB30
	public float GetColdMeterValue()
	{
		float num = this.coldMeterValue;
		if (this.decomposes)
		{
			if (num > 1.401298E-45f)
			{
				num = Mathf.Max(this.coldMeterValue - this.GetDecayValue(), 0f);
			}
			else if (num < -1.401298E-45f)
			{
				num = Mathf.Min(this.coldMeterValue - this.GetDecayValue(), 0f);
			}
		}
		return num;
	}

	// Token: 0x060029D4 RID: 10708 RVA: 0x000DF99C File Offset: 0x000DDB9C
	private float GetDecayValue()
	{
		if (!this.decomposes)
		{
			return 0f;
		}
		float num = (!this.decayPaused) ? DayNightCycle.main.timePassedAsFloat : this.timeDecayPause;
		return (num - this.timeDecayStart) * this.kDecayRate;
	}

	// Token: 0x060029D5 RID: 10709 RVA: 0x000DF9EC File Offset: 0x000DDBEC
	public string GetSecondaryTooltip()
	{
		if (!GameModeUtils.RequiresSurvival())
		{
			return string.Empty;
		}
		float num = this.GetFoodValue();
		float num2 = this.GetWaterValue();
		string result = string.Empty;
		if (this.decomposes)
		{
			if (num < 0f && num2 < 0f)
			{
				result = Language.main.Get("Rotting");
			}
			else if (num < 0.5f * this.foodValue)
			{
				result = Language.main.Get("Ripe");
			}
			else if (num < 0.9f * this.foodValue)
			{
				result = Language.main.Get("Decomposing");
			}
		}
		return result;
	}

	// Token: 0x060029D6 RID: 10710 RVA: 0x000DFAA0 File Offset: 0x000DDCA0
	public bool IsRotten()
	{
		return this.GetFoodValue() < 0f && this.GetWaterValue() < 0f;
	}

	// Token: 0x060029D7 RID: 10711 RVA: 0x000DFAC4 File Offset: 0x000DDCC4
	public void SetDecomposes(bool value)
	{
		if (!value)
		{
			this.timeDecayStart = 0f;
		}
		else if (this.timeDecayStart == 0f)
		{
			this.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
		}
		this.decomposes = value;
		base.CancelInvoke();
		if (value)
		{
			this.StartDespawnInvoke();
		}
	}

	// Token: 0x060029D8 RID: 10712 RVA: 0x000DFB20 File Offset: 0x000DDD20
	public void PauseDecay()
	{
		if (this.decayPaused)
		{
			return;
		}
		this.decayPaused = true;
		this.timeDecayPause = DayNightCycle.main.timePassedAsFloat;
	}

	// Token: 0x060029D9 RID: 10713 RVA: 0x000DFB48 File Offset: 0x000DDD48
	public void UnpauseDecay()
	{
		if (!this.decayPaused)
		{
			return;
		}
		this.decayPaused = false;
		this.timeDecayStart += DayNightCycle.main.timePassedAsFloat - this.timeDecayPause;
	}

	// Token: 0x060029DA RID: 10714 RVA: 0x000DFB7C File Offset: 0x000DDD7C
	private void StartDespawnInvoke()
	{
		float time = UnityEngine.Random.Range(0f, 5f);
		base.InvokeRepeating("IterateDespawn", time, 5f);
	}

	// Token: 0x060029DB RID: 10715 RVA: 0x000DFBAC File Offset: 0x000DDDAC
	private void IterateDespawn()
	{
		if (!this.IsRotten())
		{
			return;
		}
		if (!base.gameObject.activeSelf)
		{
			this.wasActive = false;
			return;
		}
		if (!this.wasActive)
		{
			this.timeDespawnStart = DayNightCycle.main.timePassedAsFloat;
		}
		this.wasActive = true;
		if (DayNightCycle.main.timePassedAsFloat - this.timeDespawnStart > this.despawnDelay)
		{
			base.CancelInvoke();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04002EC4 RID: 11972
	[ProtoMember(1)]
	[NonSerialized]
	public float timeDecayStart;

	// Token: 0x04002EC5 RID: 11973
	public float foodValue;

	// Token: 0x04002EC6 RID: 11974
	public float waterValue;

	// Token: 0x04002EC7 RID: 11975
	[ProtoMember(4)]
	[NonSerialized]
	public bool decayPaused;

	// Token: 0x04002EC8 RID: 11976
	[ProtoMember(5)]
	[NonSerialized]
	public float timeDecayPause;

	// Token: 0x04002EC9 RID: 11977
	public float healthValue;

	// Token: 0x04002ECA RID: 11978
	[Tooltip("How many seconds that eating this reduces rate at which the player gets cold when Exposed.")]
	public float warmthTime;

	// Token: 0x04002ECB RID: 11979
	[Tooltip("How much eating this changes the current cold meter value. Cold meter goes from 0 to 100.")]
	public float coldMeterValue;

	// Token: 0x04002ECC RID: 11980
	public bool decomposes;

	// Token: 0x04002ECD RID: 11981
	public bool despawns = true;

	// Token: 0x04002ECE RID: 11982
	public float kDecayRate;

	// Token: 0x04002ECF RID: 11983
	public float despawnDelay = 300f;

	// Token: 0x04002ED0 RID: 11984
	private bool wasActive;

	// Token: 0x04002ED1 RID: 11985
	private float timeDespawnStart;
}
