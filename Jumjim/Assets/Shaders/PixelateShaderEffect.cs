using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelateEffect : MonoBehaviour
{
    public Shader pixelateShader;
    private Material pixelateMat;

    public Vector2 pixelResolution = new Vector2(320, 180); // DUSK-like resolution

    void Start()
    {
        if (pixelateShader == null)
            pixelateShader = Shader.Find("Custom/Pixelate");

        pixelateMat = new Material(pixelateShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (pixelateMat != null)
        {
            pixelateMat.SetVector("_PixelSize", pixelResolution);
            Graphics.Blit(src, dest, pixelateMat);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
