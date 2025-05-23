using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    void OnDragStart();
    void OnDragEnd();
    void OnDrag(Vector2 delta);
}
