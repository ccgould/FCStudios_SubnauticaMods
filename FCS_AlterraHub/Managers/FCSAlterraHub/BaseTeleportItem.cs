using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Managers.FCSAlterraHub;

internal class BaseTeleportItem : MonoBehaviour
{
    private Text _baseName;
    private BaseManager _baseManager;

    private void Awake()
    {
        _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName")?.GetComponent<Text>();
        var button = GameObjectHelpers.FindGameObject(gameObject, "TeleportBtn")?.GetComponent<Button>();
        button?.onClick.AddListener((() =>
        {
            QuickLogger.Debug($"Trying to teleport to base: {_baseManager.GetBaseName()}",true);
            FCSAlterraHubService.PublicAPI.TeleportationIgnitiated?.Invoke(_baseManager);
        }));
    }

    internal void Initialize(BaseManager manager)
    {
        _baseName.text = manager.GetBaseName();
        _baseManager = manager;
    }

    
}