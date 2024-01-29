using FCS_AlterraHub.Core.Services;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace FCS_AlterraHub.Core.Components;
public class BiomeDetection : MonoBehaviour
{
    private string _currentBiome;
    [SerializeField]
    private UnityEvent OnBiomeDetected;


    public string GetCurrentBiome()
    {
        return _currentBiome;
    }

    private void Awake()
    {
        InvokeRepeating(nameof(LocateBiome),1f,1f);
    }

    private void LocateBiome()
    {
        if(string.IsNullOrEmpty(_currentBiome))
        {
            _currentBiome = BiomeManager.GetBiome(gameObject.transform);
            OnBiomeDetected?.Invoke();
        }
        else
        {
            CancelInvoke(nameof(LocateBiome));
        }


    }
}
