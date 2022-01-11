using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CannonController : MonoBehaviour
{
#if UNITY_STANDALONE_OSX
    [DllImport ("MicrobitPlugin", EntryPoint = "connectMicrobit")]
    private static extern void ConnectMicrobit();
    [DllImport ("MicrobitPlugin", EntryPoint = "disconnectMicrobit")]
    private static extern void DisconnectMicrobit();
    [DllImport ("MicrobitPlugin", EntryPoint = "getAccelerometerDataX")]
    private static extern Int16 GetAccelerometerDataX();
    [DllImport ("MicrobitPlugin", EntryPoint = "getAccelerometerDataY")]
    private static extern Int16 GetAccelerometerDataY();
    [DllImport ("MicrobitPlugin", EntryPoint = "getButtonAState")]
    private static extern byte GetButtonAState();
    [DllImport ("MicrobitPlugin", EntryPoint = "getButtonBState")]
    private static extern byte GetButtonBState();

    public delegate void DebugLogDelegate(string str);
    DebugLogDelegate debugLogFunc = msg => Debug.Log(msg);

    [DllImport ("MicrobitPlugin")]
    public static extern void set_debug_log_func(DebugLogDelegate func);
#endif
#if UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern void ConnectMicrobit();
    [DllImport ("__Internal")]
    private static extern void DisconnectMicrobit();
    [DllImport ("__Internal")]
    private static extern Int16 GetAccelerometerDataX();
    [DllImport ("__Internal")]
    private static extern Int16 GetAccelerometerDataY();
    [DllImport ("__Internal")]
    private static extern byte GetButtonAState();
    [DllImport ("__Internal")]
    private static extern byte GetButtonBState();
#endif

    // micro:bitとの接続状態を保存します
    private bool connected = false;

    // micro:bitのBボタンの状態を保存します
    // ボタンの状態が OFF → ON になったことを検出するために
    // 使用します
    private byte prevBState = 0;

    // 砲身のGameObject
    private GameObject barrelParent;
    private GameObject barrel;

    // 砲弾のGameObject
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_STANDALONE_OSX
        set_debug_log_func(debugLogFunc);
#endif

        // 砲身のGameObjectを取得します
        barrelParent = GameObject.Find("BarrelParent");
        barrel = GameObject.Find("Barrel");
    }

    // Update is called once per frame
    void Update()
    {
        // 大砲を一定速度で回転します
        Vector3 eulers = new Vector3(0, 90, 0);
        transform.Rotate(eulers * Time.deltaTime);

        if (connected)
        {
            // micro:bitの左右方向の傾きで大砲を左右に移動します
            Vector3 translation = new Vector3(GetAccelerometerDataX() / 100, 0, 0);
            transform.Translate(translation * Time.deltaTime, Space.World);

            // micro:bitの上下方向の傾きで砲身を上下します
            // micro:bitから取得した傾きを 0 から 1000 の範囲におさめます
            float clamp = Mathf.Clamp(GetAccelerometerDataY(), 0, 1000);
            // 0 から 1000 の値を、75度から5度の範囲に変換して、砲身の傾きとします
            float angle = 75 - 70 * clamp / 1000;
            Vector3 rotation = barrelParent.transform.rotation.eulerAngles;
            rotation.x = angle;
            barrelParent.transform.rotation = Quaternion.Euler(rotation);

            // micro:bitのBボタンの状態を取得します
            byte buttonBState = GetButtonBState();
            if ((prevBState == 0) && (buttonBState > 0)) {
                // micro:bitのBボタンが離された状態→押された状態に変化したら
                // 砲弾を発射します
                FireShell();
            }
            // micro:bitのBボタンの状態を保存します
            prevBState = buttonBState;
        }
        else
        {
            // 左右のキー操作で大砲を左右に移動します
            float x = 0;
            if (Input.GetKey(KeyCode.RightArrow))
            {
                x = 10;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                x = -10;
            }
            Vector3 translation = new Vector3(x, 0, 0);
            transform.Translate(translation * Time.deltaTime, Space.World);

            // 上下のキー操作で砲身を上下します
            float angle = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                angle = -30;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                angle = 30;
            }
            // 砲身が上下する角度を5度から75度に制限します
            Vector3 rotation = barrelParent.transform.rotation.eulerAngles;
            rotation.x = Mathf.Clamp(rotation.x + angle * Time.deltaTime, 5, 75);
            barrelParent.transform.rotation = Quaternion.Euler(rotation);
            
            // キーボードの'B'が押されたら、砲弾を発射します
            if (Input.GetKeyDown(KeyCode.B)) {
                FireShell();
            }
        }
    }

    // 砲弾を発射します
    private void FireShell()
    {
        GameObject shell = (GameObject) Instantiate(prefab);

        // 砲弾の位置とサイズを、砲身の位置とサイズに合わせます
        Vector3 position = barrelParent.transform.TransformPoint(shell.transform.position);
        shell.transform.position = position;

        shell.transform.localScale = barrelParent.transform.localScale;

        // 砲弾を砲身の方向に発射します
        shell.GetComponent<Rigidbody>().AddForce(barrel.transform.up * 20, ForceMode.VelocityChange);
    }

    // micro:bitと接続します
    public void OnConnect()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        ConnectMicrobit();
        connected = true;
#endif
    }

    // micro:bitとの接続を切断します
    public void OnDisconnect()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        DisconnectMicrobit();
        connected = false;
#endif

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnApplicationQuit()
    {
        if (connected) {
#if !UNITY_EDITOR && UNITY_WEBGL
            DisconnectMicrobit();
#endif
        }
    }
}
