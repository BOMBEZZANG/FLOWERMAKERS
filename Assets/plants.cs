using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Plant : MonoBehaviour
{
    [Header("욕구 상태")]
    public float maxMagicDew = 100f;
    public float currentMagicDew;
    public float dewDecreaseRate = 1f;
    public float lowDewThreshold = 30f;

    public float maxFairyEnergy = 50f;
    public float currentFairyEnergy = 0f;

    [Header("성장 관련")]
    public int growthLevel = 0;
    public float growthThreshold = 50f;
    public Vector3 growthScaleFactor = new Vector3(1.05f, 1.05f, 1.05f); // 스케일 팩터 약간 줄임 (권장)

    [Header("시각적 상태 (Sprites)")]
    public Sprite thirstySprite;        // 목마른 상태 스프라이트 (모든 단계 공통)
    // 단계별 스프라이트 추가
    public Sprite seedSprite;           // 레벨 1-3
    public Sprite sproutSprite;         // 레벨 4-6
    public Sprite youngPlantSprite;     // 레벨 7-9
    public Sprite adultPlantSprite;     // 레벨 10-12
    public Sprite flowerSprite;         // 레벨 13-15
    public Sprite fruitSprite;          // 레벨 16-18 (또는 그 이상)

    [Header("UI 연결")]
    public Slider magicDewSlider;
    public Slider fairyEnergySlider;
    public TextMeshProUGUI growthLevelText;

    [Header("요정 스폰 관련")]
    public GameObject fairyPrefab;
    public Transform spawnAreaMin;
    public Transform spawnAreaMax;
    public float initialSpawnDelay = 2.0f;
    public float fairySpawnInterval = 5.0f;
    public int maxActiveFairies = 3;

    // 내부 참조 변수
    private SpriteRenderer spriteRenderer;
    private Sprite currentStageBaseSprite; // 현재 성장 단계의 기본 스프라이트

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Plant 오브젝트에 SpriteRenderer 컴포넌트가 없습니다!");
        }
    }

    void Start()
    {
        // --- 초기화 로직 수정 ---
        // (저장된 데이터 불러오기가 있다면 이 부분은 달라져야 함)
        currentMagicDew = maxMagicDew;
        currentFairyEnergy = 0f;
        // growthLevel = 0; // 보통 저장/로드로 관리되거나 0에서 시작
        growthThreshold = maxFairyEnergy; // 초기 성장 요구량

        // 현재 레벨에 맞는 초기 단계 스프라이트 설정
        UpdateStageSprite(true); // true 플래그로 초기 설정임을 표시하거나, Start에서도 호출되도록 함

        UpdateUI();
        UpdatePlantVisuals(); // 초기 식물 외형 최종 적용

        StartCoroutine(FairySpawningCoroutine());
        Debug.Log("식물 초기화 완료 및 요정 자동 스폰 시작!");
    }

    void Update()
    {
        DecreaseDewOverTime();
        UpdatePlantVisuals(); // 목마름 상태 등 시각적 업데이트는 계속 확인
        UpdateUI();
    }

    void DecreaseDewOverTime()
    {
        if (currentMagicDew > 0)
        {
            currentMagicDew -= dewDecreaseRate * Time.deltaTime;
            currentMagicDew = Mathf.Clamp(currentMagicDew, 0, maxMagicDew);
        }
    }

    public void GiveMagicDew(float amount)
    {
        currentMagicDew += amount;
        currentMagicDew = Mathf.Clamp(currentMagicDew, 0, maxMagicDew);
        Debug.Log($"마법의 이슬 {amount} 주기! 현재 이슬: {currentMagicDew}");
        UpdatePlantVisuals(); // 이슬 상태 바뀌었으니 즉시 외형 업데이트
    }

    public void AbsorbFairyEnergy(float amount)
    {
        // 최대 레벨 제한 (예: 18)
        if (growthLevel >= 18) { Debug.Log("최대 레벨!"); return; }
        currentFairyEnergy += amount;
        Debug.Log($"요정 에너지 {amount} 흡수! 현재 에너지: {currentFairyEnergy}");
        CheckForGrowth();
    }

    void CheckForGrowth()
    {
        // 에너지가 충족되면 계속 레벨업 시도
        while (currentFairyEnergy >= growthThreshold)
        {
            // 최대 레벨 제한 (예: 18)
             if (growthLevel >= 18)
             {
                 currentFairyEnergy = growthThreshold; // 에너지가 더 이상 쌓이지 않도록 최대치로 고정 (선택적)
                 break;
             }

            currentFairyEnergy -= growthThreshold;
            growthLevel++; // 레벨 증가!
            transform.localScale = Vector3.Scale(transform.localScale, growthScaleFactor); // 크기 증가
            growthThreshold *= 1.2f; // 다음 필요 에너지 증가 (조절 가능)

            // --- 레벨 증가 후 단계별 스프라이트 업데이트 ---
            UpdateStageSprite();
            // --- 업데이트 로직 호출 ---

            Debug.Log($"성장! 레벨 {growthLevel} 달성! 다음 필요 에너지: {growthThreshold}");
        }
        currentFairyEnergy = Mathf.Clamp(currentFairyEnergy, 0, growthThreshold);
        UpdateUI();
        UpdatePlantVisuals(); // 레벨업으로 외형이 바뀔 수 있으니 최종 업데이트
    }

    // 현재 레벨에 맞는 단계별 기본 스프라이트 설정 함수 (새로 추가/수정)
    void UpdateStageSprite(bool isInitial = false)
    {
        Sprite previousStageSprite = currentStageBaseSprite; // 변경 확인용

        // 레벨에 따라 맞는 스프라이트를 currentStageBaseSprite에 할당
        if (growthLevel <= 0) // 레벨 0 (또는 시작 시점)
        {
            currentStageBaseSprite = seedSprite;
        }
        else if (growthLevel >= 1 && growthLevel <= 3)
        {
            currentStageBaseSprite = seedSprite;
        }
        else if (growthLevel >= 4 && growthLevel <= 6)
        {
            currentStageBaseSprite = sproutSprite;
        }
        else if (growthLevel >= 7 && growthLevel <= 9)
        {
            currentStageBaseSprite = youngPlantSprite;
        }
        else if (growthLevel >= 10 && growthLevel <= 12)
        {
            currentStageBaseSprite = adultPlantSprite;
        }
        else if (growthLevel >= 13 && growthLevel <= 15)
        {
            currentStageBaseSprite = flowerSprite;
        }
        else // 레벨 16 이상
        {
            currentStageBaseSprite = fruitSprite;
        }

        // (선택적) 단계가 실제로 변경되었을 때만 로그 출력 또는 효과 발생
        if (!isInitial && previousStageSprite != currentStageBaseSprite)
        {
            Debug.Log($"식물 단계 변경! 레벨 {growthLevel} -> {currentStageBaseSprite?.name}");
            // 여기에 단계 변경 시 파티클 효과 등을 추가할 수 있음
        }
    }

    IEnumerator FairySpawningCoroutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        while (true)
        {
            GameObject[] currentFairies = GameObject.FindGameObjectsWithTag("Fairy");
            if (currentFairies.Length < maxActiveFairies)
            {
                if (fairyPrefab != null && spawnAreaMin != null && spawnAreaMax != null)
                {
                    float spawnX = Random.Range(spawnAreaMin.position.x, spawnAreaMax.position.x);
                    float spawnY = Random.Range(spawnAreaMin.position.y, spawnAreaMax.position.y);
                    Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
                    GameObject fairyInstance = Instantiate(fairyPrefab, spawnPosition, Quaternion.identity);
                    Fairy fairyScript = fairyInstance.GetComponent<Fairy>();
                    if (fairyScript != null) { fairyScript.InitializeMovement(this, spawnAreaMin, spawnAreaMax); }
                }
            }
            yield return new WaitForSeconds(fairySpawnInterval);
        }
    }

    // 식물의 최종 시각적 상태 업데이트 함수 (수정)
    void UpdatePlantVisuals()
    {
        if (spriteRenderer == null) return;

        // 목마른 상태 확인
        if (currentMagicDew < lowDewThreshold)
        {
            // 목마른 스프라이트가 있고, 현재 스프라이트가 목마른 스프라이트가 아니면 변경
            if (thirstySprite != null && spriteRenderer.sprite != thirstySprite)
            {
                spriteRenderer.sprite = thirstySprite;
            }
        }
        else // 목마르지 않은 상태
        {
            // 현재 단계의 기본 스프라이트가 있고, 현재 스프라이트가 그 기본 스프라이트가 아니면 변경
            if (currentStageBaseSprite != null && spriteRenderer.sprite != currentStageBaseSprite)
            {
                spriteRenderer.sprite = currentStageBaseSprite;
            }
            // 만약 currentStageBaseSprite가 null인데 기본 모습(예: normalSprite)을 보여줘야 한다면 추가 로직 필요
            else if (currentStageBaseSprite == null && spriteRenderer.sprite != seedSprite) // 예: 초기 상태 대비
            {
                 spriteRenderer.sprite = seedSprite; // 또는 다른 기본값
            }
        }
    }

    // UI 요소 업데이트
    void UpdateUI()
    {
        if (magicDewSlider != null) { magicDewSlider.value = currentMagicDew / maxMagicDew; }
        if (fairyEnergySlider != null) { fairyEnergySlider.value = currentFairyEnergy / growthThreshold; }
        if (growthLevelText != null) { growthLevelText.text = $"Lv: {growthLevel}"; }
    }
}