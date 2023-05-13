/// <summary>
/// Hacer elementos de UI "tocables" como un boton, pero invisibles
/// Copiado directamente de:
/// https://answers.unity.com/questions/801928/46-ui-making-a-button-transparent.html
/// </summary>

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TouchableUI : Text
{
    protected override void Awake()
    {
        base.Awake();
    }
}

#if UNITY_EDITOR
// Touchable_Editor component, to prevent treating the component as a Text object.
[CustomEditor(typeof(TouchableUI))]
public class Touchable_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        // Do nothing
    }
}
#endif