using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float mouseThreshold = 10f; // 10ピクセル以上動いたらマウス操作とみなす
    [SerializeField] private GameObject firstSelectedObject; // 復活時に選ぶデフォルトのボタン

    private Vector3 _lastMousePosition;
    private GameObject _lastSelectedObject;

    void Start()
    {
        _lastMousePosition = Input.mousePosition;
        // 初期選択を設定
        if (firstSelectedObject != null) _lastSelectedObject = firstSelectedObject;
    }

    void Update()
    {
        // 1. マウスが大きく動いたかチェック
        float mouseDistance = Vector3.Distance(Input.mousePosition, _lastMousePosition);

        if (mouseDistance > mouseThreshold)
        {
            // マウス操作と判断して選択を解除（ただし、最後に選んでいたものは記憶しておく）
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                _lastSelectedObject = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(null);
            }
            _lastMousePosition = Input.mousePosition;
        }

        // 2. キーボード入力（矢印キーなど）があったかチェック
        if (IsKeyboardInputDetected())
        {
            // 選択が空なら、記憶していた場所（またはデフォルト）にフォーカスを戻す
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                GameObject target = _lastSelectedObject != null ? _lastSelectedObject : firstSelectedObject;
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        // SelectionManager の Update 内に追加
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // クリックされた瞬間、選択を解除する
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private bool IsKeyboardInputDetected()
    {
        // 矢印キー、WASD、Enter、Spaceのいずれかが押されたか
        return Input.GetAxisRaw("Horizontal") != 0 ||
               Input.GetAxisRaw("Vertical") != 0 ||
               Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return);
    }
}