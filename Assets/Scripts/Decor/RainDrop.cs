using UnityEngine;

public class RainDrop : MonoBehaviour
{
    [Header("Movement")]
    public float fallSpeed = 15f;
    public float windSpeed = -3f; // Tốc độ gió (âm là bay sang trái)
    public float fallTime = 0.4f; // Thời gian bay trước khi chạm đất

    [Header("Splash Effect")]
    public float splashTime = 0.15f; // Thời gian hiển thị vòng tròn văng nước
    public Animator animator;
    public string splashTriggerName = "Splash"; // Tên trigger trong Animator để chuyển sang animation splash

    [Header("Behavior")]
    public bool useRandomness = false; // Bật để hạt mưa rơi ngẫu nhiên, tắt để mưa rơi đều tăm tắp

    private float timer = 0f;
    private bool isSplashing = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        if (useRandomness)
        {
            // Randomize tốc độ và thời gian một chút để các hạt mưa rơi không đều nhau
            fallSpeed += Random.Range(-2f, 2f);
            windSpeed += Random.Range(-0.5f, 0.5f);
            fallTime += Random.Range(-0.1f, 0.2f);
        }
    }

    private void Update()
    {
        if (!isSplashing)
        {
            timer += Time.deltaTime;
            
            // Di chuyển xiên (rơi xuống + bay ngang)
            transform.Translate(new Vector3(windSpeed, -fallSpeed, 0) * Time.deltaTime);

            if (timer >= fallTime)
            {
                // Hạt mưa đã bay đủ thời gian -> chạm đất -> chuyển sang hiệu ứng Splash
                isSplashing = true;
                timer = 0f; // Reset timer để đếm thời gian splash
                
                if (animator != null)
                {
                    // Kích hoạt animation vòng tròn văng nước nếu có Animator
                    animator.SetTrigger(splashTriggerName);
                }
                else
                {
                    // Nếu KHÔNG có Animator, ta làm giả hiệu ứng splash bằng code
                    // Ép bẹp hạt mưa lại thành hình ellipse và mờ đi
                    transform.localScale = new Vector3(1.5f, 0.5f, 1f); 
                    if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }
        else
        {
            // Đang hiển thị Splash (vòng tròn văng nước)
            timer += Time.deltaTime;
            
            // Tùy chọn mờ dần vòng tròn
            if (spriteRenderer != null)
            {
                 float alpha = Mathf.Lerp(0.5f, 0f, timer / splashTime);
                 spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            // Hủy object sau khi kết thúc splash
            if (timer >= splashTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
