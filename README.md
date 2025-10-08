# UnityGoogleSheetsDatabase
Unity Editor tool, which helps design game-logic database (such as characters, weapons, items stats etc.) using Google Spreadsheets.

# How to
## Install the Package
### via Package Manager
To install this package using Unity Package Manager simply insert this address:
```
https://github.com/MazinGames/UnityGoogleSheetsDatabase.git
```

or for specific versions
```
https://github.com/MazinGames/UnityGoogleSheetsDatabase.git#1.0.0
```

## Setup the Spreadsheet

Create your Spreadsheet and make sure you enable access via link.

## Setup the Container

Define your custom DataContainer and any amount of Data classes as shown below:
```
using MazinGames.GoogleSheetsDatabase;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Custom/DataContainer")]
public class DataContainer : DataContainerBase
{
    [PageName("SomeSpreadsheetPage")]
    public List<ExampleData> ExampleData;
}

[System.Serializable]
public class ExampleData
{
    public string Id;

    public float SomeFloat;
    public int SomeInt;
    public string SomeString;
}
```
_**Important!** Make sure you spell variables names exactly as columns headers in the spreadsheet._

