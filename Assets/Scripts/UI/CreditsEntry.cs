using System;
using UnityEngine;

/// <summary>
/// Đại diện cho một dòng credit (vai trò + tên người).
/// Có thể thêm bao nhiêu entry tùy ý trong Inspector.
/// </summary>
[Serializable]
public class CreditsEntry
{
    [Tooltip("Vai trò / chức danh, ví dụ: 'Lead Developer', 'Art Director'")]
    public string role;

    [Tooltip("Tên người hoặc danh sách tên (cách nhau bằng dấu phẩy nếu nhiều người)")]
    [TextArea(1, 3)]
    public string names;

    [Tooltip("Nếu true, dòng này sẽ hiển thị to hơn và in đậm (dùng cho tiêu đề section)")]
    public bool isSectionHeader;

    [Tooltip("Khoảng cách thêm phía trên dòng này (pixels)")]
    public float extraSpacingAbove = 0f;
}
