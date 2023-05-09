using UnityEngine;

namespace FCS_AlterraHub.Core.Extensions;

public static class SpriteExtensions
{
    /// <summary>
    /// Converts <see cref="Sprite"/> to <see cref="Atlas.Sprite"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Atlas.Sprite ToAtlasSprite(this Sprite sprite)
    {
        return new Atlas.Sprite(sprite);
    }
}
