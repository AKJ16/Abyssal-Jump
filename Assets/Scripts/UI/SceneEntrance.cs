using UnityEngine;

public class SceneEntrance : MonoBehaviour
{
    // Attributes

    [SerializeField] private string entranceID;
    [SerializeField] private Collider2D startingChunk;

    // Methods

    public string GetEntranceID()
    {
        return entranceID;
    }

    public Collider2D GetStartingChunk() 
    {
        return startingChunk; 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
