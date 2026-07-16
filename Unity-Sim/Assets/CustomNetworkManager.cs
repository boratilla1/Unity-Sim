using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    // Bir oyuncu sunucuya baðlandýðýnda veya sahne hazýr olduðunda Mirror bu metodu tetikler
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Eðer þu anki aktif sahne TestScene ise, karakteri yaratýp oyuncuya baðla
        if (SceneManager.GetActiveScene().name == "TestScene")
        {
            base.OnServerAddPlayer(conn);
            Debug.Log($"<color=green>[NetworkManager]</color> TestScene yüklendi, oyuncu kapsülü oluþturuluyor. ConnId: {conn.connectionId}");
        }
        else
        {
            // Eðer Lobi sahnesindeysek hiçbir þey yapma, karakter yaratmayý ertele
            Debug.Log($"<color=yellow>[NetworkManager]</color> Lobi sahnesindeyiz, karakter yaratma iþlemi ertelendi.");
        }
    }
}