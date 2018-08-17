using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "GM/TilesData")]
public class TilesData : ScriptableObject
{
    // TODO: Faire une sorte de dict Python like ?
    public TileBase emptySpot;
    public TileBase correctGrave;
    public TileBase wellMaintainGrave;
    public TileBase abandonedGrave;
    public TileBase horizontalPath;
    public TileBase horizontalPathWithVisitor;
}
