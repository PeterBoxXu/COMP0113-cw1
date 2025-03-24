using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Robot Material Catalogue")]
public class RobotMaterial : ScriptableObject, IEnumerable<Material>
{
    public List<Material> Materials;

    public bool Contains(Material material)
    {
        return Materials.Contains(material);
    }

    public IEnumerator<Material> GetEnumerator()
    {
        return Materials.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Materials.GetEnumerator();
    }

    public int Count
    {
        get
        {
            return Materials.Count;
        }
    }

    public Material Get(int i)
    {
        return Materials[i];
    }

    public Material Get(string luid)
    {
        if (luid == null)
        {
            return null;
        }
        return Materials[int.Parse(luid)];
    }

    public string Get(Material material)
    {
        var index = Materials.IndexOf(material);
        return index > -1 ? $"{index}" : null;
    }
}