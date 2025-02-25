using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManagementScript : MonoBehaviour
{

    /// <summary>
    /// インスペクター側から適当なオブジェクト/コンポーネントを参照する変数
    /// </summary>
    [Header("ASSIGN OBJECTS TO INSPECTOR")]
    [Tooltip("Assign RightControllerPrefab in OVRCameraRig"), SerializeField] private GameObject _rightController;
    [Tooltip("Assign RayObject's LineRenderer in RightControllerAnchor"), SerializeField] private LineRenderer _lineRender;
    [Tooltip("Assign ScoreText in Stage"), SerializeField] private TextMeshPro _scoreText;
    [Tooltip("Assign Pause in PausePanels"), SerializeField] private GameObject _pauseCanvas;
    [Tooltip("Assign UIHelper in PausePanels"), SerializeField] private GameObject _uiHelpers;
    [Tooltip("Assign RayObject in RightControllerAnchor"), SerializeField] private GameObject _razerPointer;
    [Tooltip("Assign ResumeButton Animater in PausePanels"), SerializeField] private Animator _buttonAnimator;

    /// <summary>
    /// ゲームシステム関連の変数
    /// </summary>
    [Space(1)]
    [Header("SYSTEM SETTINGS")]
    [SerializeField, Range(0.01f, 1f)] private float _vibrationDuration = 0.01f;//コントローラー振動時間
    [SerializeField, Range(0.1f, 1f)] private float _vibrationFrequency = 0.3f;//コントローラー振動周波数
    [SerializeField, Range(0.1f, 1f)] private float _vibrationAmplitude = 0.3f;//コントローラー振動強度
    [SerializeField] private int _scorePerLine = 100;//1列削除時のスコア
    [SerializeField] private int _clearScore = 99999;//ゲームクリアののスコア
    

    /// <summary>
    /// その他変数
    /// </summary>
    private bool _wasHittingMino = false; // 前のフレームでMinoにヒットしていたかを追跡
    private int _currentScore;//現在のスコア
    private string _currentSceneName; // 現在のシーン名

    void Start()
    {
        Initialize();
        _currentSceneName = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        // レーザーの設定
        SetupLazerPointer();
        // レーザーとMinoオブジェクトの接触判定
        CheckLaserCollisionWithMino();

        //ミノをつかむ処理
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            GrabMinoWithLaser();
        }
        //ミノを離す処理
        if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
        {
            ReleaseMino();
        }
        //ポーズ処理
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
        {
            SpawnPauseMenu();
        }


    }

    /// <summary>
    /// 初期値化メソッド
    /// </summary>
    private void Initialize()
    {
        _currentScore = 0;
        OVRManager.display.displayFrequency = 90f;
    }
    /// <summary>
    /// レーザーポインターを設定するメソッド
    /// </summary>
    private void SetupLazerPointer()
    {
        _lineRender.positionCount = 2;
        _lineRender.SetPosition(0, _rightController.transform.position);
        _lineRender.SetPosition(1, _rightController.transform.position + _rightController.transform.forward * 100.0f);
        _lineRender.startWidth = 0.01f;
        _lineRender.endWidth = 0.01f;
    }

    /// <summary>
    /// 落下中のミノとレーザーポインターが触れたときのコントローラーの振動処理を行うメソッド
    /// </summary>
    /// <remarks>
    /// RaycastHit[]を用いて、右手コントローラーの直線上に位置しているコライダー情報を持つオブジェクトを取得する。
    /// 取得したオブジェクトのタグが"Cube"であり、かつ直前のフレームでミノとの衝突判定（isHittingMino）がFalseの場合コントローラーを振動させる。
    /// </remarks>
    private void CheckLaserCollisionWithMino()
    {
        bool isHittingMino = false;
        RaycastHit[] hits = Physics.RaycastAll(_rightController.transform.position, _rightController.transform.forward, 100.0f);
        foreach (var hit in hits)
        {
            if (hit.collider.tag == "Cube")
            {
                isHittingMino = true;
                if (!_wasHittingMino)
                {
                    if(!_pauseCanvas.activeSelf)
                    StartCoroutine(VibrateRightController());
                }
                break;
            }
        }
        _wasHittingMino = isHittingMino;
    }

    /// <summary>
    /// ミノを掴む処理を行うメソッド
    /// </summary>
    /// <remarks>
    /// RaycastHit[]を用いて、右手コントローラーの直線上に位置しているコライダー情報を持つオブジェクトを取得する。
    /// 取得したオブジェクトのタグが"Cube"の場合、そのオブジェクトの親オブジェクトを右手のコントローラーに設定する。
    /// <see cref="MinoControllScript"/>でミノの位置座標と回転座標を一部制限しているため、ミノの移動範囲がフィールド上に制限されている。
    /// </remarks>
    private void GrabMinoWithLaser()
    {
        RaycastHit[] hits = Physics.RaycastAll(_rightController.transform.position, _rightController.transform.forward, 100.0f);

        foreach (var hit in hits)
        {
            if (hit.collider.tag == "Cube")
            {
                hit.collider.transform.parent = _rightController.transform;
                hit.collider.GetComponent<Rigidbody>().isKinematic = true; // 掴んでいる間物理演算を無効にする
                break;
            }
        }
    }
    /// <summary>
    /// 掴んだミノを離す処理を行うメソッド
    /// </summary>
    /// <remarks>
    /// RaycastHit[]を用いて、右手コントローラーの直線上に位置しているコライダー情報を持つオブジェクトを取得する。
    /// 取得したオブジェクトのタグが"Cube"の場合、そのオブジェクトの親オブジェクトを解除し、Mathf.Round()を用いてフィールドのグリッドにスナップする。
    /// </remarks>
    private void ReleaseMino()
    {
        for (int i = 0; i < _rightController.transform.childCount; i++)
        {
            var child = _rightController.transform.GetChild(i);
            if (child.tag == "Cube")
            {
                child.parent = null;
                child.position = new Vector3(Mathf.Round(child.position.x), Mathf.Round(child.position.y), 0);
                child.GetComponent<Rigidbody>().isKinematic = false; // 離した後物理演算を有効にする
            }
        }
    }

    /// <summary>
    /// ポーズ画面を出現させ、ゲームをポーズさせる処理を行うメソッド
    /// </summary>
    private void SpawnPauseMenu()
    {
        Time.timeScale = 0;
        _razerPointer.SetActive(false);
        _uiHelpers.SetActive(true);
        _pauseCanvas.SetActive(true);
    }

    /// <summary>
    /// ポーズからゲームを再開する処理を行うメソッド
    /// </summary>
    public void ResumGame()
    {
        // ポーズ画面を閉じる処理の中で
        _buttonAnimator.Rebind();
        _buttonAnimator.Update(0f); // ボタンアニメーションを初期状態に戻す
        Time.timeScale = 1f;
        _pauseCanvas.SetActive(false);
        _uiHelpers.SetActive(false);
        _razerPointer.SetActive(true);
    }


    /// <summary>
    /// コントローラーを振動させるコルーチン
    /// </summary>
    IEnumerator VibrateRightController()
    {
        OVRInput.SetControllerVibration(_vibrationFrequency, _vibrationAmplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(_vibrationDuration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    //タイトル画面に戻る関数
    public void ToTitleScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void AddScore()
    {
        _currentScore += _scorePerLine;
        _scoreText.text = _currentScore.ToString();
        if(_currentScore >= _clearScore)
        {
            // GameClear();
        }
    }

    /// <summary>
    /// ゲームクリアとゲームオーバー画面を表示させるための関数。
    /// </summary>
    // public void GameOver()
    // {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    // }

    // private void GameClear()
    // {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    // }


}
