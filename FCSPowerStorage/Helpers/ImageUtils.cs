using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCSPowerStorage.Helpers
{
    public static class ImageUtils
    {
        private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static Sprite LoadSprite(string path, Vector2 pivot = new Vector2(), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            if (spriteCache.TryGetValue(path, out Sprite foundSprite))
            {
                return foundSprite;
            }

            Texture2D texture2D = ImageUtils.LoadTexture(path, format);
            if (!texture2D)
            {
                return null;
            }
            var sprite = TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType);
            spriteCache.Add(path, sprite);
            return sprite;
        }

        public static Sprite Load9SliceSprite(string path, RectOffset slices, Vector2 pivot = new Vector2(), TextureFormat format = TextureFormat.BC7, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            string spriteKey = path + slices;
            if (spriteCache.TryGetValue(spriteKey, out Sprite foundSprite))
            {
                return foundSprite;
            }

            Texture2D texture2D = ImageUtils.LoadTexture(path, format);
            if (!texture2D)
            {
                return null;
            }
            Vector4 border = new Vector4(slices.left, slices.right, slices.top, slices.bottom);
            var sprite = TextureToSprite(texture2D, pivot, pixelsPerUnit, spriteType, border);
            spriteCache.Add(spriteKey, sprite);
            return sprite;
        }

        public static Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.BC7, int width = 2, int height = 2)
        {
            if (textureCache.TryGetValue(path, out Texture2D tex))
            {
                return tex;
            }
            else if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                Texture2D texture2D = new Texture2D(width, height, format, false);
                if (texture2D.LoadImage(data))
                {
                    textureCache.Add(path, texture2D);
                    return texture2D;
                }
            }
            else
            {
                Console.WriteLine("[ImageUtils] ERROR: File not found " + path);
            }
            return null;
        }

        public static Sprite TextureToSprite(Texture2D tex, Vector2 pivot = new Vector2(), float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight, Vector4 border = new Vector4())
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, pixelsPerUnit, 0u, spriteType, border);
        }
    }
}
