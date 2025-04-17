using UnityEngine;

public class Fairy : MonoBehaviour
{
    [Header("요정 설정")]
    public float energyAmount = 5f;

    [Header("이동 설정")]
    public float moveSpeed = 2.0f;
    public float targetChangeDistance = 0.5f;

    [Header("효과 설정")] // 효과 관련 변수 추가
    public GameObject fairyPopEffectPrefab; // 클릭 시 생성될 파티클 효과 프리팹
    public AudioClip popSound;             // 클릭 시 재생될 효과음

    // 내부 변수
    private Plant plantScript;
    private Vector3 currentTargetPosition;
    private Transform minBounds;
    private Transform maxBounds;
    private bool isInitialized = false;

    // 스폰 시 Plant 스크립트에서 호출하여 이동 범위 설정 및 초기화
    public void InitializeMovement(Plant plant, Transform min, Transform max)
    {
        plantScript = plant;
        minBounds = min;
        maxBounds = max;

        if (minBounds == null || maxBounds == null)
        {
            Debug.LogError("Fairy 이동 경계가 설정되지 않았습니다!");
            PickNewTargetPosition(true);
        }
        else
        {
            PickNewTargetPosition();
        }
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || currentTargetPosition == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTargetPosition);

        if (distanceToTarget < targetChangeDistance)
        {
            PickNewTargetPosition();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, moveSpeed * Time.deltaTime);

            // 이동 방향으로 좌우 반전
            if (currentTargetPosition.x < transform.position.x)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    // 새로운 랜덤 목표 지점 설정 함수
    void PickNewTargetPosition(bool nearby = false)
    {
        if (!nearby && minBounds != null && maxBounds != null)
        {
            float randomX = Random.Range(minBounds.position.x, maxBounds.position.x);
            float randomY = Random.Range(minBounds.position.y, maxBounds.position.y);
            currentTargetPosition = new Vector3(randomX, randomY, 0);
        }
        else
        {
            float randomX = transform.position.x + Random.Range(-1f, 1f);
            float randomY = transform.position.y + Random.Range(-1f, 1f);
            currentTargetPosition = new Vector3(randomX, randomY, 0);
        }
    }

    // 마우스 클릭 또는 터치 시 호출됨
    [System.Obsolete]
    void OnMouseDown()
    {
        // --- 추가된 효과 재생 로직 ---
        // 효과음 재생 (오디오 클립이 할당되었을 경우)
        if (popSound != null)
        {
            // 현재 위치에서 효과음 재생 (소리가 끊기지 않음)
            AudioSource.PlayClipAtPoint(popSound, transform.position);
        }

        // 파티클 효과 생성 (프리팹이 할당되었을 경우)
        if (fairyPopEffectPrefab != null)
        {
            // 현재 위치에 파티클 효과 프리팹 인스턴스 생성
            Instantiate(fairyPopEffectPrefab, transform.position, Quaternion.identity);
        }
        // --- 효과 재생 로직 끝 ---


        // 식물에게 에너지 전달 및 자신 파괴 (기존 로직)
        if (plantScript != null)
        {
            plantScript.AbsorbFairyEnergy(energyAmount);
            Destroy(gameObject); // 효과 재생 후 파괴
        }
        else
        {
            // Plant 스크립트 참조가 없는 경우 대비
            Plant foundPlant = FindObjectOfType<Plant>();
            if (foundPlant != null)
            {
                foundPlant.AbsorbFairyEnergy(energyAmount);
                Destroy(gameObject); // 효과 재생 후 파괴
            }
            else
            {
                Debug.LogError("Plant 스크립트 참조를 찾을 수 없습니다!");
                Destroy(gameObject); // 효과 재생 후 파괴
            }
        }
    }
}