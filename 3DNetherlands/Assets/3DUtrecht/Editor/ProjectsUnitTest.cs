using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.IO;
using System;

public class ProjectsUnitTests
{

    [Test]
    public void TestProjectsCsvParse()
    {
        var csv = @"Omschrijving;RD X;RD Y;RD;Datum aanvraag;Datum oplevering;3d object
Project 'Herziening rotonde 231';150000;350000;11/8/2021;3/2/2022;projects/423_1
Bouwplan 'De Nieuwe Stad'; 178000; 312000; 2 / 1 / 2022; 14/6/2023; projects / 423_2
Overspanning viaduct Laarderweg; 80000; 350000; 3/9/2021; 30/12/2021; projects / 423_3";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 1,
            summary_index = 0,
            x_index = 1,
            y_index = 2
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(3, projects.Count);
        Assert.AreEqual(150000, projects[0].x );
        Assert.AreEqual("Project 'Herziening rotonde 231'", projects[0].summary);

    }

    [Test]
    public void TestProjectsCsvParseOneRow()
    {
        var csv = @"rd_x;rd_y;yearstart, yearend, summary, image_url
150000;350000;2021;2022;Project Herziening rotonde 231;""http://url;234""";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 1,
            summary_index = 4,
            x_index = 0,
            y_index = 1,
            image_url_index = 5
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual(150000, projects[0].x);
        Assert.AreEqual("Project Herziening rotonde 231", projects[0].summary);
        Assert.AreEqual("http://url;234", projects[0].image_url);
    }

    [Test]
    public void TestProjectsCsvParseStartAtRow0()
    {
        var csv = @"150000;350000;2021;2022;Project Herziening rotonde 231;http://url234";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 0,
            summary_index = 4,
            x_index = 0,
            y_index = 1,
            image_url_index = 5
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual(150000, projects[0].x);
        Assert.AreEqual("Project Herziening rotonde 231", projects[0].summary);
        Assert.AreEqual("http://url234", projects[0].image_url);
    }

    [Test]
    public void TestProjectsCsvParseOneRowThreeColumns()
    {
        var csv = @"150000;350000;Dit is de summary";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 0,
            summary_index = 2,
            x_index = 0,
            y_index = 1,
            
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual(150000, projects[0].x);
        Assert.AreEqual(350000, projects[0].y);
        Assert.AreEqual("Dit is de summary", projects[0].summary);       
    }

    [Test]
    public void TestProjectsCsvParseOneRowTwoColumns()
    {
        var csv = @"150000;350000";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 0,
            x_index = 0,
            y_index = 1,
        };
        Assert.Throws<Exception>(delegate { ProjectPlanning.LoadCsv(csv, mapping); });
    }

    [Test]
    public void TestProjectsCsvParseOneRowAllProperties()
    {
        var csv = @"150000;350000;2021; 2022; Dit is de titel;Dit is de summary; http://image.png ";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 0,
            x_index = 0,
            y_index = 1,
            title_index = 4,
            summary_index = 5,            
            image_url_index = 6,
            yearstart_index = 2,
            yearend_index = 3
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual(150000, projects[0].x);
        Assert.AreEqual(350000, projects[0].y);
        Assert.AreEqual("Dit is de titel", projects[0].title);
        Assert.AreEqual("Dit is de summary", projects[0].summary);
        Assert.AreEqual("http://image.png", projects[0].image_url);
        Assert.AreEqual(2021, projects[0].yearstart);
        Assert.AreEqual(2022, projects[0].yearend);
    }

    [Test]
    public void TestProjectsCsvParseOneRowAllPropertiesDubbleQuotesAndSemicolon()
    {
        var csv = @"150000;350000;2021; 2022; Dit ""is"" de titel;Dit is de summary; ""http://image.png?param=a;abc"" ";

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 0,
            x_index = 0,
            y_index = 1,
            title_index = 4,
            summary_index = 5,
            image_url_index = 6,
            yearstart_index = 2,
            yearend_index = 3
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual(150000, projects[0].x);
        Assert.AreEqual(350000, projects[0].y);
        Assert.AreEqual(@"Dit ""is"" de titel", projects[0].title);
        Assert.AreEqual("Dit is de summary", projects[0].summary);
        Assert.AreEqual("http://image.png?param=a;abc", projects[0].image_url);
        Assert.AreEqual(2021, projects[0].yearstart);
        Assert.AreEqual(2022, projects[0].yearend);
    }

    [Test]
    public void LoadAndParseCsvFile()
    {
        string csv = File.ReadAllText(@"F:\Data\Projecten CSV\Projecten_Convert_JSON_to_CSV_CLEAN_test.csv");

        var mapping = new ProjectPlanningMapping()
        {
            startAtRow = 1,
            summary_index = 11,
            longitude_index = 1,
            latitude_index = 0
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        Assert.AreEqual(199, projects.Count );
        Assert.AreEqual(119397.34375, projects[0].x);
        Assert.AreEqual(482779.5, projects[0].y);

        Assert.AreEqual("Vanaf 2019. We gaan de A.J. Ernststraat tussen Buitenveldertselaan en Van der Boechorststraat opnieuw inrichten vanwege bouw internationale school.", projects[0].summary);

        //Assert.AreEqual("Project 'Herziening rotonde 231'", projects[0].description);

    }

    [Test]
    public void LoadAndParseCsvFileUtrecht()
    {
        string csv = File.ReadAllText(@"F:\Data\Projecten CSV\gu_programma_ruimtelijke_ontwikkeling_20200615_20200630.csv");

        var mapping = new ProjectPlanningMapping()
        {
            //title_index = 2,
            startAtRow = 1,
            summary_index = 4,
            longitude_index = 11,
            latitude_index = 12
        };
        var projects = ProjectPlanning.LoadCsv(csv, mapping);

        //Assert.AreEqual(199, projects.Count);
        //Assert.AreEqual(119397.34375, projects[0].x);
        //Assert.AreEqual(482779.5, projects[0].y);

        //Assert.AreEqual("Vanaf 2019. We gaan de A.J. Ernststraat tussen Buitenveldertselaan en Van der Boechorststraat opnieuw inrichten vanwege bouw internationale school.", projects[0].summary);

        //Assert.AreEqual("Project 'Herziening rotonde 231'", projects[0].description);

    }

}
