using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class scr_GameController : MonoBehaviour
{
    #region MainData
    GameObject[,] CellObjects = new GameObject[4, 4]; //Игровые клетки
    GameObject[,] GameplayObjects = new GameObject[4, 4]; //Объекты поставленные на поле
    int[,] GCF = new int[4, 4]; //Игровое поле в виде чисел

    int GlobalScore = 0; //Максимальное число очков
    int CurrentScore = 0; //Текущее число очков
    int WinStreak = 0; //Череда побед
    int CurrentHealth = 1; //Количество попыток

    public string CurrentMap = "Castle";//Текущая карта 
    bool TrainingCompleted = false;//Пройдено ли обучение
    bool GameIsStarted = false; //Проверка начата ли игра
    bool KnightsTurn = true; //Проверка чей ход
    bool ThereAreStillMoves = false; //Отвечает за наличие оставшихся ходов

    [Header("Gameplay")]
    public GameObject FirstPlayerObject; //Объект первого игрока
    public GameObject SecondPlayerObject; //Объект вторго игрока
    public GameObject Stone; //Объект камня

    enum FO //FieldObjects
    {
        Empty, Knight, Orc, Stone
    }

    //Start program
    void Start()
    {
        for (int i = 0; i < 4; i++) //Авто заполнение таблицы (Строка/Y)
        {
            for (int j = 0; j < 4; j++) //Авто заполнение таблицы (Столбец/X)
            {
                GameObject currentCell = GameObject.Find("dot_" + j + i);
                CellObjects[i, j] = currentCell;
                currentCell.GetComponent<scr_CellControl>().MyPossitonX = j;
                currentCell.GetComponent<scr_CellControl>().MyPossitonY = i;
            }
        }
    }
    #endregion

    #region AIData
    int[,] KnightsCellCost = new int[4, 4]; //Стоймость клеток
    int[,] OrcsCellCost = new int[4, 4]; //Стоймость клеток

    bool AIisKnights = false; //Проверка за кого играет ИИ

    int WLPP1x; //Точка победы/поражения 1 (x)
    int WLPP2x; //Точка победы/поражения 2 (x)
    int WLPP3x; //Точка победы/поражения 3 (x)
    int WLPP1y; //Точка победы/поражения 1 (y)
    int WLPP2y; //Точка победы/поражения 2 (y)
    int WLPP3y; //Точка победы/поражения 3 (y)

    bool AIHaveAPlan = false; //Имеет ли ИИ план игры

    int PP1x; //Точка плана 1 (x)
    int PP2x; //Точка плана 2 (x)
    int PP3x; //Точка плана 3 (x)
    int PP1y; //Точка плана 1 (y)
    int PP2y; //Точка плана 2 (y)
    int PP3y; //Точка плана 3 (y)
    #endregion

    #region UIData
    float CurrentCameraPositionZ = 5;
    float CurrentWorldAngle = 22.5f;

    [Header("UI")]
    //All
    public GameObject MainCamera; //Объект камеры
    public GameObject WorldCircle; //Объект круга мира
    public TextMeshProUGUI GameScoreText; //Текст очков (текст)
    public TextMeshProUGUI GameScoreAmount; //Текст очков (число)
    public TextMeshProUGUI GameScoreAmountPlus; //Текст добавления очков (число)
    //StartScreen
    public GameObject StartScreen; //Окно при старте игры
    //MainMeny
    public GameObject MainMeny; //Окно главного меню игры
    public GameObject SwipeSupport; //Окно диалога конца игры
    public Button PlayInMainMeny; //Кнопка начала игры
    public TextMeshProUGUI CurrentMapText; //Текст текущей карты
    //InGame
    public GameObject MenyInGame; //Окно меню в игре
    //EndGame
    public GameObject EndPartDialoge; //Окно диалога конца раунда
    public GameObject EndGameDialoge; //Окно диалога конца игры
    public TextMeshProUGUI EndPartStatys; //Текст в диалоге конца раунда
    public TextMeshProUGUI EndGameStatys; //Текст в диалоге конца игры

    public bool PermissionToTurn = true; //Разрешение на разворот камеры 
    #endregion

    #region Gameplay
    public void StartGame() //Старт партии
    {
        GameScoreText.text = "Score";
        ChoiceSide();
        GameIsStarted = true;
        for (int i = 0; i < 4; i++)
        {
            StoneGenerator();
        }

        if (AIisKnights && KnightsTurn)
            AIAnalysis();
        else if (!AIisKnights && !KnightsTurn)
            AIAnalysis();
    }
    void ChoiceSide() //Выбор стороны
    {
        switch (CurrentMap)
        {
            case "Castle":
                AIisKnights = false;
                break;
            case "Village":
                AIisKnights = false;
                break;
            case "Canyon":
                AIisKnights = true;
                break;
            case "Cave":
                AIisKnights = true;
                break;
        }
    }
    void StoneGenerator() //Генератор препятствий 
    {
        int randomNum1 = Random.Range(0, 4);
        int randomNum2 = Random.Range(0, 4);
        if (GCF[randomNum1, randomNum2] == 0)
        {
            GameObject CurrentStone = Instantiate(Stone, CellObjects[randomNum1, randomNum2].transform.position, Quaternion.identity);
            GameplayObjects[randomNum1, randomNum2] = CurrentStone;
            GCF[randomNum1, randomNum2] = 3;
        }
        else
        {
            StoneGenerator();
        }
    }
    public void PositionSelect(int selectY, int selectX) //Запрос разрешения для создание фишки
    {
        if (GameIsStarted)
            if (GCF[selectY, selectX] == 0)
            {
                if (KnightsTurn)
                {
                    GameObject currentFP = Instantiate(FirstPlayerObject, CellObjects[selectY, selectX].transform.position, Quaternion.identity);
                    GameplayObjects[selectY, selectX] = currentFP;
                    GCF[selectY, selectX] = 1;

                }
                else
                {
                    GameObject currentSP = Instantiate(SecondPlayerObject, CellObjects[selectY, selectX].transform.position, Quaternion.identity);
                    GameplayObjects[selectY, selectX] = currentSP;
                    GCF[selectY, selectX] = 2;
                }

                if (KnightsTurn && !AIisKnights || !KnightsTurn && AIisKnights)
                    AddScore(10);

                FieldAnalysis("CheckKnightsWin");
                FieldAnalysis("CheckOrcsWin");
                FieldAnalysis("CheckThereAreStillMoves");

                SideChange();
            }
    }
    void FieldAnalysis(string task) //Анализ поля
    {
        ThereAreStillMoves = false;

        //Horizontal
        LineAnalysis(task, "H", 0, 0);
        LineAnalysis(task, "H", 0, 1);
        LineAnalysis(task, "H", 1, 0);
        LineAnalysis(task, "H", 1, 1);
        LineAnalysis(task, "H", 2, 0);
        LineAnalysis(task, "H", 2, 1);
        LineAnalysis(task, "H", 3, 0);
        LineAnalysis(task, "H", 3, 1);
        //Vertical
        LineAnalysis(task, "V", 0, 0);
        LineAnalysis(task, "V", 1, 0);
        LineAnalysis(task, "V", 0, 1);
        LineAnalysis(task, "V", 1, 1);
        LineAnalysis(task, "V", 0, 2);
        LineAnalysis(task, "V", 1, 2);
        LineAnalysis(task, "V", 0, 3);
        LineAnalysis(task, "V", 1, 3);
        //UpDown
        LineAnalysis(task, "UD", 0, 0);
        LineAnalysis(task, "UD", 1, 1);
        LineAnalysis(task, "UD", 0, 1);
        LineAnalysis(task, "UD", 1, 0);
        //DownUp
        LineAnalysis(task, "DU", 0, 3);
        LineAnalysis(task, "DU", 1, 2);
        LineAnalysis(task, "DU", 0, 2);
        LineAnalysis(task, "DU", 1, 3);

        if (task == "CheckThereAreStillMoves" && !ThereAreStillMoves)
        {
            EndPart("Tie");
        }
    }
    void LineAnalysis(string task, string angle, int y, int x) //Анализ линии
    {
        switch (task)
        {
            case "CheckKnightsWin": //Проверка на победили ли рыцари
                switch (angle)
                {
                    case "H":
                        if (GCF[x, y] == (int)FO.Knight && GCF[x + 1, y] == (int)FO.Knight && GCF[x + 2, y] == (int)FO.Knight)
                        {
                            EndPart("Knights");
                        }
                        break;
                    case "V":
                        if (GCF[x, y] == (int)FO.Knight && GCF[x, y + 1] == (int)FO.Knight && GCF[x, y + 2] == (int)FO.Knight)
                        {
                            EndPart("Knights");
                        }
                        break;
                    case "UD":
                        if (GCF[x, y] == (int)FO.Knight && GCF[x + 1, y + 1] == (int)FO.Knight && GCF[x + 2, y + 2] == (int)FO.Knight)
                        {
                            EndPart("Knights");
                        }
                        break;
                    case "DU":
                        if (GCF[x, y] == (int)FO.Knight && GCF[x - 1, y + 1] == (int)FO.Knight && GCF[x - 2, y + 2] == (int)FO.Knight)
                        {
                            EndPart("Knights");
                        }
                        break;
                }
                break;
            case "CheckOrcsWin": //Проверка на победили ли орки
                switch (angle)
                {
                    case "H":
                        if (GCF[x, y] == (int)FO.Orc && GCF[x + 1, y] == (int)FO.Orc && GCF[x + 2, y] == (int)FO.Orc)
                        {
                            EndPart("Orcs");
                        }
                        break;
                    case "V":
                        if (GCF[x, y] == (int)FO.Orc && GCF[x, y + 1] == (int)FO.Orc && GCF[x, y + 2] == (int)FO.Orc)
                        {
                            EndPart("Orcs");
                        }
                        break;
                    case "UD":
                        if (GCF[x, y] == (int)FO.Orc && GCF[x + 1, y + 1] == (int)FO.Orc && GCF[x + 2, y + 2] == (int)FO.Orc)
                        {
                            EndPart("Orcs");
                        }
                        break;
                    case "DU":
                        if (GCF[x, y] == (int)FO.Orc && GCF[x - 1, y + 1] == (int)FO.Orc && GCF[x - 2, y + 2] == (int)FO.Orc)
                        {
                            EndPart("Orcs");
                        }
                        break;
                }
                break;
            case "CheckThereAreStillMoves": //Проверка остались ли ходы в игре
                switch (angle)
                {
                    case "H":
                        if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x + 1, y] == (int)FO.Empty || GCF[x + 1, y] == (int)FO.Knight) && (GCF[x + 2, y] == (int)FO.Empty || GCF[x + 2, y] == (int)FO.Knight))
                        {
                            ThereAreStillMoves = true;
                        }
                        else if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x + 1, y] == (int)FO.Empty || GCF[x + 1, y] == (int)FO.Orc) && (GCF[x + 2, y] == (int)FO.Empty || GCF[x + 2, y] == (int)FO.Orc))
                        {
                            ThereAreStillMoves = true;
                        }
                        break;
                    case "V":
                        if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x, y + 1] == (int)FO.Empty || GCF[x, y + 1] == (int)FO.Knight) && (GCF[x, y + 2] == (int)FO.Empty || GCF[x, y + 2] == (int)FO.Knight))
                        {
                            ThereAreStillMoves = true;
                        }
                        else if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x, y + 1] == (int)FO.Empty || GCF[x, y + 1] == (int)FO.Orc) && (GCF[x, y + 2] == (int)FO.Empty || GCF[x, y + 2] == (int)FO.Orc))
                        {
                            ThereAreStillMoves = true;
                        }
                        break;
                    case "UD":
                        if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x + 1, y + 1] == (int)FO.Empty || GCF[x + 1, y + 1] == (int)FO.Knight) && (GCF[x + 2, y + 2] == (int)FO.Empty || GCF[x + 2, y + 2] == (int)FO.Knight))
                        {
                            ThereAreStillMoves = true;
                        }
                        else if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x + 1, y + 1] == (int)FO.Empty || GCF[x + 1, y + 1] == (int)FO.Orc) && (GCF[x + 2, y + 2] == (int)FO.Empty || GCF[x + 2, y + 2] == (int)FO.Orc))
                        {
                            ThereAreStillMoves = true;
                        }
                        break;
                    case "DU":
                        if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x - 1, y + 1] == (int)FO.Empty || GCF[x - 1, y + 1] == (int)FO.Knight) && (GCF[x - 2, y + 2] == (int)FO.Empty || GCF[x - 2, y + 2] == (int)FO.Knight))
                        {
                            ThereAreStillMoves = true;
                        }
                        else if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x - 1, y + 1] == (int)FO.Empty || GCF[x - 1, y + 1] == (int)FO.Orc) && (GCF[x - 2, y + 2] == (int)FO.Empty || GCF[x - 2, y + 2] == (int)FO.Orc))
                        {
                            ThereAreStillMoves = true;
                        }
                        break;
                }
                break;
        }
    }
    void SideChange() //Смена сторон
    {
        if (KnightsTurn)
        {
            KnightsTurn = false;
            if (!AIisKnights)
                AIAnalysis();
        }
        else
        {
            KnightsTurn = true;
            if (AIisKnights)
                AIAnalysis();
        }
    }
    #endregion

    #region AI
    void AIAnalysis() //Древо аналитики ИИ
    {
        if (AIFieldAnalysis(AIinfo(), 2)) //Проверка на возможность победить
        {
            AIMove(WLPP1x, WLPP1y, WLPP2x, WLPP2y, WLPP3x, WLPP3y);
        }
        else
        {
            if (AIFieldAnalysis(!AIinfo(), 2)) //Проверка на возможность проиграть (Высокая опасность)
            {
                AIMove(WLPP1x, WLPP1y, WLPP2x, WLPP2y, WLPP3x, WLPP3y);
            }
            else
            {
                if (AIFieldAnalysis(!AIinfo(), 1)) //Проверка на возможность проиграть (Низкая опасность)
                {
                    AIMove(WLPP1x, WLPP1y, WLPP2x, WLPP2y, WLPP3x, WLPP3y);
                }
                else //Постройка плана победы
                {
                    if (AIHaveAPlan) //Проверка на наличие плана
                    {
                        if (PlanCheck()) //Реален ли план сейчас
                        {
                            AIMove(WLPP1x, WLPP1y, WLPP2x, WLPP2y, WLPP3x, WLPP3y);
                        }
                        else //Если нет то создаем новый
                        {
                            CreateAIPlan();
                        }
                    }
                    else //Если нет то создаем новый
                    {
                        CreateAIPlan();
                    }
                }
            }
        }
    }
    bool AIinfo() //ИИ узнает какую сторону ему брать при проверке на победу
    {
        if (AIisKnights)
            return true;
        else
            return false;
    }
    bool PlanCheck() //Проверка выполнимости плана
    {
        if (AIisKnights)
        {
            if ((GCF[PP1y, PP1x] == (int)FO.Empty || GCF[PP1y, PP1x] == (int)FO.Knight) && (GCF[PP2y, PP2x] == (int)FO.Empty || GCF[PP2y, PP2x] == (int)FO.Knight) && (GCF[PP3y, PP3x] == (int)FO.Empty || GCF[PP3y, PP3x] == (int)FO.Knight))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if ((GCF[PP1y, PP1x] == (int)FO.Empty || GCF[PP1y, PP1x] == (int)FO.Orc) && (GCF[PP2y, PP2x] == (int)FO.Empty || GCF[PP2y, PP2x] == (int)FO.Orc) && (GCF[PP3y, PP3x] == (int)FO.Empty || GCF[PP3y, PP3x] == (int)FO.Orc))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    void CreateAIPlan() //Создание плана для ИИ
    {
        switch (SelectBestPosition())
        {
            case "00": //00
                ChoiceDirection("H", 0, 0);
                ChoiceDirection("V", 0, 0);
                ChoiceDirection("UD", 0, 0);
                break;
            case "01": //01
                ChoiceDirection("H", 0, 1);
                ChoiceDirection("V", 0, 0);
                ChoiceDirection("V", 0, 1);
                ChoiceDirection("UD", 0, 1);
                break;
            case "02": //02
                ChoiceDirection("H", 0, 2);
                ChoiceDirection("V", 0, 0);
                ChoiceDirection("V", 0, 1);
                ChoiceDirection("DU", 2, 0);
                break;
            case "03": //03
                ChoiceDirection("H", 0, 3);
                ChoiceDirection("V", 0, 1);
                ChoiceDirection("DU", 2, 1);
                break;
            case "10": //10
                ChoiceDirection("H", 0, 0);
                ChoiceDirection("H", 1, 0);
                ChoiceDirection("V", 1, 0);
                ChoiceDirection("UD", 1, 0);
                break;
            case "11": //11
                ChoiceDirection("H", 0, 1);
                ChoiceDirection("H", 1, 1);
                ChoiceDirection("V", 0, 1);
                ChoiceDirection("V", 1, 1);
                ChoiceDirection("UD", 0, 0);
                ChoiceDirection("UD", 1, 1);
                ChoiceDirection("DU", 2, 0);
                break;
            case "12": //12
                ChoiceDirection("H", 0, 2);
                ChoiceDirection("H", 1, 2);
                ChoiceDirection("V", 0, 1);
                ChoiceDirection("V", 1, 1);
                ChoiceDirection("DU", 3, 0);
                ChoiceDirection("DU", 2, 1);
                ChoiceDirection("UD", 0, 1);
                break;
            case "13": //13
                ChoiceDirection("H", 0, 3);
                ChoiceDirection("H", 1, 3);
                ChoiceDirection("V", 1, 1);
                ChoiceDirection("DU", 3, 1);
                break;
            case "20": //20
                ChoiceDirection("H", 0, 0);
                ChoiceDirection("H", 1, 0);
                ChoiceDirection("V", 2, 0);
                ChoiceDirection("DU", 2, 0);
                break;
            case "21": //21
                ChoiceDirection("H", 0, 1);
                ChoiceDirection("H", 1, 1);
                ChoiceDirection("V", 2, 0);
                ChoiceDirection("V", 2, 1);
                ChoiceDirection("DU", 3, 0);
                ChoiceDirection("DU", 2, 1);
                ChoiceDirection("UD", 1, 0);
                break;
            case "22": //22
                ChoiceDirection("H", 0, 2);
                ChoiceDirection("H", 1, 2);
                ChoiceDirection("V", 2, 0);
                ChoiceDirection("V", 2, 1);
                ChoiceDirection("UD", 0, 0);
                ChoiceDirection("UD", 1, 1);
                ChoiceDirection("DU", 3, 1);
                break;
            case "23": //23
                ChoiceDirection("H", 0, 3);
                ChoiceDirection("H", 1, 3);
                ChoiceDirection("V", 2, 1);
                ChoiceDirection("UD", 0, 1);
                break;
            case "30": //30
                ChoiceDirection("H", 1, 0);
                ChoiceDirection("V", 3, 0);
                ChoiceDirection("DU", 3, 0);
                break;
            case "31": //31
                ChoiceDirection("H", 1, 1);
                ChoiceDirection("V", 3, 0);
                ChoiceDirection("V", 3, 1);
                ChoiceDirection("DU", 3, 1);
                break;
            case "32": //32
                ChoiceDirection("H", 1, 2);
                ChoiceDirection("V", 3, 0);
                ChoiceDirection("V", 3, 1);
                ChoiceDirection("UD", 1, 0);
                break;
            case "33": //33
                ChoiceDirection("H", 1, 3);
                ChoiceDirection("V", 3, 1);
                ChoiceDirection("UD", 1, 1);
                break;
        }
        AIHaveAPlan = true;
        AIMove(PP1x, PP1y, PP2x, PP2y, PP3x, PP3y);
    }
    string SelectBestPosition() //Выбор лучшей позиции на поле
    {
        int bestPosition = 0; //Лучшая позиция для плана
        int bestX = 0;
        int bestY = 0;

        CellCostAnalysis(); //Запускаем обновление стоймостей

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (AIisKnights)
                {
                    if (KnightsCellCost[i, j] > bestPosition) //Если найдена точка получше то записываем ее
                    {
                        bestPosition = KnightsCellCost[i, j];
                        bestX = j;
                        bestY = i;
                    }
                }
                else
                {
                    if (OrcsCellCost[i, j] > bestPosition) //Если найдена точка получше то записываем ее
                    {
                        bestPosition = OrcsCellCost[i, j];
                        bestX = j;
                        bestY = i;
                    }
                }
            }
        }

        if (bestX == 0 && bestY == 0)
        {
            return "00";
        }
        else if (bestX == 0 && bestY == 1)
        {
            return "01";
        }
        else if (bestX == 0 && bestY == 2)
        {
            return "02";
        }
        else if (bestX == 0 && bestY == 3)
        {
            return "03";
        }
        if (bestX == 1 && bestY == 0)
        {
            return "10";
        }
        else if (bestX == 1 && bestY == 1)
        {
            return "11";
        }
        else if (bestX == 1 && bestY == 2)
        {
            return "12";
        }
        else if (bestX == 1 && bestY == 3)
        {
            return "13";
        }
        if (bestX == 2 && bestY == 0)
        {
            return "20";
        }
        else if (bestX == 2 && bestY == 1)
        {
            return "21";
        }
        else if (bestX == 2 && bestY == 2)
        {
            return "22";
        }
        else if (bestX == 2 && bestY == 3)
        {
            return "23";
        }
        if (bestX == 3 && bestY == 0)
        {
            return "30";
        }
        else if (bestX == 3 && bestY == 1)
        {
            return "31";
        }
        else if (bestX == 3 && bestY == 2)
        {
            return "32";
        }
        else if (bestX == 3 && bestY == 3)
        {
            return "33";
        }
        else
        {
            return "";
        }
    }
    void ChoiceDirection(string angle, int x, int y) //Проверка лини направления для плана (Завершение создания плана)
    {
        switch (angle)
        {
            case "H":
                if (AIisKnights)
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x + 1, y] == (int)FO.Empty || GCF[x + 1, y] == (int)FO.Knight) && (GCF[x + 2, y] == (int)FO.Empty || GCF[x + 2, y] == (int)FO.Knight))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y;
                        PP2x = x + 1;
                        PP3y = y;
                        PP3x = x + 2;
                    }
                }
                else
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x + 1, y] == (int)FO.Empty || GCF[x + 1, y] == (int)FO.Orc) && (GCF[x + 2, y] == (int)FO.Empty || GCF[x + 2, y] == (int)FO.Orc))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y;
                        PP2x = x + 1;
                        PP3y = y;
                        PP3x = x + 2;
                    }
                }
                break;
            case "V":
                if (AIisKnights)
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x, y + 1] == (int)FO.Empty || GCF[x, y + 1] == (int)FO.Knight) && (GCF[x, y + 2] == (int)FO.Empty || GCF[x, y + 2] == (int)FO.Knight))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x;
                        PP3y = y + 2;
                        PP3x = x;
                    }
                }
                else
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x, y + 1] == (int)FO.Empty || GCF[x, y + 1] == (int)FO.Orc) && (GCF[x, y + 2] == (int)FO.Empty || GCF[x, y + 2] == (int)FO.Orc))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x;
                        PP3y = y + 2;
                        PP3x = x;
                    }
                }
                break;
            case "UD":
                if (AIisKnights)
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x + 1, y + 1] == (int)FO.Empty || GCF[x + 1, y + 1] == (int)FO.Knight) && (GCF[x + 2, y + 2] == (int)FO.Empty || GCF[x + 2, y + 2] == (int)FO.Knight))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x + 1;
                        PP3y = y + 2;
                        PP3x = x + 2;
                    }
                }
                else
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x + 1, y + 1] == (int)FO.Empty || GCF[x + 1, y + 1] == (int)FO.Orc) && (GCF[x + 2, y + 2] == (int)FO.Empty || GCF[x + 2, y + 2] == (int)FO.Orc))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x + 1;
                        PP3y = y + 2;
                        PP3x = x + 2;
                    }
                }
                break;
            case "DU":
                if (AIisKnights)
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Knight) && (GCF[x - 1, y + 1] == (int)FO.Empty || GCF[x - 1, y + 1] == (int)FO.Knight) && (GCF[x - 2, y + 2] == (int)FO.Empty || GCF[x - 2, y + 2] == (int)FO.Knight))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x - 1;
                        PP3y = y + 2;
                        PP3x = x - 2;
                    }
                }
                else
                {
                    if ((GCF[x, y] == (int)FO.Empty || GCF[x, y] == (int)FO.Orc) && (GCF[x - 1, y + 1] == (int)FO.Empty || GCF[x - 1, y + 1] == (int)FO.Orc) && (GCF[x - 2, y + 2] == (int)FO.Empty || GCF[x - 2, y + 2] == (int)FO.Orc))
                    {
                        PP1y = y;
                        PP1x = x;
                        PP2y = y + 1;
                        PP2x = x - 1;
                        PP3y = y + 2;
                        PP3x = x - 2;
                    }
                }
                break;
        }

    }
    void CellCostAnalysis() //Оценка стоймости клеток
    {
        KnightsCellCost[0, 0] = KCCALine(0, 0, "V") + KCCALine(0, 0, "H") + KCCALine(0, 0, "UD");
        KnightsCellCost[1, 0] = KCCALine(0, 0, "V") + KCCALine(1, 0, "V") + KCCALine(1, 0, "H") + KCCALine(1, 0, "UD");
        KnightsCellCost[2, 0] = KCCALine(0, 0, "V") + KCCALine(1, 0, "V") + KCCALine(2, 0, "H") + KCCALine(0, 2, "DU");
        KnightsCellCost[3, 0] = KCCALine(1, 0, "V") + KCCALine(3, 0, "H") + KCCALine(1, 2, "DU");
        KnightsCellCost[0, 1] = KCCALine(0, 1, "V") + KCCALine(0, 0, "H") + KCCALine(0, 1, "H") + KCCALine(0, 1, "UD");
        KnightsCellCost[1, 1] = KCCALine(0, 1, "V") + KCCALine(1, 1, "V") + KCCALine(1, 0, "H") + KCCALine(1, 1, "H") + KCCALine(0, 0, "UD") + KCCALine(1, 1, "UD") + KCCALine(0, 2, "DU");
        KnightsCellCost[2, 1] = KCCALine(0, 1, "V") + KCCALine(1, 1, "V") + KCCALine(2, 0, "H") + KCCALine(2, 1, "H") + KCCALine(0, 3, "DU") + KCCALine(1, 2, "DU") + KCCALine(1, 0, "UD");
        KnightsCellCost[3, 1] = KCCALine(1, 1, "V") + KCCALine(3, 0, "H") + KCCALine(3, 1, "H") + KCCALine(1, 3, "DU");
        KnightsCellCost[0, 2] = KCCALine(0, 2, "V") + KCCALine(0, 0, "H") + KCCALine(0, 1, "H") + KCCALine(0, 2, "DU");
        KnightsCellCost[1, 2] = KCCALine(0, 2, "V") + KCCALine(1, 2, "V") + KCCALine(1, 0, "H") + KCCALine(1, 1, "H") + KCCALine(0, 3, "DU") + KCCALine(1, 2, "DU") + KCCALine(0, 1, "UD");
        KnightsCellCost[2, 2] = KCCALine(0, 2, "V") + KCCALine(1, 2, "V") + KCCALine(2, 0, "H") + KCCALine(2, 1, "H") + KCCALine(0, 0, "UD") + KCCALine(1, 1, "UD") + KCCALine(1, 3, "DU");
        KnightsCellCost[3, 2] = KCCALine(1, 2, "V") + KCCALine(3, 0, "H") + KCCALine(3, 1, "H") + KCCALine(1, 0, "UD");
        KnightsCellCost[0, 3] = KCCALine(0, 3, "V") + KCCALine(0, 1, "H") + KCCALine(0, 3, "DU");
        KnightsCellCost[1, 3] = KCCALine(0, 3, "V") + KCCALine(1, 3, "V") + KCCALine(1, 1, "H") + KCCALine(1, 3, "DU");
        KnightsCellCost[2, 3] = KCCALine(0, 3, "V") + KCCALine(1, 3, "V") + KCCALine(2, 1, "H") + KCCALine(0, 1, "UD");
        KnightsCellCost[3, 3] = KCCALine(1, 3, "V") + KCCALine(3, 1, "H") + KCCALine(1, 1, "UD");

        OrcsCellCost[0, 0] = OCCALine(0, 0, "V") + OCCALine(0, 0, "H") + OCCALine(0, 0, "UD");
        OrcsCellCost[1, 0] = OCCALine(0, 0, "V") + OCCALine(1, 0, "V") + OCCALine(1, 0, "H") + OCCALine(1, 0, "UD");
        OrcsCellCost[2, 0] = OCCALine(0, 0, "V") + OCCALine(1, 0, "V") + OCCALine(2, 0, "H") + OCCALine(0, 2, "DU");
        OrcsCellCost[3, 0] = OCCALine(1, 0, "V") + OCCALine(3, 0, "H") + OCCALine(1, 2, "DU");
        OrcsCellCost[0, 1] = OCCALine(0, 1, "V") + OCCALine(0, 0, "H") + OCCALine(0, 1, "H") + OCCALine(0, 1, "UD");
        OrcsCellCost[1, 1] = OCCALine(0, 1, "V") + OCCALine(1, 1, "V") + OCCALine(1, 0, "H") + OCCALine(1, 1, "H") + OCCALine(0, 0, "UD") + OCCALine(1, 1, "UD") + OCCALine(0, 2, "DU");
        OrcsCellCost[2, 1] = OCCALine(0, 1, "V") + OCCALine(1, 1, "V") + OCCALine(2, 0, "H") + OCCALine(2, 1, "H") + OCCALine(0, 3, "DU") + OCCALine(1, 2, "DU") + OCCALine(1, 0, "UD");
        OrcsCellCost[3, 1] = OCCALine(1, 1, "V") + OCCALine(3, 0, "H") + OCCALine(3, 1, "H") + OCCALine(1, 3, "DU");
        OrcsCellCost[0, 2] = OCCALine(0, 2, "V") + OCCALine(0, 0, "H") + OCCALine(0, 1, "H") + OCCALine(0, 2, "DU");
        OrcsCellCost[1, 2] = OCCALine(0, 2, "V") + OCCALine(1, 2, "V") + OCCALine(1, 0, "H") + OCCALine(1, 1, "H") + OCCALine(0, 3, "DU") + OCCALine(1, 2, "DU") + OCCALine(0, 1, "UD");
        OrcsCellCost[2, 2] = OCCALine(0, 2, "V") + OCCALine(1, 2, "V") + OCCALine(2, 0, "H") + OCCALine(2, 1, "H") + OCCALine(0, 0, "UD") + OCCALine(1, 1, "UD") + OCCALine(1, 3, "DU");
        OrcsCellCost[3, 2] = OCCALine(1, 2, "V") + OCCALine(3, 0, "H") + OCCALine(3, 1, "H") + OCCALine(1, 0, "UD");
        OrcsCellCost[0, 3] = OCCALine(0, 3, "V") + OCCALine(0, 1, "H") + OCCALine(0, 3, "DU");
        OrcsCellCost[1, 3] = OCCALine(0, 3, "V") + OCCALine(1, 3, "V") + OCCALine(1, 1, "H") + OCCALine(1, 3, "DU");
        OrcsCellCost[2, 3] = OCCALine(0, 3, "V") + OCCALine(1, 3, "V") + OCCALine(2, 1, "H") + OCCALine(0, 1, "UD");
        OrcsCellCost[3, 3] = OCCALine(1, 3, "V") + OCCALine(3, 1, "H") + OCCALine(1, 1, "UD");
    }
    int KCCALine(int positionY, int positionX, string direct) // KnightsCellCostAnalysis проверка линии
    {
        switch (direct)
        {
            case "H":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Knight) && (GCF[positionY, positionX + 1] == (int)FO.Empty || GCF[positionY, positionX + 1] == (int)FO.Knight) && (GCF[positionY, positionX + 2] == (int)FO.Empty || GCF[positionY, positionX + 2] == (int)FO.Knight))
                    return 1;
                else
                    return 0;
            case "V":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Knight) && (GCF[positionY + 1, positionX] == (int)FO.Empty || GCF[positionY + 1, positionX] == (int)FO.Knight) && (GCF[positionY + 2, positionX] == (int)FO.Empty || GCF[positionY + 2, positionX] == (int)FO.Knight))
                    return 1;
                else
                    return 0;
            case "UD":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Knight) && (GCF[positionY + 1, positionX + 1] == (int)FO.Empty || GCF[positionY + 1, positionX + 1] == (int)FO.Knight) && (GCF[positionY + 2, positionX + 2] == (int)FO.Empty || GCF[positionY + 2, positionX + 2] == (int)FO.Knight))
                    return 1;
                else
                    return 0;
            case "DU":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Knight) && (GCF[positionY + 1, positionX - 1] == (int)FO.Empty || GCF[positionY + 1, positionX - 1] == (int)FO.Knight) && (GCF[positionY + 2, positionX - 2] == (int)FO.Empty || GCF[positionY + 2, positionX - 2] == (int)FO.Knight))
                    return 1;
                else
                    return 0;
            default: return 0;
        }
    }
    int OCCALine(int positionY, int positionX, string direct) // OrcsCellCostAnalysis проверка линии
    {
        switch (direct)
        {
            case "H":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Orc) && (GCF[positionY, positionX + 1] == (int)FO.Empty || GCF[positionY, positionX + 1] == (int)FO.Orc) && (GCF[positionY, positionX + 2] == (int)FO.Empty || GCF[positionY, positionX + 2] == (int)FO.Orc))
                    return 1;
                else
                    return 0;
            case "V":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Orc) && (GCF[positionY + 1, positionX] == (int)FO.Empty || GCF[positionY + 1, positionX] == (int)FO.Orc) && (GCF[positionY + 2, positionX] == (int)FO.Empty || GCF[positionY + 2, positionX] == (int)FO.Orc))
                    return 1;
                else
                    return 0;
            case "UD":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Orc) && (GCF[positionY + 1, positionX + 1] == (int)FO.Empty || GCF[positionY + 1, positionX + 1] == (int)FO.Orc) && (GCF[positionY + 2, positionX + 2] == (int)FO.Empty || GCF[positionY + 2, positionX + 2] == (int)FO.Orc))
                    return 1;
                else
                    return 0;
            case "DU":
                if ((GCF[positionY, positionX] == (int)FO.Empty || GCF[positionY, positionX] == (int)FO.Orc) && (GCF[positionY + 1, positionX - 1] == (int)FO.Empty || GCF[positionY + 1, positionX - 1] == (int)FO.Orc) && (GCF[positionY + 2, positionX - 2] == (int)FO.Empty || GCF[positionY + 2, positionX - 2] == (int)FO.Orc))
                    return 1;
                else
                    return 0;
            default: return 0;
        }
    }
    bool AIFieldAnalysis(bool checkPeoples, int analysisLevel) //Анализ положения на игровом поле
    {
        //Horizontal (-)
        if (AILineAnalysis(checkPeoples, 0, 0, 1, 0, 2, 0) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 0; WLPP2x = 1; WLPP2y = 0; WLPP3x = 2; WLPP3y = 0; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 0, 2, 0, 3, 0) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 0; WLPP2x = 2; WLPP2y = 0; WLPP3x = 3; WLPP3y = 0; return true;
        }
        else if (AILineAnalysis(checkPeoples, 0, 1, 1, 1, 2, 1) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 1; WLPP2x = 1; WLPP2y = 1; WLPP3x = 2; WLPP3y = 1; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 1, 2, 1, 3, 1) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 1; WLPP2x = 2; WLPP2y = 1; WLPP3x = 3; WLPP3y = 1; return true;
        }
        else if (AILineAnalysis(checkPeoples, 0, 2, 1, 2, 2, 2) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 2; WLPP2x = 1; WLPP2y = 2; WLPP3x = 2; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 2, 2, 2, 3, 2) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 2; WLPP2x = 2; WLPP2y = 2; WLPP3x = 3; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 0, 3, 1, 3, 2, 3) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 3; WLPP2x = 1; WLPP2y = 3; WLPP3x = 2; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 3, 2, 3, 3, 3) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 3; WLPP2x = 2; WLPP2y = 3; WLPP3x = 3; WLPP3y = 3; return true;
        }
        //Vertical (|)
        else if (AILineAnalysis(checkPeoples, 0, 0, 0, 1, 0, 2) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 0; WLPP2x = 0; WLPP2y = 1; WLPP3x = 0; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 0, 1, 0, 2, 0, 3) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 1; WLPP2x = 0; WLPP2y = 2; WLPP3x = 0; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 0, 1, 1, 1, 2) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 0; WLPP2x = 1; WLPP2y = 1; WLPP3x = 1; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 1, 1, 2, 1, 3) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 1; WLPP2x = 1; WLPP2y = 2; WLPP3x = 1; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 2, 0, 2, 1, 2, 2) == analysisLevel)
        {
            WLPP1x = 2; WLPP1y = 0; WLPP2x = 2; WLPP2y = 1; WLPP3x = 2; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 2, 1, 2, 2, 2, 3) == analysisLevel)
        {
            WLPP1x = 2; WLPP1y = 1; WLPP2x = 2; WLPP2y = 2; WLPP3x = 2; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 3, 0, 3, 1, 3, 2) == analysisLevel)
        {
            WLPP1x = 3; WLPP1y = 0; WLPP2x = 3; WLPP2y = 1; WLPP3x = 3; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 3, 1, 3, 2, 3, 3) == analysisLevel)
        {
            WLPP1x = 3; WLPP1y = 1; WLPP2x = 3; WLPP2y = 2; WLPP3x = 3; WLPP3y = 3; return true;
        }
        //UpDawn (\)
        else if (AILineAnalysis(checkPeoples, 0, 0, 1, 1, 2, 2) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 0; WLPP2x = 1; WLPP2y = 1; WLPP3x = 2; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 1, 2, 2, 3, 3) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 1; WLPP2x = 2; WLPP2y = 2; WLPP3x = 3; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 1, 0, 2, 1, 3, 2) == analysisLevel)
        {
            WLPP1x = 1; WLPP1y = 0; WLPP2x = 2; WLPP2y = 1; WLPP3x = 3; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 0, 1, 1, 2, 2, 3) == analysisLevel)
        {
            WLPP1x = 0; WLPP1y = 1; WLPP2x = 1; WLPP2y = 2; WLPP3x = 2; WLPP3y = 3; return true;
        }
        //DownUp (/)
        else if (AILineAnalysis(checkPeoples, 3, 0, 2, 1, 1, 2) == analysisLevel)
        {
            WLPP1x = 3; WLPP1y = 0; WLPP2x = 2; WLPP2y = 1; WLPP3x = 1; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 2, 1, 1, 2, 0, 3) == analysisLevel)
        {
            WLPP1x = 2; WLPP1y = 1; WLPP2x = 1; WLPP2y = 2; WLPP3x = 0; WLPP3y = 3; return true;
        }
        else if (AILineAnalysis(checkPeoples, 2, 0, 1, 1, 0, 2) == analysisLevel)
        {
            WLPP1x = 2; WLPP1y = 0; WLPP2x = 1; WLPP2y = 1; WLPP3x = 0; WLPP3y = 2; return true;
        }
        else if (AILineAnalysis(checkPeoples, 3, 1, 2, 2, 1, 3) == analysisLevel)
        {
            WLPP1x = 3; WLPP1y = 1; WLPP2x = 2; WLPP2y = 2; WLPP3x = 1; WLPP3y = 3; return true;
        }
        else
            return false;
    }
    int AILineAnalysis(bool checkPeoples, int x1, int y1, int x2, int y2, int x3, int y3) //Точка проверки 
    {
        int counter = 0;

        if (checkPeoples)
        {
            if (GCF[y1, x1] != (int)FO.Orc && GCF[y1, x1] != (int)FO.Stone && GCF[y2, x2] != (int)FO.Orc && GCF[y2, x2] != (int)FO.Stone && GCF[y3, x3] != (int)FO.Orc && GCF[y3, x3] != (int)FO.Stone) //Проверка на свободность клеток (можно ли собрать ряд)
            {
                if (GCF[y1, x1] == 1)
                    counter++;
                if (GCF[y2, x2] == 1)
                    counter++;
                if (GCF[y3, x3] == 1)
                    counter++;
            }
        }
        else
        {
            if (GCF[y1, x1] != (int)FO.Knight && GCF[y1, x1] != (int)FO.Stone && GCF[y2, x2] != (int)FO.Knight && GCF[y2, x2] != (int)FO.Stone && GCF[y3, x3] != (int)FO.Knight && GCF[y3, x3] != (int)FO.Stone) //Проверка на свободность клеток (можно ли собрать ряд)
            {
                if (GCF[y1, x1] == 2)
                    counter++;
                if (GCF[y2, x2] == 2)
                    counter++;
                if (GCF[y3, x3] == 2)
                    counter++;
            }
        }

        return counter;
    }
    void AIMove(int x1, int y1, int x2, int y2, int x3, int y3) //Выбор места для фишки на выбранной линии
    {
        if (AIisKnights)
        {
            if (GCF[y1, x1] == (int)FO.Empty && OrcsCellCost[y1, x1] > OrcsCellCost[y2, x2] && OrcsCellCost[y1, x1] > OrcsCellCost[y3, x3])
            {
                PositionSelect(y1, x1);
            }
            else if (GCF[y2, x2] == (int)FO.Empty && OrcsCellCost[y2, x2] > OrcsCellCost[y1, x1] && OrcsCellCost[y2, x2] > OrcsCellCost[y3, x3])
            {
                PositionSelect(y2, x2);
            }
            else if (GCF[y3, x3] == (int)FO.Empty && OrcsCellCost[y3, x3] > OrcsCellCost[y1, x1] && OrcsCellCost[y3, x3] > OrcsCellCost[y2, x2])
            {
                PositionSelect(y3, x3);
            }
            else
            {
                if (GCF[y1, x1] == (int)FO.Empty)
                    PositionSelect(y1, x1);
                else if ((GCF[y2, x2] == (int)FO.Empty))
                    PositionSelect(y2, x2);
                else if ((GCF[y3, x3] == (int)FO.Empty))
                    PositionSelect(y3, x3);
                else
                    AIRandomSpawner();
            }
        }
        else
        {
            if (GCF[y1, x1] == (int)FO.Empty && KnightsCellCost[y1, x1] > KnightsCellCost[y2, x2] && KnightsCellCost[y1, x1] > KnightsCellCost[y3, x3])
            {
                PositionSelect(y1, x1);
            }
            else if (GCF[y2, x2] == (int)FO.Empty && KnightsCellCost[y2, x2] > KnightsCellCost[y1, x1] && KnightsCellCost[y2, x2] > KnightsCellCost[y3, x3])
            {
                PositionSelect(y2, x2);
            }
            else if (GCF[y3, x3] == (int)FO.Empty && KnightsCellCost[y3, x3] > KnightsCellCost[y1, x1] && KnightsCellCost[y3, x3] > KnightsCellCost[y2, x2])
            {
                PositionSelect(y3, x3);
            }
            else
            {
                if (GCF[y1, x1] == (int)FO.Empty)
                    PositionSelect(y1, x1);
                else if ((GCF[y2, x2] == (int)FO.Empty))
                    PositionSelect(y2, x2);
                else if ((GCF[y3, x3] == (int)FO.Empty))
                    PositionSelect(y3, x3);
                else
                    AIRandomSpawner();
            }
        }
    }
    void AIRandomSpawner() //Случайная точка спавна
    {
        int randomNum1 = Random.Range(0, 4);
        int randomNum2 = Random.Range(0, 4);
        if (GCF[randomNum1, randomNum2] == (int)FO.Empty)
        {
            PositionSelect(randomNum1, randomNum2);
        }
        else
        {
            if (
                GCF[0, 0] != 0 && GCF[1, 0] != 0 && GCF[2, 0] != 0 && GCF[3, 0] != 0 &&
                GCF[0, 1] != 0 && GCF[1, 1] != 0 && GCF[2, 1] != 0 && GCF[3, 1] != 0 &&
                GCF[0, 2] != 0 && GCF[1, 2] != 0 && GCF[2, 2] != 0 && GCF[3, 2] != 0 &&
                GCF[0, 3] != 0 && GCF[1, 3] != 0 && GCF[2, 3] != 0 && GCF[3, 3] != 0
                )
            {
                EndPart("Tie");
            }
            else
            {
                AIRandomSpawner();
            }
        }
    }
    #endregion

    #region UI
    //Training
    IEnumerator TrainingSwipeMap() //Показ информации о свайпе при выборе карты
    {
        SwipeSupport.SetActive(true);
        yield return new WaitForSeconds(3);
        SwipeSupport.SetActive(false);
    }
    //Meny   
    public void ButtonGoToSelectMeny() //Перейти в меню выбора
    {
        StartScreen.SetActive(false);
        MainMeny.SetActive(true);
        StartCoroutine(CameraMove());
        //Запуск тренеровки
        if (!TrainingCompleted)
        {
            StartCoroutine(TrainingSwipeMap());
        }
    }
    public void ButtonStart() //Запуск партии
    {
        MainMeny.SetActive(false);
        MenyInGame.SetActive(true);
        StartGame();
    }
    public IEnumerator CameraMove(int currZ = 5)
    {
        MainCamera.transform.position = new Vector3(0, 27, currZ);
        yield return new WaitForSeconds(0.002f);
        if (currZ > -30)
            StartCoroutine(CameraMove(currZ - 1));
    }
    public IEnumerator RotateWorld(string direction, int currAngel = 90) //Перемещение карты под камеру
    {
        switch (direction)
        {
            case "Right":
                WorldCircle.transform.Rotate(new Vector3(0, -1, 0));
                yield return new WaitForSeconds(0.002f);
                if (currAngel > 1)
                    StartCoroutine(RotateWorld("Right", currAngel - 1));
                break;
            case "Left":
                WorldCircle.transform.Rotate(new Vector3(0, 1, 0));
                yield return new WaitForSeconds(0.002f);
                if (currAngel > 1)
                    StartCoroutine(RotateWorld("Left", currAngel - 1));
                break;
        }
    }
    public void RotateWorldTech(string direction) //Перемещение карты под камеру для прочего
    {
        PlayInMainMeny.gameObject.SetActive(false);
        switch (direction)
        {
            case "Right":
                switch (CurrentMap)
                {
                    case "Castle": CurrentMap = "Cave"; break;
                    case "Village": CurrentMap = "Castle"; break;
                    case "Canyon": CurrentMap = "Village"; break;
                    case "Cave": CurrentMap = "Canyon"; break;
                }
                break;
            case "Left":
                switch (CurrentMap)
                {
                    case "Castle": CurrentMap = "Village"; break;
                    case "Village": CurrentMap = "Canyon"; break;
                    case "Canyon": CurrentMap = "Cave"; break;
                    case "Cave": CurrentMap = "Castle"; break;
                }
                break;
        }
        Invoke("ShowNewMapInformation", 1.5f);
    }
    void ShowNewMapInformation() //Отключение кнопки играть и информации о карте для RotateWorld
    {
        PlayInMainMeny.gameObject.SetActive(false);
        CurrentMapText.gameObject.SetActive(true);
        switch (CurrentMap)
        {
            case "Castle": CurrentMapText.text = "Castle"; break;
            case "Village": CurrentMapText.text = "Village"; break;
            case "Canyon": CurrentMapText.text = "Canyon"; break;
            case "Cave": CurrentMapText.text = "Cave"; break;
        }
        Invoke("ShowNewMapInformationOff", 1);
    }
    void ShowNewMapInformationOff() //Отключение информации о карте и показ кнопки играть
    {
        PlayInMainMeny.gameObject.SetActive(true);
        CurrentMapText.gameObject.SetActive(false);
        PermissionToTurn = true;
    }
    //InGame
    void EndPart(string side) //Вывод сообщения о конце игровой партии
    {
        GameIsStarted = false;
        if (CurrentHealth > 0)
        {
            if (side != "Tie")
            {
                EndPartStatys.text = side + " wins!"; //Пишем сообщение для игрока

                if (side == "Knights" && !AIisKnights) //Если победил увеличиваем очки
                {
                    AddScore(100, true);
                }
                else if (side == "Orcs" && AIisKnights) //Если победил увеличиваем очки
                {
                    AddScore(100, true);
                }
                if (side == "Knights" && AIisKnights) //Если победил компютер то отнмаем здоровье
                {
                    CurrentHealth--;
                }
                else if (side == "Orcs" && !AIisKnights) //Если победил компютер то отнмаем здоровье
                {
                    CurrentHealth--;
                }
            }
            else
            {
                EndPartStatys.text = "TIE"; //Пишем сообщение для игрока
                AddScore(25);
            }
            MenyInGame.SetActive(false);
            EndPartDialoge.SetActive(true);
            Invoke("EventNextPart", 2);
        }
        else
        {
            if (side != "Tie")
            {
                EndGameStatys.text = side + " wins!"; //Пишем сообщение для игрока
                if (side == "Knights" && AIisKnights) //Если победил компьютер то заканчиваем игру
                {
                    EndGameTech();
                }
                else if (side == "Orcs" && !AIisKnights) //Если победил компьютер то отнмаем здоровье
                {
                    EndGameTech();
                }
                else
                {
                    AddScore(100,true);
                    EndPartStatys.text = side + " wins!"; //Пишем сообщение для игрока
                    MenyInGame.SetActive(false);
                    EndPartDialoge.SetActive(true);
                    Invoke("EventNextPart", 2);
                }
            }
            else
            {
                AddScore(25);
                EndPartStatys.text = "TIE"; //Пишем сообщение для игрока
                MenyInGame.SetActive(false);
                EndPartDialoge.SetActive(true);
                Invoke("EventNextPart", 2);
            }
        }
    }
    void EndGameTech() //Вывод сообщения о конце игровой партии (Открытие диалогового окна)
    {
        if (CurrentScore > GlobalScore)
            GlobalScore = CurrentScore;
        EndGameDialoge.SetActive(true); //Активируем объект интерфейса
    }
    void AddScore(int amount, bool addWinStreak = false) //Добавление очков
    {
        if (addWinStreak && WinStreak > 0)
        {
            switch (WinStreak)
            {
                case 1: CurrentScore += 10; break;
                case 2: CurrentScore += 20; break;
                case 3: CurrentScore += 30; break;
                case 4: CurrentScore += 40; break;
                case 5: CurrentScore += 50; break;
                case 6: CurrentScore += 60; break;
                case 7: CurrentScore += 70; break;
                case 8: CurrentScore += 80; break;
                case 9: CurrentScore += 90; break;
                default: CurrentScore += 100; break;
            }
            CurrentScore += amount;
            GameScoreAmountPlus.text = "+" + amount + " +Win streak (" + WinStreak + ")";
        }
        else
        {
            CurrentScore += amount;
            GameScoreAmountPlus.text = "+" + amount;
        }

        GameScoreAmountPlus.gameObject.SetActive(true);
        Invoke("ShowAddScoreOff", 1);
    }
    void ShowAddScoreOff() //Обновление показателя очков на табло и скрытие добавления очков
    {
        GameScoreAmountPlus.gameObject.SetActive(false);
        if (CurrentScore < 1)
        {
            GameScoreAmount.text = "000000";
        }
        else if (CurrentScore > 0 && CurrentScore < 100)
        {
            GameScoreAmount.text = "0000" + CurrentScore;
        }
        else if (CurrentScore > 99 && CurrentScore < 1000)
        {
            GameScoreAmount.text = "000" + CurrentScore;
        }
        else if (CurrentScore > 999 && CurrentScore < 10000)
        {
            GameScoreAmount.text = "00" + CurrentScore;
        }
        else if (CurrentScore > 9999 && CurrentScore < 100000)
        {
            GameScoreAmount.text = "0" + CurrentScore;
        }
        else if (CurrentScore > 99999)
        {
            GameScoreAmount.text = "" + CurrentScore;
        }
    }
    public void ButtonGoToSelectMenyInGame() //Кнопка возврата в меню
    {
        CurrentHealth = 1;
        CurrentScore = 0;
        WinStreak = 0;
        ShowAddScoreOff();

        //Clear field
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                if (GCF[j, i] != 0)
                {
                    GameplayObjects[j, i].GetComponent<scr_GameObjectController>().Clear(); //Удаляем объект с поля
                    GCF[j, i] = 0; //Очищаем клетку в массиве
                }
        }

        GameScoreText.text = "Best Score";

        EndGameDialoge.SetActive(false);
        MainMeny.SetActive(true);
    }
    public void EventNextPart() //Заставка между раундами
    {
        EndPartDialoge.SetActive(false);
        MenyInGame.SetActive(true);
        //Clear field
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                if (GCF[j, i] != 0)
                {
                    GameplayObjects[j, i].GetComponent<scr_GameObjectController>().Clear(); //Удаляем объект с поля
                    GCF[j, i] = 0; //Очищаем клетку в массиве
                }
        }
        StartGame(); //Перезапускаем игру
    }
    public void ButtonNextGame() //Кнопка новой игры
    {
        //Clear field
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                if (GCF[j, i] != 0)
                {
                    GameplayObjects[j, i].GetComponent<scr_GameObjectController>().Clear(); //Удаляем объект с поля
                    GCF[j, i] = 0; //Очищаем клетку в массиве
                }
        }
        EndGameDialoge.SetActive(false); //Отключаем объект интерфейса

        CurrentHealth = 1;
        CurrentScore = 0;
        WinStreak = 0;
        ShowAddScoreOff();

        StartGame(); //Перезапускаем игру
    }
    #endregion

    #region TEST

    [Header("ArrayMap")]
    public GameObject Map;
    public Text[] arrMapObj = new Text[16];
    bool ArrayMapOn = false;
    public void BUTTONKNIGHT()
    {
        CellCostAnalysis();
        arrMapObj[0].text = "" + KnightsCellCost[0, 0];
        arrMapObj[1].text = "" + KnightsCellCost[1, 0];
        arrMapObj[2].text = "" + KnightsCellCost[2, 0];
        arrMapObj[3].text = "" + KnightsCellCost[3, 0];
        arrMapObj[4].text = "" + KnightsCellCost[0, 1];
        arrMapObj[5].text = "" + KnightsCellCost[1, 1];
        arrMapObj[6].text = "" + KnightsCellCost[2, 1];
        arrMapObj[7].text = "" + KnightsCellCost[3, 1];
        arrMapObj[8].text = "" + KnightsCellCost[0, 2];
        arrMapObj[9].text = "" + KnightsCellCost[1, 2];
        arrMapObj[10].text = "" + KnightsCellCost[2, 2];
        arrMapObj[11].text = "" + KnightsCellCost[3, 2];
        arrMapObj[12].text = "" + KnightsCellCost[0, 3];
        arrMapObj[13].text = "" + KnightsCellCost[1, 3];
        arrMapObj[14].text = "" + KnightsCellCost[2, 3];
        arrMapObj[15].text = "" + KnightsCellCost[3, 3];
    }
    public void BUTTONORC()
    {
        CellCostAnalysis();
        arrMapObj[0].text = "" + OrcsCellCost[0, 0];
        arrMapObj[1].text = "" + OrcsCellCost[1, 0];
        arrMapObj[2].text = "" + OrcsCellCost[2, 0];
        arrMapObj[3].text = "" + OrcsCellCost[3, 0];
        arrMapObj[4].text = "" + OrcsCellCost[0, 1];
        arrMapObj[5].text = "" + OrcsCellCost[1, 1];
        arrMapObj[6].text = "" + OrcsCellCost[2, 1];
        arrMapObj[7].text = "" + OrcsCellCost[3, 1];
        arrMapObj[8].text = "" + OrcsCellCost[0, 2];
        arrMapObj[9].text = "" + OrcsCellCost[1, 2];
        arrMapObj[10].text = "" + OrcsCellCost[2, 2];
        arrMapObj[11].text = "" + OrcsCellCost[3, 2];
        arrMapObj[12].text = "" + OrcsCellCost[0, 3];
        arrMapObj[13].text = "" + OrcsCellCost[1, 3];
        arrMapObj[14].text = "" + OrcsCellCost[2, 3];
        arrMapObj[15].text = "" + OrcsCellCost[3, 3];
    }
    public void BUTTONARRAYMAP()
    {
        if (ArrayMapOn)
        {
            Map.SetActive(false);
            ArrayMapOn = false;
        }
        else
        {
            Map.SetActive(true);
            ArrayMapOn = true;
        }
    }
    #endregion
}