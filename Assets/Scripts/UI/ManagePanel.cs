using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagePanel : MonoBehaviour
{
    public GameObject BSP_Panel;

    public GameObject Dungeon3D_Panel;

    public GameObject Flowers_Panel;

    public void OpenBSP_Panel()
    {
        BSP_Panel.SetActive(true);
    }

    public void CloseBSP_Panel()
    {
        BSP_Panel.SetActive(false);
    }

    public void OpenDungeon_Panel()
    {
        Dungeon3D_Panel.SetActive(true);
    }

    public void CloseDungeon_Panel()
    {
        Dungeon3D_Panel.SetActive(false);
    }

    public void OpenFlower_Panel()
    {
        Flowers_Panel.SetActive(true);
    }

    public void CloseFlower_Panel()
    {
        Flowers_Panel.SetActive(false);
    }
}
