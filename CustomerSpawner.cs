using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class CustomerSpawner : SoundsSystem
{
    [SerializeField] daySystem daySystem;
    [SerializeField] BakeSys bakeSys;
    [SerializeField] CoolingSys coolingSys;
    [SerializeField] SellingSys sellingSys;
    [SerializeField] BakeryManager bakeryManager;
    [SerializeField] InventorySys inventorySys;
    [SerializeField] BalanceManager balanceManager;
    [SerializeField] CashRegisterSys cashRegisterSys;
    public TutorialManager tutorialManager;
    public bool canSpawnCustomers = true;

    public GameObject customerPrefab; 
    public Transform spawnPoints;  
    public Transform waitingPosition;  
    public Transform goPosition;   
    public Transform exitPosition;   
    public Transform[] queuePositions; 
    public int maxCustomers = 5;
    public float spawnInterval = 0f;  

    public List<GameObject> _customerIsSpawned = new List<GameObject>();
    public List<GameObject> _customerIsGo = new List<GameObject>();

    public int currentCustomerCount = 0;
    public int CurrentCustomerCount => currentCustomerCount;
    private Queue<Transform> availablePositions; 

    public float duration = 0.5f;  
    public float strength = 0.1f;  
    public int vibrato = 5;       
    public float randomness = 90f; 
    private Vector3 defaultScale = Vector3.one;

    public SpriteRenderer orderImage;         
    private Sprite currentOrder;      
    [HideInInspector] public bool orderGeneratedForFirstCustomer = false; 

    [SerializeField] private List<Sprite> skinSprites;      
    [SerializeField] private List<Sprite> faceSprites;      
    [SerializeField] private List<Sprite> frontHairSprites; 
    [SerializeField] private List<Sprite> backHairSprites;  
    [SerializeField] private List<Sprite> frontOutfitSprites;   
    [SerializeField] private List<Sprite> backOutfitSprites;    
    [SerializeField] private List<Sprite> reactionSprite;    

    private bool isShowingOrders = false;
    public List<Sprite> alreadyOrderedBreads = new List<Sprite>(); 
    List<Sprite> availableBreadSprites = new List<Sprite>();
    List<Sprite> availableCooledBreads = new List<Sprite>();
    public bool isDayDouble = false;

    [SerializeField] private float stepInterval = 0.4f;

    void Start()
    {
        availablePositions = new Queue<Transform>(queuePositions);
        alreadyOrderedBreads.Clear(); 
    }

    void Update()
    {
        if (daySystem.dayIndex >= 3)
        {
            isDayDouble = true;
        }
    }

    // Randomizes customer visuals
    private void ApplyRandomAppearance(GameObject customer)
    {
        Transform hair = customer.transform.Find("hair");
        Transform skin = customer.transform.Find("skin");
        Transform face = customer.transform.Find("face");
        Transform outfit = customer.transform.Find("cloth");

        if (hair == null || skin == null || face == null || outfit == null)
        {
            return;
        }

        if (hair && skin && face && outfit)
        {
            int skinIndex = Random.Range(0, skinSprites.Count);
            int faceIndex = Random.Range(0, faceSprites.Count);
            int frontHairIndex = Random.Range(0, backHairSprites.Count);
            int frontOutfitIndex = Random.Range(0, backOutfitSprites.Count);

            hair.GetComponent<SpriteRenderer>().sprite = backHairSprites[frontHairIndex];
            skin.GetComponent<SpriteRenderer>().sprite = skinSprites[skinIndex];
            face.GetComponent<SpriteRenderer>().sprite = faceSprites[faceIndex];
            outfit.GetComponent<SpriteRenderer>().sprite = backOutfitSprites[frontOutfitIndex];

            CustomerAppearance appearance = customer.GetComponent<CustomerAppearance>();
            if (appearance == null)
            {
                appearance = customer.AddComponent<CustomerAppearance>();
            }

            appearance.HairIndex = frontHairIndex;
            appearance.OutfitIndex = frontOutfitIndex;
        }
    }
 
    private void SwitchSide(GameObject customer, bool showFront)
    {
        Transform hair = customer.transform.Find("hair");
        Transform outfit = customer.transform.Find("cloth");
        CustomerAppearance appearance = customer.GetComponent<CustomerAppearance>();
        if (hair && outfit && showFront)
        {
            hair.GetComponent<SpriteRenderer>().sprite = frontHairSprites[appearance.HairIndex];
            outfit.GetComponent<SpriteRenderer>().sprite = frontOutfitSprites[appearance.OutfitIndex];
        }
    }

    public void StartSpawning()
    {
        if (tutorialManager.canSpawnOneClient)
        {
            SpawnCustomer();
        }
        else
        {
            StartCoroutine(SpawnCustomers()); 
        }
    }

    IEnumerator SpawnCustomers()
    {
        while (canSpawnCustomers) 
        {
            spawnInterval = Random.Range(balanceManager.minSpawnInterval, balanceManager.maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval); 
            if (currentCustomerCount < maxCustomers && availablePositions.Count > 0)
            {
                SpawnCustomer();
            }
        }
    }

    // Instantiates a new customer and assigns them to the next queue position
    void SpawnCustomer()
    {
        int nextAvailablePositionIndex = _customerIsSpawned.Count;
        if (nextAvailablePositionIndex < queuePositions.Length) 
        {
            Transform waitingPosition = queuePositions[nextAvailablePositionIndex];
            GameObject newCustomer = Instantiate(customerPrefab, spawnPoints.position, Quaternion.identity);
            _customerIsSpawned.Add(newCustomer);
            currentCustomerCount++;

            ApplyRandomAppearance(newCustomer);

            Transform hair = newCustomer.transform.Find("hair");
            Transform skin = newCustomer.transform.Find("skin");
            Transform face = newCustomer.transform.Find("face");
            Transform outfit = newCustomer.transform.Find("cloth");
            Transform reaction = newCustomer.transform.Find("reaction");
            skin.GetComponent<SpriteRenderer>().sortingOrder = 10 + nextAvailablePositionIndex +3 ;
            face.GetComponent<SpriteRenderer>().sortingOrder = 11 + nextAvailablePositionIndex +3;
            hair.GetComponent<SpriteRenderer>().sortingOrder = 12 + nextAvailablePositionIndex + 3;
            outfit.GetComponent<SpriteRenderer>().sortingOrder = 11 + nextAvailablePositionIndex + 3;
            reaction.GetComponent<SpriteRenderer>().sortingOrder = 10 + nextAvailablePositionIndex + 3;

            StartCoroutine(MoveToPosition(newCustomer.transform, waitingPosition));
        }
    }

    private (Sprite, Sprite) GenerateCustomerOrder()
    {
        ChangeListBread();

        Sprite firstOrder = GetRandomBreadWithProbability();
        Sprite secondOrder = GetRandomBreadWithProbability();
     
        if (firstOrder == secondOrder)
        {
            secondOrder = GetRandomBreadWithProbability(firstOrder);
        }

        return (firstOrder, secondOrder);
    }

    // Weighted random: prioritizes breads currently available on the shelves
    private Sprite GetRandomBreadWithProbability(Sprite excludeBread = null)
    {
        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= 0.6f && availableBreadSprites.Count > 0) 
        {
            cashRegisterSys.extraPatience = 0;
            return GetBreadFromList(availableBreadSprites, excludeBread);
        }
        else if (randomValue <= 0.85f && availableCooledBreads.Count > 0) 
        {
            cashRegisterSys.extraPatience = 0;
            return GetBreadFromList(availableCooledBreads, excludeBread);
        }
        else 
        {
            cashRegisterSys.extraPatience = 5;
            return GetRandomBread(excludeBread);
        }
    }

    private Sprite GetBreadFromList(List<Sprite> breadList, Sprite excludeBread = null)
    {
        if (breadList.Count == 0) return null;

        Sprite selectedBread = breadList[0];
        breadList.RemoveAt(0);

        if (selectedBread == excludeBread && breadList.Count > 0)
        {
            selectedBread = breadList[0];
            breadList.RemoveAt(0);
        }

        return selectedBread;
    }

    private Sprite GetRandomBread(Sprite excludeBread = null)
    {
        List<Sprite> allBreads = bakeryManager.cooledBreadSprites
            .Where(bread => bread != excludeBread)
            .ToList();
        if (allBreads.Count == 0) return null;

        return allBreads[Random.Range(0, allBreads.Count)];
    }

    // Filters and updates available bread lists to ensure order variety
    private void ChangeListBread()
    {
        availableBreadSprites = sellingSys.GetAvailableBreadSprites()
            .Where(bread => !alreadyOrderedBreads.Contains(bread))
            .ToList();

        availableCooledBreads = bakeryManager.cooledBreadSprites
            .Where(bread => !alreadyOrderedBreads.Contains(bread))
            .ToList();
    }

    IEnumerator MoveToPosition(Transform customer, Transform targetPosition)
    {
        if (customer == null) yield break;
        float moveSpeed = 2f;

        customer.DOShakeScale(duration, strength, vibrato, randomness)
            .SetEase(Ease.OutElastic)
            .SetLoops(-1, LoopType.Restart);
        float stepTimer = 0f;
        while (Vector3.Distance(customer.position, targetPosition.position) > 0.1f)
        {
            
            if (customer == null || daySystem.isDayOver) yield break;

            customer.position = Vector3.MoveTowards(
                customer.position,
                targetPosition.position,
                moveSpeed * Time.deltaTime
            );
            
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                PlaySound(0, volume: 0.5f); 
            }

            yield return null;
        }
        customer.DOKill();
        
        customer.DOScale(defaultScale, 0.3f) 
            .SetEase(Ease.OutSine); 
        customer.position = targetPosition.position;
        
    }
  
    public void ReactionCustomer( int index)
    {
        GameObject firstCustomer = _customerIsSpawned[0];
        CustomerAppearance appearance = firstCustomer.GetComponent<CustomerAppearance>();
    
        if (appearance.Order == null && appearance.FirstOrder == null)
        {
            ShowOrdersForAllCustomers();
            return;
        }
        Transform reaction = firstCustomer.transform.Find("reaction");

        reaction.GetComponent<SpriteRenderer>().sprite = reactionSprite[index];
        reaction.localScale = Vector3.one;
        reaction.DOScale(1.25f, 0.5f) 
                .SetLoops(3, LoopType.Yoyo) 
                .SetEase(Ease.InOutSine) 
                .OnComplete(() => {
                    reaction.GetComponent<SpriteRenderer>().DOFade(0, 0.5f) 
                        .OnComplete(() => {
                            reaction.gameObject.SetActive(false);

                            reaction.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                        });
                });
        reaction.gameObject.SetActive(true);
    }

    public void ShowOrdersForAllCustomers()
    {
        if (isShowingOrders) return;

        isShowingOrders = true;

        if (_customerIsSpawned.Count > 0)
        {
            foreach (GameObject customer in _customerIsSpawned)
            {
                ChangeListBread();

                CustomerAppearance appearance = customer.GetComponent<CustomerAppearance>();
                if (appearance == null)
                {
                    appearance = customer.AddComponent<CustomerAppearance>();
                }

                if (appearance.Order == null && appearance.SecondOrder == null)
                {
                    (Sprite firstOrder, Sprite secondOrder) = GenerateCustomerOrder();
                    appearance.Order = firstOrder;
                    appearance.SecondOrder = secondOrder;
                    appearance.FirstOrder = firstOrder;

                    if (!appearance.IsOrderTypeGenerated && isDayDouble)
                    {
                        appearance.IsDoubleOrder = appearance.SecondOrder != null && Random.value > 0.5f;
                        appearance.IsOrderTypeGenerated = true;
                    }

                    if (appearance.IsDoubleOrder)
                    {
                        cashRegisterSys.extraPatience = 3;
                        alreadyOrderedBreads.Add(appearance.FirstOrder);
                        alreadyOrderedBreads.Add(appearance.SecondOrder);
                    }
                    else
                    {
                        alreadyOrderedBreads.Add(appearance.Order);
                    }
                }
                UpdateCustomerUI(customer, appearance);
            }
        }
        else
        {
            Debug.Log("there are no customers in the queue to display orders.");
        }

        isShowingOrders = false;
    }

    private void UpdateCustomerUI(GameObject customer, CustomerAppearance appearance)
    {
        int customerIndex = _customerIsSpawned.IndexOf(customer);

        Transform singleOrderUI = customer.transform.GetChild(0);
        Transform doubleOrderUI = customer.transform.GetChild(1);

        singleOrderUI.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 13 + customerIndex + 3;
        doubleOrderUI.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 13 + customerIndex + 3;

        SpriteRenderer singleOrderRenderer = singleOrderUI.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer firstDoubleOrderRenderer = doubleOrderUI.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer secondDoubleOrderRenderer = doubleOrderUI.GetChild(1).GetComponent<SpriteRenderer>();

        singleOrderRenderer.sortingOrder = 14 + customerIndex + 3;
        firstDoubleOrderRenderer.sortingOrder = 14 + customerIndex + 3;
        secondDoubleOrderRenderer.sortingOrder = 14 + customerIndex + 3;

        if (appearance.IsDoubleOrder)
        {
            firstDoubleOrderRenderer.sprite = appearance.FirstOrder;
            secondDoubleOrderRenderer.sprite = appearance.SecondOrder;

            singleOrderUI.gameObject.SetActive(false);
            doubleOrderUI.gameObject.SetActive(true);
        }
        else
        {
            singleOrderRenderer.sprite = appearance.Order;

            singleOrderUI.gameObject.SetActive(true);
            doubleOrderUI.gameObject.SetActive(false);
        }
    }

    // Returns the order type (0 for single, 1 for double)
    public int GetSelfActiveOrder()
    {
        Transform firstCustomerPosition = queuePositions[0];
        GameObject firstCustomer = null;

        foreach (GameObject customer in _customerIsSpawned)
        {
            if (Vector3.Distance(customer.transform.position, firstCustomerPosition.position) < 0.1f)
            {
                firstCustomer = customer;
                break;
            }
        }

        if (firstCustomer != null)
        {
            CustomerAppearance appearance = firstCustomer.GetComponent<CustomerAppearance>();
            if (appearance.IsDoubleOrder)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }

    // Retrieves the specific item sprites for the first customer's order
    public (Sprite, Sprite) GetOrderForFirstCustomer()
    {
        if (currentCustomerCount > 0)
        {
            Transform firstCustomerPosition = queuePositions[0];
            GameObject firstCustomer = null;
            
            foreach (GameObject customer in _customerIsSpawned)
            {
                if (Vector3.Distance(customer.transform.position, firstCustomerPosition.position) < 0.1f)
                {
                    firstCustomer = customer;
                    break;
                }
            }

            if (firstCustomer != null)
            {
                CustomerAppearance appearance = firstCustomer.GetComponent<CustomerAppearance>();
                
                if(appearance.IsDoubleOrder)
                {
                    return (appearance.FirstOrder, appearance.SecondOrder);
                }
                else
                {
                    return (appearance.Order, null);
                }
            }
        }
        return (null, null);
    }

    public bool GetCustomerIsPoint()
    {
        if (currentCustomerCount > 0)
        {
            Transform firstCustomerPosition = queuePositions[0];

            foreach (GameObject customer in _customerIsSpawned)
            {
                return (Vector3.Distance(customer.transform.position, firstCustomerPosition.position) < 0.1f);
            }
        }
        return false;
    }

    public void RemoveFirstCustomer()
    {
        if (_customerIsSpawned.Count > 0)
        {
            GameObject firstCustomer = _customerIsSpawned[0];
            CustomerAppearance appearance = firstCustomer.GetComponent<CustomerAppearance>();
            if (appearance.IsDoubleOrder)
            {
                GameObject orderImageObject = firstCustomer.transform.GetChild(1).gameObject;
                orderImageObject.SetActive(false); 
            }
            else
            {
                GameObject orderImageObject = firstCustomer.transform.GetChild(0).gameObject;
                orderImageObject.SetActive(false);
            }
            
            StartCoroutine(MoveCustomerOut(firstCustomer));
            _customerIsGo.Add(firstCustomer);
            _customerIsSpawned.RemoveAt(0);
            currentCustomerCount--;

            ShiftQueueForward(); 

            sellingSys.UpdateAvailableItems();
            orderGeneratedForFirstCustomer = false;
            if (tutorialManager.canSpawnOneClient)
            {
                Debug.Log("everyone is sleeping");
                StartSpawning();
            }
        }
    }

    // Moves all customers one step forward when the first one leaves
    public void ShiftQueueForward()
    {
        for (int i = 0; i < _customerIsSpawned.Count; i++)
        {
            Transform nextPosition = queuePositions[i];
            GameObject customer = _customerIsSpawned[i];
            
            Transform hair = customer.transform.Find("hair");
            Transform skin = customer.transform.Find("skin");
            Transform face = customer.transform.Find("face");
            Transform outfit = customer.transform.Find("cloth");
            Transform reaction = customer.transform.Find("reaction");
            skin.GetComponent<SpriteRenderer>().sortingOrder = 10 + i + 2;
            face.GetComponent<SpriteRenderer>().sortingOrder = 11 + i + 2;
            hair.GetComponent<SpriteRenderer>().sortingOrder = 12 + i + 2;
            outfit.GetComponent<SpriteRenderer>().sortingOrder = 11 + i + 2;
            reaction.GetComponent<SpriteRenderer>().sortingOrder = 10 + i + 2;

            StartCoroutine(MoveToPosition(customer.transform, nextPosition));
        }
    }

    IEnumerator MoveCustomerOut(GameObject customer)
    {
        if (customer != null)
        {
            SwitchSide(customer, true);
            yield return StartCoroutine(MoveToPosition(customer.transform, goPosition));
            if (daySystem.isDayOver) yield break;
            yield return StartCoroutine(MoveToPosition(customer.transform, exitPosition));
            customer.transform.DOKill();
            yield return new WaitForSeconds(5f);
            Destroy(customer);
        }
    }

    public void StopSpawning()
    {
        canSpawnCustomers = false;
        StopCoroutine(SpawnCustomers());
    }

    public void ClearQueue()
    {
        StopAllCoroutines();

        foreach (GameObject customer in _customerIsSpawned)
        {
            if (customer != null)
            {
                customer.transform.DOKill();
                Destroy(customer);
            }
        }

        foreach (GameObject customer in _customerIsGo)
        {
            if (customer != null)
            {
                customer.transform.DOKill();
                Destroy(customer);
            }
        }
        _customerIsSpawned.Clear();
        _customerIsGo.Clear();
        currentCustomerCount = 0;
        availablePositions = new Queue<Transform>(queuePositions);
    }
}

public class CustomerAppearance : MonoBehaviour
{
    public int HairIndex { get; set; }
    public int OutfitIndex { get; set; }
    public Sprite Order { get; set; }
    public Sprite FirstOrder { get; set; } 
    public Sprite SecondOrder { get; set; }
    public bool IsDoubleOrder { get; set; } 
    public bool IsOrderTypeGenerated { get; set; }
}
