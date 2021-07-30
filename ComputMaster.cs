using System;
using UnityEngine;

public class ComputeMaster : Monobehaviour
{
    public int width, height, dustCount;
    public float sizeRatio;
    public ComputeShader dustShader;

    private RenderTexture output, currents;
    private ComputeBuffer dustBuffer;

    private void OnRenderImage (RenderTexture source, RenderTexture dest)
    {
        Render(dest);
    }

    private void Render (RenderTexture tex)
    {
        InitTextureMaps();

        dustShader.SetTexture(1, "OutputMap", output);
        int threadX = Mathf.CeilToInt(dustCount / 16);
        dustShader.Dispatch(1, threadX, 1, 1);

        Graphics.Blit(output, dest);
    }

    private void OnEnable()
    {
        List<Dust> dust = new List<Dust>();

        for (int i = 0; i < dustCount; i++)
        {
            Dust d = new Dust();

            Vector3 col = Random.InsideUnitSphere;
            d.colour = new Vector4(col.x, col.y, col.z, 1);

            d.pos.x = Random.Range(0, width);
            d.pos.y = Random.Range(0, height);

            d.direction = Random.InsideUnitCircle.normalized;

            d.speed = 1;

            dust.Add(d);
        }

        dustBuffer = new ComputeBuffer(dustCount, 36);
        dustBuffer.SetData(dust);
    }

    private void OnDisable()
    {
        if (dustBuffer != null)
        {
            dustBuffer.Release();
        }
    }

    void InitTextureMaps ()
    {
        if (output == null || output.width != width || output.height != height)
        {
            if (output != null)
            {
                output.Release();
            }
            if (currents != null)
            {
                currents.Release();
            }

            output = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            output.enableReadWrite = true;
            output.Create;

            dustShader.SetFloat("sizeRatio", sizeRatio);
            dustShader.SetFloat("speedLimit", 1);
            dustShader.SetInt("width", width);
            dustShader.SetInt("height", height);
            dustShader.SetInt("dustCount", dustCount);
            dustShader.SetBuffer("dustParticles", dustBuffer);

            currents = new RenderTexture(Mathf.CeilToInt(width / sizeRatio), Mathf.CeilToInt(height / sizeRatio), 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            currents.enableReadWrite = true;
            currents.Create;

            dustShader.SetTexture(0, "CurrentsMap", currents);
            int threadX = Mathf.CeilToInt((width / sizeRatio) / 8.0f);
            int threadY = Mathf.CeilToInt((height / sizeRatio) / 8.0f);
            dustShader.Dispatch(0, threadX, threadY, 1);
        }
    }

    struct Dust
    {
        public Vector4 colour;
        public Vector2 pos, direction;
        public float speed;
    }
}
