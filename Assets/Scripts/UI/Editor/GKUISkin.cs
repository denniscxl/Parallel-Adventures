using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GKUI
{
    public class GKUISkin : ScriptableObject
    {

        [System.Serializable]
        public class TextureSkin
        {
            public Texture2D tex;
            public Sprite sprite;
            public Color color = Color.white;

            public Image.Type imageType = Image.Type.Simple;
            public bool raycast = false;

        }
        public TextureSkin texSkin = new TextureSkin();

        [System.Serializable]
        public class ButtonSkin
        {
            public Texture2D normalTex;

            public Sprite normalSprite;
            public Sprite highLightSprite;
            public Sprite pressSprite;
            public Sprite disableSprite;

            public Color normalColor = Color.white;
            public Color highLightColor = Color.white;
            public Color pressColor = Color.white;
            public Color disableColor = Color.white;

            public Animation animation;
            public AudioClip clickClip;

            public Image.Type imageType = Image.Type.Simple;
        }
        public ButtonSkin btnSkin = new ButtonSkin();

        [System.Serializable]
        public class TextSkin
        {
            public Font font;
            public string content = "";
            public int size = 24;
            public int lineSpace = 1;
            public int alignmentLR = 1; // 0: Left 1: Middle 2: Right.
            public int alignmentTB = 1; // 0: Top 1: Middle 2: Bottom.
            public bool richText = true;
            public Color color = Color.black;
        }
        public TextSkin textSkin = new TextSkin();
    }
}