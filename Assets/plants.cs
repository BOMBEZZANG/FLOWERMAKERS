using UnityEngine;
using UnityEngine.UI; // Slider 등 기본 UI 요소 사용
using TMPro;          // TextMeshPro UI 요소 사용을 위해 추가

public class Plant : MonoBehaviour
{
    [Header("욕구 상태")]
    public float maxMagicDew = 100f;        // 최대 마법의 이슬
    public float currentMagicDew;           // 현재 마법의 이슬
    public float dewDecreaseRate = 1f;      // 초당 이슬 감소량

    public float maxFairyEnergy = 50f;     // 성장에 필요한 최대 요정 에너지 (성장 후 증가 가능)
    public float currentFairyEnergy = 0f;   // 현재 요정 에너지

    [Header("성장 관련")]
    public int growthLevel = 0;             // 현재 성장 레벨
    public float growthThreshold = 50f;     // 다음 레벨로 성장하기 위한 에너지 요구량
    public Vector3 growthScaleFactor = new Vector3(1.2f, 1.2f, 1.2f); // 성장 시 커질 크기 배율

    [Header("UI 연결")]
    public Slider magicDewSlider;           // 마법의 이슬 상태를 표시할 슬라이더
    public Slider fairyEnergySlider;        // 요정 에너지 상태를 표시할 슬라이더
    public TextMeshProUGUI growthLevelText; // 성장 레벨을 표시할 TextMeshPro 텍스트 (UGUI)

    [Header("요정 스폰 관련")]
    public GameObject fairyPrefab;          // 연결할 요정 프리팹
    public Transform spawnAreaMin;          // 스폰 영역 최소 좌표 (빈 오브젝트로 위치 지정)
    public Transform spawnAreaMax;          // 스폰 영역 최대 좌표 (빈 오브젝트로 위치 지정)

    void Start()
    {
        // 게임 시작 시 초기화
        currentMagicDew = maxMagicDew;
        currentFairyEnergy = 0f;
        growthThreshold = maxFairyEnergy; // 초기 성장 요구량 설정 (최대 에너지량과 동일하게 시작)
        UpdateUI(); // 초기 UI 업데이트
        Debug.Log("식물 초기화 완료!");
    }

    void Update()
    {
        // 매 프레임 실행
        DecreaseDewOverTime(); // 시간이 지남에 따라 이슬 감소
        UpdateUI(); // 매 프레임 UI 업데이트 (필요에 따라 성능 최적화 가능)
    }

    // 시간이 지남에 따라 마법의 이슬 감소
    void DecreaseDewOverTime()
    {
        if (currentMagicDew > 0)
        {
            currentMagicDew -= dewDecreaseRate * Time.deltaTime; // Time.deltaTime을 곱해 프레임 독립적인 초당 감소량으로 만듦
            currentMagicDew = Mathf.Clamp(currentMagicDew, 0, maxMagicDew); // 0과 최대값 사이로 제한
        }
        // else // 이슬이 0일 때 처리 (프로토타입 이후 추가)
        // {
        //     Debug.Log("마법의 이슬이 부족해요!");
        // }
    }

    // '마법의 이슬 주기' 버튼에 연결될 함수 (버튼 Inspector에서 호출 시 amount 설정)
    public void GiveMagicDew(float amount)
    {
        currentMagicDew += amount;
        currentMagicDew = Mathf.Clamp(currentMagicDew, 0, maxMagicDew); // 최대값 제한
        Debug.Log($"마법의 이슬 {amount} 주기! 현재 이슬: {currentMagicDew}");
        // UpdateUI(); // 필요 시 즉시 업데이트 (Update에서 이미 처리 중)
    }

    // 요정에게서 에너지 흡수 (Fairy 스크립트에서 호출될 함수)
    public void AbsorbFairyEnergy(float amount)
    {
        if (growthLevel >= 10) // 예시: 최대 레벨 도달 시 에너지 흡수 중단
        {
            Debug.Log("최대 레벨에 도달했습니다!");
            return;
        }

        currentFairyEnergy += amount;
        // 최대 에너지량을 초과해도 일단 누적하도록 둘 수 있음 (CheckForGrowth에서 처리)
        // currentFairyEnergy = Mathf.Clamp(currentFairyEnergy, 0, maxFairyEnergy); // 필요 시 최대값 제한
        Debug.Log($"요정 에너지 {amount} 흡수! 현재 에너지: {currentFairyEnergy}");
        CheckForGrowth(); // 에너지 흡수 후 성장 확인
        // UpdateUI(); // 필요 시 즉시 업데이트 (Update에서 이미 처리 중)
    }

    // 성장 조건 확인 및 성장 처리
    void CheckForGrowth()
    {
        // 에너지가 성장 요구량 이상이면 계속 성장 시도 (레벨업 후 남은 에너지로 바로 다음 레벨 가능)
        while (currentFairyEnergy >= growthThreshold)
        {
            if (growthLevel >= 10) break; // 예시: 최대 레벨 제한

            // 성장 조건 충족
            currentFairyEnergy -= growthThreshold; // 성장 사용분 차감
            growthLevel++;
            transform.localScale = Vector3.Scale(transform.localScale, growthScaleFactor); // 크기 키우기

            // 다음 레벨을 위한 요구량 증가 (예시: 레벨마다 1.2배씩 증가)
            growthThreshold *= 1.2f;
            // maxFairyEnergy = growthThreshold; // 에너지 바의 최대치를 성장 요구량과 동기화 시킬 경우

            Debug.Log($"성장! 레벨 {growthLevel} 달성! 다음 필요 에너지: {growthThreshold}");
            // 성장 이펙트 (파티클 등) - 프로토타입 이후 추가
        }
        // 성장 후 남은 에너지가 있을 수 있으므로, 에너지 바 최대값을 초과하지 않도록 제한
        currentFairyEnergy = Mathf.Clamp(currentFairyEnergy, 0, growthThreshold); // 현재 레벨의 최대치(다음 성장 요구량)로 제한
        UpdateUI(); // 성장 후 UI 즉시 업데이트
    }

    // '요정 생성' 버튼에 연결될 함수
    public void SpawnFairy()
    {
        if (fairyPrefab == null)
        {
            Debug.LogError("Fairy Prefab이 연결되지 않았습니다!");
            return;
        }
        if (spawnAreaMin == null || spawnAreaMax == null)
        {
             Debug.LogError("스폰 영역(Spawn Area Min/Max)이 연결되지 않았습니다!");
            return;
        }

        // 스폰 영역 내 랜덤 위치 계산
        float spawnX = Random.Range(spawnAreaMin.position.x, spawnAreaMax.position.x);
        float spawnY = Random.Range(spawnAreaMin.position.y, spawnAreaMax.position.y);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0); // 2D이므로 z는 0

        // 요정 인스턴스 생성
        Instantiate(fairyPrefab, spawnPosition, Quaternion.identity); // Quaternion.identity는 회전 없음
        Debug.Log("요정 생성!");
    }


    // UI 요소 업데이트
    void UpdateUI()
    {
        if (magicDewSlider != null)
        {
            // 슬라이더 값은 0과 1 사이여야 하므로 현재값을 최대값으로 나눔
            magicDewSlider.value = currentMagicDew / maxMagicDew;
        }
        if (fairyEnergySlider != null)
        {
            // 현재 에너지 / 현재 레벨의 성장 요구량으로 표시 (진척도)
             fairyEnergySlider.value = currentFairyEnergy / growthThreshold;
        }
        if (growthLevelText != null)
        {
            growthLevelText.text = $"Lv: {growthLevel}";
        }
    }
}