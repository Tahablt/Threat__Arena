using UnityEngine;
using TMPro; // TextMeshPro kullanýmý için þart

public class FPSCounter : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI fpsText; // Inspector'dan sürükle býrak yapacaðýmýz yer
    [SerializeField] private float updateInterval = 0.5f; // Sayacýn yenilenme hýzý

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;

    void Start()
    {
        // MOBÝL ÝÇÝN KRÝTÝK: FPS'i 60'a sabitler. 
        // Telefonun boþuna 120 basýp ýsýnmasýný ve kasmaya baþlamasýný engeller.
        Application.targetFrameRate = 60;

        if (fpsText == null)
        {
            Debug.LogError("FPS Text referansý boþ! Lütfen Inspector'dan bir TextMeshPro objesi sürükleyin.");
        }
    }

    void Update()
    {
        // Kareleri ve geçen süreyi say
        frameCount++;
        dt += Time.unscaledDeltaTime; // Oyun durdurulsa bile FPS sayacý çalýþmaya devam eder

        // Belirlenen aralýk dolduðunda hesapla
        if (dt > updateInterval)
        {
            fps = frameCount / dt;

            // Ekrana yazdýr (F0 yaparak küsuratlarý sildik, daha temiz durur: "60 FPS")
            if (fpsText != null)
            {
                fpsText.text = string.Format("{0:F0} FPS", fps);

                // Performansa göre renk belirle
                if (fps >= 55)
                    fpsText.color = Color.green; // Yað gibi akýyor
                else if (fps >= 30)
                    fpsText.color = Color.yellow; // Ýdare eder
                else
                    fpsText.color = Color.red; // Kasýyor, bir þeyler sil kanka!
            }

            // Sýfýrla ve baþtan baþla
            frameCount = 0;
            dt -= updateInterval;
        }
    }
}