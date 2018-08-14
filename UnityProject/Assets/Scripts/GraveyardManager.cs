using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GraveyardManager : MonoBehaviour
{
    private Grid grid;
    private Tilemap tileMap;
    private Vector3Int lastMouseCellPosition;

    // Use this for initialization
    void Start()
    {
        grid = GetComponentInParent<Grid>();
        tileMap = GetComponent<Tilemap>();
    }


    private void OnMouseDown()
    {
        if (!GameManager.instance.IsGameOver && GameManager.instance.IsGameStarted)
        {
            Vector3Int cellPosition = GetCellUnderMouse();

            // place the grave if it's an empty place
            if (tileMap.GetTile(cellPosition) == GameManager.instance.emptySpot)
            {
                if (GameManager.instance.Bury(cellPosition))
                {
                    AudioSource.PlayClipAtPoint(GameManager.instance.burySound, Camera.main.transform.position);
                    tileMap.SetTile(cellPosition, GameManager.instance.wellMaintainGrave);
                }
            }
            // replace the old grave by a new one
            else if (tileMap.GetTile(cellPosition) == GameManager.instance.abandonedGrave ||
                     tileMap.GetTile(cellPosition) == GameManager.instance.correctGrave ||
                     tileMap.GetTile(cellPosition) == GameManager.instance.wellMaintainGrave)
            {
                if (GameManager.instance.Unbury(cellPosition))
                {
                    StartCoroutine(UnburyAnimation(cellPosition));
                }
            }
        }
    }

    private IEnumerator UnburyAnimation(Vector3Int cellPosition)
    {
        tileMap.SetTile(cellPosition, GameManager.instance.emptySpot);
        AudioSource.PlayClipAtPoint(GameManager.instance.unburySound, Camera.main.transform.position);
        yield return new WaitForSeconds(0.25f);
        AudioSource.PlayClipAtPoint(GameManager.instance.burySound, Camera.main.transform.position);
        tileMap.SetTile(cellPosition, GameManager.instance.wellMaintainGrave);
    }

    private void OnMouseOver()
    {
        if (lastMouseCellPosition != GetCellUnderMouse())
        {
            Vector3Int cellPosition = GetCellUnderMouse();

            if (GameManager.instance.IsSomeoneBuriedHere(cellPosition))
            {
                Vector3 worldCellPosition = Input.mousePosition;
                worldCellPosition.z = 0;
                worldCellPosition.y = worldCellPosition.y - worldCellPosition.y % 64;
                worldCellPosition.x = worldCellPosition.x - worldCellPosition.x % 64;
                GameManager.instance.DisplayGraveInfo(cellPosition, worldCellPosition);
            }
            else
            {
                GameManager.instance.HideGraveInfo();
            }
        }

        lastMouseCellPosition = GetCellUnderMouse();
            
    }

    private Vector3Int GetCellUnderMouse()
    {
        // get mouse click's position in 2d plane
        Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pz.z = 0;

        // convert mouse click's position to Grid position
        GridLayout gridLayout = transform.parent.GetComponentInParent<GridLayout>();
        return gridLayout.WorldToCell(pz);
    }
}
