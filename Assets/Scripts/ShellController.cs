using UnityEngine;

public class ShellController : MonoBehaviour
{
    // 地面との衝突音
    public AudioClip landing;

    private AudioSource audioSource;

    // 地面と衝突したかどうか
    private bool land = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 0) {
            // 地面の外に落ちたとき、地面との衝突処理をおこないます
            Land();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Plane") {
            // 地面との衝突処理をおこないます
            Land();
        }
        if (collision.gameObject.name.StartsWith("Enemy")) {
            // 敵と衝突したらObjectを破棄します
            Destroy(gameObject);
        }
    }

    // 地面との衝突処理をおこないます
    private void Land()
    {
        if (land) {
            // 既に処理されていれば何もしません
            return;
        }

        land = true;

        // 地面との衝突音を再生します
        audioSource.PlayOneShot(landing, 0.7F);

        // 2秒後にこのObjectを破棄するようにします
        Destroy(gameObject, 2);
    }
}
