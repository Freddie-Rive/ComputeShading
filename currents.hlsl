#pragma kernel InitCurrentMap

RWTexture2D<float4> CurrentMap;

uint Hash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(Hash(seed)) / 4294967295.0; // 2^32-1
}

[numthreads(8, 8, 1)]
void InitCurrentMap (uint3 id: SV_DispatchThreadID)
{
    float x, y, z;
    
    x = Random(id.x); //Generate random vector2
    y = Random(id.y);
    
    z = sqrt((x * x) + (y * y)); //Normalize Vector
    x /= z;
    y /= z;

    CurrentMap[id.xy] = (x, y, 0, 0); //add vector to texture
}

#pragma kernel MoveDust

RWTexture2D<float4> OutputMap;
float sizeRatio, speedLimit;
int dustCount, width, height;

struct Dust
{
    float4 colour;
    float2 pos, direction;
    float speed;
};

StructuredBuffer<Dust> dustParticles;

[numthreads(16,1,1)]
void MoveDust (uint3 id : SV_DispatchThreadID)
{
    if (id.x >= dustCount) { return; } 
    
    uint2 posI;
    Dust d = dustParticles[id.x];
    
    posI.x = round(d.pos.x);
    posI.y = round(d.pos.y);
    
    float2 current = CurrentMap[posI / sizeRatio].xy;
    
    float2 diff = d.direction - diff;
    
    diff /= 10;
    
    d.speed = min(max(d.speed + dot(d.direction, current), -speedLimit), speedLimit);

    d.direction -= diff;
    float h = sqrt((d.direction.x * d.direction.x) + (d.direction.y * d.direction.y));
    d.direction.x /= h;
    d.direction.y /= h;
    
    OutputMap[posI] = (0, 0, 0, 0);
    
    d.pos += (d.direction * d.speed);
    
    if (d.pos.x < 0)
    {
        d.pos.x = width;
    }
    else if (d.pos > width)
    {
        d.pos.x = 0;
    }
    if (d.pos.y < 0)
    {
        d.pos.y = height;
    }
    else if (d.pos.y > height)
    {
        d.pos.y = 0;
    }
    
        
    posI.x = round(d.pos.x);
    posI.y = round(d.pos.y);
    
    OutputMap[posI] = d.colour;
}
