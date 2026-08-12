using LabFusion.Network;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuPopupsHelper
{
    public static GameObject PopupsRoot { get; private set; } = null;

    public static void OnInitializeMelon()
    {
        MenuToolbarHelper.OnInitializeMelon();

        NetworkLayerManager.LogInCompleted += OnLogInChanged;
        NetworkLayerManager.LogOutCompleted += OnLogInChanged;
    }

    public static void PopulatePopups(GameObject popups)
    {
        PopupsRoot = popups;

        MenuToolbarHelper.PopulateToolbar(popups.transform.Find("grid_Toolbar").gameObject);

        UpdateLogIn();
    }

    private static void OnLogInChanged(NetworkLayer layer) => UpdateLogIn();

    private static void UpdateLogIn()
    {
        if (NetworkLayerManager.IsLoggedIn)
        {
            OnLogInCompleted();
        }
        else
        {
            OnLogOutCompleted();
        }
    }

    private static void OnLogInCompleted()
    {
        if (PopupsRoot == null)
        {
            return;
        }

        PopupsRoot.SetActive(true);
    }

    private static void OnLogOutCompleted()
    {
        if (PopupsRoot == null)
        {
            return;
        }

        PopupsRoot.SetActive(false);
    }
}
