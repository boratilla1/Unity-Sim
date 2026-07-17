using UnityEngine;
using Mirror; // Að kütüphanemizi dahil ediyoruz

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Etkileþim Ayarlarý")]
    public float etkilesimMesafesi = 3f;
    public float isinKalinligi = 0.2f;
    public LayerMask etkilesimKatmanlari;

    [Header("Referanslar")]
    public Transform elTutmaNoktasi; // Kameranýn önündeki boþ obje (Grip Point)

    private GameObject tutulanObje;

    void Update()
    {
        // Çok oyunculu ortamda, oyuncu sadece KENDÝ karakterini kontrol etmelidir.
        // Baþkasýnýn kamerasýndan ýþýn atmamak için bu kontrol hayati önem taþýr.
        if (!isLocalPlayer) return;

        EtkilesimKontrolu();
    }

    void EtkilesimKontrolu()
    {
        Vector3 baslangicNoktasi = transform.position;
        Vector3 yon = transform.forward;
        RaycastHit hit;

        if (Physics.SphereCast(baslangicNoktasi, isinKalinligi, yon, out hit, etkilesimMesafesi, etkilesimKatmanlari))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Baktýðýmýz objenin að üzerinde bir kimliði var mý?
                NetworkIdentity hedefKimlik = hit.collider.GetComponent<NetworkIdentity>();

                if (hedefKimlik != null && hit.collider.CompareTag("Gripable"))
                {
                    if (tutulanObje == null)
                    {
                        // Obje tutulmuyorsa, sunucudan objeyi alma izni iste
                        CmdEsyayiAl(hedefKimlik);
                    }
                }
                // Rafa koyma mantýðý buraya eklenecek
            }
        }
    }

    // [Command] etiketli fonksiyonlar sadece Sunucuda (Server/Host) çalýþýr.
    // Ýstemciler (Clients) hile yapamasýn diye kararlarý sunucu verir.
    [Command]
    void CmdEsyayiAl(NetworkIdentity hedefObje)
    {
        // Güvenlik kontrolü: Eðer obje zaten baþka bir oyuncunun yetkisindeyse (baþkasý tutuyorsa) iþlemi iptal et.
        if (hedefObje.connectionToClient != null) return;

        // Objenin að yetkisini (Authority) bu komutu gönderen oyuncuya ata.
        hedefObje.AssignClientAuthority(connectionToClient);

        // Artýk yetki bizde. Tüm oyuncularda bu objenin bizim elimize geçmesi için RPC çaðýrýyoruz.
        RpcEsyayiEleYerlestir(hedefObje);
    }

    // [ClientRpc] etiketli fonksiyonlar sunucu tarafýndan tetiklenir ve TÜM istemcilerde (herkesin ekranýnda) çalýþýr.
    [ClientRpc]
    void RpcEsyayiEleYerlestir(NetworkIdentity hedefObje)
    {
        // 1. Objenin fiziðini kapat (ellerde titreme veya çarpýþma yapmasýn)
        Rigidbody rb = hedefObje.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 2. Objeyi görsel olarak karakterin eline (elTutmaNoktasi) sabitle
        hedefObje.transform.SetParent(elTutmaNoktasi);
        hedefObje.transform.localPosition = Vector3.zero;
        hedefObje.transform.localRotation = Quaternion.identity;

        // 3. Eðer bu kod yerel oyuncuda (bizde) çalýþýyorsa, objeyi hafýzaya al
        if (isLocalPlayer)
        {
            tutulanObje = hedefObje.gameObject;
        }
    }
}