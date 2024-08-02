using JohnsonControls.Metasys.BasicServices;
using JohnsonControls.Metasys.BasicServices.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MetasysAttribute = JohnsonControls.Metasys.BasicServices.MetasysAttribute;



namespace MetasysServicesExampleApp.FeaturesDemo
{
  public class JsonOutputDemo
  {
    private readonly MetasysClient client;
    private readonly LogInitializer log;

    public JsonOutputDemo(MetasysClient client)
    {
      this.client = client;
      log = new LogInitializer(typeof(JsonOutputDemo));
    }

    #region CREATING A CLIENT

    private void CreateClientSignature1()
    {
      /* SNIPPET 1: START */
      var client = new MetasysClient("hostname");
      /* SNIPPET 1: END */

    }

    private void CreateClientSignature2()
    {
      /* SNIPPET 2: START */
      CultureInfo culture = new CultureInfo("it-IT");
      var client = new MetasysClient("hostname", true, ApiVersion.v2, culture);
      /* SNIPPET 2: END */
    }

    private void CreateClientSignature3()
    {
      /* SNIPPET 3: START */
      var client = new MetasysClient("hostname", logClientErrors: false);
      /* SNIPPET 3: END */
    }

    private void LogError()
    {
      /* SNIPPET 4: START */
      // Initialize Logger with your context Class
      var log = new LogInitializer(typeof(Program));
      try
      {
        // Your Try logic here...
      }
      catch (Exception ex)
      {
        log.Logger.Error(string.Format("An error occured - {0}", ex.Message));
      }
      /* SNIPPET 4: END */
    }

    private void ChangeApiVersion()
    {
      /* SNIPPET 5: START */
      // Changing Api version after creating a client
      var client = new MetasysClient("hostname", version: ApiVersion.v3);
      client.Version = ApiVersion.v2;
      /* SNIPPET 5: END */
    }

    private void ChangeHostname()
    {
      /* SNIPPET 6: START */
      // Changing Metasys Server after creating a client
      var client = new MetasysClient("hostname");
      client.Hostname = "WIN2016-VM2";
      /* SNIPPET 6: END */
    }

    #endregion

    #region LOGIN AND ACCESS TOKENS
    private void TryLogin_Refresh()
    {

      /* SNIPPET 1: START */
      // Automatically refresh token using plain credentials
      client.TryLogin("username", "password");
      // Do not automatically refresh token using plain credentials
      client.TryLogin("username", "password", false);
      /* SNIPPET 1: END */

      /* SNIPPET 2: START */
      // Read target from Credential Manager and automatically refresh token
      client.TryLogin("metasys-energy-app");
      // Read target from Credential Manager and do not refresh token
      client.TryLogin("metasys-energy-app", false);
      /* SNIPPET 2: END */

      /* SNIPPET 3: START */
      AccessToken accessToken = client.TryLogin("metasys-energy-app");
      Console.WriteLine(accessToken);
      /*
          {
            "Issuer": "metasysserver",
            "IssuedTo": "metasysapiuser",
            "Token": "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkJzR0lWelVZcjN0MkI0RGRtT1ljMTdBLVZJOCIsImtpZCI6IkJzR0lWelVZcjN0MkI0RGRtT1ljMTdBLVZJOCJ9.eyJpc3MiOiJodHRwOi8vbG9jYWxob3N0Ojk1MDYvQVBJLkF1dGhlbnRpY2F0aW9uU2VydmljZSIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6OTUwNi9BUEkuQXV0aGVudGljYXRpb25TZXJ2aWNlL3Jlc291cmNlcyIsImV4cCI6MTU4OTI5MzEzMCwibmJmIjoxNTg5MjkxMzMwLCJjbGllbnRfaWQiOiJtZXRhc3lzX3VpIiwic2NvcGUiOlsibWV0YXN5c19hcGkiLCJvZmZsaW5lX2FjY2VzcyIsIm9wZW5pZCJdLCJzdWIiOiI4ZGE4YjE4Yy1lMTk1LTRlMmMtOGU2Zi0zNTE2Zjc0ZWFhNGIiLCJhdXRoX3RpbWUiOjE1ODkyOTEzMzAsImlkcCI6Imlkc3J2IiwiVXNlcklkIjoiMSIsIlVzZXJOYW1lIjoibWV0YXN5c3N5c2FnZW50IiwiSXNBZG1pbiI6IlRydWUiLCJJc1Bhc3N3b3JkQ2hhbmdlUmVxdWlyZWQiOiJGYWxzZSIsIklzVGVybXNBbmRDb25kaXRpb25zUmVxdWlyZWQiOiJGYWxzZSIsIkxpY2Vuc2VJbmZvIjoie1wiSXNMaWNlbnNlZFwiOnRydWUsXCJMaWNlbnNlVHlwZVwiOlwiZ3JhY2VcIn0iLCJDdWx0dXJlIjoiZW4tVVMiLCJhbXIiOlsicGFzc3dvcmQiXX0.egzw1bs1831pEBWWXbBOYWGU5wFsI3sEnL7RgCIHHbHmcxtpPPqLq54znpoUoLFrMUeymZj5rkrt_mF-CNIpCE3halZNAH-CR1U46LTZi5CMaDfYlP-wHxikAGV5GwFjlHjGNOUaFtd7n4yC5sH08pHQfXXD5gKDm_FVMfUJXAo-E8gmrkU0wMn5U2FRyQyj7Yhq6jaj7MPTF__Xz46sG3WtDr45WK2NmuwiLDv408URZ5fJxlMngRpjSIONHVAIwna_H0AguHiIELkvuRVYcRqIH5kb1YdFt-3fsnTV9xwpozZZ44dh-4I7x466I-UGlLHAnScWILUbPcpRNWm0Uw",
            "Expires": "2020-05-12T14:18:51Z"
          }
       */
      /* SNIPPET 3: END */

      /* SNIPPET 4: START */
      client.Refresh();
      /* SNIPPET 4: END */
    }
    #endregion

    #region GET AN OBJECT ID
    private void GetObjectIdentifier()
    {
      /* SNIPPET 1: START */
      var objectId = client.GetObjectIdentifier("WIN-21DJ9JV9QH6:EECMI-NCE25-2/FCB.10FEC11 - V6 Unit.E4 Network Outdoor Temperature");
      Console.WriteLine(objectId);
      /*
      d5d96cd3-db4a-52e0-affd-8bc3393c30ec
       */
      /* SNIPPET 1: END */
    }
    #endregion

    #region GET A PROPERTY
    private void ReadProperty(ObjectId objectId)
    {
      /* SNIPPET 1: START */
      Variant property = client.ReadProperty(objectId, "presentValue");
      Console.WriteLine(property);
      /*
         {
            "StringValue": "72",
            "StringValueEnumerationKey": null,
            "NumericValue": 72.0,
            "BooleanValue": true,
            "ArrayValue": null,
            "Attribute": "presentValue",
            "Id": "f1469e25-c46c-5009-b92e-d82603e742a4",
            "Reliability": "Reliable",
            "ReliabilityEnumerationKey": "reliabilityEnumSet.reliable",
            "Priority": "0 (No Priority)",
            "PriorityEnumerationKey": "writePriorityEnumSet.priorityNone",
            "IsReliable": true
          }
       */
      /* SNIPPET 1: END */
    }
    private void ReadPropertyMultiple(ObjectId id1, ObjectId id2)
    {
      /* SNIPPET 2: START */
      List<ObjectId> ids = [id1, id2];
      List<string> attributes = new List<string> { "name", "description", "presentValue" };
      IEnumerable<VariantMultiple> results = client.ReadPropertyMultiple(ids, attributes);
      VariantMultiple multiple1 = results.FindById(id1);
      Console.WriteLine(multiple1);
      /*
          {
              "Id": "d5d96cd3-db4a-52e0-affd-8bc3393c30ec",
              "Variants": [
              {
                  "StringValue": "ZN-T",
                  "StringValueEnumerationKey": "ZN-T",
                  "NumericValue": 0.0,
                  "BooleanValue": false,
                  "ArrayValue": null,
                  "Attribute": "name",
                  "Id": "d5d96cd3-db4a-52e0-affd-8bc3393c30ec",
                  "Reliability": "Reliable",
                  "ReliabilityEnumerationKey": "reliabilityEnumSet.reliable",
                  "Priority": null,
                  "PriorityEnumerationKey": null,
                  "IsReliable": true
              },
              {
                  "StringValue": "Zone Temperature",
                  "StringValueEnumerationKey": "Zone Temperature",
                  "NumericValue": 0.0,
                  "BooleanValue": false,
                  "ArrayValue": null,
                  "Attribute": "description",
                  "Id": "d5d96cd3-db4a-52e0-affd-8bc3393c30ec",
                  "Reliability": "Reliable",
                  "ReliabilityEnumerationKey": "reliabilityEnumSet.reliable",
                  "Priority": null,
                  "PriorityEnumerationKey": null,
                  "IsReliable": true
              }
              ]
          }
      */
      Variant multiple1Description = multiple1.FindAttributeByName("description");
      /* SNIPPET 2: END */
    }
    #endregion

    #region WRITE A PROPERTY
    private void WriteProperty()
    {
      /* SNIPPET 1: START */
      var id = client.GetObjectIdentifier("WIN-21DJ9JV9QH6:EECMI-NCE25-2/FCB.10FEC11 - V6 Unit.E4 Network Outdoor Temperature");
      client.WriteProperty(id, "description", "This is an AV.");
      /* SNIPPET 1: END */
    }
    private void WritePropertyMultiple(ObjectId id1, ObjectId id2)
    {
      /* SNIPPET 2: START */
      List<ObjectId> ids = [id1, id2];
      // Write to many attributes values using a list of tuples
      List<(string, object)> attributesList = new List<(string, object)> { ("description", "This is an AV.") };
      client.WritePropertyMultiple(ids, attributesList);
      // Write to many attributes values using a dictionary of attribute/value pairs
      Dictionary<string, object> attributesDictionary = new Dictionary<string, object> { { "description", "This is an AV." } };
      client.WritePropertyMultiple(ids, attributesDictionary);
      /* SNIPPET 2: END */
    }
    #endregion

    #region GET AND SEND COMMANDS
    private void GetCommands(ObjectId objectId)
    {
      /* SNIPPET 1: START */
      List<Command> commands = client.GetCommands(objectId).ToList();
      Command command = commands.FindById("adjust");
      Console.WriteLine(command);
      /*
          {
            "Title": "Adjust",
            "TitleEnumerationKey": "commandIdEnumSet.adjustCommand",
            "CommandId": "Adjust",
            "Items": [
              {
                "Title": "Value",
                "Type": "number",
                "EnumerationValues": null,
                "Minimum": null,
                "Maximum": null
              }
            ]
          }
      */
      /* SNIPPET 1: END */
    }
    private void SendCommands(ObjectId objectId, List<Command> commands)
    {
      /* SNIPPET 2: START */
      Command adjust = commands.FindById("adjust");
      Command operatorOverride = commands.FindById("operatorOverride");
      Command release = commands.FindById("release");
      Console.WriteLine(release);
      /*
        {
            "Title": "Release",
            "TitleEnumerationKey": "commandIdEnumSet.releaseCommand",
            "CommandId": "Release",
            "Items": [
              {
                "Title": "oneOf",
                "Type": "enum",
                "EnumerationValues": [
                  {
                    "Title": "Present Value",
                    "TitleEnumerationKey": "attributeEnumSet.presentValue"
                  }
                ],
                "Minimum": 1.0,
                "Maximum": 1.0
              },
              {
                "Title": "oneOf",
                "Type": "enum",
                "EnumerationValues": [
                  {
                    "Title": "0 (No Priority)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityNone"
                  },
                  {
                    "Title": "1 (Manual Life Safety)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityManualEmergency"
                  },
                  {
                    "Title": "2 (Auto Life Safety)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityFireApplications"
                  },
                  {
                    "Title": "3 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority3"
                  },
                  {
                    "Title": "4 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority4"
                  },
                  {
                    "Title": "5 (Critical Equipment)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityCriticalEquipment"
                  },
                  {
                    "Title": "6 (Minimum On Off)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityMinimumOnOff"
                  },
                  {
                    "Title": "7 (Heavy Equip Delay)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityHeavyEquipDelay"
                  },
                  {
                    "Title": "8 (Operator Override)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityOperatorOverride"
                  },
                  {
                    "Title": "9 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority9"
                  },
                  {
                    "Title": "10 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority10"
                  },
                  {
                    "Title": "11 (Demand Limiting)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityDemandLimiting"
                  },
                  {
                    "Title": "12 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority12"
                  },
                  {
                    "Title": "13 (Load Rolling)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityLoadRolling"
                  },
                  {
                    "Title": "14 (Application)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priority14"
                  },
                  {
                    "Title": "15 (Scheduling)",
                    "TitleEnumerationKey": "writePriorityEnumSet.prioritySchedulingOst"
                  },
                  {
                    "Title": "16 (Default)",
                    "TitleEnumerationKey": "writePriorityEnumSet.priorityDefault"
                  }
                ],
                "Minimum": 1.0,
                "Maximum": 1.0
              }
            ]
          }
      */
      var list1 = new List<object> { 70 };
      client.SendCommand(objectId, adjust.CommandId, list1);
      var list2 = new List<object> { 75 };
      client.SendCommand(objectId, operatorOverride.CommandId, list2);
      var list3 = new List<object> { "attributeEnumSet.presentValue", "writePriorityEnumSet.priorityNone" };
      client.SendCommand(objectId, release.CommandId, list3);
      /* SNIPPET 2: END */
    }
    #endregion

    #region GET NETWORK DEVICES AND OBJECT CHILDREN
    private void GetNetworkDeviceTypes_GetNetworkDevices()
    {
      /* SNIPPET 1: START */
      List<MetasysObjectType> types = client.GetNetworkDeviceTypes().ToList();
      Console.WriteLine(types[0]);
      /*
      {
        "Description": "NAE55-NIE59",
        "DescriptionEnumerationKey": "objectTypeEnumSet.n50Class",
        "Id": 185
      }
      */
      int type1 = types[0].Id;
      List<MetasysObject> devices = client.GetNetworkDevices(type1.ToString()).ToList();
      MetasysObject device = devices.LastOrDefault();
      Console.WriteLine(device);
      /*
          {
            "ItemReference": "Win2016-VM2:vNAE2343996",
            "Id": "142558f8-c4c7-5f89-be97-d806adb72053",
            "Name": "vNAE2343996",
            "Description": "",
            "Type": null,
            "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/185",
            "Category": null,
            "Children": [],
            "ChildrenCount": 0
          }
      */

      List<MetasysObject> devices2 = client.GetNetworkDevices(NetworkDeviceTypeEnum.SNC).ToList();
      MetasysObject device2 = devices2.LastOrDefault();
      Console.WriteLine(device2);
      /*
          {
            "ItemReference": "WIN-21DJ9JV9QH6:EECMI-SNC-KNX",
            "Id": "69b3c2a5-1090-5418-afd9-5efc7186e42f",
            "Name": "EECMI-SNC-KNX",
            "Description": "",
            "Type": null,
            "TypeUrl": "https://win-21dj9jv9qh6/api/v3/enumSets/508/members/448",
            "ObjectType": null,
            "Category": null,
            "Children": [],
            "ChildrenCount": 0
          }
      */


      /* SNIPPET 1: END */
    }

    private void GetObjects()
    {
      /* SNIPPET 2: START */
      var parentId = client.GetObjectIdentifier("WIN-21DJ9JV9QH6:EECMI-NCE25-2/FCB");
      // Get direct children (1 level)
      List<MetasysObject> directChildren = client.GetObjects(parentId).ToList();
      MetasysObject lastChild = directChildren.LastOrDefault();
      Console.WriteLine(lastChild);
      /*
          {
            "ItemReference": "Win2016-VM2:vNAE2343996/Field Bus MSTP1.VAV-08.ZN-T",
            "Id": "d5d96cd3-db4a-52e0-affd-8bc3393c30ec",
            "Name": "ZN-T",
            "Description": null,
            "Type": null,
            "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/601",
            "Category": null,
            "Children": [],
            "ChildrenCount": 0
          }
         */
      // Get direct children (1 level) with internal objects
      directChildren = client.GetObjects(parentId, includeInternalObjects: true).ToList();
      // Get descendant for 2 levels (it could take long time, depending on the number of objects)
      List<MetasysObject> level2Descendants = client.GetObjects(parentId, 2).ToList();
      MetasysObject level1Parent = level2Descendants.FindByName("Time");
      Console.WriteLine(level1Parent);
      /*
        {
          "ItemReference": "Win2016-VM2:vNAE2343996/WeatherForecast.Time",
          "Id": "22bb952e-7557-5de9-b7e5-dce39e21addd",
          "Name": "Time",
          "Description": null,
          "Type": null,
          "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/176",
          "Category": null,
          "Children": [
          {
              "ItemReference": "Win2016-VM2:vNAE2343996/WeatherForecast.Time.Day",
              "Id": "5886a93f-9260-553c-995e-6a65374de85d",
              "Name": "Day",
              "Description": null,
              "Type": null,
              "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/165",
              "Category": null,
              "Children": [],
              "ChildrenCount": 0
          },
          {
              "ItemReference": "Win2016-VM2:vNAE2343996/WeatherForecast.Time.Hour",
              "Id": "6a50d3af-d0a2-537c-a2f7-9c1b5f271cc5",
              "Name": "Hour",
              "Description": null,
              "Type": null,
              "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/165",
              "Category": null,
              "Children": [],
              "ChildrenCount": 0
          },
          {
              "ItemReference": "Win2016-VM2:vNAE2343996/WeatherForecast.Time.Minute",
              "Id": "19a53f38-2fd7-5ac3-a12c-f3b9704ac194",
              "Name": "Minute",
              "Description": null,
              "Type": null,
              "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/165",
              "Category": null,
              "Children": [],
              "ChildrenCount": 0
          },
          {
              "ItemReference": "Win2016-VM2:vNAE2343996/WeatherForecast.Time.Year",
              "Id": "74dfc214-22c1-57a7-ace5-606636d0049c",
              "Name": "Year",
              "Description": null,
              "Type": null,
              "TypeUrl": "https://win2016-vm2/api/v2/enumSets/508/members/165",
              "Category": null,
              "Children": [],
              "ChildrenCount": 0
          }
          ],
          "ChildrenCount": 4
      }
         */
      /* SNIPPET 2: END */
    }
    #endregion

    #region LOCALIZATION OF METASYS ENUMERATIONS
    private void Localize()
    {
      /* SNIPPET 1: START */
      client.Culture = new CultureInfo("en-US");
      /* SNIPPET 1: END */

      /* SNIPPET 2: START */
      // Access from the client object (uses client.Culture as input)
      string translated = client.Localize("reliabilityEnumSet.reliable");
      Console.WriteLine(translated);
      /*
          Reliable
        */
      // Access without instantiating a client
      translated = ResourceManager.Localize("reliabilityEnumSet.reliable",
          new CultureInfo("it-IT"));
      Console.WriteLine(translated);
      /*
          Affidabile
        */
      /* SNIPPET 2: END */
    }
    #endregion

    #region SPACES AND EQUIPMENT
    private void GetSpaces()
    {
      /* SNIPPET 1: START */
      // Retrieves the list of Buildings using SpaceTypeEnum helper
      List<MetasysObject> buildings = client.GetSpaces(SpaceTypeEnum.Building).ToList();
      MetasysObject building = buildings.LastOrDefault();
      Console.WriteLine(building);
      /*
          {
            "ItemReference": "Win2016-VM2:Win2016-VM2/JCI.Building 1",
            "Id": "164aaba2-0fb3-5b5d-bfe9-49cf6b797c93",
            "Name": "North America (BACnet)",
            "Description": null,
            "Type": 2,
            "TypeUrl": "https://win2016-vm2/api/v2/enumSets/1766/members/1",
            "Category": "Building",
            "Children": [],
            "ChildrenCount": 0
          }
       */
      // Retrieves all the spaces in a flat hierarchy
      List<MetasysObject> spaces = client.GetSpaces().ToList();
      MetasysObject firstSpace = spaces.FirstOrDefault();
      Console.WriteLine(firstSpace);
      /*
          {
            "ItemReference": "Win2016-VM2:Win2016-VM2/JCI.Building 1.Floor 1.Milwaukee.507 E Michigan Street Campus",
            "Id": "896ba096-db3c-5038-8505-636785906cca",
            "Name": "507 E Michigan Street Campus",
            "Description": null,
            "Type": 2,
            "TypeUrl": "https://win2016-vm2/api/v2/enumSets/1766/members/3",
            "Category": "Room",
            "Children": [],
            "ChildrenCount": 0
          }
       */
      /* SNIPPET 1: END */
    }

    private void GetSpaceTypes()
    {
      /* SNIPPET 2: START */
      IEnumerable<MetasysObjectType> spaceTypes = client.GetSpaceTypes();
      foreach (var type in spaceTypes)
      {
        Console.WriteLine(type);
      }
      /*
          {
            "Description": "Building",
            "DescriptionEnumerationKey": "Building",
            "Id": 1
          }
          {
            "Description": "Floor",
            "DescriptionEnumerationKey": "Floor",
            "Id": 2
          }
          {
            "Description": "Generic",
            "DescriptionEnumerationKey": "Generic",
            "Id": 0
          }
          {
            "Description": "Room",
            "DescriptionEnumerationKey": "Room",
            "Id": 3
          }
       */
      /* SNIPPET 2: END */
    }

    private void GetEquipment()
    {
      /* SNIPPET 3: START */
      List<MetasysObject> equipment = client.GetEquipment().ToList();
      MetasysObject sampleEquipment = equipment.FirstOrDefault();
      Console.WriteLine(sampleEquipment);
      /*
           {
            "ItemReference": "Win2016-VM2:Win2016-VM2/equipment.vNAE2343947.Field Bus MSTP1.AHU-07",
            "Id": "6c6e18b8-015f-572a-814c-1e5d66142850",
            "Name": "AHU-07",
            "Description": null,
            "Type": 3,
            "TypeUrl": null,
            "Category": null,
            "Children": [],
            "ChildrenCount": 0
          }
       */
      /* SNIPPET 3: END */

    }

    private void GetSpaceEquipment_GetSpaceChildren(Guid spaceId)
    {
      /* SNIPPET 4: START */
      IEnumerable<MetasysObject> spaceEquipment = client.GetSpaceEquipment(spaceId);

      IEnumerable<MetasysObject> spaceChildren = client.GetSpaceChildren(spaceId);
      MetasysObject sampleSpaceEquipment = spaceEquipment.FirstOrDefault();
      /* SNIPPET 4: END */
    }

    private void GetEquipmentPoints(MetasysObject sampleEquipment)
    {
      /* SNIPPET 5: START */
      IEnumerable<MetasysPoint> equipmentPoints = client.GetEquipmentPoints(new Guid(sampleEquipment.Id));
      MetasysPoint point = equipmentPoints.FindByShortName("Analog Input-1");
      string presentValue = point.PresentValue?.StringValue;
      Console.WriteLine(point);
      /*
          {
            "EquipmentName": "AHU-07",
            "ShortName": "CLG-O",
            "Label": "Cooling Output",
            "Category": "",
            "IsDisplayData": true,
            "ObjectId": "9dd107cf-e8dc-583a-9557-813395ae1971",
            "AttributeUrl": "https://win2016-vm2/api/v2/enumSets/509/members/85",
            "ObjectUrl": "https://win2016-vm2/api/v2/objects/9dd107cf-e8dc-583a-9557-813395ae1971",
            "PresentValue": {
              "StringValue": "0",
              "StringValueEnumerationKey": null,
              "NumericValue": 0.0,
              "BooleanValue": false,
              "ArrayValue": null,
              "Attribute": "presentValue",
              "Id": "9dd107cf-e8dc-583a-9557-813395ae1971",
              "Reliability": "Reliable",
              "ReliabilityEnumerationKey": "reliabilityEnumSet.reliable",
              "Priority": "0 (No Priority)",
              "PriorityEnumerationKey": "writePriorityEnumSet.priorityNone",
              "IsReliable": true
            }
          }
       */
      /* SNIPPET 5: END */
    }
    #endregion

    #region ALARMS
    private void GetAlarms()
    {
      /* SNIPPET 1: START */
      AlarmFilter alarmFilter = new AlarmFilter
      {
        StartTime = new DateTime(2020, 6, 1),
        EndTime = new DateTime(2020, 6, 18),
        ExcludeAcknowledged = true
      };
      PagedResult<Alarm> alarmsPager = client.Alarms.Get(alarmFilter);
      // Prints the number of records fetched and paging information
      Console.WriteLine("Total:" + alarmsPager.Total);
      Console.WriteLine("Current page:" + alarmsPager.CurrentPage);
      Console.WriteLine("Page size:" + alarmsPager.PageSize);
      Console.WriteLine("Pages:" + alarmsPager.PageCount);
      /* Console Output: Start
          Total:4611
          Current page:1
          Page size:100
          Pages:47
      Console Output: End */
      Alarm alarm = alarmsPager.Items.ElementAt(0);
      Console.WriteLine(alarm);
      /* Console Output: Start
      {
        "Self": "https://win-21dj9jv9qh6/api/v3/alarms/e03d81f9-69de-48e8-92d7-81167df19f6c",
        "Id": "e03d81f9-69de-48e8-92d7-81167df19f6c",
        "ItemReference": "WIN-21DJ9JV9QH6:EECMI-NCE25-3",
        "Name": "EECMI-NCE25-3",
        "Message": "WIN-21DJ9JV9QH6:EECMI-NCE25-3 is offline",
        "IsAckRequired": true,
        "TypeUrl": null,
        "Type": "alarmValueEnumSet.avOffline",
        "Priority": 106,
        "TriggerValue": {
          "Value": "",
          "UnitsUrl": null,
          "Units": "unitEnumSet.noUnits"
        },
        "CreationTime": "2020-06-17T11:22:30Z",
        "IsAcknowledged": false,
        "IsDiscarded": false,
        "CategoryUrl": null,
        "Category": "objectCategoryEnumSet.systemCategory",
        "ObjectUrl": "https://win-21dj9jv9qh6/api/v3/objects/e03d81f9-69de-48e8-92d7-81167df19f6c",
        "AnnotationsUrl": "https://win-21dj9jv9qh6/api/v3/alarms/e03d81f9-69de-48e8-92d7-81167df19f6c/annotations"
      }
      Console Output: End */
      /* SNIPPET 1: END */
    }

    private void GetSingleAlarm_GetAlarmsForAnObject_GetAlarmsForNetworkDevice(Alarm alarm, Guid objectId, AlarmFilter alarmFilter, MetasysObject device)
    {
      /* SNIPPET 2: START */
      Alarm singleAlarm = client.Alarms.FindById(alarm.Id);
      PagedResult<Alarm> objectAlarms = client.Alarms.GetForObject(objectId, alarmFilter);
      PagedResult<Alarm> deviceAlarms = client.Alarms.GetForNetworkDevice(device.Id, alarmFilter);
      /* SNIPPET 2: END */
    }

    private void GetAlarmsAnnotation(Alarm alarm)
    {
      /* SNIPPET 3: START */
      IEnumerable<AlarmAnnotation> annotations = client.Alarms.GetAnnotations(alarm.Id);
      AlarmAnnotation firstAnnotation = annotations.FirstOrDefault();
      Console.WriteLine(firstAnnotation);
      /*
      {
          "AlarmUrl": "https://win-ervotujej94/api/v2/alarms/f0f64d5c-b70e-8754-836c-1ac99182f4e4",
          "Text": "Test Annotation 00",
          "User": "metasyssysagent",
          "CreationTime": "2020-05-27T06:21:31Z",
          "Action": "none"
      }
      */
      /* SNIPPET 3: END */
    }
    #endregion

    #region TRENDS
    private void GetTrendedAttributes_GetSamples()
    {
      /* SNIPPET 1: START */
      var trendedObjectId = client.GetObjectIdentifier("WIN-21DJ9JV9QH6:EECMI-NCE25-2/FCB.10FEC11 - V6 Unit.E4 Network Outdoor Temperature");
      // Get attributes where trend extension is configured
      List<MetasysAttribute> trendedAttributes = client.Trends.GetTrendedAttributes(trendedObjectId);
      int attributeId = trendedAttributes[0].Id;
      TimeFilter timeFilter = new TimeFilter
      {
        StartTime = new DateTime(2020, 6, 5),
        EndTime = new DateTime(2020, 6, 6)
      };
      PagedResult<Sample> samplesPager = client.Trends.GetSamples(trendedObjectId, attributeId, timeFilter);
      // Prints the number of records fetched and paging information
      Console.WriteLine("Total:" + samplesPager.Total);
      Console.WriteLine("Current page:" + samplesPager.CurrentPage);
      Console.WriteLine("Page size:" + samplesPager.PageSize);
      Console.WriteLine("Pages:" + samplesPager.PageCount);
      /*
          Total:145
          Current page:1
          Page size:100
          Pages:2
       */
      Sample firstSample = samplesPager.Items.FirstOrDefault();
      Console.WriteLine(firstSample);
      /*
          {
            "Value": 82.0,
            "Unit": "deg F",
            "Timestamp": "2020-05-12T05:00:00Z",
            "IsReliable": true
          }
        */
      /* SNIPPET 1: END */
    }
    #endregion

    #region AUDITS
    private void GetAudits()
    {
      /* SNIPPET 1: START */
      AuditFilter auditFilter = new AuditFilter
      {
        StartTime = new DateTime(2020, 06, 01),
        EndTime = new DateTime(2020, 06, 24),
        OriginApplications = OriginApplicationsEnum.SystemSecurity | OriginApplicationsEnum.AuditTrails,
        ActionTypes = ActionTypeEnum.Subsystem | ActionTypeEnum.Command
      };
      PagedResult<Audit> auditsPager = client.Audits.Get(auditFilter);
      // Prints the number of records fetched and paging information
      Console.WriteLine("Total:" + auditsPager.Total);
      Console.WriteLine("Current page:" + auditsPager.CurrentPage);
      Console.WriteLine("Page size:" + auditsPager.PageSize);
      Console.WriteLine("Pages:" + auditsPager.PageCount);
      /*
          Total:323
          Current page:1
          Page size:100
          Pages:4
       */
      Audit audit = auditsPager.Items.FirstOrDefault();
      Console.WriteLine(audit);
      /*
      {
        "Id": "8e3b3738-2f5f-494d-bde1-fac15da28c86",
        "CreationTime": "2020-06-23T16:45:54.697Z",
        "ActionTypeUrl": null,
        "ActionType": "auditActionTypeEnumSet.subsystemAuditActionType",
        "Discarded": false,
        "StatusUrl": null,
        "Status": null,
        "PreData": null,
        "PostData": "::1",
        "Parameters": "[]",
        "ErrorString": null,
        "User": "testuser",
        "Signature": null,
        "ObjectUrl": "https://win-21dj9jv9qh6/api/v3/objects/1949c631-7823-5230-b951-aae3f8c9d64a",
        "AnnotationsUrl": null,
        "Legacy": {
          "FullyQualifiedItemReference": "WIN-21DJ9JV9QH6:WIN-21DJ9JV9QH6",
          "ItemName": "EECMI-ADS11",
          "ClassLevel": "auditClassesEnumSet.userActionAuditClass",
          "OriginApplication": "auditOriginAppEnumSet.systemSecurityAuditOriginApp",
          "Description": "auditTrailStringsEnumSet.atstrSecurityUserLoginSuccessful"
        },
        "Self": "https://win-21dj9jv9qh6/api/v3/audits/8e3b3738-2f5f-494d-bde1-fac15da28c86"
      }
       */
      /* SNIPPET 1: END */
    }

    private void GetSingleAudit_GetAuditsForAnObject(Audit audit, Guid objectId, AuditFilter auditFilter)
    {
      /* SNIPPET 2: START */
      Audit singleAudit = client.Audits.FindById(audit.Id);
      PagedResult<Audit> objectAudits = client.Audits.GetForObject(objectId, auditFilter);
      /* SNIPPET 2: END */
    }

    private void GetAuditsAnnotation(Audit audit)
    {
      /* SNIPPET 3: START */
      Guid auditId = new Guid("f798591b-e6d6-441f-8f7b-596fc2e6c003");
      IEnumerable<AuditAnnotation> annotations = client.Audits.GetAnnotations(auditId);
      AuditAnnotation firstAnnotation = annotations.FirstOrDefault();
      Console.WriteLine(firstAnnotation);
      /*
      {
          "auditUrl": "https://win-ervotujej94/api/v2/audits/40aff6ec-ecb2-4b65-a504-0ac659756956",
          "creationTime": "2020-06-05T15:58:40.407Z",
          "user": "MetasysSysAgent",
          "text": "TEST AUDIT ANNOTATION 02",
          "signature": null,
          "action": "none"
      }
      */
      /* SNIPPET 3: END */
    }

    private void DiscardSingleAudit(Audit audit)
    {

      /* SNIPPET 4: START */
      string annotationText = "Reason why the audit has been discarded";
      client.Audits.Discard(audit.Id, annotationText);
      /* SNIPPET 4: END */
    }

    private void AddAuditAnnotation(Audit audit)
    {
      /* SNIPPET 5: START */
      string annotationText = "This is the text of the annotation";
      client.Audits.AddAnnotation(audit.Id, annotationText);
      /* SNIPPET 5: END */
    }

    private void AddAuditAnnotationMultiple()
    {
      /* SNIPPET 6: START */
      var requests = new List<BatchRequestParam>();
      BatchRequestParam request1 = new BatchRequestParam { ObjectId = new Guid("f179a59c-ce36-47e1-afa6-f873b80259ed"), Resource = "THIS IS THE FIRST AUDIT ANNOTATION" };
      requests.Add(request1);
      BatchRequestParam request2 = new BatchRequestParam { ObjectId = new Guid("bccc567c-300b-4d38-894c-104cdd6e2836"), Resource = "THIS IS THE SECOND AUDIT ANNOTATION" };
      requests.Add(request2);

      IEnumerable<Result> results = client.Audits.AddAnnotationMultiple(requests);
      Result resultItem = results.ElementAt(0);
      Console.WriteLine(resultItem);
      /*
      {
        "Id": "e0fb025a-d8a2-4258-91ea-c4026c1620d1",
        "Status": 201,
        "Annotation": "THIS IS THE FIRST AUDIT ANNOTATION"
      }            /*
      */
      /* SNIPPET 6: END */
    }
    private void DiscardAuditMultiple()
    {

      /* SNIPPET 7: START */
      var requests = new List<BatchRequestParam>();
      BatchRequestParam request1 = new BatchRequestParam { ObjectId = new Guid("030d3998-3145-44d9-b378-886e52d373c1"), Resource = "THIS IS THE FIRST DISCARD ANNOTATION" };
      requests.Add(request1);
      BatchRequestParam request2 = new BatchRequestParam { ObjectId = new Guid("181a6677-ccbd-4b97-9438-5b331ffd38c2"), Resource = "THIS IS THE SECOND DISCARD ANNOTATION" };
      requests.Add(request2);

      IEnumerable<Result> results = client.Audits.DiscardMultiple(requests);
      Result resultItem = results.ElementAt(0);
      Console.WriteLine(resultItem);
      /*
      {
        "Id": "e0fb025a-d8a2-4258-91ea-c4026c1620d1",
        "Status": 204,
        "Annotation": "THIS IS THE FIRST DISCARD ANNOTATION"
      }
      */
      /* SNIPPET 7: END */
    }
    private void GetServerTime()
    {
      /* SNIPPET 8: START */
      DateTime dateTime = client.GetServerTime();
      Console.WriteLine(dateTime.ToString());
      /*
       23/07/2020 17:06:33
      */
      /* SNIPPET 8: END */
    }

    #endregion

    public void Run()
    {
      try
      {
        /**** Common vars shared between different snippets: modify the following input according to your testing server *****/
        Guid objectId = new Guid("8e3b3738-2f5f-494d-bde1-fac15da28c86");
        Guid id1 = objectId;
        Guid id2 = new Guid("01e025e8-0fb3-59da-a9b8-2f238c6f011c");

        // Common objects are mocked to do not waste precious roundtrip time on each snippet
        List<Command> commands = new List<Command> { new Command { CommandId = "Adjust" }, new Command { CommandId = "OperatorOverride" }, new Command(), new Command(), new Command { CommandId = "Release" } };

        Guid equipmentId = new Guid("2f321415-35d9-5c48-b643-60c5132cc001");

        Guid spaceId = new Guid("78c022db-f109-5a0c-86e7-d0d7d3210ea7");

        MetasysObject building = new MetasysObject { Id = new Guid("2f321415-35d9-5c48-b643-60c5132cc001") };

        MetasysObject sampleEquipment = new MetasysObject { Id = new Guid("0e8c034b-1586-5ed9-8455-dd33e99c2c7e") };

        MetasysObject device = new MetasysObject { Id = new Guid("21c605fb-4755-5d65-8e9f-4fc8283b0366") };

        AlarmFilter alarmFilter = new AlarmFilter
        {
          StartTime = new DateTime(2020, 06, 01),
          EndTime = new DateTime(2020, 09, 30),
          ExcludeAcknowledged = true
        };

        Alarm alarm = new Alarm { Id = new Guid("d66cf3da-529d-4a81-9479-04834c7a2209") };

        AuditFilter auditFilter = new AuditFilter
        {
          StartTime = new DateTime(2020, 06, 01),
          EndTime = new DateTime(2020, 06, 24),
          OriginApplications = OriginApplicationsEnum.SystemSecurity | OriginApplicationsEnum.AuditTrails,
          ActionTypes = ActionTypeEnum.Subsystem | ActionTypeEnum.Command
        };

        Audit audit = new Audit { Id = new Guid("f798591b-e6d6-441f-8f7b-596fc2e6c003") };

        Audit auditForAnnotation = new Audit { Id = new Guid("f179a59c-ce36-47e1-afa6-f873b80259ed") };

        /********************************************************************************************************************/
        var option = "";
        while (option != "99")
        {
          PrintMenu();
          Console.Write("\r\nSelect an option: ");
          option = Console.ReadLine();
          Console.WriteLine();
          switch (option)
          {
            case "1":
              TryLogin_Refresh();
              break;
            case "2":
              GetObjectIdentifier();
              break;
            case "3":
              ReadProperty(objectId);
              break;
            case "4":
              ReadPropertyMultiple(id1, id2);
              break;
            case "5":
              WriteProperty();
              break;
            case "6":
              WritePropertyMultiple(id1, id2);
              break;
            case "7":
              GetCommands(objectId);
              break;
            case "8":
              SendCommands(objectId, commands);
              break;
            case "9":
              GetNetworkDeviceTypes_GetNetworkDevices();
              break;
            case "10":
              GetObjects();
              break;
            case "11":
              Localize();
              break;
            case "12":
              GetSpaces();
              break;
            case "13":
              GetSpaceTypes();
              break;
            case "14":
              GetEquipment();
              break;
            case "15":
              GetSpaceEquipment_GetSpaceChildren(spaceId);
              break;
            case "16":
              GetEquipmentPoints(sampleEquipment);
              break;
            case "17":
              GetAlarms();
              break;
            case "18":
              GetSingleAlarm_GetAlarmsForAnObject_GetAlarmsForNetworkDevice(alarm, objectId, alarmFilter, device);
              break;
            case "19":
              GetAlarmsAnnotation(alarm);
              break;
            case "20":
              GetTrendedAttributes_GetSamples();
              break;
            case "21":
              GetAudits();
              break;
            case "22":
              GetSingleAudit_GetAuditsForAnObject(audit, objectId, auditFilter);
              break;
            case "23":
              GetAuditsAnnotation(audit);
              break;
            case "24":
              DiscardSingleAudit(audit);
              break;
            case "25":
              ChangeApiVersion();
              break;
            case "26":
              ChangeHostname();
              break;
            case "27":
              AddAuditAnnotation(auditForAnnotation);
              break;
            case "28":
              AddAuditAnnotationMultiple();
              break;
            case "29":
              DiscardAuditMultiple();
              break;
            case "30":
              GetServerTime();
              break;
            case "99":
              return; // Exit from JSON output demo
          }
          Console.WriteLine();
          Console.WriteLine("Press Enter to return to the JSON output menu...");
          Console.ReadLine();
          Console.WriteLine();
        }
      }
      catch (Exception exception)
      {
        log.Logger.Error(string.Format("{0}", exception.Message));
        Console.WriteLine("\n \nAn Error occurred. Press Enter to return to Main Menu");
        Console.ReadLine();
      }
    }

    private static void PrintMenu()
    {
      Console.Clear();
      Console.WriteLine("Choose an option:");
      Console.WriteLine("1) TryLogin, Refresh");
      Console.WriteLine("2) GetObjectIdentifier");
      Console.WriteLine("3) ReadProperty");
      Console.WriteLine("4) ReadPropertyMultiple");
      Console.WriteLine("5) WriteProperty");
      Console.WriteLine("6) WritePropertyMultiple");
      Console.WriteLine("7) GetCommands");
      Console.WriteLine("8) SendCommand");
      Console.WriteLine("9) GetNetworkDeviceTypes, GetNetworkDevices");
      Console.WriteLine("10) GetObjects");
      Console.WriteLine("11) Localize");
      Console.WriteLine("12) GetSpaces");
      Console.WriteLine("13) GetSpaceTypes");
      Console.WriteLine("14) GetEquipment");
      Console.WriteLine("15) GetSpaceEquipment, GetSpaceChildren");
      Console.WriteLine("16) GetEquipmentPoints");
      Console.WriteLine("17) GetAlarms");
      Console.WriteLine("18) GetSingleAlarm, GetAlarmsForAnObject, GetAlarmsForNetworkDevice");
      Console.WriteLine("19) GetAlarmAnnotations");
      Console.WriteLine("20) GetTrendedAttributes, GetSamples");
      Console.WriteLine("21) GetAudits");
      Console.WriteLine("22) GetSingleAudit, GetAuditsForAnObject");
      Console.WriteLine("23) GetAuditAnnotations");
      Console.WriteLine("24) DiscardAudit");
      Console.WriteLine("25) ChangeApiVersion");
      Console.WriteLine("26) ChangeHostname");
      Console.WriteLine("27) AddAuditAnnotation");
      Console.WriteLine("28) AddAuditAnnotationMultiple");
      Console.WriteLine("29) DiscardAuditMultiple");
      Console.WriteLine("30) GetServerTime");
      Console.WriteLine("99) Exit");
    }
  }
}
