using UnityEngine;

public class Fairy : MonoBehaviour
{
    public float energyAmount = 5f; // 이 요정이 제공하는 에너지 양

    private Plant plantScript; // 식물 스크립트 참조

    void Start()
    {
        // 게임 시작 시 Plant 스크립트를 찾아 저장 (프로토타입에서는 간단하게 Find 사용)
        plantScript = FindObjectOfType<Plant>();
        if (plantScript == null)
        {
            Debug.LogError("씬에 Plant 오브젝트 또는 스크립트를 찾을 수 없습니다!");
        }
    }

    // 마우스 클릭 또는 터치 시 호출됨 (Collider가 있어야 작동)
    void OnMouseDown()
    {
        Debug.Log("요정 클릭됨!");
        if (plantScript != null)
        {
            // 식물에게 에너지 전달
            plantScript.AbsorbFairyEnergy(energyAmount);

            // 요정 자신을 파괴
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Plant 스크립트 참조가 없습니다!");
        }
    }

     // (선택 사항) 화면 밖으로 나가면 스스로 파괴
    void OnBecameInvisible()
    {
        // Destroy(gameObject);
    }
}