using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.JukeBox.Mono;
using FCSCommon.Utilities;
using FMOD;
using ProtoBuf;
using Story;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static uGUI_Overlays;

// Token: 0x02000496 RID: 1174
[ProtoContract]
public class JukeboxInstance : MonoBehaviour, IConstructable
{
    public string file
    {
        get
        {
            return this._file;
        }
        set
        {
            this._positionDirty = -0.5f;
            if (this._file != value)
            {
                this._file = value;
                JukeboxV2.TrackInfo info = JukeboxV2.GetInfo(this._file);
                this.SetLabel(info.label);
                this.SetLength(info.length);
            }
        }
    }
    private bool isControlling
    {
        get
        {
            JukeboxInstance instance = JukeboxV2.instance;
            return instance != null && instance == this;
        }
    }

    public bool IsConstructed;
    public bool IsInitialized = false;

    private void Awake()
    {
        this.canvas = GetComponentInChildren<Canvas>(true);
               
        var playBTN = GameObjectHelpers.FindGameObject(gameObject, "PlayBTN").GetComponent<Button>();
        playBTN.onClick.AddListener(OnButtonPlayPause);

        var stopBTN = GameObjectHelpers.FindGameObject(gameObject, "StopBTN").GetComponent<Button>();
        stopBTN.onClick.AddListener(OnButtonStop);

        var previousBTN = GameObjectHelpers.FindGameObject(gameObject, "PreviousBTN").GetComponent<Button>();
        previousBTN.onClick.AddListener(OnButtonPrevious);

        var nextBTN = GameObjectHelpers.FindGameObject(gameObject, "NextBTN").GetComponent<Button>();
        nextBTN.onClick.AddListener(OnButtonNext);

        var repeatBTN = GameObjectHelpers.FindGameObject(gameObject, "RepeatBTN").GetComponent<Button>();
        repeatBTN.onClick.AddListener(OnButtonRepeat);

        var shuffleBTN = GameObjectHelpers.FindGameObject(gameObject, "ShuffleBTN").GetComponent<Button>();
        shuffleBTN.onClick.AddListener(OnButtonShuffle);
    }

    private void OnEnable()
    {
        if (!_runStartUpOnEnable) return;

        if (!IsInitialized)
        {
            Initialize();
        }


    }

    public void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;

        if (constructed)
        {

            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }
    }

    private void Initialize()
    {
        if (IsInitialized) return;
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnUpdate));
        this.canvas.enabled = true;
        SetEnabled(true);
        JukeboxInstance.all.Add(this);
        IsInitialized = true;
    }

    public void SetEnabled(bool state)
    {
        base.enabled = state;
        if (base.enabled)
        {
            //canvasLink.SetCanvasesEnabled(canvasLink.renderer.isVisible);
            //canvasLink.SetRectMasksEnabled(canvasLink.renderer.isVisible);
        }
    }

    // Token: 0x06002585 RID: 9605 RVA: 0x000B4D80 File Offset: 0x000B2F80
    private void OnDisable()
    {
        JukeboxInstance.all.Remove(this);
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnUpdate));
        this.canvas.enabled = false;
        SetEnabled(false);
        if (this.isControlling)
        {
            JukeboxV2.Stop();
        }
    }

    // Token: 0x06002586 RID: 9606 RVA: 0x000B4DD4 File Offset: 0x000B2FD4
    private void Start()
    {
        this._baseComp = base.gameObject.GetComponentInParent<Base>();
        this._powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();               
        
        if (this.flashRenderer != null)
        {
            this._materials = this.flashRenderer.materials;
        }

        for (int i = 0; i < imagesSpectrum.Length; i++)
        {
            var image = imagesSpectrum[i];
            
            var mat = MaterialHelpers.CreateUIBarShader(image.material, "Specturm", "JukeboxSpectrumOn", ModelPrefab.ModBundle,false);
            image.material = mat;
        }

        this._materialPosition = MaterialHelpers.CreateUIBarShader(this.imagePosition.material, "JukeboxVolume", "JukeboxVolumeOn", ModelPrefab.ModBundle);
        this.imagePosition.material = this._materialPosition;
        
        this._materialVolume = MaterialHelpers.CreateUIBarShader(this.imageVolume.material, "JukeboxVolume", "JukeboxVolumeOn", ModelPrefab.ModBundle);
        this.imageVolume.material = this._materialVolume;

        Material material = this.imagesSpectrum[0].material;
        this._materialsSpectrum = new Material[this.imagesSpectrum.Length];
        for (int i = 0; i < this.imagesSpectrum.Length; i++)
        {
            Material material2 = new Material(material);
            this.imagesSpectrum[i].material = material2;
            this._materialsSpectrum[i] = material2;
        }
        JukeboxV2.TrackInfo info = JukeboxV2.GetInfo(this._file);
        this.SetLabel(info.label);
        this.SetLength(info.length);
        this.textPosition.text = JukeboxV2.FormatTime(0U, 0U, 0U);
        this.imageRepeat.sprite = this.spritesRepeat[(int)this.repeat];
        this.imageShuffle.sprite = (this.shuffle ? this.spriteShuffleOn : this.spriteShuffleOff);
        this.UpdateVolumeSlider();
        this.textVolume.text = AuxPatchers.JukeBoxVolumeFormat(volume);
        this.OnRelease();

        volumePointEventTrigger.onDrag.AddListener(OnVolume);
        volumePointEventTrigger.onPointerUp.AddListener(OnVolume);
        volumePointEventTrigger.onInitializePotentialDrag.AddListener(OnVolumeInitDrag);
        volumePointEventTrigger.onPointerHover.AddListener(OnVolumeHover);


        timelinePointEventTrigger.onBeginDrag.AddListener(OnPositionBeginDrag);
        timelinePointEventTrigger.onDrag.AddListener(OnPositionDrag);
        timelinePointEventTrigger.onEndDrag.AddListener(OnPositionEndDrag);
        timelinePointEventTrigger.onPointerUp.AddListener(OnPositionUp);
        timelinePointEventTrigger.onPointerDown.AddListener(OnPositionDown);
        timelinePointEventTrigger.onInitializePotentialDrag.AddListener(OnPositionInitDrag);
        timelinePointEventTrigger.onPointerHover.AddListener(OnPositionHover);
    }

    // Token: 0x06002587 RID: 9607 RVA: 0x000B4F5E File Offset: 0x000B315E
    private void OnUpdate()
    {
        this.UpdatePower();
        this.UpdateUI();
        this.UpdateEffects();
        this.NotifyLabelChange();
        this.rectMask.enabled = this.LOD.IsFull();
    }

    // Token: 0x06002588 RID: 9608 RVA: 0x000B4F90 File Offset: 0x000B3190
    private void OnDestroy()
    {
        if (this._materials != null)
        {
            for (int i = 0; i < this._materials.Length; i++)
            {
                Material material = this._materials[i];
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                }
            }
        }
    }

    // Token: 0x06002589 RID: 9609 RVA: 0x000B4FD0 File Offset: 0x000B31D0
    public void OnControl()
    {
        this.SetPositionKnobVisible(true);
    }

    // Token: 0x0600258A RID: 9610 RVA: 0x000B4FDC File Offset: 0x000B31DC
    public void OnRelease()
    {
        this.SetPositionKnobVisible(false);
        if (this._cachedPosition != 4294967295U)
        {
            this._cachedPosition = uint.MaxValue;
            this._stringPosition = "-:--";
            this.textPosition.text = this._stringPosition;
            this._position = 0f;
            this.UpdateSpectrum(true);
            this.UpdatePositionSlider();
        }
    }

    // Token: 0x0600258B RID: 9611 RVA: 0x000B5034 File Offset: 0x000B3234
    public static void NotifyInfo(string id, JukeboxV2.TrackInfo info)
    {
        for (int i = 0; i < JukeboxInstance.all.Count; i++)
        {
            JukeboxInstance jukeboxInstance = JukeboxInstance.all[i];
            if (jukeboxInstance.file == id)
            {
                jukeboxInstance.SetLabel(info.label);
                jukeboxInstance.SetLength(info.length);
            }
        }
    }

    // Token: 0x0600258C RID: 9612 RVA: 0x000B5088 File Offset: 0x000B3288
    private void UpdatePower()
    {
        if (!this.isControlling || !JukeboxV2.isStartingOrPlaying || JukeboxV2.paused)
        {
            return;
        }
        if (!this.ConsumePower())
        {
            JukeboxV2.Stop();
            this.SetLabel(AuxPatchers.JukeboxNoPower());
        }
    }

    // Token: 0x0600258D RID: 9613 RVA: 0x000B50C4 File Offset: 0x000B32C4
    private bool ConsumePower()
    {
        
        if (!GameModeUtils.RequiresPower())
        {
            return true;
        }
        float amount = Time.deltaTime * 0.1f * this.volume;
        float num;
        return (this._baseComp == null || this._baseComp.powerStatus == PowerSystem.Status.Normal && this._powerRelay != null && this._powerRelay.ConsumeEnergy(amount, out num));
    }

    // Token: 0x0600258E RID: 9614 RVA: 0x000B5134 File Offset: 0x000B3334
    private void UpdateUI()
    {
        if (this.isControlling)
        {
            uint length = JukeboxV2.length;
            this.SetLength(length);


            if (Time.unscaledTime > this._positionDirty + 0.5f)
            {
                float num = this._positionDrag ? ((float)(this._position * (float)length)) : (float)JukeboxV2.position;
                if (JukeboxV2.ToSeconds((uint)num, ref this._cachedPosition))
                {
                    this._stringPosition = JukeboxV2.FormatTime(this._cachedPosition);
                    this.textPosition.text = this._stringPosition;
                }

                this._position = (((float)length > 0f) ? (num / (float)length) : 0f);
                this.UpdatePositionSlider();
            }
            this.UpdateSpectrum(false);
        }
        this.UpdatePositionLabel();
        this.imagePositionKnob.sprite = (this._positionHover ? this.spriteKnobHover : this.spriteKnobNormal);
        this._positionHover = false;
        this.imageVolumeKnob.sprite = (this._volumeHover ? this.spriteKnobHover : this.spriteKnobNormal);
        this._volumeHover = false;
        this.imagePlayPause.sprite = ((this.isControlling && JukeboxV2.isStartingOrPlaying && !JukeboxV2.paused) ? this.spritePause : this.spritePlay);
    }

    // Token: 0x0600258F RID: 9615 RVA: 0x000B5264 File Offset: 0x000B3464
    private void UpdateEffects()
    {
        if (this._materials == null)
        {
            return;
        }
        float num = this._flashValue;
        float num2 = this._highValue;
        if (this.isControlling)
        {
            float num3 = 0f;
            float num4 = 0f;
            List<float> spectrum = JukeboxV2.spectrum;
            if (spectrum != null && spectrum.Count > 0)
            {
                int num5 = Mathf.Min(2, spectrum.Count);
                for (int i = 0; i < num5; i++)
                {
                    float b = spectrum[i];
                    num3 = Mathf.Max(num3, b);
                }
                int count = spectrum.Count;
                for (int j = num5; j < count; j++)
                {
                    float b2 = spectrum[j];
                    num4 = Mathf.Max(num4, b2);
                }
            }
            if (num < num3)
            {
                num = num3;
            }
            else
            {
                num = Mathf.SmoothDamp(num, num3, ref this._flashVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
            }
            if (num2 < num4)
            {
                num2 = num4;
            }
            else
            {
                num2 = Mathf.SmoothDamp(num2, num4, ref this._highVelocity, 0.05f, float.PositiveInfinity, Time.deltaTime);
            }
            num = Mathf.Clamp(num, 0.2f, 1f);
            num2 = Mathf.Clamp01(num2 * 1.75f) * 0.39999998f + 0.6f;
        }
        else if (num != 0f)
        {
            num = 0f;
            this._flashVelocity = 0f;
        }
        else if (num2 != 0f)
        {
            num2 = 0f;
            this._highVelocity = 0f;
        }
        if (this._highValue != num2)
        {
            this._highValue = num2;
            Color value = Color.Lerp(this.flashColor0, this.flashColor2, this._highValue);
            this._materials[2].SetColor(ShaderPropertyID._Color, value);
        }
        if (this._flashValue != num)
        {
            this._flashValue = num;
            Color value2 = Color.Lerp(this.flashColor0, this.flashColor1, this._flashValue);
            this._materials[2].SetColor(ShaderPropertyID._GlowColor, value2);
        }
    }

    // Token: 0x06002590 RID: 9616 RVA: 0x000B543C File Offset: 0x000B363C
    private void NotifyLabelChange()
    {
        if (!this.labelChanged)
        {
            return;
        }
        this.labelChanged = false;
        string text = this.textFile.text;
        if (!this.isControlling || !JukeboxV2.isStartingOrPlaying || JukeboxV2.paused)
        {
            return;
        }
    }

    // Token: 0x06002591 RID: 9617 RVA: 0x000B54B0 File Offset: 0x000B36B0
    private void UpdatePositionLabel()
    {
        RectTransform rectTransform = this.textFile.rectTransform;
        RectTransform rectTransform2 = rectTransform.parent as RectTransform;
        float preferredWidth = this.textFile.preferredWidth;
        float width = rectTransform2.rect.width;
        float num = preferredWidth - width;
        if (num > 0f)
        {
            float num2 = num / 100f;
            float num3 = MathExtensions.Trapezoid(1f, num2, 1f, num2, Time.time - this._labelScrollStart, true);
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = -num * num3;
            rectTransform.anchoredPosition = anchoredPosition;
        }
    }

    // Token: 0x06002592 RID: 9618 RVA: 0x000B5544 File Offset: 0x000B3744
    private void UpdatePositionSlider()
    {
        float num = this.isControlling ? this._position : 0f;
        this._materialPosition.SetFloat(ShaderPropertyID._Amount, num);
        RectTransform rectTransform = this.imagePositionKnob.rectTransform;
        RectTransform rectTransform2 = rectTransform.parent as RectTransform;
        rectTransform.anchoredPosition = new Vector2(rectTransform2.rect.width * num, 0f);
    }

    // Token: 0x06002593 RID: 9619 RVA: 0x000B55B0 File Offset: 0x000B37B0
    private void UpdateVolumeSlider()
    {
        this._materialVolume.SetFloat(ShaderPropertyID._Amount, this.volume);
        RectTransform rectTransform = this.imageVolumeKnob.rectTransform;
        RectTransform rectTransform2 = rectTransform.parent as RectTransform;
        rectTransform.anchoredPosition = new Vector2(rectTransform2.rect.width * this.volume, 0f);
    }

    // Token: 0x06002594 RID: 9620 RVA: 0x000B5610 File Offset: 0x000B3810
    private void UpdateSpectrum(bool reset)
    {
        List<float> spectrum = JukeboxV2.spectrum;
        int num = this._materialsSpectrum.Length;
        float num2 = 0f;
        if (spectrum != null && spectrum.Count > 0)
        {
            num2 = (float)(spectrum.Count - 1) / (float)num;
        }
        else
        {
            reset = true;
        }
        for (int i = 0; i < num; i++)
        {
            float num3;
            if (reset)
            {
                num3 = 0f;
            }
            else
            {
                int index = (int)(num2 * (float)i);
                num3 = spectrum[index];
                num3 = FMODExtensions.RemapDb(FMODExtensions.LinearToDb(num3));
            }
            this._materialsSpectrum[i].SetFloat(ShaderPropertyID._Amount, num3);
        }
    }

    // Token: 0x06002595 RID: 9621 RVA: 0x000B569C File Offset: 0x000B389C
    private void SetLabel(string text)
    {
        this.textFile.text = text;
        RectTransform rectTransform = this.textFile.rectTransform;
        RectTransform rectTransform2 = rectTransform.parent as RectTransform;
        float preferredWidth = this.textFile.preferredWidth;
        float width = rectTransform2.rect.width;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
        float num = preferredWidth - width;
        if (num > 0f)
        {
            this._labelScrollStart = Time.time;
            this.UpdatePositionLabel();
        }
        else
        {
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = 0.5f * -num;
            rectTransform.anchoredPosition = anchoredPosition;
        }
        this.labelChanged = true;
    }

    // Token: 0x06002596 RID: 9622 RVA: 0x000B5734 File Offset: 0x000B3934
    private void SetLength(uint length)
    {
        if (JukeboxV2.ToSeconds(length, ref this._cachedLength))
        {
            if (length == 0U)
            {
                this._stringLength = "-:--";
                this.textLength.text = this._stringLength;
                return;
            }
            this._stringLength = JukeboxV2.FormatTime(this._cachedLength);
            this.textLength.text = this._stringLength;
        }
    }

    // Token: 0x06002597 RID: 9623 RVA: 0x000B5794 File Offset: 0x000B3994
    private void SetPositionKnobVisible(bool visible)
    {
        float a = visible ? 1f : 0f;
        CanvasRenderer canvasRenderer = this.imagePositionKnob.canvasRenderer;
        Color color = canvasRenderer.GetColor();
        color.a = a;
        canvasRenderer.SetColor(color);
    }

    // Token: 0x06002598 RID: 9624 RVA: 0x000B57D4 File Offset: 0x000B39D4
    private void SwitchTrack(bool forward)
    {
        string next = JukeboxV2.GetNext(this, forward);
        if (string.IsNullOrEmpty(next))
        {
            JukeboxV2.Scan();
            next = JukeboxV2.GetNext(this, forward);
        }
        if (!string.IsNullOrEmpty(next))
        {
            this.file = next;
            return;
        }
        this._file = string.Empty;

        this.SetLabel(AuxPatchers.JukeBoxNoMusicFound(JukeboxV2.fullMusicPath));
    }

    // Token: 0x06002599 RID: 9625 RVA: 0x000B5834 File Offset: 0x000B3A34
    public void OnButtonPlayPause()
    {
        if (string.IsNullOrEmpty(this.file) || !JukeboxV2.HasFile(this._file))
        {
            this.SwitchTrack(true);
        }
        if (this.isControlling && JukeboxV2.isStartingOrPlaying)
        {
            JukeboxV2.paused = !JukeboxV2.paused;
        }
        else if (this.ConsumePower())
        {
            JukeboxV2.Play(this);
            JukeboxV2.TrackInfo info = JukeboxV2.GetInfo(this.file);
            this.SetLabel(info.label);
            this.SetLength(info.length);
        }
        else
        {
            this.SetLabel(AuxPatchers.JukeboxNoPower());
        }
    }

    // Token: 0x0600259A RID: 9626 RVA: 0x000B58E2 File Offset: 0x000B3AE2
    public void OnButtonStop()
    {
        this.imagePlayPause.sprite = this.spritePlay;
        if (this.isControlling)
        {
            JukeboxV2.Stop();
        }
    }

    // Token: 0x0600259B RID: 9627 RVA: 0x000B5902 File Offset: 0x000B3B02
    public void OnButtonPrevious()
    {
        this.SwitchTrack(false);
        if (this.isControlling)
        {
            JukeboxV2.Play(this);
        }
    }

    // Token: 0x0600259C RID: 9628 RVA: 0x000B5919 File Offset: 0x000B3B19
    public void OnButtonNext()
    {
        this.SwitchTrack(true);
        if (this.isControlling)
        {
            JukeboxV2.Play(this);
        }
    }

    // Token: 0x0600259D RID: 9629 RVA: 0x000B5930 File Offset: 0x000B3B30
    public void OnButtonRepeat()
    {
        this.repeat = JukeboxV2.GetNextRepeat(this.repeat);
        this.imageRepeat.sprite = this.spritesRepeat[(int)this.repeat];
        if (this.isControlling)
        {
            JukeboxV2.repeat = this.repeat;
        }
    }

    // Token: 0x0600259E RID: 9630 RVA: 0x000B5970 File Offset: 0x000B3B70
    public void OnButtonShuffle()
    {
        this.shuffle = !this.shuffle;
       this.imageShuffle.sprite = (this.shuffle ? this.spriteShuffleOn : this.spriteShuffleOff);
        if (this.isControlling)
        {
            JukeboxV2.shuffle = this.shuffle;
        }
    }

    // Token: 0x0600259F RID: 9631 RVA: 0x000B59C0 File Offset: 0x000B3BC0
    public void OnPositionDown(PointerEventData eventData)
    {
        this._positionDrag = true;
        this._positionDirty = -0.5f;
        float position;
        if (MaterialExtensions.GetBarValue(this.imagePosition.rectTransform, eventData, this._materialPosition, true, out position))
        {
            this._position = position;
            this.UpdatePositionSlider();
        }
    }

    // Token: 0x060025A0 RID: 9632 RVA: 0x000B5A08 File Offset: 0x000B3C08
    public void OnPositionUp(PointerEventData eventData)
    {
        this._positionDrag = false;
        if (!this.isControlling)
        {
            return;
        }
        float position;
        if (MaterialExtensions.GetBarValue(this.imagePosition.rectTransform, eventData, this._materialPosition, true, out position))
        {
            this._position = position;
            this.UpdatePositionSlider();
            this._positionDirty = Time.unscaledTime;
            JukeboxV2.position = (uint)(this._position * JukeboxV2.length);
        }
    }

    // Token: 0x060025A1 RID: 9633 RVA: 0x000B5A6D File Offset: 0x000B3C6D
    public void OnPositionInitDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    // Token: 0x060025A2 RID: 9634 RVA: 0x000B5A76 File Offset: 0x000B3C76
    public void OnPositionBeginDrag(PointerEventData eventData)
    {
        this._positionDrag = true;
    }

    // Token: 0x060025A3 RID: 9635 RVA: 0x000B5A7F File Offset: 0x000B3C7F
    public void OnPositionDrag(PointerEventData eventData)
    {
        this.OnPositionDown(eventData);
    }

    // Token: 0x060025A4 RID: 9636 RVA: 0x000B5A88 File Offset: 0x000B3C88
    public void OnPositionEndDrag(PointerEventData eventData)
    {
        this._positionDrag = false;
    }

    // Token: 0x060025A5 RID: 9637 RVA: 0x000B5A91 File Offset: 0x000B3C91
    public void OnPositionHover(PointerEventData eventData)
    {
        this._positionHover = true;
    }

    // Token: 0x060025A6 RID: 9638 RVA: 0x000B5A6D File Offset: 0x000B3C6D
    public void OnVolumeInitDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    // Token: 0x060025A7 RID: 9639 RVA: 0x000B5A9C File Offset: 0x000B3C9C
    public void OnVolume(PointerEventData eventData)
    {
        float arg;
        if (MaterialExtensions.GetBarValue(this.imageVolume.rectTransform, eventData, this._materialVolume, true, out arg))
        {
            this.volume = arg;
            this.UpdateVolumeSlider();
            this.textVolume.text = AuxPatchers.JukeBoxVolumeFormat(arg);
            if (this.isControlling)
            {
                JukeboxV2.volume = this.volume;
            }
        }
    }

    // Token: 0x060025A8 RID: 9640 RVA: 0x000B5B00 File Offset: 0x000B3D00
    public void OnVolumeHover(PointerEventData eventData)
    {
        this._volumeHover = true;
    }

    // Token: 0x060025A9 RID: 9641 RVA: 0x000B5B09 File Offset: 0x000B3D09
    public void OnScan()
    {
        JukeboxV2.Scan();
        if (string.IsNullOrEmpty(this._file) || !JukeboxV2.HasFile(this._file))
        {
            this.SwitchTrack(true);
        }
    }

    // Token: 0x060025AA RID: 9642 RVA: 0x000B5B34 File Offset: 0x000B3D34
    public bool GetSoundPosition(out Vector3 position, out float min, out float power)
    {
        Vector3 position2 = ((Player.main != null) ? Player.main.transform : MainCamera.camera.transform).position;
        Speaker.GetSpeakers(Speaker.GetHost(this), position2, 100f, JukeboxInstance.speakers);
        position = Vector3.zero;
        power = 0f;
        min = float.MaxValue;
        if (JukeboxInstance.speakers.Count > 0)
        {
            for (int i = 0; i < JukeboxInstance.speakers.Count; i++)
            {
                Vector3 vector = JukeboxInstance.speakers[i].position - position2;
                float sqrMagnitude = position.sqrMagnitude;
                float sqrMagnitude2 = vector.sqrMagnitude;
                if (sqrMagnitude2 < min)
                {
                    min = sqrMagnitude2;
                }
                power += 1f / sqrMagnitude2;
                float num = (sqrMagnitude == 0f) ? 1f : (sqrMagnitude / (sqrMagnitude + sqrMagnitude2));
                position.Set(position.x + num * (vector.x - position.x), position.y + num * (vector.y - position.y), position.z + num * (vector.z - position.z));
            }
            position += position2;
            min = Mathf.Sqrt(min);
            return true;
        }
        return false;
    }

    public bool IsDeconstructionObstacle()
    {
        return true;
    }

    public bool CanDeconstruct(out string reason)
    {
        reason= string.Empty; return true;
    }

    // Token: 0x04002764 RID: 10084
    private const int currentVersion = 1;

    // Token: 0x04002765 RID: 10085
    private const int flashFreqBars = 2;

    // Token: 0x04002766 RID: 10086
    private const float flashSmoothTime = 0.2f;

    // Token: 0x04002767 RID: 10087
    private const float flashMaxSpeed = float.PositiveInfinity;

    // Token: 0x04002768 RID: 10088
    private const float highSmoothTime = 0.05f;

    // Token: 0x04002769 RID: 10089
    private const float highMaxSpeed = float.PositiveInfinity;

    // Token: 0x0400276A RID: 10090
    private const float highMin = 0.6f;

    // Token: 0x0400276B RID: 10091
    private const float moveDelay = 1f;

    // Token: 0x0400276C RID: 10092
    private const float moveSpeed = 100f;


    // Token: 0x04002770 RID: 10096
    private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateAfterInput;

    // Token: 0x04002771 RID: 10097
    private const float positionDirtyTimeout = 0.5f;

    // Token: 0x04002772 RID: 10098
    private const float powerConsumption = 0.1f;

    // Token: 0x04002774 RID: 10100
    public Color flashColor0 = new Color(0f, 0f, 0f, 1f);

    // Token: 0x04002775 RID: 10101
    public Color flashColor1 = Color.cyan;

    // Token: 0x04002776 RID: 10102
    public Color flashColor2 = new Color(0f, 0.503f, 1f, 1f);

    // Token: 0x04002777 RID: 10103
    [AssertNotNull]
    public Canvas canvas;

    // Token: 0x04002778 RID: 10104
    [AssertNotNull]
    public CanvasLink canvasLink;

    // Token: 0x04002779 RID: 10105
    public Renderer flashRenderer;

    // Token: 0x0400277A RID: 10106
    [AssertNotNull]
    public TextMeshProUGUI textFile;

    // Token: 0x0400277B RID: 10107
    [AssertNotNull]
    public TextMeshProUGUI textPosition;

    // Token: 0x0400277C RID: 10108
    [AssertNotNull]
    public Image imagePosition;

    // Token: 0x0400277D RID: 10109
    [AssertNotNull]
    public TextMeshProUGUI textLength;

    // Token: 0x0400277E RID: 10110
    [AssertNotNull]
    public Image imageRepeat;

    // Token: 0x0400277F RID: 10111
    [AssertNotNull]
    public Image imagePlayPause;

    // Token: 0x04002780 RID: 10112
    [AssertNotNull]
    public Image imageShuffle;

    // Token: 0x04002781 RID: 10113
    [AssertNotNull]
    public Image imagePositionKnob;

    // Token: 0x04002782 RID: 10114
    [AssertNotNull]
    public Image imageVolume;

    // Token: 0x04002783 RID: 10115
    [AssertNotNull]
    public Image imageVolumeKnob;

    // Token: 0x04002784 RID: 10116
    [AssertNotNull]
    public TextMeshProUGUI textVolume;

    //// Token: 0x04002785 RID: 10117
    [AssertNotNull]
    public Image[] imagesSpectrum;

    // Token: 0x04002786 RID: 10118
    [AssertNotNull]
    public Sprite spritePlay;

    // Token: 0x04002787 RID: 10119
    [AssertNotNull]
    public Sprite spritePause;

    // Token: 0x04002788 RID: 10120
    [AssertNotNull]
    public Sprite spriteShuffleOn;

    // Token: 0x04002789 RID: 10121
    [AssertNotNull]
    public Sprite spriteShuffleOff;

    // Token: 0x0400278A RID: 10122
    [AssertNotNull]
    public Sprite[] spritesRepeat;

    // Token: 0x0400278B RID: 10123
    [AssertNotNull]
    public Sprite spriteKnobNormal;

    // Token: 0x0400278C RID: 10124
   [AssertNotNull]
    public Sprite spriteKnobHover;

    // Token: 0x0400278D RID: 10125
    [AssertNotNull]
    public BehaviourLOD LOD;

    // Token: 0x0400278E RID: 10126
    [AssertNotNull]
    public RectMask2D rectMask;

    // Token: 0x0400278F RID: 10127
    [ProtoMember(1)]
    [NonSerialized]
    public int version = 1;

    // Token: 0x04002790 RID: 10128
    [ProtoMember(2)]
    [NonSerialized]
    public string _file = string.Empty;

    // Token: 0x04002791 RID: 10129
    [ProtoMember(3)]
    [NonSerialized]
    public float volume = 1f;

    // Token: 0x04002792 RID: 10130
    [ProtoMember(4)]
    [NonSerialized]
    public JukeboxV2.Repeat repeat = JukeboxV2.Repeat.All;

    // Token: 0x04002793 RID: 10131
    [ProtoMember(5)]
    [NonSerialized]
    public bool shuffle;

    // Token: 0x04002794 RID: 10132
    private static List<JukeboxInstance> all = new List<JukeboxInstance>();

    // Token: 0x04002795 RID: 10133
    private float _position;

    // Token: 0x04002796 RID: 10134
    private string _stringPosition;

    // Token: 0x04002797 RID: 10135
    private string _stringLength;

    // Token: 0x04002798 RID: 10136
    private uint _cachedPosition;

    // Token: 0x04002799 RID: 10137
    private uint _cachedLength = uint.MaxValue;

    // Token: 0x0400279A RID: 10138
    private float _positionDirty = -0.5f;

    // Token: 0x0400279B RID: 10139
    private float _highValue = -1f;

    // Token: 0x0400279C RID: 10140
    private float _flashValue = -1f;

    // Token: 0x0400279D RID: 10141
    private float _flashVelocity;

    // Token: 0x0400279E RID: 10142
    private float _highVelocity;

    // Token: 0x0400279F RID: 10143
    private Material[] _materials;

    // Token: 0x040027A0 RID: 10144
    private Material _materialPosition;

    // Token: 0x040027A1 RID: 10145
    private Material _materialVolume;

    // Token: 0x040027A2 RID: 10146
    private float _labelScrollStart;

    // Token: 0x040027A3 RID: 10147
    private Material[] _materialsSpectrum;

    // Token: 0x040027A4 RID: 10148
    private bool _positionHover;

    // Token: 0x040027A5 RID: 10149
    private bool _volumeHover;

    // Token: 0x040027A6 RID: 10150
    private bool _positionDrag;

    // Token: 0x040027A7 RID: 10151
    private Base _baseComp;

    // Token: 0x040027A8 RID: 10152
    private PowerRelay _powerRelay;

    // Token: 0x040027A9 RID: 10153
    private bool labelChanged;

    // Token: 0x040027AA RID: 10154
    private static List<Speaker> speakers = new List<Speaker>();
    private bool _runStartUpOnEnable;
    public PointerEventTrigger volumePointEventTrigger;
    public PointerEventTrigger timelinePointEventTrigger;

    //public Slider volumeSlider;

    //public Slider timSlider;

    // Token: 0x02000BC2 RID: 3010
    [Serializable]
    public class MaterialProperty
    {
        // Token: 0x040060E8 RID: 24808
        public int materialId;

        // Token: 0x040060E9 RID: 24809
        public string name;

        // Token: 0x040060EA RID: 24810
        [HideInInspector]
        [NonSerialized]
        public Material material;

        // Token: 0x040060EB RID: 24811
        [HideInInspector]
        [NonSerialized]
        public int propertyId;
    }
}
