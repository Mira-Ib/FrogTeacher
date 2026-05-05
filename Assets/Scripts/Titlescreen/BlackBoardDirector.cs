using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BlackboardDirector : MonoBehaviour
{
    [SerializeField] private FormulaView formulaView;
    [SerializeField] private SymbolAnimator[] symbolAnimators;
    [SerializeField] private FormulaData[] allFormulas;

    [Header("タイミングの調整")]
    [Tooltip("数式が表示されてから、消し始めるまでの待機時間（秒）")]
    [SerializeField] private float waitTimeBeforeErase = 1.5f; // ★追加
    [Tooltip("消去後、次の数式が表示されるまでの待機時間（秒）")]
    [SerializeField] private float waitTimeAfterErase = 0.5f;  // ★追加

    public async UniTask PlayBlackboardLoopAsync(CancellationToken token)
    {
        foreach (var symbol in symbolAnimators)
        {
            symbol.PlayLoopAsync(token).Forget();
        }

        while (!token.IsCancellationRequested)
        {
            var playlist = CreateShuffledPlaylist();

            foreach (var formulaData in playlist)
            {
                if (token.IsCancellationRequested) break;

                // ① パラパラ漫画再生
                await formulaView.PlayFlipbookAsync(formulaData.frames, 0.1f, token);

                // ② ★修正：インスペクターで設定した時間だけ待機する
                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTimeBeforeErase), cancellationToken: token);

                // ③ 左から右へのマスクで消去
                await formulaView.EraseByMaskAsync(token);

                // ④ ★修正：インスペクターで設定した時間だけ待機する
                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTimeAfterErase), cancellationToken: token);
            }
        }
    }

    private List<FormulaData> CreateShuffledPlaylist()
    {
        var mainFormula = allFormulas.First(f => f.isMainFormula);
        var others = allFormulas.Where(f => !f.isMainFormula)
                                .OrderBy(x => System.Guid.NewGuid())
                                .ToList();

        var result = new List<FormulaData> { mainFormula };
        result.AddRange(others);
        return result;
    }
}