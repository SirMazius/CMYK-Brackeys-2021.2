using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public float radius = 0.5f;
    public Color notSelectedColor = Color.white;
    public Color selectedColor = Color.yellow;


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = (Selection.activeGameObject == gameObject) ? selectedColor : notSelectedColor;
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
