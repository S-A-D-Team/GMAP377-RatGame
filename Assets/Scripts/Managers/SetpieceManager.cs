using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SetpieceManager : MonoBehaviour
{
    public static SetpieceManager Instance { get; private set; }
    [Tooltip("Toggle on to allow setpieces to be triggered even when the player is not in their sector")]
    public bool allowGlobalActivation;
    private List<Sector> sectors = new List<Sector>();
    private List<Setpiece> setpieces = new List<Setpiece>();
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
        lastKnownSector = null;
    }
    public void SelfDestroy()
    {
        Instance = null;
        Destroy(gameObject);
    }

    public void RegisterSector(Sector sector)
    {
        sectors.Add(sector);
    }

    public void RegisterSetpiece(Setpiece sp)
    {
        setpieces.Add(sp);
    }

    public void UnregisterSetpiece(Setpiece sp)
    {
        setpieces.Remove(sp);
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
        if (allowGlobalActivation || searchSector == null)
        {
            int rn = Random.Range(0, setpieces.Count);
            Setpiece spToTrigger = setpieces[rn];
            spToTrigger.TriggerSetpieceEvent();
        }
        else
        {
            List<Setpiece> setpiecesInSector = setpieces.Where(sp => sp.sector == searchSector).ToList();
            int rn = Random.Range(0, setpiecesInSector.Count);
            Setpiece spToTrigger = setpiecesInSector[rn];
            spToTrigger.TriggerSetpieceEvent();
        }
        
    }

}
