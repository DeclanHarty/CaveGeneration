
using UnityEngine;

public class TunnelPoint : Draggable
{
    private MapEditor mapEditor;

    [SerializeField]
    private GameObject sprite;
    private int index;

    public bool isSelected;
    // Start is called before the first frame update

    void OnMouseDown()
    {
        mapEditor.SetSelectedTunnelPoint(this);
        Select();
    }

    public Vector2 GetPosition(){
        return transform.position;
    }

    public void SetIndex(int index){
        this.index = index;
    }

    public int GetIndex(){
        return index;
    }

    public void SetMapEditor(MapEditor controller){
        mapEditor = controller;
    }

    public void Unselect(){
        isSelected = false;
        sprite.GetComponent<SpriteRenderer>().color = Color.gray;
    }

    public void Select(){
        isSelected = true;
        sprite.GetComponent<SpriteRenderer>().color = Color.red;
    }
}
