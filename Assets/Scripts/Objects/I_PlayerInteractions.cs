using UnityEngine;

public interface I_PlayerInteractions
{
    void HideWallInteraction(GameObject hideWall);
    void EndLevelInteraction(GameObject endLevelTrigger);
    void CollectibleTreasureInteraction(GameObject collectible);
    void ArrowBonusInteraction(GameObject arrowBonus);
    void TimeBonusInteraction(GameObject timeBonus);

}
