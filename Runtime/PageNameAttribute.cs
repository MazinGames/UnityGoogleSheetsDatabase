using System;

namespace MazinGames.GoogleSheetsDatabase
{
    public class PageNameAttribute : Attribute
    {
        public readonly string Name;

        public PageNameAttribute(string name)
        {
            Name = name;
        }
    }
}