using UnityEngine;
public class scr_CellControl : MonoBehaviour
{
    #region MainData
    public int MyPossitonX;
    public int MyPossitonY;
    public GameObject Controller;
    scr_GameController GC;
    #endregion

    #region Logik
    void Start() //Запуск программы
    {
        GC = Controller.GetComponent<scr_GameController>(); //Поиск скрипта игрового контроллера
    }
    void OnMouseDown() //Нажатае на клетку
    {
        GC.PositionSelect(MyPossitonY, MyPossitonX);
    }
    #endregion
}
