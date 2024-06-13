using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EncounterFramework
{
    [HotSwappable]
    public static class UIUtils
    {
        public const float LineHeight = 24f;
        public const float FirstColumnWidth = 150;
        public const float SecondColumnWidth = 200;
        public static bool ButtonSelectable(Rect rect, string label)
        {
            TextAnchor anchor = Text.Anchor;
            Color color = GUI.color;
            MouseoverSounds.DoRegion(rect);
            GUI.color = Widgets.NormalOptionColor;
            if (Mouse.IsOver(rect))
            {
                GUI.color = Widgets.MouseoverOptionColor;
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            bool wordWrap = Text.WordWrap;
            if (rect.height < Text.LineHeight * 2f)
            {
                Text.WordWrap = false;
            }
            var labelRect = rect;
            labelRect.xMin += 10;
            Widgets.Label(labelRect, label);
            Text.Anchor = anchor;
            GUI.color = color;
            Text.WordWrap = wordWrap;
            return Widgets.ButtonInvisible(rect, doMouseoverSound: false);
        }
        public static void DrawPawnPortrait(Rect pawnTemplateRect, Pawn pawn)
        {
            var pawnSize = new Vector2(pawnTemplateRect.width, pawnTemplateRect.height);
            var oldValue = Prefs.HatsOnlyOnMap;
            Prefs.HatsOnlyOnMap = false;
            PortraitsCache.SetDirty(pawn);
            GUI.DrawTexture(pawnTemplateRect, PortraitsCache.Get(pawn, pawnSize, Rot4.South));
            Prefs.HatsOnlyOnMap = oldValue;
            if (pawn.equipment?.Primary != null)
            {
                var weaponSize = pawnSize.x / 2f;
                var pawnRect = new Rect(pawnTemplateRect.x + (weaponSize / 2f), pawnTemplateRect.y + (weaponSize / 2f) + (pawnTemplateRect.height / 10f), weaponSize, weaponSize);
                var angle = pawn.equipment.Primary.def.equippedAngleOffset + 50;
                Matrix4x4 matrix = Matrix4x4.identity;
                if (angle != 0f)
                {
                    matrix = GUI.matrix;
                    UI.RotateAroundPivot(angle, pawnRect.center);
                }
                GUI.DrawTexture(pawnRect, pawn.equipment.Primary.Graphic.MatAt(Rot4.South).mainTexture);
                if (angle != 0f)
                {
                    GUI.matrix = matrix;
                }
            }
        }
    }
}