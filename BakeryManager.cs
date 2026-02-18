using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BakeryManager : MonoBehaviour
{
    public FridgeSys fridgeSys;
    TrashSys trashSys;
    CashRegisterSys cashRegisterSys;
    BakeSys bakeSys;
    CoolingSys coolingSys;
    InventorySys inventorySys;
    public SellingSys sellingSys;
    [SerializeField] public daySystem daysSystem;
    [SerializeField] public CustomerSpawner customerSpawner;
    [SerializeField] public TutorialManager tutorialManager;
    public BalanceManager balanceManager;

    public movePerson _canMove;
    public GameObject _player;

    [SerializeField] public GameObject _pointCash;
    [SerializeField] public GameObject _pointFridge;      
    [SerializeField] public GameObject _pointOven;        
    [SerializeField] public GameObject _pointCoolingRack; 
    [SerializeField] public GameObject _pointSellRack;    
    [SerializeField] public GameObject _pointTrashCan;    
    
    [SerializeField] public List<Sprite> rawBreadSprites; 
    [SerializeField] public List<Sprite> bakedBreadSprites; 
    [SerializeField] public List<Sprite> burnedBreadSprites; 
    [SerializeField] public List<Sprite> cooledBreadSprites; 
    [SerializeField] public List<string> breadTypes;

    [SerializeField] public List<Sprite> rawBreadSpritesTwo; 
    [SerializeField] public List<Sprite> bakedBreadSpritesTwo; 
    [SerializeField] public List<Sprite> burnedBreadSpritesTwo; 
    [SerializeField] public List<Sprite> cooledBreadSpritesTwo; 
    [SerializeField] public List<string> breadTypesTwo;

    [SerializeField] public List<Sprite> rawBreadSpritesThree; 
    [SerializeField] public List<Sprite> bakedBreadSpritesThree; 
    [SerializeField] public List<Sprite> burnedBreadSpritesThree; 
    [SerializeField] public List<Sprite> cooledBreadSpritesThree; 
    [SerializeField] public List<string> breadTypesThree;
    public bool isAddBunsFirst = false;
    public bool isAddBunsSecorn = false;

    [SerializeField] GameObject _arrow;
    [SerializeField] private GameObject[] _arrowPoints;
    [SerializeField] private GameObject[] _arrowPoiOven;
    [SerializeField] private GameObject[] _arrowPoiCooling;
    [SerializeField] private GameObject[] _lightOven;
    [SerializeField] private GameObject[] _lightCooling;
    private bool _isChouseBun = false;
    private bool _isChouseOven = false;
    private bool _isChouseCooling = false;
    private bool isFirstEntryToOven = true;
    private bool isFirstEntryToCooling = true;
    private bool _initialized = false;

    private int selectedBreadIndex = 0;
    [SerializeField] private int _gridWidth = 2;
    [SerializeField] private ScrollRect scrollRect;

    private void Start()
    {
        fridgeSys = GetComponent<FridgeSys>();
        trashSys = GetComponent<TrashSys>();
        cashRegisterSys = GetComponent<CashRegisterSys>();
        bakeSys = GetComponent<BakeSys>();
        coolingSys = GetComponent<CoolingSys>();
        inventorySys = GetComponent<InventorySys>();
        sellingSys = GetComponent<SellingSys>();
        _arrow.transform.position = _arrowPoints[0].transform.position;
    }

    // Clears interaction flags and disables related UI elements
    public void exitObjec()
    {
        _isChouseBun = false;
        _isChouseOven = false;
        _isChouseCooling = false;
        fridgeSys.fridgePanel.SetActive(false);
        _canMove.PublicCanMove = false;
        tutorialManager.canInteract = false;
        _arrow.SetActive(false);
        isFirstEntryToOven = true;
        isFirstEntryToCooling = true;
        _initialized = false;
        fridgeSys.highlightPrefab.SetActive(false);
    }

    private void Update()
    {
        ChangeBuns();
        if (_isChouseBun)
        {
            ChouseBun();
        }
        else if (_isChouseOven)
        {
            ChooseOvenSlot();
        }
        else if (_isChouseCooling)
        {
            ChouseCoolingSlot();
        }
        else if (fridgeSys.fridgePanel.activeSelf)
        {
            HandleKeyboardInput();  
        }
        else
        {
            InteractionWithPoints(); 
        }  
    }

    // Updates available bread types and sprites based on the current day progression
    public void ChangeBuns()
    {
        if (daysSystem.dayIndex >= 4 && daysSystem.dayIndex < 6 && !isAddBunsFirst)
        {
            rawBreadSprites.Clear();
            bakedBreadSprites.Clear();
            burnedBreadSprites.Clear();
            cooledBreadSprites.Clear();
            breadTypes.Clear();
            rawBreadSprites.AddRange(rawBreadSpritesTwo);
            bakedBreadSprites.AddRange(bakedBreadSpritesTwo);
            burnedBreadSprites.AddRange(burnedBreadSpritesTwo);
            cooledBreadSprites.AddRange(cooledBreadSpritesTwo);
            breadTypes.AddRange(breadTypesTwo);
            
            isAddBunsFirst = true;
        }
        else if (daysSystem.dayIndex >= 6 && !isAddBunsSecorn)
        {
            rawBreadSprites.Clear();
            bakedBreadSprites.Clear();
            burnedBreadSprites.Clear();
            cooledBreadSprites.Clear();
            breadTypes.Clear();
            rawBreadSprites.AddRange(rawBreadSpritesThree);
            bakedBreadSprites.AddRange(bakedBreadSpritesThree);
            burnedBreadSprites.AddRange(burnedBreadSpritesThree);
            cooledBreadSprites.AddRange(cooledBreadSpritesThree);
            breadTypes.AddRange(breadTypesThree);
            isAddBunsSecorn = true;
        }
    }

    // Handles player interaction flow with bakery UI and objects.
    private void HandleKeyboardInput()
    {
        HighlightSelectedButton();
        int rowCount = Mathf.CeilToInt((float)fridgeSys.breadButtons.Count / _gridWidth);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selectedBreadIndex >= _gridWidth)
            {
                selectedBreadIndex -= _gridWidth;
            }
            else
            {
                selectedBreadIndex = fridgeSys.breadButtons.Count + selectedBreadIndex - _gridWidth;
            }
            HighlightSelectedButton();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selectedBreadIndex + _gridWidth < fridgeSys.breadButtons.Count)
            {
                selectedBreadIndex += _gridWidth;
            }
            else
            {
                selectedBreadIndex = (selectedBreadIndex + _gridWidth) % fridgeSys.breadButtons.Count;
            }
            HighlightSelectedButton();
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedBreadIndex = (selectedBreadIndex - 1 + fridgeSys.breadButtons.Count) % fridgeSys.breadButtons.Count;
            HighlightSelectedButton();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedBreadIndex = (selectedBreadIndex + 1) % fridgeSys.breadButtons.Count;
            HighlightSelectedButton();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            string selectedBreadType = breadTypes[selectedBreadIndex];
            fridgeSys.OnBreadSelected(selectedBreadType);
        }
    }

    public void HighlightSelectedButton()
    {
        RectTransform buttonRect = fridgeSys.breadButtons[selectedBreadIndex].GetComponent<RectTransform>();
        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scrollRectTransform,
            RectTransformUtility.WorldToScreenPoint(Camera.main, buttonRect.position),
            Camera.main,
            out localPoint
        );

        float newScrollPosition = Mathf.Clamp01(1 + (localPoint.y * 3 / scrollRectTransform.rect.height));
        scrollRect.verticalNormalizedPosition = newScrollPosition;
        fridgeSys.highlightPrefab.transform.position = buttonRect.position;
        fridgeSys.highlightPrefab.SetActive(true);
    }

    private void ChouseBun()
    {
        if (_isChouseBun == true)
        {
            if (!_initialized)
            {
                _arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                (Sprite first, Sprite second) = customerSpawner.GetOrderForFirstCustomer();
                string firstCustomerOrder = first?.name;
                string secondCustomerOrder = second?.name;

                if (customerSpawner.GetSelfActiveOrder() == 1)
                {
                    if ((cashRegisterSys.isActiveFirstCustomer == true || cashRegisterSys.isActiveSecondCustomer == true)
                        && customerSpawner.GetSelfActiveOrder() == 1)
                    {
                        for (int i = 0; i < sellingSys.breadOnSellRack.Count; i++)
                        {
                            if (sellingSys.breadOnSellRack[i] != null
                                && (sellingSys.breadOnSellRack[i].Type == firstCustomerOrder))
                            {
                                sellingSys._inedPointSellRack = i;
                                break;
                            }
                            else if (sellingSys.breadOnSellRack[i] != null && sellingSys.breadOnSellRack[i].Type == secondCustomerOrder)
                            {
                                sellingSys._inedPointSellRack = i;
                                break;
                            }
                        }
                    }
                }
                else if (second == null)
                {
                    if (sellingSys.breadOnSellRack != null)
                    {
                        for (int i = 0; i < sellingSys.breadOnSellRack.Count; i++)
                        {
                            if (sellingSys.breadOnSellRack[i] != null && sellingSys.breadOnSellRack[i].Type == firstCustomerOrder)
                            {
                                sellingSys._inedPointSellRack = i; 
                                break;
                            }
                        }
                    }
                    
                }
                
                if (InventorySys.currentBread != null && InventorySys.currentBread.State == BreadState.Cooled)
                {
                    for (int i = 0; i < sellingSys.breadOnSellRack.Count; i++)
                    {
                        if (sellingSys.breadOnSellRack[i] == null)
                        {
                            sellingSys._inedPointSellRack = i; 
                            break;
                        }
                    }
                }

                _arrow.transform.position = _arrowPoints[sellingSys._inedPointSellRack].transform.position;
                _initialized = true; 
                _arrow.SetActive(true);
            }
            _arrow.transform.position = _arrowPoints[sellingSys._inedPointSellRack].transform.position;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                sellingSys._inedPointSellRack = (sellingSys._inedPointSellRack - 1 + sellingSys.sellRackImage.Length) % sellingSys.sellRackImage.Length;
                _arrow.transform.position = _arrowPoints[sellingSys._inedPointSellRack].transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                sellingSys._inedPointSellRack = (sellingSys._inedPointSellRack + 1) % sellingSys.sellRackImage.Length;
                _arrow.transform.position = _arrowPoints[sellingSys._inedPointSellRack].transform.position;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (InventorySys.currentBread != null && InventorySys.currentBread.State == BreadState.Cooled )
                {
                    sellingSys.PlaceOnSellRack();
                }
                else
                {
                    sellingSys.CollectPlaceOnSellRack();
                }

                _canMove.PublicCanMove = true; 
                _isChouseBun = false;          
                _arrow.SetActive(false);      
                _initialized = false; 
            }
        }
    }

    private void ChooseOvenSlot()
    {
        if(_isChouseOven ==true)
        {
            if (isFirstEntryToOven)
            {
                _arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                
                if (InventorySys.currentBread != null && InventorySys.currentBread.State == BreadState.Raw)
                {
                    int freeSlot = -1;

                    // first free slot
                    for (int i = 0; i < BakeSys.breadInOven.Length; i++)
                    {
                        if (BakeSys.breadInOven[i] == null && !bakeSys.brakeBakery[i])
                        {
                            freeSlot = i;
                            break;
                        }
                    }

                    if (freeSlot != -1)
                    {
                        bakeSys.currentOvenSlot = freeSlot;
                    }
                    else
                    {
                        bakeSys.currentOvenSlot = FindLongestBakingSlot();
                    }
                }
                else 
                {
                    bakeSys.currentOvenSlot = FindLongestBakingSlot();
                }
                _arrow.transform.position = _arrowPoiOven[bakeSys.currentOvenSlot].transform.position;
                _arrow.SetActive(true);
                isFirstEntryToOven = false; 
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                bakeSys.currentOvenSlot = (bakeSys.currentOvenSlot - 1 + 2) % 2;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                bakeSys.currentOvenSlot = (bakeSys.currentOvenSlot + 1) % 2;
            }
            
            _arrow.transform.position = _arrowPoiOven[bakeSys.currentOvenSlot].transform.position;
            for (int i = 0; i < _lightCooling.Length; i++)
            {
                _lightOven[i].SetActive(false);
            }

            _lightOven[bakeSys.currentOvenSlot].SetActive(true);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (InventorySys.currentBread != null)
                {
                    bakeSys.StartBakingProcess();
                }
                else
                {
                    bakeSys.CollectBakedBread();
                }
                _lightOven[bakeSys.currentOvenSlot].SetActive(false);
                _canMove.PublicCanMove = true; 
                _isChouseOven = false;         
                _arrow.SetActive(false);      
                isFirstEntryToOven = true;
            }
        }
    }

    private int FindLongestBakingSlot()
    {
        int longestSlot = -1;
        float longestTime = -1f;

        for (int i = 0; i < bakeSys.currentBakeTimer.Length; i++)
        {
            float timeInSlot = 0f;
            if(BakeSys.breadInOven[i] != null)
            {
                if (BakeSys.breadInOven[i].State == BreadState.InOven)
                {
                    timeInSlot = balanceManager.bakeTime - bakeSys.currentBakeTimer[i];
                }
                else if (BakeSys.breadInOven[i].State == BreadState.Baked)
                {
                    timeInSlot = balanceManager.bakeTime + bakeSys.burnTimer[i];
                }
                else if(BakeSys.breadInOven[i].State == BreadState.Burned)
                {
                    timeInSlot = balanceManager.bakeTime * 2 + bakeSys.burnTimer[i];
                }
                else
                {
                    continue;
                }
            }
            else
            {
                continue;
            }

            if (timeInSlot > longestTime)
            {
                longestTime = timeInSlot;
                longestSlot = i;
            }
        }
        return longestSlot == -1 ? 0 : longestSlot;
    }

    private void ChouseCoolingSlot()
    {
        if (_isChouseCooling == true)
        {
            if (isFirstEntryToCooling)
            {
                _arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                coolingSys.currentCoolingSlot = FindLongestCoolingSlot();
                if (InventorySys.currentBread != null && InventorySys.currentBread.State == BreadState.Baked)
                {
                    for (int i = 0; i < CoolingSys.coolingBread.Length; i++)
                    {
                        if (CoolingSys.coolingBread[i] == null)
                        {
                            coolingSys.currentCoolingSlot = i; 
                            break;
                        }
                    }
                }
                _arrow.transform.position = _arrowPoiCooling[coolingSys.currentCoolingSlot].transform.position;
                _arrow.SetActive(true);
                isFirstEntryToCooling = false; 
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                coolingSys.currentCoolingSlot = (coolingSys.currentCoolingSlot - 1 + 2) % 2; 

            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                coolingSys.currentCoolingSlot = (coolingSys.currentCoolingSlot + 1) % 2;

            }

            _arrow.transform.position = _arrowPoiCooling[coolingSys.currentCoolingSlot].transform.position;
            for (int i = 0; i < _lightCooling.Length; i++)
            {
                _lightCooling[i].SetActive(false);
            }

            _lightCooling[coolingSys.currentCoolingSlot].SetActive(true);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (InventorySys.currentBread != null && InventorySys.currentBread.State == BreadState.Baked)
                {
                    coolingSys.PlaceOnCoolingRack();
                }
                else
                {
                    coolingSys.CollectCooledBread();
                }
                _lightCooling[coolingSys.currentCoolingSlot].SetActive(false);
                _canMove.PublicCanMove = true; 
                _isChouseCooling = false;          
                _arrow.SetActive(false);       
                isFirstEntryToCooling = true;
            }
        }
    }

    private int FindLongestCoolingSlot()
    {
        int longestSlot = -1; 

        for (int i = 0; i < coolingSys.currentCoolingTime.Length; i++)
        {
            if (!coolingSys.isCooling[i] && coolingSys.currentCoolingTime[i] < 0)
            {
                if (longestSlot == -1 || coolingSys.currentCoolingTime[i] > coolingSys.currentCoolingTime[longestSlot])
                {
                    longestSlot = i; 
                }
            }
        }
        return longestSlot == -1 ? 0 : longestSlot;
    }

    public bool IsPlayerNear(GameObject point)
    {
        return Vector3.Distance(_player.transform.position, point.transform.position) < 1.5f;
    }

    private void InteractionWithPoints()
    {
        if (tutorialManager.canInteract)
        {
            if (IsPlayerNear(_pointFridge) && Input.GetKeyDown(KeyCode.Space))
            {
                _canMove.PublicCanMove = false;
                fridgeSys.OpenFridge();

                selectedBreadIndex = 0;
                HighlightSelectedButton();
            }
            else if (IsPlayerNear(_pointOven) && Input.GetKeyDown(KeyCode.Space))
            {
                if (!_isChouseOven) 
                {
                    _canMove.PublicCanMove = false;  
                    _isChouseOven = true;            
                    _arrow.SetActive(true);      
                    _arrow.transform.position = _arrowPoiOven[bakeSys.currentOvenSlot].transform.position; ;
                }
            }
            else if (IsPlayerNear(_pointCoolingRack) && Input.GetKeyDown(KeyCode.Space))
            {
                if (!_isChouseCooling) 
                {
                    _canMove.PublicCanMove = false;  
                    _isChouseCooling = true;          
                    _arrow.SetActive(true);      
                    _arrow.transform.position = _arrowPoiCooling[coolingSys.currentCoolingSlot].transform.position; 
                }
            }
            else if (IsPlayerNear(_pointSellRack) && Input.GetKeyDown(KeyCode.Space))
            {
                if (!_isChouseBun) 
                {
                    _canMove.PublicCanMove = false; 
                    _isChouseBun = true;           
                    _arrow.SetActive(true);       
                    _arrow.transform.position = _arrowPoints[sellingSys._inedPointSellRack].transform.position;
                }

            }
            else if (IsPlayerNear(_pointCash) && Input.GetKeyDown(KeyCode.Space))
            {
                cashRegisterSys.CashRegister();
            }

            else if (IsPlayerNear(_pointTrashCan) && Input.GetKeyDown(KeyCode.Space))
            {
                trashSys.TrashCan();
            }
        }
    }

    public void EmptyInventory()
    {
        InventorySys.currentBread = null;
        inventorySys.UpdateInventoryDisplay();

    }
}
