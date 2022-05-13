using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;


//Manejador de TODAS las habilidades (instanciar, gastar, UI, etc.)
public class SkillsController : MonoBehaviour
{
    public static SkillsController self;

    //Lista de prefabs de drops de habilidades
    public Dictionary<SkillType, GameObject> skillDropsPrefabs;

    //Lista de habilidades disponibles
    public List<Skill> skills;
    public int maxSkills = 2;


    void Awake()
    {
        //Singleton
        if (self == null)
            self = this;
        else
            Destroy(gameObject);
    }

    //Genera un objeto de habilidad especifico en un punto
    public void CreateSkillDrop(SkillType type, Vector3 vector3)
    {
        GameObject skillGO = Instantiate(skillDropsPrefabs[type]);
        Skill skillController = skillGO.GetComponent<Skill>();
    }

    //Genera un drop de habilidad de teñir de un color en un punto
    public void CreateDyeSkillDrop(InkColorIndex dyeColor, Vector3 point)
    {
        switch(dyeColor)
        {
            case InkColorIndex.CYAN:
                CreateSkillDrop(SkillType.DYE_CYAN, point);
                break;
            case InkColorIndex.MAGENTA:
                CreateSkillDrop(SkillType.DYE_MAGENTA, point);
                break;
            case InkColorIndex.YELLOW:
                CreateSkillDrop(SkillType.DYE_YELLOW, point);
                break;
        }
    }

    //Generar un drop de habilidad de eliminar selectivamente en area un color
    public void CreateBlackBombSkillDrop(Vector3 point)
    {
        CreateSkillDrop(SkillType.BLACK_BOMB, point);
    }

    //Añadir la habilidad que contiene el drop
    public void AddSkillFromDrop(GameObject skillGO)
    {
        //Añadimos su habilidad a la lista
        skills.Add(skillGO.GetComponentInChildren<Skill>());
        //Desactivamos la visualización
        skillGO.GetComponentInChildren<Renderer>().enabled = false;
    }

    //Elimina la primera habilidad y su drop de la lista de disponibles con ese tipo
    public void RemoveSkill(SkillType type)
    {
        for(int i=0; i<skills.Count ; i++)
        {
            if(skills[i].type == type)
            {
                Destroy(skills[i].gameObject);
                skills.RemoveAt(i);
                break;
            }
        }
    }
}
