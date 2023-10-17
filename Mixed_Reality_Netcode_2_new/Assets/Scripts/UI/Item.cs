using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObject/Items")]
public class Item : ScriptableObject
{
    public GameObject prefabToSpawn;
    public Sprite icon;
}
