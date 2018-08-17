using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GraveyardManager : MonoBehaviour
{
    private Tilemap tileMap;
    // On which cell was the mouse over last frame.
    private Vector3Int lastMouseCellPosition;
    /// <summary>
    /// Prevent to bury a grave between an "unburying".
    /// </summary>
    private bool onAnimPause = false;

    // Use this for initialization
    void Start()
    {
        tileMap = GetComponent<Tilemap>();
    }


    private void OnMouseDown()
    {
        if (!GameManager.instance.IsGameOver && GameManager.instance.IsGameStarted && !onAnimPause)
        {
            Vector3Int cellPosition = GetCellUnderMouse();

            // If there is already someone buried there
            if (GameManager.instance.IsSomeoneBuriedHere(cellPosition))
            {
                if (GameManager.instance.Unbury(cellPosition))
                {
                    StartCoroutine(UnburyAnimation(cellPosition));
                }
            }
            else if (tileMap.GetTile(cellPosition) == GameManager.instance.tilesData.emptySpot)
            {
                if (GameManager.instance.Bury(cellPosition))
                {
                    BuryAnimation(cellPosition);
                }
            }
        }
    }

    private void BuryAnimation(Vector3Int cellPosition)
    {
        AudioSource.PlayClipAtPoint(GameManager.instance.audio.burySound, Camera.main.transform.position);
        tileMap.SetTile(cellPosition, GameManager.instance.tilesData.wellMaintainGrave);
    }

    private IEnumerator UnburyAnimation(Vector3Int cellPosition)
    {
        tileMap.SetTile(cellPosition, GameManager.instance.tilesData.emptySpot);
        AudioSource.PlayClipAtPoint(GameManager.instance.audio.unburySound, Camera.main.transform.position);
        onAnimPause = true;
        yield return new WaitForSeconds(GameManager.instance.param.unburyAnimationPause);
        onAnimPause = false;
        BuryAnimation(cellPosition);
    }

    private void OnMouseOver()
    {
        if (lastMouseCellPosition != GetCellUnderMouse() && !onAnimPause)
        {
            Vector3Int cellPosition = GetCellUnderMouse();

            if (GameManager.instance.IsSomeoneBuriedHere(cellPosition))
            {
                // TODO: Afficher pile au milieu de la tombe
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
