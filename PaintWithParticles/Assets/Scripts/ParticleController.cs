using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public struct FaceCube
{
    public Vector3 position;
    public Color color;
}

public class ParticleController : MonoBehaviour
{
    [SerializeField] private ComputeShader com;
    public RenderTexture renderTexture;
    [SerializeField] private Material drawMaterial;
    [SerializeField] private FaceCube[] data;
   
    
    private ParticleSystem part;
    ParticleSystem.Particle[] particles;
    [SerializeField] private Texture tex;
    private int resX;
    private int resY;
    
    void Start()
    {
        resX = tex.width;
        resY = tex.height;
        
        GameObject o = new GameObject("Particles");
        part = o.AddComponent<ParticleSystem>();
        var col = part.collision;
        col.enabled = true;
        col.mode = ParticleSystemCollisionMode.Collision3D;
        var main = part.main;
        part.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        main.maxParticles = resX * resY;
        main.startSpeed = 0;
        main.startLifetime = 150;
        
        var emis = part.emission;
        emis.rateOverTime = 1000000;
     
        CreateRenderTex();
       
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateParticles();
        }
    }


    private void CreateParticles()
    {
        SetCubes();
        SetAndReadData();
    }

    private void CreateRenderTex()
    {
        renderTexture = new RenderTexture(resX, resY, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        RenderTexture temp = RenderTexture.GetTemporary(renderTexture.width,renderTexture.height,24,RenderTextureFormat.ARGBFloat);
        Graphics.Blit(renderTexture,temp);
        Graphics.Blit(temp,renderTexture,drawMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
    

    private void SetCubes()
    {
        data = new FaceCube[resX * resY];
        for (int i = 0; i < resX*resY; i++)
        {
            FaceCube f = new FaceCube();
            f.color = Color.black;
            f.position= Vector3.zero;
            data[i] = f;
        }
    }

    private void SetAndReadData()
    {
        int kernel = com.FindKernel("CSMain");
       
        com.SetTexture(kernel,"Result",renderTexture);
        com.SetInt("width",resX);
        int colorSize = sizeof(float) * 4;
        int vectorSize = sizeof(float) * 3;
        int totalSize = colorSize + vectorSize;
        
        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);
        com.SetBuffer(kernel,"cubes",cubesBuffer);
        com.Dispatch(kernel,resX,resY,1);
        cubesBuffer.GetData(data);
        
        var particles = new ParticleSystem.Particle[part.main.maxParticles];
        var currentAmount = part.GetParticles(particles);
        
        
        for (int i = 0; i < currentAmount; i++)
        {
            FaceCube cube = data[i];

            particles[i].position = cube.position;
            particles[i].startColor = cube.color;
        }
        part.SetParticles(particles, currentAmount);
       
        cubesBuffer.Dispose();
    }
}
