using UnityEngine;
using System.Collections;

public class TubeSelection : MonoBehaviour
{
    private static TubeController selectedTube = null; // 🌟 全局静态变量，所有瓶子共享
    private float moveDistance = 0.3f; // 上移高度
    private Vector3 originalPosition; // 记录初始位置
    private AudioSource audioSource; // 🎵 音效播放组件

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.position; // 记录瓶子的原始位置
    }

    void Update()
    {
        // 🌟 如果不是被选中的瓶子，并且位置偏离原始位置 -> 自动回到原位
        if (selectedTube != this.GetComponent<TubeController>() && selectedTube != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, 5f * Time.deltaTime);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("管子被点击了：" + gameObject.name);
        //Debug.Log(selectedTube);
        TubeController clickedTube = GetComponent<TubeController>();

        if (selectedTube == null)
        {
            selectedTube = clickedTube; // 选中瓶子

            if (audioSource != null) // 🎵 播放音效
            {
                audioSource.Play();
            }

            // Move up
            // 这里会有一个bug，当第2次点击太快时，这个动画会未执行完，从而执行其他动画，然后继续走未执行的路径
            StopAllCoroutines();
            StartCoroutine(MoveBottle(originalPosition + Vector3.up * moveDistance, 20f));
        }
        else if (selectedTube == clickedTube)
        {
            // 🌟 再次点击相同的瓶子 -> 取消选中，回到原位
            selectedTube = null;
            StopAllCoroutines();
            StartCoroutine(MoveBottle(originalPosition, 5f));
        }
        else
        {
            // 倒水
            selectedTube.PourWater(clickedTube);
            // 取消选中
            selectedTube = null;
        }
    }

    IEnumerator MoveBottle(Vector3 targetPos, float moveSpeed)
    {
        Debug.Log("[TubeSelection]MoveBottle");
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos; // 确保精准移动到位
    }
}
