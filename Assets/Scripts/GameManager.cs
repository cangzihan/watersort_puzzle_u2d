using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 单例模式方便全局访问
    public static GameManager Instance;

    private TubeController firstSelected;

    void Awake()
    {
        Instance = this;
    }

    // 当有管子被点击时调用
    public void OnTubeSelected(TubeController selectedTube)
    {
        if (firstSelected == null)
        {
            // 记录第一个选中的管子
            firstSelected = selectedTube;
            // 这里可以添加高亮显示效果
            Debug.Log("选中管子：" + selectedTube.gameObject.name);
        }
        else
        {
            Debug.Log("尝试倒水：" + firstSelected.gameObject.name + " → " + selectedTube.gameObject.name);
            
            // 第二次选择，执行倒水操作
            TubeController sourceTube = firstSelected;
            TubeController targetTube = selectedTube;
            firstSelected = null; // 重置选择

            AttemptPour(sourceTube, targetTube);
        }
    }

    // 倒水逻辑判断
    void AttemptPour(TubeController source, TubeController target)
    {
        // 获取源管子顶部连续同色液体数量
        Color? sourceColor;
        int pourCount = source.GetTopSameColorCount(out sourceColor);

        if (!sourceColor.HasValue)
            Debug.Log("无法倒水");
            return;

        // 判断目标管子的顶部液体颜色（如果有的话）
        int emptySlots = target.GetEmptySlotCount();
        int targetTopCount = target.GetTopSameColorCount(out Color? targetColor);

        // 合法条件：目标为空或顶部颜色与源相同，并且有足够空间
        if ((target.GetEmptySlotCount() > 0) &&
            (targetTopCount == 0 || (targetColor.HasValue && targetColor == sourceColor)) &&
            (emptySlots >= pourCount))
        {
            Debug.Log("执行倒水");
            // 执行倒水：先从源管子移除液体，再倒入目标管子
            source.RemoveTopLiquid(pourCount);
            target.PourLiquid(pourCount, sourceColor.Value);
        }
        else
        {
            // 倒水操作不合法，提示玩家（例如播放错误音效或闪烁管子）
            Debug.Log("无法倒水");
        }
    }
}
