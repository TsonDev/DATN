using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chứa toàn bộ danh sách credits.
/// Tạo asset: Right-click trong Project → Create → Credits → Credits Data
/// </summary>
[CreateAssetMenu(fileName = "CreditsData", menuName = "Credits/Credits Data", order = 0)]
public class CreditsData : ScriptableObject
{
    [Header("Cấu hình tổng")]
    [Tooltip("Tiêu đề hiện ở đầu credits, ví dụ: tên game")]
    public string gameTitle = "GAME TITLE";

    [Tooltip("Dòng chữ hiện cuối credits")]
    public string closingMessage = "Cảm ơn bạn đã chơi!";

    [Space(10)]
    [Header("Danh sách Credits")]
    [Tooltip("Thêm bao nhiêu entry tùy ý. Mỗi entry là một dòng 'Vai trò - Tên'.")]
    public List<CreditsEntry> entries = new List<CreditsEntry>();
}
