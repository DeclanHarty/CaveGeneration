using UnityEngine;

public class Draggable : MonoBehaviour
{
    public void OnMouseDrag(){
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        WhenMouseDragged();
    }

    public virtual void WhenMouseDragged(){}
}
