using UnityEngine;
using System.Collections.Generic;

public class TubeController : MonoBehaviour
{
    // 管子中所有的液体槽，要求在 Inspector 中按从底部到顶部顺序排列
    public List<SpriteRenderer> liquidSlots;

    // 用来记录当前液体颜色（可选，用于后续倒水逻辑）
    public List<Color?> currentLiquids = new List<Color?>();
    // 用于存储每层液体的颜色（可以用 Color 或字符串标识）
    public List<Color?> liquidColors = new List<Color?>();

    // 每个管子的最大层数（例如4）
    public int maxLayers = 4;
    public int maxCapacity = 4; // 每个瓶子最大液体层数

    public AudioClip pourWaterSound; // 倒水音效
    private AudioSource audioSource; // 🎵 音效播放组件

    private Stack<Color> liquidStack = new Stack<Color>(); // 用于存储当前液体的堆栈

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // 初始化瓶子液体槽（确保按从底到上的顺序）
        //foreach (SpriteRenderer slot in liquidSlots) // 直接遍历 SpriteRenderer
        //{
        //    if (slot.color.a > 0) // 透明度 > 0 代表有液体
        //    {
        //        liquidStack.Push(slot.color); // 存入颜色栈
        //    }
        //}
    }

    // 设置槽位颜色，并更新显示
    public void SetLiquidAt(int index, Color? color)
    {
        if (index >= 0 && index < maxLayers)
        {
            liquidColors[index] = color;
            // 更新槽位的 Sprite Renderer
            if (color.HasValue)
                liquidSlots[index].color = color.Value;
            else
                liquidSlots[index].color = new Color(1, 1, 1, 0);  // 设为透明
        }
    }

    // 获取顶部连续同色的液体数量，从上往下检查
    public int GetTopSameColorCount(out Color? topColor)
    {
        topColor = null;
        int count = 0;
        for (int i = maxLayers - 1; i >= 0; i--)
        {
            if (liquidColors[i].HasValue)
            {
                if (topColor == null)
                {
                    topColor = liquidColors[i];
                    count = 1;
                }
                else if (liquidColors[i] == topColor)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                // 空槽不计入连续层数
                break;
            }
        }
        return count;
    }

    // 检查是否有足够的空槽接收液体
    public int GetEmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < maxLayers; i++)
        {
            if (!liquidColors[i].HasValue)
                count++;
        }
        return count;
    }

    // 倒入液体，从另一个管子倒入液体块
    // numToPour: 倒入的块数，pourColor: 倒入的颜色
    public void PourLiquid(int numToPour, Color pourColor)
    {
        for (int i = 0; i < numToPour; i++)
        {
            // 找到第一个空槽，从底部开始填充
            for (int j = 0; j < maxLayers; j++)
            {
                if (!liquidColors[j].HasValue)
                {
                    SetLiquidAt(j, pourColor);
                    break;
                }
            }
        }
    }

    // 从顶部倒出液体
    public void RemoveTopLiquid(int count)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = maxLayers - 1; j >= 0; j--)
            {
                if (liquidColors[j].HasValue)
                {
                    SetLiquidAt(j, null);
                    break;
                }
            }
        }
    }

    // 填充瓶子，tubeColors 的顺序为从底部到顶部 
    public void FillTube(List<Color> tubeColors)
    {
        // 先清空当前数据
        currentLiquids.Clear();
        liquidStack.Clear(); // ✅ 关键！保证 liquidStack 被正确填充

        for (int i = 0; i < liquidSlots.Count; i++)
        {
            if (i < tubeColors.Count)
            {
                // 设置液体槽颜色（确保液体槽有 Sprite 并且颜色不透明）
                liquidSlots[i].color = new Color(tubeColors[i].r, tubeColors[i].g, tubeColors[i].b, 1f);
                currentLiquids.Add(liquidSlots[i].color);

                // ✅ 重要！同步更新 liquidStack（从底部到顶部）
                liquidStack.Push(liquidSlots[i].color);
            }
            else
            {
                // 超出部分设为空（或透明）
                liquidSlots[i].color = new Color(1, 1, 1, 0);
                currentLiquids.Add(null);
            }
        }

        Debug.Log("当前瓶子 " + gameObject.name + " 的液体槽数量：" + liquidSlots.Count + "，当前液体数量：" + liquidStack.Count);
    }

    // 清空瓶子，使所有液体槽显示为空
    public void ClearTube()
    {
        currentLiquids.Clear();
        liquidStack.Clear(); // ✅ 关键！清空液体堆栈

        for (int i = 0; i < liquidSlots.Count; i++)
        {
            liquidSlots[i].color = new Color(1, 1, 1, 0);
            currentLiquids.Add(null);
        }
    }

    public bool CanPourInto(TubeController targetTube)
    {
        Debug.Log("检测能否倒水");
        if (targetTube.IsFull()) 
        {
            //Debug.Log("目标瓶已满，不能倒入");
            return false; // 目标瓶已满，不能倒入
        }
        if (liquidStack.Count == 0) 
        {
            //Debug.Log("当前瓶子是空的，不能倒水");
            return false; // 当前瓶子是空的，不能倒水
        }
        if (targetTube.liquidStack.Count == 0) 
        {
            //Debug.Log("目标瓶是空的，任何颜色都能倒");
            return true; // 目标瓶是空的，任何颜色都能倒
        }

        // 只能倒入相同颜色的水
        return targetTube.liquidStack.Peek() == liquidStack.Peek();
    }

    public void PourWater(TubeController targetTube)
    {
        if (!CanPourInto(targetTube)) return; // 不能倒水，直接返回

        Color pouringColor = liquidStack.Peek(); // 获取当前瓶子最上层的颜色
        int pourAmount = 0;

        // 倒水逻辑：将相同颜色的水倒入目标瓶，直到目标瓶满或当前颜色倒完
        while (liquidStack.Count > 0 && liquidStack.Peek() == pouringColor && targetTube.liquidStack.Count < maxCapacity)
        {
            liquidStack.Pop(); // 移除当前瓶子顶层水
            targetTube.liquidStack.Push(pouringColor); // 倒入目标瓶
            pourAmount++;
        }
        // 🎵 播放倒水音效
        if (pourWaterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pourWaterSound);
        }

        // 更新 UI 显示
        UpdateLiquidDisplay();
        targetTube.UpdateLiquidDisplay();

        // **成功倒水后检查游戏是否结束**
        gm2.Instance.CheckGameOver();
    }

    public bool IsFull()
    {
        return liquidStack.Count >= maxCapacity;
    }

    public List<Color> GetCurrentColors()
    {
        return new List<Color>(liquidStack); // 直接构造 List<Color>
    }

    private void UpdateLiquidDisplay()
    {
        // 清空所有液体槽颜色
        foreach (var slot in liquidSlots)
        {
            slot.color = new Color(0, 0, 0, 0); // 透明，表示空槽
        }

        // 逆序填充液体槽（从底部开始填充）
        Color[] liquidArray = liquidStack.ToArray(); // Stack 转换为数组
        int index = 0; // 从底部槽位开始填充

        for (int i = liquidArray.Length - 1; i >= 0; i--) // 逆序遍历
        {
            if (index >= liquidSlots.Count) break; // 防止超出索引
            liquidSlots[index].color = liquidArray[i]; // 从底部开始填充
            index++;
        }
    }
}
