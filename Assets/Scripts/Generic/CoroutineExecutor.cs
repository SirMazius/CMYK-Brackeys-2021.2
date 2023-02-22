using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// Clase creada para ejecutar corutinas en clases que no extienden de MonoBehaviour. 
/// 
/// IMPORTANTE: Para ejecutar una corrutina es necesario llamar a StartCoroutine en un MonoBehaviour que esté activo, y que debe 
/// estar asociado a un gameobject que esté activo en la jerarquia de la escena
/// 
/// </summary>
public class CoroutineExecutor : MonoBehaviour
{
    //Lazy Singleton (no se crea hasta que se necesita)
    private static CoroutineExecutor _instance = null;

    //Devuelve la instancia singleton válida del componente, destruyendo duplicados y componentes mal creados y creando uno nuevo si es necesario
    public static CoroutineExecutor Instance
    {
        get 
        {
            //Si no hay instancia asignada, buscamos una valida ya creada y borramos las demas
            if(!_instance)
            {
                var executors = FindObjectsOfType<CoroutineExecutor>();
                foreach(var e in executors)
                {
                    if (!_instance && IsInstanceOK(e))
                        _instance = e;
                    else
                        DestroyExecutor(e);
                }
            }

            //Si no esta ya creado en un gameobject activo, creamos uno
            if (!_instance || !_instance.gameObject || !_instance.gameObject.activeInHierarchy)
            {
                //Destruimos instancias mal creadas
                DestroyExecutor(_instance);

                //Creamos un nuevo objeto en escena con el componente ejecutor de corrutinas
                _instance = CreateExecutor();
            }

            return _instance;
        }
    }


    #region GESTIÓN INSTANCIAS

    //Devuelve si es una instancia con gameobject activo en jerarquía
    private static bool IsInstanceOK(CoroutineExecutor executor)
    {
        return (executor && executor.gameObject && executor.gameObject.activeInHierarchy);
    }

    //Destruye el ejecutor y su gameobject (si tiene)
    private static void DestroyExecutor(CoroutineExecutor executor)
    {
        if (executor)
        {
            if(executor.gameObject)
                Destroy(executor.gameObject);

            Destroy(executor);
        }
    }

    //Creamos un nuevo objeto en escena con el componente ejecutor de corrutinas
    private static CoroutineExecutor CreateExecutor()
    {
        GameObject go = new GameObject("CoroutineExecutor");
        return go.AddComponent<CoroutineExecutor>();
    }
    
    #endregion
}
