public interface ILecturePlayable
{
    // スキップ時に呼ばれる。進行中のアニメーションを止め、最終状態をセットする
    void FastForward();
}
