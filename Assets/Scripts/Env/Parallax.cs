using UnityEngine;
using UnityEngine.Tilemaps;


public class Parallax : MonoBehaviour
{
    // Attrbutes

    [Tooltip("0 = Moves with camera. 1 = Stationary in world. 0.5 = Half speed.")]
    [SerializeField] private float parallaxEffect;
    [Tooltip("Check this if your background image is a seamless, repeating tile.")]
    [SerializeField] private bool isRepeating = true;

    private float startPos;
    private float spriteLength;
    private Transform cam;

    // Methods

    private void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position.x;

        SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
        if (sRenderer != null)
        {
            spriteLength = sRenderer.bounds.size.x;
        }
        else
        {
            Tilemap tilemap = GetComponent<Tilemap>();
            if (tilemap != null)
            {
                tilemap.CompressBounds();
                spriteLength = tilemap.localBounds.size.x;
            }
        }
    }

    private void Update()
    {
        float temp = (cam.position.x * (1 - parallaxEffect));
        float distance = (cam.position.x * parallaxEffect);

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        if (isRepeating && spriteLength > 0)
        {
            if (temp > startPos + spriteLength)
            {
                startPos += spriteLength;
            }
            else if (temp < startPos - spriteLength)
            {
                startPos -= spriteLength;
            }
        }
    }
}
