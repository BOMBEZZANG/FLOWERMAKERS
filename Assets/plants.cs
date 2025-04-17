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
    public Vector3 growthScaleFactor = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("시각적 상태 (Sprites)")]
    public Sprite normalSprite;
    public Sprite thirstySprite;

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
    public int maxActiveFairies = 3; // 최대 활성 요정 수 (새로 추가)

    // 내부 참조 변수
    private SpriteRenderer spriteRenderer;

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
        currentMagicDew = maxMagicDew;
        currentFairyEnergy = 0f;
        growthThreshold = maxFairyEnergy;

        UpdateUI();
        UpdatePlantVisuals();

        StartCoroutine(FairySpawningCoroutine());
        Debug.Log("식물 초기화 완료 및 요정 자동 스폰 시작!");
    }

    void Update()
    {
        DecreaseDewOverTime();
        UpdatePlantVisuals();
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
        UpdatePlantVisuals();
    }

    public void AbsorbFairyEnergy(float amount)
    {
        if (growthLevel >= 10) { Debug.Log("최대 레벨!"); return; }
        currentFairyEnergy += amount;
        Debug.Log($"요정 에너지 {amount} 흡수! 현재 에너지: {currentFairyEnergy}");
        CheckForGrowth();
    }

    void CheckForGrowth()
    {
        while (currentFairyEnergy >= growthThreshold)
        {
            if (growthLevel >= 10) break;
            currentFairyEnergy -= growthThreshold;
            growthLevel++;
            transform.localScale = Vector3.Scale(transform.localScale, growthScaleFactor);
            growthThreshold *= 1.2f;
            Debug.Log($"성장! 레벨 {growthLevel} 달성! 다음 필요 에너지: {growthThreshold}");
        }
        currentFairyEnergy = Mathf.Clamp(currentFairyEnergy, 0, growthThreshold);
        UpdateUI();
    }

    // 요정 자동 스폰 코루틴 수정
    IEnumerator FairySpawningCoroutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            // --- 스폰 전 현재 요정 수 확인 로직 추가 ---
            // "Fairy" 태그를 가진 모든 활성 게임 오브젝트를 찾습니다.
            GameObject[] currentFairies = GameObject.FindGameObjectsWithTag("Fairy");
            int currentFairyCount = currentFairies.Length;

            // 현재 요정 수가 최대치보다 적을 경우에만 스폰 로직 진행
            if (currentFairyCount < maxActiveFairies)
            {
                // Debug.Log($"현재 요정 수: {currentFairyCount}, 스폰 진행..."); // 필요시 로그 활성화

                // --- 기존 스폰 로직 시작 ---
                if (fairyPrefab == null || spawnAreaMin == null || spawnAreaMax == null)
                {
                    Debug.LogError("스폰 설정 오류!");
                    // yield break; // 설정 오류 시 코루틴 중단보다는 다음 시도까지 기다리는 것이 나을 수 있음
                }
                else
                {
                    float spawnX = Random.Range(spawnAreaMin.position.x, spawnAreaMax.position.x);
                    float spawnY = Random.Range(spawnAreaMin.position.y, spawnAreaMax.position.y);
                    Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

                    GameObject fairyInstance = Instantiate(fairyPrefab, spawnPosition, Quaternion.identity);

                    Fairy fairyScript = fairyInstance.GetComponent<Fairy>();
                    if (fairyScript != null)
                    {
                        fairyScript.InitializeMovement(this, spawnAreaMin, spawnAreaMax);
                    }
                    else
                    {
                        Debug.LogError("생성된 요정 인스턴스에 Fairy 스크립트가 없습니다!");
                    }
                }
                // --- 기존 스폰 로직 끝 ---
            }
            // else // 요정 수가 최대치 이상이면 로그 출력 (필요시)
            // {
            //     Debug.Log($"현재 요정 수: {currentFairyCount}, 최대치({maxActiveFairies}) 도달. 스폰 건너뜀.");
            // }

            // 다음 스폰 시도까지 대기
            yield return new WaitForSeconds(fairySpawnInterval);
        }
    }

    void UpdatePlantVisuals()
    {
        if (spriteRenderer == null) return;

        if (currentMagicDew < lowDewThreshold)
        {
            if (thirstySprite != null && spriteRenderer.sprite != thirstySprite)
            {
                spriteRenderer.sprite = thirstySprite;
            }
        }
        else
        {
            if (normalSprite != null && spriteRenderer.sprite != normalSprite)
            {
                spriteRenderer.sprite = normalSprite;
            }
        }
    }

    void UpdateUI()
    {
        if (magicDewSlider != null) { magicDewSlider.value = currentMagicDew / maxMagicDew; }
        if (fairyEnergySlider != null) { fairyEnergySlider.value = currentFairyEnergy / growthThreshold; }
        if (growthLevelText != null) { growthLevelText.text = $"Lv: {growthLevel}"; }
    }
}