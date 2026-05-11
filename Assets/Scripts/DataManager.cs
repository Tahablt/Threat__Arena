using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // Oyundaki tüm eþyalarý burada toplayacaðýz
    public List<ItemData> tumEsyalar;

    private void Awake()
    {
        Instance = this;
    }
}