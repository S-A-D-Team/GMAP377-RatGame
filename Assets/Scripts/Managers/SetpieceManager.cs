using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SetpieceManager : MonoBehaviour
{
    public static SetpieceManager Instance { get; private set; }
    private List<Sector> sectors;
    private List<Setpiece> setpieces;
    private Sector currentPlayerSector;
    private Sector lastKnownSector;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterSector(Sector sector)
    {
        sectors.Add(sector);
    }

    public void RegisterSetpiece(Setpiece sp)
    {
        setpieces.Add(sp);
    }

    public void UpdatePlayerSector(Sector sector)
    {
        if (!sectors.Contains(sector))
        {
            RegisterSector(sector);
        }

        currentPlayerSector = sector;
        lastKnownSector = currentPlayerSector;

    }

    public void ExitPlayerSector()
    {
        currentPlayerSector = null;
    }

    public void TriggerSetpiece()
    {
        Sector searchSector = currentPlayerSector == null ? lastKnownSector : currentPlayerSector;
        if  (currentPlayerSector == null)
        {
            return;
        }
        List<Setpiece> setpiecesInSector = setpieces.Where(sp => sp.sector == searchSector).ToList();
        int rn = Random.Range(0, setpiecesInSector.Count);
        Setpiece spToTrigger = setpiecesInSector[rn];
        spToTrigger.TriggerSetpieceEvent();
    }

}
