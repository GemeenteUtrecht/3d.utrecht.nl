using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class ProjectPlanningMapping
{
    public int? startAtRow;
    public int? title_index;
    public int? summary_index;
    public int? image_url_index;
    public int? yearstart_index;
    public int? yearend_index;
    public int? x_index;
    public int? y_index;
    public int? longitude_index;
    public int? latitude_index;
}

public class ProjectPlanning
{
    public string title;
    public string summary;
    public string image_url;
    public int yearstart;
    public int yearend;
    public double x;
    public double y;

    public static List<ProjectPlanning> LoadCsv(string csv, ProjectPlanningMapping mapping)
    {
        if (mapping.startAtRow == null) throw new Exception("mapping.startAtRow is not set");

        var lines = CsvParser.ReadLines(csv, mapping.startAtRow.Value);

       var projects = new List<ProjectPlanning>();

        foreach(var columns in lines)
        {
            if (columns.Length < 3)
            {
                Debug.Log($"A project needs at least 3 columns: {columns[0]}");
                continue;
                throw new Exception("A project needs at least 3 columns ");
            }

            var project = new ProjectPlanning();
                                   
            project.title = mapping.title_index != null ? columns[mapping.title_index.Value] : "";
            project.summary = mapping.summary_index != null ? columns[mapping.summary_index.Value] : "";
            project.image_url = mapping.image_url_index != null ? columns[mapping.image_url_index.Value] : "";
            project.yearstart = mapping.yearstart_index != null ? int.Parse(columns[mapping.yearstart_index.Value]) : 0;
            project.yearend = mapping.yearend_index != null ? int.Parse(columns[mapping.yearend_index.Value]) : 0;

            if (mapping.longitude_index != null && mapping.latitude_index != null)
            {
                try
                {
                    var lon_index = mapping.longitude_index.Value;
                    var lat_index = mapping.latitude_index.Value;
                    var lon = double.Parse(columns[lon_index]);
                    var lat = double.Parse(columns[lat_index]);
                    var rd = ConvertCoordinates.CoordConvert.WGS84toRD(lon, lat);
                    project.x = rd.x;
                    project.y = rd.y;
                }
                catch
                {
                    Debug.Log("Error parsing the x/y coordinates");
                }
            }
            else
            {
                project.x = double.Parse(columns[mapping.x_index.Value]);
                project.y = double.Parse(columns[mapping.y_index.Value]);
            }

            projects.Add(project);
            
        }

        return projects;
    }
}

