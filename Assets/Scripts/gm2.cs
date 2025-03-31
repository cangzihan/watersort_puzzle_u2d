using System.Collections.Generic;
using UnityEngine;

public class gm2 : MonoBehaviour
{
    public static gm2 Instance; // 确保是单例模式

    // 预设 4 种液体颜色（在 Inspector 中设置）
    public Color[] liquidColors;  // 数组大小应为 4

    // 游戏中所有瓶子的引用（总共6个，前4个填满，后2个为空瓶）
    public List<TubeController> tubes; 

    // 每个瓶子的容量（例如 4 层）
    public int capacity = 4;

    // 装水瓶子的数量
    public int water_count = 4;

    public GameObject ClearPage; // 需要控制的 UI 组件
    private AudioSource audioSource; // 🎵 音效播放组件

    void Awake()
    {
        Instance = this; // 让其他类可以访问 GameManager
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // 第一步：准备液体数据，每个颜色出现 capacity 次（这里为4次）
        List<Color> liquidPool = new List<Color>();
        foreach (Color c in liquidColors)
        {
            for (int i = 0; i < capacity; i++)
            {
                liquidPool.Add(c);
            }
        }

        // 第二步：打乱液体池中的顺序（随机洗牌）
        for (int i = 0; i < liquidPool.Count; i++)
        {
            Color temp = liquidPool[i];
            int randomIndex = Random.Range(i, liquidPool.Count);
            liquidPool[i] = liquidPool[randomIndex];
            liquidPool[randomIndex] = temp;
        }

        // 第三步：将液体分配到前 water_count 个瓶子中，每个瓶子 capacity 层
        for (int i = 0; i < water_count; i++)
        {
            TubeController tube = tubes[i];
            // 从液体池中取出对应的部分
            List<Color> tubeColors = liquidPool.GetRange(i * capacity, capacity);
            tube.FillTube(tubeColors);  // 调用 TubeController 的填充方法
        }

        // 剩余瓶子保持为空
        for (int i = water_count; i < tubes.Count; i++)
        {
            tubes[i].ClearTube();  // 可在 TubeController 中实现清空功能
        }
        //Debug.Log("液体池数量：" + liquidPool.Count);
        //Debug.Log("瓶子数量：" + tubes.Count);
    }

    public void ResetGame()
    {
        if (audioSource != null) // 🎵 播放音效
        {
            audioSource.Play();
        }

        // 清空所有瓶子
        foreach (TubeController tube in tubes)
        {
            tube.ClearTube();
        }

        // 重新初始化游戏
        Start();

        HidePanel();
    }

    public void CheckGameOver()
    {
        HashSet<Color> uniqueColors = new HashSet<Color>();
        bool allFullOrEmpty = true;

        foreach (TubeController tube in tubes)
        {
            List<Color> tubeColors = tube.GetCurrentColors();
            int liquidCount = tubeColors.Count;

            // 如果瓶子是空的，跳过颜色检查
            if (liquidCount == 0)
            {
                continue;
            }

            // 检查是否是单色瓶（所有液体颜色相同）
            Color firstColor = tubeColors[0];
            bool isSingleColor = tubeColors.TrueForAll(color => color == firstColor);

            if (!isSingleColor)
            {
                Debug.Log("❌ 瓶子 " + tube.gameObject.name + " 不是单色");
                return; // 发现混色瓶，直接结束，不判定游戏结束
            }

            // 记录唯一颜色
            uniqueColors.Add(firstColor);

            // 检查是否所有瓶子都是满的或空的
            if (liquidCount < tube.maxCapacity) 
            {
                allFullOrEmpty = false; // 发现未装满的瓶子，条件不满足
            }
        }

        // 最终游戏结束判定
        if (allFullOrEmpty)
        {
            Debug.Log("🎉 游戏结束！所有瓶子均为单色且满，或空瓶！");
            ShowPanel();
        }
        else
        {
            Debug.Log("未满足结束条件：所有满/空？" + allFullOrEmpty);
        }
    }

    public void HidePanel()
    {
        ClearPage.SetActive(false);  // 隐藏 Panel
    }

    public void ShowPanel()
    {
        ClearPage.SetActive(true);  // 显示 Panel
    }
}
