using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    // 敵として生成するGameObject
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        // 敵となるGameObjectを生成します
        for (int x = -15; x <= 15; ++x)
        {
            Vector3 position = new Vector3(x, Random.Range(2F, 10F), Random.Range(8F, 12F));
            GameObject enemy = (GameObject) Instantiate(prefab, position, Quaternion.identity, transform);
        }
    }
}
