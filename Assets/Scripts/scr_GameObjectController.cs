using UnityEngine;

public class scr_GameObjectController : MonoBehaviour
{
    #region ObjectControl
    public void Clear() //Удаляет объект
    {
        Destroy(gameObject);
    }
    #endregion
}