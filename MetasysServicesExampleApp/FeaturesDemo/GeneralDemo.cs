using JohnsonControls.Metasys.BasicServices;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MetasysServicesExampleApp.FeaturesDemo
{
    public class GeneralDemo
    {
        private MetasysClient client;
        private LogInitializer log;

        public GeneralDemo(MetasysClient client)
        {
            this.client = client;
            log = new LogInitializer(typeof(GeneralDemo));
        }
        public void Run()
        {
            try
            {
                #region GetObjectIdentifier
                Console.WriteLine("\nIndicate the object you want to run this example code on.");
                Console.Write("Enter the fully qualified reference of the object (Example: \"site:device/itemReference\"): ");
                string object1 = Console.ReadLine();

                Console.WriteLine("\nIndicate the second object you want to run this example code on.");
                Console.Write("Enter the fully qualified reference of the object: ");
                string object2 = Console.ReadLine();

                Console.WriteLine("\n\nGetObjectIdentifier...");

                // These variables are needed to run the other sections
                var id1 = client.GetObjectIdentifier(object1);
                Console.WriteLine($"{object1} id: {id1}");

                var id2 = client.GetObjectIdentifier(object2);
                Console.WriteLine($"{object2} id: {id2}");

                List<ObjectId> ids = [id1, id2];
                #endregion

                #region ReadProperty

                Console.WriteLine("\n\nReadProperty...");

                Console.Write("\n!!!Please note ReadProperty will return null if the attribute does not exist, and will cause an exception in this example!!!");
                Console.Write("\nEnter an attribute of the objects (Examples: name, description, presentValue): ");
                string attribute1 = Console.ReadLine();

                Console.Write("Enter a second attribute of the objects: ");
                string attribute2 = Console.ReadLine();

                List<string> attributes = new List<string>() { attribute1, attribute2 };

                Console.WriteLine($"Get {attribute1} property...");

                Variant result1Attr1 = client.ReadProperty(id1, attribute1);
                Variant result2Attr1 = client.ReadProperty(id2, attribute1);

                Console.WriteLine($"{result1Attr1.Id} {result1Attr1.Attribute} values (string, int, bool, reliability): " +
                    $"\n{result1Attr1.StringValue}, {result1Attr1.NumericValue}, {result1Attr1.BooleanValue}, {result1Attr1.Reliability}");
                Console.WriteLine($"{result2Attr1.Id} {result2Attr1.Attribute} values (string, int, bool, reliability): " +
                    $"\n{result2Attr1.StringValue}, {result2Attr1.NumericValue}, {result2Attr1.BooleanValue}, {result2Attr1.Reliability}");

                #endregion

                #region ReadPropertyMultiple

                Console.WriteLine("\n\nReadPropertyMultiple...");

                Console.WriteLine($"\nGet {attribute1}, {attribute2} properties for each object...");

                IEnumerable<VariantMultiple> results = client.ReadPropertyMultiple(ids, attributes);

                foreach (var varMultiple in results)
                {
                    // Grab the list of Variants for each id
                    var variants = varMultiple.Values;
                    foreach (var result in variants)
                    {
                        Console.WriteLine($"{result.Id} {result.Attribute} values (string, int, bool, reliability): " +
                            $"\n{result.StringValue}, {result.NumericValue},  {result.BooleanValue}, {result.Reliability}");
                    }
                }

                #endregion

                #region WriteProperty Methods

                Console.Write("\nEnter an attribute of the objects to change (don't worry, this will be reset to it's original value): ");
                string attribute3 = Console.ReadLine();

                Console.Write("\nEnter one new value for this attribute (this will be applied to both objects): ");
                object change = Console.ReadLine();

                // Get original values, this code will throw an exception if the
                Variant result1Attr3 = client.ReadProperty(id1, attribute3);
                Variant result2Attr3 = client.ReadProperty(id2, attribute3);
                Console.WriteLine($"{result1Attr3.Id} {result1Attr3.Attribute} original value: {result1Attr3.StringValue}");
                Console.WriteLine($"{result2Attr3.Id} {result2Attr3.Attribute} original value: {result2Attr3.StringValue}");

                Console.WriteLine("\nWriteProperty...");

                // Change value
                client.WriteProperty(id1, attribute3, change);
                System.Threading.Thread.Sleep(1000);

                // View changes
                Variant result1Attr3Updated = client.ReadProperty(id1, attribute3);
                Console.WriteLine($"{result1Attr3Updated.Id} {result1Attr3Updated.Attribute} updated value: {result1Attr3Updated.StringValue}");

                // Reset value
                client.WriteProperty(id1, attribute3, result1Attr3.StringValue);
                System.Threading.Thread.Sleep(1000);

                Console.WriteLine("\nWrite Property Multiple...");

                // Change value
                List<(string, object)> attributes2 = new List<(string, object)>() { (attribute3, change) };
                client.WritePropertyMultiple(ids, attributes2);
                System.Threading.Thread.Sleep(1000);

                // View changes
                Variant result1Attr3UpdatedM = client.ReadProperty(id1, attribute3);
                Variant result2Attr3UpdatedM = client.ReadProperty(id2, attribute3);
                Console.WriteLine($"{result1Attr3UpdatedM.Id} {result1Attr3UpdatedM.Attribute} updated value: {result1Attr3UpdatedM.StringValue}");
                Console.WriteLine($"{result2Attr3UpdatedM.Id} {result2Attr3UpdatedM.Attribute} updated value: {result2Attr3UpdatedM.StringValue}");
                System.Threading.Thread.Sleep(1000);

                // Reset Values
                client.WriteProperty(id1, attribute3, result1Attr3.StringValue);
                client.WriteProperty(id2, attribute3, result2Attr3.StringValue);

                #endregion

                #region Commands

                Console.WriteLine("\n\nGetCommands...");

                Console.WriteLine($"{id1} Commands:");

                IEnumerable<Command> commands = client.GetCommands(id1);

                if (commands != null && commands.Count() > 0)
                {
                    Console.WriteLine($"{commands.Count()}\n");
                    foreach (Command command in commands)
                    {
                        Console.WriteLine($"{command.ToString()}\n");
                    }
                }
                else
                {
                    Console.WriteLine("No commands found.");
                }


                Console.WriteLine("\nSendCommand...");

                Console.Write("\nPlease enter a commandId to execute: ");
                string cmd = Console.ReadLine();

                Console.WriteLine("\nEnter a number for a number type, enter the key for an enum type.");
                Console.WriteLine("Please enter the values for the command in order followed by an empty line: ");

                List<object> list = new List<object>() { };
                string option = "";
                do
                {
                    option = Console.ReadLine();
                    if (option != null && option.Length > 0)
                    {
                        list.Add(option);
                    }
                } while (option != null && option.Length > 0);

                Console.WriteLine("\nSending Command, please wait...");

                client.SendCommand(id1, cmd, list);
                System.Threading.Thread.Sleep(5000);

                Console.WriteLine("Sent successfully.");

                #endregion

                #region GetNetworkDevices

                Console.WriteLine("\n\nGetNetworkDevices...");

                IEnumerable<MetasysObjectType> types = client.GetNetworkDeviceTypes();
                foreach (var type in types)
                {
                    Console.WriteLine($"\nAvailable Type {type.Id}: {type.Description}, {type.DescriptionEnumerationKey}");
                    string typeId = type.Id.ToString();
                    IEnumerable<MetasysObject> devices = client.GetNetworkDevices(typeId);
                    Console.WriteLine($"Devices found: {devices.Count()}");
                    Console.WriteLine($"First Device: {devices.ElementAt(0).Name}");
                }

                #endregion

                #region GetObjects

                Console.WriteLine("\n\nGet Objects..");

                Console.WriteLine("\nPlease enter the depth of objects to retrieve for the first object.");
                Console.Write("(1 = only this object, 2 = immediate children only): ");

                int level = Convert.ToInt32(Console.ReadLine());
                IEnumerable<MetasysObject> objects = client.GetObjects(id2, level);
                if (objects.Count() > 0)
                {
                    MetasysObject obj = objects.ElementAt(0);

                    Console.WriteLine($"Parent object: {obj.Id}");

                    for (int i = 1; i < level; i++)
                    {
                        Console.WriteLine($"Child at level {i}: {obj.Id} - {obj.Name}");
                        if (objects.ElementAt(0).ChildrenCount > 0)
                        {
                            obj = objects.ElementAt(0).Children.ElementAt(0);
                        }
                        else
                        {
                            Console.WriteLine("This object has no children.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("This object has no children.");
                }
            }
            #endregion
            catch (Exception exception)
            {
                log.Logger.Error(string.Format("An error occured while getting general information - {0}", exception.Message));
                Console.WriteLine("\n \nAn Error occurred. Press Enter to return to Main Menu");
            }
            Console.ReadLine();
        }
    }
}
