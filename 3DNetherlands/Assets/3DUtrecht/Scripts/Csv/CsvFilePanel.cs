using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Interface.Search;
using Netherlands3D.Interface.SidePanel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CsvFilePanel : MonoBehaviour
{
    public GameObject marker;
    public Transform GeneratedFieldsContainer;

    private bool isInited;
    private Dictionary<string,bool> selectedColumnsToDisplay = new System.Collections.Generic.Dictionary<string,bool>();
    private InputField inputfield;
    private string csv;
    private CsvGeoLocation csvGeoLocation;


    IEnumerator GetCsvFromWebserver(string url)
    {        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                csv = www.downloadHandler.text;
                csvGeoLocation = new CsvGeoLocation(csv);

                if (csvGeoLocation.CoordinateColumns.Length == 0) yield break; 

                //PropertiesPanel.Instance.AddLabel("X-coördinaat (gedetecteerd)");
                //PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.CoordinateColumns, (action) =>
                //{
                //    Debug.Log($"xcoordinate: {action}");
                //}, csvGeoLocation.XColumnName);

                //PropertiesPanel.Instance.AddLabel("Y-coördinaat (gedetecteerd)");
                //PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.CoordinateColumns, (action) =>
                //{
                //    Debug.Log($"xcoordinate: {action}");
                //}, csvGeoLocation.YColumnName);

                PropertiesPanel.Instance.AddLabel("Label");
                PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
                {
                    Debug.Log($"label: {action}");
                    csvGeoLocation.LabelColumnName = action;
                    csvGeoLocation.SetlabelIndex(action);

                }, "");

                PropertiesPanel.Instance.AddSpacer(10);
                PropertiesPanel.Instance.AddLabel("Welke informatie wilt u zichtbaar maken als er op een label geklikt wordt?");
                PropertiesPanel.Instance.AddSpacer(10);

                foreach (var column in csvGeoLocation.Columns)
                {
                    if (csvGeoLocation.CoordinateColumns.Contains(column)) continue;

                    selectedColumnsToDisplay.Add(column, true);
                    PropertiesPanel.Instance.AddActionCheckbox(column, true, (action) =>
                    {
                        selectedColumnsToDisplay[column] = action;
                    });
                }

                PropertiesPanel.Instance.AddActionButtonBig("Toon data", (action) =>
                {
                    MapAndShow();                    
                });
            }
        }
    }


    void MapAndShow()
    {
        ShowAll();

        PropertiesPanel.Instance.ClearGeneratedFields();

        PropertiesPanel.Instance.AddLabel("Csv opgeslagen");

        //show saved csv from localstorage
    }


    List<TextMesh> labels = new List<TextMesh>();

    void ShowAll()
    {
        int count = 0;

        var firstrow = csvGeoLocation.Rows[0];
        double firstrow_x = double.Parse(firstrow[csvGeoLocation.XColumnIndex]);
        bool isRd = csvGeoLocation.IsRd(firstrow_x);

        foreach (var row in csvGeoLocation.Rows)
        {
            count++;
            var search = Instantiate(marker);
            var billboard = search.GetComponent<Billboard>();
            var textmesh = search.GetComponentInChildren<TextMesh>();
            textmesh.text = row[csvGeoLocation.LabelColumnIndex];

            labels.Add(textmesh);

            billboard.Index = count;
            billboard.ClickAction = (action =>
            {
                Show(action);
            });

            double x = double.Parse(row[csvGeoLocation.XColumnIndex]);
            double y = double.Parse(row[csvGeoLocation.YColumnIndex]);

            Vector3 pos;

            if (isRd)
            {
                pos = ConvertCoordinates.CoordConvert.RDtoUnity(new Vector3RD(x, y, 7));
            }
            else
            {
                pos = ConvertCoordinates.CoordConvert.WGS84toUnity(new Vector3WGS(y, x, 7));
            }

            search.transform.position = pos;
        }
    }

    private void UpdateLabels()
    {
        for (int i = 0; i < csvGeoLocation.Rows.Count; i++) 
        {
            var row = csvGeoLocation.Rows[i];
            labels[i].text = row[csvGeoLocation.LabelColumnIndex];
        }
     }

    void Show(int index)
    {
        PropertiesPanel.Instance.ClearGeneratedFields();

        PropertiesPanel.Instance.AddLabel("Label");
        PropertiesPanel.Instance.AddActionDropdown(csvGeoLocation.ColumnsExceptCoordinates, (action) =>
        {
            Debug.Log($"label: {action}");
            csvGeoLocation.LabelColumnName = action;
            csvGeoLocation.SetlabelIndex(action);

            UpdateLabels();

        }, csvGeoLocation.LabelColumnName);


        var row = csvGeoLocation.Rows[index];

        for (int i = 0; i < row.Length; i++)
        {
            var column = csvGeoLocation.Columns[i];
            var text = row[i];

            if (selectedColumnsToDisplay.ContainsKey(column) &&  selectedColumnsToDisplay[column])
            {
                PropertiesPanel.Instance.AddLabel(column);

                if (text.StartsWith("http"))
                {
                    PropertiesPanel.Instance.AddLink(text, text);
                }
                else
                {
                    PropertiesPanel.Instance.AddActionButtonText(text, (action) =>
                    {
                        Debug.Log("Filter");
                    });
                }

                //PropertiesPanel.Instance.AddSpacer(5);
            }
        }
    }
    

    private void OnEnable()
    {
        if (isInited) return;
        isInited = true;

        PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);

        inputfield = PropertiesPanel.Instance.AddInputText();

        PropertiesPanel.Instance.AddActionButtonBig("Laad csv bestand", (action) =>
        {
            var csvurl = inputfield.text.Trim();

            if (string.IsNullOrEmpty(csvurl)) return;
           
            StartCoroutine(GetCsvFromWebserver(csvurl));
        });

        
    }

}
