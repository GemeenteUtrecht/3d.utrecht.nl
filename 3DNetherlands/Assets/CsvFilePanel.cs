using Netherlands3D;
using Netherlands3D.Interface.Search;
using Netherlands3D.Interface.SidePanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CsvFilePanel : MonoBehaviour
{
    private bool isInited;
    private List<string[]> csvdata;
    private string[] columns;
    public GameObject marker;
    public Transform GeneratedFieldsContainer;

    public string csvfile;
    public int longIndex;
    public int latIndex;
    public int titleIndex;
    public int summaryIndex;

    private Dictionary<string,bool> selectedColumnsToDisplay = new System.Collections.Generic.Dictionary<string,bool>();


    InputField inputfield;
    List<ProjectPlanning> projectPlanningList;
    string csv;


    IEnumerator GetCsvFromWebserver(string filename)
    {
        //Uri baseUri = new Uri(Config.activeConfiguration.webserverRootPath);

        Uri baseUri = new Uri(@"http://3d.utrecht.nl/");
        var uri = new Uri(baseUri, filename);

        using (UnityWebRequest www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                csv = www.downloadHandler.text;
                csvdata = CsvParser.ReadLines(csv, 0);
                columns = csvdata[0];
                
                PropertiesPanel.Instance.AddLabel("Kolom X-coordinaat");
                PropertiesPanel.Instance.AddActionDropdown(columns, (action) =>
                {
                    Debug.Log($"xcoordinate: {action}");
                }, "");

                PropertiesPanel.Instance.AddLabel("Kolom y-coordinaat");
                PropertiesPanel.Instance.AddActionDropdown(csvdata[0], (action) =>
                {
                    Debug.Log($"xcoordinate: {action}");
                }, "");

                PropertiesPanel.Instance.AddSpacer(10);
                PropertiesPanel.Instance.AddLabel("Welke informatie wilt u zichtbaar maken als er op een label geklikt wordt?");
                PropertiesPanel.Instance.AddSpacer(10);

                foreach(var column in columns)
                {                    
                    selectedColumnsToDisplay.Add(column, true);
                    PropertiesPanel.Instance.AddActionCheckbox(column, true, (action) =>
                    {
                        selectedColumnsToDisplay[column] = action;                        
                    });
                }

                PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
                {
                    MapAndShow();
                    //Debug.Log("Load the csv data in the world");
                });







            }
        }
    }


    void MapAndShow()
    {
        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 1,
            summary_index = summaryIndex,
            longitude_index = longIndex,
            latitude_index = latIndex
        };

        projectPlanningList = ProjectPlanning.LoadCsv(csv, mapping);

        ShowAll();

        PropertiesPanel.Instance.ClearGeneratedFields();

        PropertiesPanel.Instance.AddLabel("Csv opgeslagen");

        //show saved csv from localstorage

    }


    void ShowAll()
    {
        int count = 0;
        foreach (var project in projectPlanningList)
        {
            count++;
            var search = Instantiate(marker);
            var billboard = search.GetComponent<Billboard>();
            billboard.Index = count;
            billboard.ClickAction = (action =>
            {
                Show(action);
            });

            var pos = ConvertCoordinates.CoordConvert.RDtoUnity(new Vector3((float)project.x, (float)project.y, 7));

            search.transform.position = pos;
        }
    }

    void Show(int index)
    {
        Debug.Log($"csv file click index: {index}" );

        PropertiesPanel.Instance.ClearGeneratedFields();
        var row = csvdata[index];
        
        for(int i = 0; i< row.Length; i++)
        {
            var column = columns[i];
            var text = row[i];

            if (selectedColumnsToDisplay[column])
            {
                if (text.StartsWith("http"))
                {
                    PropertiesPanel.Instance.AddLink(text, text);
                }
                else
                {
                    PropertiesPanel.Instance.AddTextfield(text);
                }
                
                PropertiesPanel.Instance.AddSpacer(10);
            }
        }




    }
    

    private void OnEnable()
    {
        if (isInited) return;
        isInited = true;

        PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
        
        inputfield = PropertiesPanel.Instance.AddInputText();

        PropertiesPanel.Instance.AddSpacer(20);

        PropertiesPanel.Instance.AddActionButtonBig("Laad csv bestand", (action) => {

            var csvurl = inputfield.text;

            if (string.IsNullOrEmpty(csvurl)) return;
            

            StartCoroutine(GetCsvFromWebserver($"csvfiles/{csvfile}"));
        });
        
    }

}
