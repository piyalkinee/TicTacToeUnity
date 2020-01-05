using UnityEngine;
using UnityEngine.EventSystems;

public class scr_SwipeControl : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    #region MainData
    public GameObject GameController;
    scr_GameController GC;
    #endregion

    #region SwipeLogic
    private void Start()
    {
        GC = GameController.GetComponent<scr_GameController>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if ((Mathf.Abs(eventData.delta.x)) > (Mathf.Abs(eventData.delta.y)))
        {
            if (eventData.delta.x > 0)
            {
                if (GC.PermissionToTurn)
                {
                    GC.RotateWorldTech("Right");
                    StartCoroutine(GC.RotateWorld("Right"));
                }              
            }
            else
            {
                if (GC.PermissionToTurn)
                {
                    GC.RotateWorldTech("Left");
                    StartCoroutine(GC.RotateWorld("Left"));
                }  
            }
            GC.PermissionToTurn = false;
        }
        else
        {
            if (eventData.delta.y > 0)
            {

            }
            else
            {

            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        //Просто нужен
    }
    #endregion
}
