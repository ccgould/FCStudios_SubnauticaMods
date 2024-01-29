using FCS_AlterraHub.API;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using FMODUnity;
using Nautilus.FMod.Interfaces;
using Nautilus.Utility;
using System.Collections;
using UnityEngine;
using UWE;


namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono.FMod;
internal class DrillSoundHandler : IFModSound
{
    private Sound _drillSound;
    private DSP _dsp;
    private RESULT _dspResult;

    public DrillSoundHandler()
    {
        var drillSound = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName("fcsproductionsolutionsbundle", FileSystemHelper.ModDirLocation).LoadAsset<AudioClip>("soE");
        _drillSound = AudioUtils.CreateSound(drillSound,MODE.LOOP_NORMAL);

        _dspResult =  RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out this._dsp);
        this._dsp.setMeteringEnabled(true, false);
        this._dsp.setParameterInt(1, 0);
        this._dsp.setParameterInt(0, 128);


        //FMOD.DSP_DESCRIPTION desc = new DSP_DESCRIPTION();
        //_dspResult = FMODUnity.RuntimeManager.CoreSystem.createDSP(ref desc, out _dsp);


        if (_dspResult != RESULT.OK)
        {

            QuickLogger.Error($"Failed to create FMOD DSP with result: {_dspResult}");
            return;
        }

        //_dsp.setParameterFloat((int)DSP_LOWPASS.CUTOFF, 22000f);

    }

    public bool TryPlaySound(out Channel channel)
    {
        if (_dspResult != RESULT.OK)
        {
            channel = default;
            return false;
        }

        if (AudioUtils.TryPlaySound(_drillSound,AudioUtils.BusPaths.SurfaceAmbient, out channel)) 
        {
            var player = Vector3.zero.ToFMODVector();
            var velocity = Vector3.zero.ToFMODVector();

            channel.setVolume(Plugin.Configuration.MasterDeepDrillerVolume);
            channel.setPriority(0);
            channel.set3DAttributes(ref player, ref velocity);
            channel.addDSP(-3, _dsp);
            channel.set3DMinMaxDistance(1f, 1f);
            //CoroutineHost.StartCoroutine(PlayLooping(channel));
            return true;
        }

        return false;
    }

    private IEnumerator PlayLooping(Channel channel)
    {

        RESULT getCurrentSoundResult = channel.getCurrentSound(out Sound sound);
        if (getCurrentSoundResult != RESULT.OK)
        {
            QuickLogger.Warning($"[DrillSoundHandler] getCurrentSound failed {getCurrentSoundResult}");
            yield break;
        }

        var player = Player.main.transform.position.ToFMODVector();
        var velocity = Vector3.zero.ToFMODVector();

        channel.setPriority(0);
        channel.set3DAttributes(ref player, ref velocity);

        while (true)
        {
            channel.setVolume(Plugin.Configuration.MasterDeepDrillerVolume);

            if(channel.isPlaying(out bool playing) == RESULT.OK && playing)
            {
                yield return null;
                continue;
            }

            if(!AudioUtils.TryPlaySound(sound, AudioUtils.BusPaths.SurfaceAmbient,out channel))
            {
                QuickLogger.Warning("Failed to restart sound");
                yield break;
            }

            channel.setPriority(0);
            channel.set3DAttributes(ref player, ref velocity);

            yield return null;
        }

        QuickLogger.Debug($"Looped Sound Stopped");

    }
}
