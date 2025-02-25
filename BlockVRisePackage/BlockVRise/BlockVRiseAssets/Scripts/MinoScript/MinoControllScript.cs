using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MinoControllScript : MonoBehaviour
{
    /// <summary>
    /// インスペクター側から適当なオブジェクトを参照する変数
    /// </summary>
    [Header("ASSIGN OBJECTS TO INSPECTOR")]
    [Tooltip("Assign MinoLimmitTimer at Prefab"), SerializeField] private GameObject _timerTextPrefab;
    [Tooltip("Assign DestructionEffect at Effect"), SerializeField] private GameObject _destructionEffect;

    /// <summary>
    /// ゲームシステム関連の変数
    /// </summary>
    [Space(1)]
    [Header("SYSTEM SETTINGS")]
    [SerializeField] private float _fallInterval = 1f;// ミノの落下処理のインターバル時間
    [SerializeField, Range(0.01f, 0.5f)] private float _overlapThreshold = 0.2f;// ブロックのオーバーラップ許容値
    [SerializeField, Range(1f, 30f)] private float _holdDuration = 5f;// ミノ操作可能時間
    [SerializeField, Range(1f, 30f)] private float _yellowThresholdSeconds = 3f;// ミノ操作可能時間オブジェクトが黄色に変わる残り秒数
    [SerializeField, Range(1f, 30f)] private float _redThresholdSeconds = 1f;// ミノ操作可能時間オブジェクトが赤色に変わる残り秒数

    /// <summary>
    /// その他変数
    /// </summary>
    private float _previousFallTime;// 前回のミノが落下した時間
    private const int _gridWidth = 10;// グリッドの横方向の広さ
    private const int _gridHeight = 20;// グリッドの縦方向の高さ
    public static Transform[,] Grid = new Transform[_gridWidth, _gridHeight]; // グリッドに設置されたオブジェクト情報を格納する配列
    private string _currentSceneName; // 現在のシーン名
    private bool _isGameOver = false;// ゲームオーバー判定
    private Vector3 _initialPosition;// 直前フレームのミノの座標
    private float _currentRotation = 0;// ミノの回転角度
    private float _holdStartTime;// ミノを掴んだ時の時間
    private bool _isHeld = false;// ミノを掴んでるかの判定
    private TextMeshPro _timerTextInstance;
    void Start()
    {
        SetCurrentSceneName();
        InitializeTimerText();
    }

    void Update()
    {
        FixZPosition(); //Z軸を固定
        FixRotation(); //回転軸を固定
        UpdateTimerTextPosition(); //制限時間タイマーの表示位置の更新

        if (IsBlockHeld())
        {
            if (!_isHeld)
            {
                StartHoldTimer();
            }
            if (IsHoldTimeExceeded())
            {
                HandleBlockExceedingHoldTime();
            }
            else
            {
                UpdateTimerText();
                HandleBlockPlacement();
                HandleBlockRotation();
            }
            SaveCurrentPosition();
        }
        else
        {
            if (_isHeld)
            {
                if (!IsHoldTimeExceeded())
                {
                    StopHoldTimer();
                }
            }
            HandleBlockFall();
        }
    }

    private void SetCurrentSceneName()
    {
        _currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void InitializeTimerText()
    {
        GameObject timerTextObject = Instantiate(_timerTextPrefab, new Vector3(transform.position.x, transform.position.y, -1.7f), Quaternion.identity);
        _timerTextInstance = timerTextObject.GetComponent<TextMeshPro>();
        _timerTextInstance.text = _holdDuration.ToString("F1")+"sec";
    }

    private void FixZPosition()
    {
        Vector3 position = transform.position;
        position.z = 0;
        transform.position = position;
    }

    private void FixRotation()
    {
        transform.eulerAngles = new Vector3(0, 0, _currentRotation);
    }

    private void UpdateTimerTextPosition()
    {
        _timerTextInstance.transform.position = new Vector3(transform.position.x, transform.position.y, -1.7f);
    }

    /// <summary>
    /// 親オブジェクトの有無を調べるメソッド
    /// </summary>
    /// <remarks>
    /// 親オブジェクトとなる右手コントローラーオブジェクトが存在している場合、ミノを掴んでいる判定になる
    /// </remarks>
    /// <returns>親オブジェクトの有無</returns>
    private bool IsBlockHeld()
    {
        return transform.parent != null;
    }

    /// <summary>
    /// ミノを掴んだ時に実行するメソッド
    /// </summary>
    /// <remarks>
    /// 掴み始めた時間を記録し、掴んでいるかを記録する変数である_isHeldをtrueにする
    /// </remarks>
    private void StartHoldTimer()
    {
        _holdStartTime = Time.time;
        _isHeld = true;
    }

    /// <summary>
    /// ミノを離した時に実行するメソッド
    /// </summary>
    /// <remarks>
    /// 残りの操作可能時間を計算し、掴んでいるかを記録する変数である_isHeldをfalseにする
    /// </remarks>
    private void StopHoldTimer()
    {
        _holdDuration -= Time.time - _holdStartTime;
        _isHeld = false;
    }

    /// <summary>
    /// ミノの操作制限時間を超えているかどうかを判定するメソッド
    /// </summary>
    private bool IsHoldTimeExceeded()
    {
        return (Time.time - _holdStartTime) >= _holdDuration;
    }

    /// <summary>
    /// ミノの操作制限時間経過時のタイマーオブジェクトの設定をするメソッド
    /// </summary>
    private void HandleBlockExceedingHoldTime()
    {
        _timerTextInstance.text = "0.0sec";
        _timerTextInstance.color = Color.black;
        this.tag = "Untagged";
        SnapToGrid();
        _isHeld = false;
    }

    /// <summary>
    /// ミノをグリッド上にスナップするメソッド
    /// </summary>
    /// ! GameManagementScriptでも同様の処理を行っているからなくしてもいいかも
    private void SnapToGrid()
    {
        transform.parent = null;
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
        transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    /// <summary>
    /// ミノの操作制限時間オブジェクトの更新をするメソッド
    /// </summary>
    private void UpdateTimerText()
    {
        float timerText = _holdDuration-(Time.time - _holdStartTime);
        ChengeTimerTextColor(timerText);
        _timerTextInstance.text = timerText.ToString("F1")+"sec";
    }

    /// <summary>
    /// 残り時間に応じて、ミノの操作制限時間オブジェクトの色を変更するメソッド
    /// </summary>
    private void ChengeTimerTextColor(float timerText)
    {
        if (timerText < _redThresholdSeconds)
        {
            _timerTextInstance.color = new Color (1f, 0, 0, 1f);
        }
        else if (timerText < _yellowThresholdSeconds)
        {
            _timerTextInstance.color = new Color (1f, 1f, 0, 1f);
        }
    }

    /// <summary>
    /// ミノを位置調節のメソッド
    /// </summary>
    /// <remarks>
    /// ブロックが枠外に存在している、またはブロックが重複する時、直前の位置にミノを戻し、コントローラーを微振動させる。
    /// </remarks>
    private void HandleBlockPlacement()
    {
        if (IsOutSideGrid() || IsBlockOverlapping())
        {
            transform.position = _initialPosition;
            StartCoroutine(VibrateController());
        }
    }

    /// <summary>
    /// ミノの回転処理のメソッド
    /// </summary>
    /// <remarks>
    /// Aボタンを押すと左回転、Bボタンを押すと右回転
    /// </remarks>
    private void HandleBlockRotation()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            RotateBlockCounterClockwise();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            RotateBlockClockwise();
        }
    }

    /// <summary>
    /// 現在のフレームの座標を保存
    /// </summary>
    private void SaveCurrentPosition()
    {
        _initialPosition = transform.position;
    }

    /// <summary>
    /// ミノの左回転処理のメソッド
    /// </summary>
    /// <remarks>
    /// 回転後枠外に存在するか、他ブロックと重複する場合、処理を戻してエラーSEを流しコントローラーを振動させる
    /// </remarks>
    public void RotateBlockCounterClockwise()
    {
        _currentRotation += 90;
        transform.eulerAngles = new Vector3(0, 0, _currentRotation);

        if (IsOutSideGrid() || IsBlockOverlapping())
        {
            FindObjectOfType<SoundManager>().SpawnPlayErrorMinoSpinSE(transform.position);
            _currentRotation -= 90;
            transform.eulerAngles = new Vector3(0, 0, _currentRotation);
            StartCoroutine(VibrateController());
        }
    }

    /// <summary>
    /// ミノの右回転処理のメソッド
    /// </summary>
    /// <remarks>
    /// 回転後枠外に存在するか、他ブロックと重複する場合、処理を戻してエラーSEを流しコントローラーを振動させる
    /// </remarks>
    public void RotateBlockClockwise()
    {
        _currentRotation -= 90;
        transform.eulerAngles = new Vector3(0, 0, _currentRotation);

        if (IsOutSideGrid() || IsBlockOverlapping())
        {
            FindObjectOfType<SoundManager>().SpawnPlayErrorMinoSpinSE(transform.position);
            _currentRotation += 90;
            transform.eulerAngles = new Vector3(0, 0, _currentRotation);
            StartCoroutine(VibrateController());
        }
    }

    /// <summary>
    /// ミノの落下処理のメソッド
    /// </summary>
    /// <remarks>
    /// 前回落下処理から_fallInterval秒経している場合、ミノを落下させる。
    /// 落下処理後枠外にはみ出る場合（IsValidMovementがfalseの場合）ミノを1マス上昇させる。
    /// その後グリッドへの登録(AddToGrid())、ラインがそろっているかの確認(CheckLines())を行い、タグの無効化とこのスクリプトの無効化を行い、新しいミノをスポーンさせる(SpawnNewMino())
    /// </remarks>
    /// *特定の条件下において、ミノが枠外または既存ブロックに重複して存在できてしまうバグが存在する。
    /// *その対策としてwhile (!IsValidMovement())で重複がなくなるまでミノを上昇させている。
    /// *枠外に存在している場合いくら上に上昇させても意味がないので、(int)_gridHeight回数分上昇処理を行ってもIsValidMovement()がfalseの場合、ミノが枠外に存在しているとみなし、そのミノ自体を削除する
    private void HandleBlockFall()
    {
        //Moves down automatically
        if (Time.time - _previousFallTime >= _fallInterval)
        {
            transform.position += new Vector3(0, -1, 0);
            //Process when mino is placed at the bottom or on top of an existing block
            if (!IsValidMovement())
            {
                int checkUpperLimit = (int)_gridHeight;
                while (!IsValidMovement())//Move up until there are no more overlapping
                {
                    transform.position -= new Vector3(0, -1, 0);
                    checkUpperLimit --;
                    if (checkUpperLimit <= 0)//If there are duplicates after 20 MoveUp, delete Mino.
                    {
                        Destroy(_timerTextInstance);
                        Destroy(gameObject);
                        //Spawn a new Mino
                        SpawnNewMino();
                        break;
                    }
                }

                if (checkUpperLimit > 0)
                {
                    //Reproduction of Mino installation SE
                    FindObjectOfType<SoundManager>().SpawnPlaySetMinoSE(transform.position);
                    Destroy(_timerTextInstance);
                    AddToGrid();
                    if (_isGameOver)
                    {
                        //Addition of game over counts in LogDataOutput
                        _isGameOver = false;
                    }
                    CheckLines();

                    this.tag = "Untagged";
                    this.enabled = false;

                    //Spawn a new Mino
                    SpawnNewMino();
                }
            }
            _previousFallTime = Time.time;
        }
    }

    /// <summary>
    /// ミノの位置が正常か確認するメソッド
    /// </summary>
    /// <remarks>
    /// ミノを構成するすべてのブロックの座標が枠内に存在しているか、また他のブロックと重複していないかを調査する。
    /// </remarks>
    /// <returns>ミノの位置が正常であるかどうか</returns>
    private bool IsValidMovement()
    {
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);
            //minoがステージによりはみ出さないように制御
            if (roundX < 0 || roundX >= _gridWidth || roundY < 0 || roundY >= _gridHeight)
            {
                return false;
            }
            if (Grid[roundX, roundY] != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 新しいミノをスポーンするスクリプトを呼び出すメソッド
    /// </summary>
    private void SpawnNewMino()
    {
        FindObjectOfType<SpawnMino>().SpawnNewMino();
    }

    /// <summary>
    /// グリッドにブロックを登録するメソッド
    /// </summary>
    /// <remarks>
    /// Grid配列にブロックを登録する。登録するブロックがy=>17の場合ゲームオーバー処理を行う
    /// </remarks>
    private void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);

            Grid[roundX, roundY] = children;

            //Game over when a block exists at y=17 or more
            if (roundY >= 17)
            {
                // FindObjectOfType<GameManagement>().GameOver();
                GameOver();
            }
        }
    }

    /// <summary>
    /// ゲームオーバー処理を行うメソッド
    /// </summary>
    /// <remarks>
    /// ゲームオーバー時、フィールドとグリッドのブロックをすべて削除し、フィールドをリセットする
    /// </remarks>
    private void GameOver()
    {
        _isGameOver = true;
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
        for (int i = _gridHeight-1; i >= 0; i--)
        {
            for (int j = 0; j < _gridWidth; j++)
            {
                if (Grid[j, i] != null)
                {
                    GameObject destructionEffectInstance = Instantiate(_destructionEffect);
                    destructionEffectInstance.transform.position = Grid[j, i].gameObject.transform.position;
                    Destroy(Grid[j, i].gameObject);
                    Grid[j, i] = null;
                }
            }
        }
        StartCoroutine(GameOverDilay());
    }

    /// <summary>
    /// ラインがそろっているか確認し処理を行うメソッド
    /// </summary>
    private void CheckLines()
    {
        for (int i = _gridHeight - 1; i >= 0; i--)
        {
            if (IsLineComplete(i))
            {
                DestroyLine(i);
                StartCoroutine(DestroyCoroutine(i));
            }
        }
    }

    /// <summary>
    /// ブロック破壊から落下までのラグを処理するコルーチン
    /// </summary>
    /// <param name="deleteLine">ブロックがそろったラインのY軸座標</param>
    private IEnumerator DestroyCoroutine(int deleteLine)
    {
        //Mino Delete SE Playback
        FindObjectOfType<SoundManager>().SpawnPlayDeleteMinoSE(deleteLine);//効果音
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
        yield return new WaitForSeconds(0.5f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        ShiftLinesDown(deleteLine);
    }

    /// <summary>
    /// ゲームオーバー時二重チェック処理
    /// </summary>
    private IEnumerator GameOverDilay()
    {
        //Mino Delete SE Playback
        // OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        // OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
        yield return null;
        for (int i = _gridHeight-1; i >= 0; i--)
        {
            for (int j = 0; j < _gridWidth; j++)
            {
                if (Grid[j, i] != null)
                {
                    GameObject destructionEffectInstance = Instantiate(_destructionEffect);
                    destructionEffectInstance.transform.position = Grid[j, i].gameObject.transform.position;
                    Destroy(Grid[j, i].gameObject);
                    Grid[j, i] = null;
                }
            }
        }
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    /// <summary>
    /// ラインがそろっているかを判定するメソッド
    /// </summary>
    ///<remarks>
    /// そろっている場合スコアも加算する
    /// </remarks>
    /// <param name="checkLine">チェックするラインのY座標</param>
    /// <returns>ラインがそろっているかどうか</returns>
    private bool IsLineComplete(int checkLine)
    {
        for (int j = 0; j < _gridWidth; j++)
        {
            if (Grid[j, checkLine] == null)
                return false;
        }

        //Add to score
        FindObjectOfType<GameManagementScript>().AddScore();
        return true;
    }

    /// <summary>
    /// 引数で指定した列のブロックを削除するメソッド
    /// </summary>
    /// <param name="deleteLine">削除するラインのY座標</param>
    private void DestroyLine(int deleteLine)
    {
        for (int j = 0; j < _gridWidth; j++)
        {
            GameObject destructionEffectInstance = Instantiate(_destructionEffect);
            destructionEffectInstance.transform.position = Grid[j, deleteLine].gameObject.transform.position;
            Destroy(Grid[j, deleteLine].gameObject);
            Grid[j, deleteLine] = null;
        }
    }

    /// <summary>
    /// 削除した列の上のブロックを下げるメソッド
    /// </summary>
    /// <param name="deletedLine">削除されたラインのY座標</param>
    private void ShiftLinesDown(int deletedLine)
    {
        for (int y = deletedLine; y < _gridHeight; y++)
        {
            for (int j = 0; j < _gridWidth; j++)
            {
                if (Grid[j, y] != null)
                {
                    Grid[j, y - 1] = Grid[j, y];
                    Grid[j, y] = null;
                    Grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    /// <summary>
    /// ミノと既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <remarks>
    /// 上下左右に加え、斜め方向を加えた計八方向を確認している
    /// </remarks>
    /// <returns>ミノと既存ブロックが重複しているかどうか</returns>
    private bool IsBlockOverlapping()
    {
        return CheckOverlapBottom() || CheckOverlapLeft() || CheckOverlapRight() || CheckOverlapTop() || CheckOverlapTopLeft() || CheckOverlapTopRight() || CheckOverlapBottomLeft() || CheckOverlapBottomRight();
    }

    /// <summary>
    /// ミノの下側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの下側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapBottom()
    {
        foreach (Transform children in transform)
        {
            Vector3 child = children.position;
            int x = Mathf.RoundToInt(child.x);
            float y = child.y + _overlapThreshold;
            if (y >= 0 && y < _gridHeight && Grid[x, Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの左側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの左側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapLeft()
    {
        foreach (Transform children in transform)
        {
            Vector3 child = children.position;
            float x = child.x + _overlapThreshold;
            int y = Mathf.RoundToInt(child.y);
            if (x >= 0 && x < _gridWidth && Grid[Mathf.FloorToInt(x), y] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの右側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの右側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapRight()
    {
        foreach (Transform children in transform)
        {
            Vector3 child = children.position;
            float x = child.x + (1f-_overlapThreshold);
            int y = Mathf.RoundToInt(child.y);
            if (x >= 0 && x < _gridWidth && Grid[Mathf.FloorToInt(x), y] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの上側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの上側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapTop()
    {
        foreach (Transform children in transform)
        {
            Vector3 child = children.position;
            int x = Mathf.RoundToInt(child.x);
            float y = child.y + (1f-_overlapThreshold);
            if (y >= 0 && y < _gridHeight && Grid[x, Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの左上側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの左上側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapTopLeft()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPosition = child.position;
            float x = childPosition.x + _overlapThreshold;
            float y = childPosition.y + (1f - _overlapThreshold);
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight && Grid[Mathf.FloorToInt(x), Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの右上側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの右上側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapTopRight()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPosition = child.position;
            float x = childPosition.x + (1f - _overlapThreshold);
            float y = childPosition.y + (1f - _overlapThreshold);
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight && Grid[Mathf.FloorToInt(x), Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの左下側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの左下側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapBottomLeft()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPosition = child.position;
            float x = childPosition.x + _overlapThreshold;
            float y = childPosition.y + _overlapThreshold;
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight && Grid[Mathf.FloorToInt(x), Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノの右下側と既存ブロックの重複を確認するメソッド
    /// </summary>
    /// <returns>ミノの右下側と既存ブロックが重複しているかどうか</returns>
    private bool CheckOverlapBottomRight()
    {
        foreach (Transform child in transform)
        {
            Vector3 childPosition = child.position;
            float x = childPosition.x + (1f - _overlapThreshold);
            float y = childPosition.y + _overlapThreshold;
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight && Grid[Mathf.FloorToInt(x), Mathf.FloorToInt(y)] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ミノが枠内に存在しているかどうかを確認するメソッド
    /// </summary>
    /// <returns>ミノが枠内に存在しているかどうか</returns>
    private bool IsOutSideGrid()
    {
        foreach (Transform children in transform)
        {
            if (children.position.x < 0 || children.position.x >= _gridWidth - 1 || children.position.y < 0 || children.position.y > _gridHeight - 1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// エラー時にコントローラーを振動させるメソッド
    /// </summary>
    IEnumerator VibrateController()
    {
        OVRInput.SetControllerVibration(0.1f, 0.3f, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}