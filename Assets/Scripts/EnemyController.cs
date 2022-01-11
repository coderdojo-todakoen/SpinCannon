using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 一定速度でゆっくり回転させるための回転速度
    private float angle;

    // Start is called before the first frame update
    void Start()
    {
        // 回転速度をランダムに決定します
        angle = Random.Range(-10, 10);
    }

    // Update is called once per frame
    void Update()
    {
        // 一定速度で回転します
        Vector3 eulers = new Vector3(0, angle, 0);
        transform.Rotate(eulers * Time.deltaTime);
    }

    // 衝突が検出されたとき呼び出されます
    private void OnCollisionEnter(Collision collision)
    {
        // 砲弾と衝突したかどうかをチェックします
        if (collision.gameObject.name.StartsWith("Shell")) {
            // 爆発音を鳴らします
            GetComponent<AudioSource>().Play();

            // ParticleSystemを再生します
            ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();

            // 非表示にします
            GetComponent<Renderer>().enabled = false;

            // 3秒後にこのObjectを破棄するようにします
            Destroy(gameObject, 3);
        }
    }
}
