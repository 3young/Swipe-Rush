using UnityEngine;

/// <summary>
/// 레벨 데이터를 정의하는 ScriptableObject 클래스
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "SwipeRush/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;         // 레벨 이름
    public int width = 9;            // 보드 가로 크기
    public int height = 5;           // 보드 세로 크기
    public Gem[] availableGems;      // 레벨에서 사용 가능한 보석 타입 배열

    [Header("Stone Block Settings")]
    public int maxStoneBlocks = 5;       // 최대 스톤 블록 개수
    public float stoneSpawnChance = 0.05f; // 스톤 블록 생성 확률
}
