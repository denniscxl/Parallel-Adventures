using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace GKUI
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class Gradient : BaseMeshEffect
    {
        [SerializeField]
        private Color32 topColor = Color.white;
        [SerializeField]
        private Color32 bottomColor = Color.black;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;

            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);

            ModifyVertices(vertexList);

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }

        public override void ModifyMesh(Mesh mesh)
        {
            if (!IsActive())
            {
                return;
            }

            Vector3[] vertexList = mesh.vertices;
            int count = mesh.vertexCount;
            if (count > 0)
            {
                float bottomColorY = vertexList[0].y;
                float topColorY = vertexList[0].y;

                for (int i = 1; i < count; i++)
                {
                    float y = vertexList[i].y;
                    if (y > topColorY)
                    {
                        topColorY = y;
                    }
                    else if (y < bottomColorY)
                    {
                        bottomColorY = y;
                    }
                }
                List<Color32> colors = new List<Color32>();
                float uiElementHeight = topColorY - bottomColorY;
                for (int i = 0; i < count; i++)
                {
                    colors.Add(Color32.Lerp(bottomColor, topColor, (vertexList[i].y - bottomColorY) / uiElementHeight));
                }
                mesh.SetColors(colors);
            }
        }

        public void ModifyVertices(List<UIVertex> vertexList)
        {
            if (!IsActive() || vertexList.Count < 4)
            {
                return;
            }
#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
            if (vertexList.Count == 4)
            {
                SetVertexColor(vertexList, 0, bottomColor);
                SetVertexColor(vertexList, 1, topColor);
                SetVertexColor(vertexList, 2, topColor);
                SetVertexColor(vertexList, 3, bottomColor);
#else //This if has to be changed if you are using version 5.2.1p3 or later patches of 5.2.1 Use the bottomColor code for it to work.
            if (vertexList.Count == 6)
            {
                SetVertexColor(vertexList, 0, bottomColor);
                SetVertexColor(vertexList, 1, topColor);
                SetVertexColor(vertexList, 2, topColor);
                SetVertexColor(vertexList, 3, topColor);
                SetVertexColor(vertexList, 4, bottomColor);
                SetVertexColor(vertexList, 5, bottomColor);
#endif
            }
            else
            {
                float bottomColorPos = vertexList[vertexList.Count - 1].position.y;
                float topColorPos = vertexList[0].position.y;

                float height = topColorPos - bottomColorPos;

                for (int i = 0; i < vertexList.Count; i++)
                {
                    UIVertex v = vertexList[i];
                    v.color *= Color.Lerp(topColor, bottomColor, ((v.position.y) - bottomColorPos) / height);
                    vertexList[i] = v;
                }
            }
        }

        private void SetVertexColor(List<UIVertex> vertexList, int index, Color color)
        {
            UIVertex v = vertexList[index];
            v.color = color;
            vertexList[index] = v;
        }
    }
}