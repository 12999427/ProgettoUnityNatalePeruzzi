using UnityEngine;

public interface I_PlayerInteractions
{
    void RoseOfWindInteraction(Transform player, GameObject roseOfWind);
    void EndLevelInteraction(Transform player, GameObject endLevelTrigger);
    void CollectibleTreasureInteraction(Transform player, GameObject collectible);
    void ArrowBonusInteraction(Transform player, GameObject arrowBonus);
    void TimeBonusInteraction(Transform player, GameObject timeBonus);

}
