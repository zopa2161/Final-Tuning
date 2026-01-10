public class CollisionResult
{ 
    public bool isCollision;
    public CollisionType collisionType;
    public ImpactDirection dir;
    public string message;
    

    public CollisionResult()
    {
        isCollision = false;
    }

    public void Reset()
    {
        isCollision = false;
    }
    
}

public enum ImpactDirection// 미리 계산되어서 들어옴,,
{
    Front,
    Rear,
    Left,
    Right
}
public enum CollisionType { Wall, Car, Obstacle}