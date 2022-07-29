using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


/// <summary>
/// Clase MonoBehaviour singleton (serializada con Odin) de la que heredar para converir a un componente en sigleton
/// 
/// https://forum.unity.com/threads/singleton-monobehaviour-script.99971/
/// https://gist.github.com/kurtdekker/775bb97614047072f7004d6fb9ccce30
/// </summary>
/// <typeparam name="T">Tipo heredero de esta clase</typeparam>
public class SingletonMono<T> : SerializedMonoBehaviour where T : SingletonMono<T>
{
    /// <summary>
    /// Instancia singleton referenciada para esta clase
    /// </summary>
    private static T _self = default(T);

    /// <summary>
    /// Acceso estático a la referencia con logica para generar o buscar una instancia existente
    /// </summary>
    public static T self
    { 
        get
        {
            //Si no hay un objeto referenciado buscamos o creamos uno
            if (_self == default(T))
            {
                T[] objects = (T[]) FindObjectsOfType(typeof(T));

                //Si se encuentran ya creados, nos quedamos con el primero y eliminamos el resto
                if (objects.Length > 0)
                {
                    _self = objects[0];

                    for (int i = 1; i < objects.Length; i++)
                    {
                        Debug.LogError("Varias instancias del singleton " + typeof(T).Name 
                            + ". Eliminando instancia en GO " + objects[i].gameObject.name);
                        Destroy(objects[i].gameObject);
                    }
                }
                //Si no hay en escena creamos un gameobject con este componente
                else
                {
                    _self = new GameObject().AddComponent<T>();
                    _self.name = _self.GetType().ToString();
                }
            }

            return _self;
        }
    }

    protected virtual void Awake()
    {
        //Si no hay un singleton asignado, esta instancia convierte en él
        if(_self == default(T))
        {
            _self = (T)this;
        }
        //Si un singleton asignado distinto, se elimina esta instancia
        else if(_self != this)
        {
            Debug.LogError("Singleton ya existente. Destruyendo instancia de "+typeof(T).Name);
            Destroy(gameObject);
        }
    }
}
