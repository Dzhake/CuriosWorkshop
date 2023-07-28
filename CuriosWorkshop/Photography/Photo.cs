using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Social, RogueCategories.Trade)]
    public class Photo : CustomItem, IItemUsable
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Photo>()
                     .WithName(new CustomNameInfo
                     {
                         English = "Photo",
                         Russian = @"Фото",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "",
                         Russian = @"",
                     })
                     .WithSprite(Properties.Resources.Photo, 256f);
        }

        public Texture2D? genTexture;
        public string? genSpriteName;

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.itemValue = 10;
            Item.stackable = false;
            Item.noCountText = true;
            Item.goesInToolbar = true;
        }
        public bool UseItem()
        {
            gc.audioHandler.Play(Owner, "OpenMenu");
            // TODO: open the taken picture in a window
            return true;
        }

        public override string GetSprite()
            => genSpriteName ??= GenerateSprite(genTexture);

        private static string GenerateSprite(Texture2D? picture)
        {
            if (picture is null) return nameof(Photo);

            Texture2D polaroid = RogueUtilities.ConvertToSprite(Properties.Resources.Photo).texture;
            int width = polaroid.width;
            int height = polaroid.height;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(polaroid.GetPixels());

            const int templateScale = 4; // 256 / 64
            const float frameCornerX = 10 * templateScale;
            const float frameCornerY = 23 * templateScale;
            const float frameWidth = 40f * templateScale;
            const float frameHeight = 35f * templateScale;
            const float frameAngle = 12f * Mathf.Deg2Rad;

            int pictureWidth = picture.width;
            int pictureHeight = picture.height;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (texture.GetPixel(x, y) == Color.green)
                    {
                        float offsetX = x - frameCornerX;
                        float offsetY = y - frameCornerY;
                        float frameX = offsetX * Mathf.Cos(frameAngle) - offsetY * Mathf.Sin(frameAngle);
                        float frameY = offsetX * Mathf.Sin(frameAngle) + offsetY * Mathf.Cos(frameAngle);
                        int pictureX = Mathf.RoundToInt(frameX / frameWidth * picture.width);
                        int pictureY = Mathf.RoundToInt(frameY / frameHeight * picture.height);
                        pictureX = Mathf.Clamp(pictureX, 0, pictureWidth - 1);
                        pictureY = Mathf.Clamp(pictureY, 0, pictureHeight - 1);
                        texture.SetPixel(x, y, picture.GetPixel(pictureX, pictureY));
                    }
                }

            byte[] textureData = texture.EncodeToPNG();
            string spriteName = $"procedural_{new System.Random().Next():X4}_Photo";
            RogueLibs.CreateCustomSprite(spriteName, SpriteScope.Items, textureData, 256f);
            return spriteName;
        }


    }
}
