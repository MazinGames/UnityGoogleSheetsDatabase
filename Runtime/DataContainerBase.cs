using UnityEngine;

namespace MazinGames.GoogleSheetsDatabase
{
    public abstract class DataContainerBase : ScriptableObject
    {
        [SerializeField] [HideInInspector] public string _documentID;
    }
}