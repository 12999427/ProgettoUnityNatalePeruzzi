using UnityEngine;

public class MazeTileBehavior : MonoBehaviour
{
    [SerializeField] private GameObject[] Walls;
    [SerializeField] private GameObject VisitedBlock;

    public void Visit()
    {
        VisitedBlock.SetActive(false);
    }

    public void SetWallActive (int wallNum, bool active)
    {
        Walls[wallNum].SetActive(active);
    }

}
