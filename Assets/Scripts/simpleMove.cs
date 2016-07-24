using UnityEngine;
using System.Collections;

public class simpleMove : MonoBehaviour {
	// Update is called once per frame
    public float speed;
    public ArrowDirState direction;
    public SpecialBase SpecialScript = null;
    public GameObject target;
    public bool heavy = false;

    void Update ()
    {
        transform.Translate(Vector3.down * speed * Time.unscaledDeltaTime);
	}

    public virtual void DoRotation()
    {
        switch (direction)
        {
            case ArrowDirState.RightUp:
                transform.Rotate(0, 0, -45);
                break;
            case ArrowDirState.Right:
                transform.Rotate(0, 0, 0);
                break;

            case ArrowDirState.RightDown:
                transform.Rotate(0, 0, 45);
                break;

            case ArrowDirState.Down:
                transform.Rotate(0, 0, 90);
                break;

            case ArrowDirState.LeftDown:
                GetComponent<SpriteRenderer>().flipX = true;
                transform.Rotate(0, 0, 45);
                break;

            case ArrowDirState.Left:
                GetComponent<SpriteRenderer>().flipX = true;
                break;

            case ArrowDirState.LeftUp:
                GetComponent<SpriteRenderer>().flipX = true;
                transform.Rotate(0, 0, -45);
                break;

            case ArrowDirState.Up:
                transform.Rotate(0, 0, -90);
                break;
        }
    }
}
