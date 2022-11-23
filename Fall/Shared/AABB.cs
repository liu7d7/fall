namespace Fall.Shared
{
  public struct aabb
  {
    public float MinX;
    public float MinY;
    public float MinZ;
    public float MaxX;
    public float MaxY;
    public float MaxZ;

    public aabb(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
      MinX = minX;
      MinY = minY;
      MinZ = minZ;
      MaxX = maxX;
      MaxY = maxY;
      MaxZ = maxZ;
    }
  }
}