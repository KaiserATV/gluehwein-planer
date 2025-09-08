using UnityEngine;

public class TestMovingScript : MonoBehaviour
{
    // Start is called before the first frame update
    // -                  - x  +
    // z     tl                     tr
    // +     bl                     br
    //static Vector3 topLeft = new Vector3(-129, 20, 42);
    //static Vector3 topRight = new Vector3(-76,20,42);
    //static Vector3 bottomRight = new Vector3(-76, 20, 171);
    //static Vector3 bottomLeft = new Vector3(-129, 20, 171);
    //static Vector3 Center = new Vector3(-102.5f, 0, 106.5f);

    static Vector3 topLeft = new Vector3(-41, 20, -62);
    static Vector3 topRight = new Vector3(12, 20, -62);
    static Vector3 bottomRight = new Vector3(12, 20, 66);
    static Vector3 bottomLeft = new Vector3(-41, 20, 66);


    static Vector3 Center = new Vector3(-14.5f, 0, 2f);

    public static float speed = 4.0f;

    Vector3 towards = topRight;
    Direction currentDir = Direction.Right;
    Vector3 movingDirection;

    void Start()
    {
        this.transform.position = topLeft;
        movingDirection = GetDirectionVector(currentDir);
    }


    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, towards) > 0.5f)
        {
            Vector3 dir = Center - transform.position;
            if (dir != Vector3.zero)
            {
                this.transform.SetPositionAndRotation(Vector3.MoveTowards(this.transform.position, towards, speed * Time.deltaTime), Quaternion.LookRotation(dir));
            }
        }
        else
        {
            SetNewDirectionAndTowards(towards);
        }
    }

    void SetNewDirectionAndTowards(Vector3 old)
    {
        if (old.x == topRight.x)
        {
            if (old.z == topRight.z)
            {
                currentDir = Direction.Down;
                movingDirection = GetDirectionVector(Direction.Down);
                towards = bottomRight;
            }
            else
            {
                currentDir = Direction.Left;
                movingDirection = GetDirectionVector(Direction.Left);
                towards = bottomLeft;
            }
        }
        else //must be left
        {
            if (old.z == topLeft.z)//is top left
            {
                currentDir = Direction.Right;
                movingDirection = GetDirectionVector(Direction.Right);
                towards = topRight;
            }
            else
            {
                currentDir = Direction.Up;
                movingDirection = GetDirectionVector(Direction.Up);
                towards = topLeft;
            }
        }
    }

    Vector3 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Right:
                return new(0, 0, 1);
            case Direction.Left:
                return new(0, 0, -1);
            case Direction.Up:
                return new(-1, 0, 0);
            case Direction.Down:
                return new(1, 0, 0);
            default:
                return new(0, 0, 0);
        }
    }

}

public enum Direction
{
    Right,
    Left,
    Up,
    Down
}