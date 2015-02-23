using UnityEngine;
using System.Collections;

/*
 * Attribution: Part of this solution is taken from http://answers.unity3d.com/questions/686293/using-gl-to-draw-after-ongui.html
 */

public class DrawLine : MonoBehaviour {
    private Vector3 _xyPos;
    private Material _lineMaterial;

    void Start()
    {
        CreateLineMaterial();
    }

    private void CreateLineMaterial()
    {
        _lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
            "SubShader { Pass { " +
            "    Blend SrcAlpha OneMinusSrcAlpha " +
            "    ZWrite Off Cull Off Fog { Mode Off } " +
            "    BindChannels {" +
            "      Bind \"vertex\", vertex Bind \"color\", color }" +
            "} } }");
        _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        _lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        _xyPos = Input.mousePosition;
    }

    IEnumerator OnPostRenderMeth()
    {
        yield return new WaitForEndOfFrame();

        GL.PushMatrix();
        GL.LoadPixelMatrix();
        _lineMaterial.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex(new Vector3(_xyPos.x / (float)Screen.width, 0, 0));
        GL.Vertex(new Vector3(_xyPos.x / (float)Screen.width, 1, 0));

        GL.Color(Color.green);
        GL.Vertex(new Vector3(0, _xyPos.y / (float)Screen.height, 0));
        GL.Vertex(new Vector3(1, _xyPos.y / (float)Screen.height, 0));

        GL.End();
        GL.PopMatrix();
    }

    void OnPostRender()
    {
        StartCoroutine(OnPostRenderMeth());
    }
}
