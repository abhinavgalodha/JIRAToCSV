using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO;

namespace Experis.JIRA
{
    class Program
    {
        static void Main()
        {
            //Console.WriteLine("JIRA TO EXCELL");
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine("Enter the URL of JIRA Server");
            //string jiraURL = Console.ReadLine();
            //Console.WriteLine("Enter the Project  name");
            //string projectName = Console.ReadLine();
            //Console.WriteLine("Enter the Login/password info");
            //string loginInfo = Console.ReadLine();
            //Console.WriteLine("Enter the Date range ");
            //string dateRange = Console.ReadLine();
            //Console.WriteLine("Enter the CSV Save location ");
            //string csvlocation = Console.ReadLine();

            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://jira.atlassian.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                //HttpResponseMessage response = await client.GetAsync("rest/api/latest/project");
                HttpResponseMessage response = await client.GetAsync("rest/api/latest/search?jql=project=SRCTREE");
                if (response.IsSuccessStatusCode)
                {
                    string test = await response.Content.ReadAsStringAsync();
                    try
                    {
                        //dynamic data = Json.Decode(test);
                        //Object o = JObject.Parse(test);
                        var ab = JsonConvert.DeserializeObject<Rootobject>(test);

                        var resultlist = ab.issues.Select(x => x.fields);

                        StringBuilder header = new StringBuilder();
                        header.Append("IssueID,");
                        var listOfProperties = typeof(Fields).GetProperties();
                        var listOfPropertiesOfIssue = listOfProperties.Select(x => x.Name);

                        header.Append(string.Join(",", listOfPropertiesOfIssue.ToList()));
                        header.Append(Environment.NewLine);

                        StringBuilder values = new StringBuilder();

                        foreach (var issue in ab.issues)
                        {
                            values.Append(issue.id).Append(",");
                            foreach (var item in listOfProperties)
                            {
                                var currentPropertyName = item.Name;
                                var valueOfProperty = typeof(Fields).GetProperty(currentPropertyName).GetValue(issue.fields);
                                if (item.PropertyType.IsArray)
                                {
                                    if (valueOfProperty != null && ((Array)valueOfProperty).Length > 0)
                                    {
                                        StringBuilder concatenatedListValue = new StringBuilder();
                                        for (int i = 0; i < ((Array)valueOfProperty).Length; i++)
                                        {
                                            concatenatedListValue = new StringBuilder();
                                            concatenatedListValue.Append("{");
                                            var valueOfItem = ((Array)valueOfProperty).Cast<object>().ToList()[i].ToString();
                                            concatenatedListValue.Append(valueOfItem);
                                            concatenatedListValue.Append("}");
                                            if (i != ((Array)valueOfProperty).Length - 1)
                                            {
                                                concatenatedListValue.Append(";");
                                            }
                                        }
                                        string concatenatedListValueHandlingNewLine = concatenatedListValue.ToString();
                                        if (concatenatedListValueHandlingNewLine.Contains(Environment.NewLine))
                                        {
                                            concatenatedListValueHandlingNewLine = concatenatedListValueHandlingNewLine.ToString().Replace("\"", "");
                                            concatenatedListValueHandlingNewLine = "\"" + concatenatedListValueHandlingNewLine + "\"";
                                        }
                                        values.Append(concatenatedListValueHandlingNewLine);
                                    }
                                }
                                else
                                {
                                    if (valueOfProperty!= null &&  valueOfProperty.ToString().Contains(Environment.NewLine))
                                    {
                                        valueOfProperty = valueOfProperty.ToString().Replace("\"", "");
                                        valueOfProperty = "\"" + valueOfProperty + "\"";
                                    }
                                    values.Append(valueOfProperty);
                                }

                                //bool isCollection = valueOfProperty is IEnumerable<object>;
                                //if (isCollection)
                                //{
                                //    if (valueOfProperty != null && ((List<object>)valueOfProperty).Count > 0)
                                //    {
                                //        if (item.Name == "")
                                //        {

                                //        }
                                //        var concatenatedValue = ((List<object>)valueOfProperty).Aggregate((totalValue, next) => next + " ; " + totalValue);
                                //        values.Append("\"");
                                //        values.Append(concatenatedValue);
                                //        values.Append("\"");

                                //    }
                                //}
                                //else
                                //{
                                //    values.Append(valueOfProperty);
                                //}
                                values.Append(",");
                            }
                            values.AppendLine(Environment.NewLine);
                        }
                        header.Append(values.ToString());
                        File.WriteAllText(@"c:\temp\1.csv", header.ToString());
                    }

                    catch (Exception ex)
                    {
                            
                    }                
                    
                }

            }
        }
    }


    //public class Issuetype
    //{
    //    public string self { get; set; }
    //    public string id { get; set; }
    //    public string description { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public bool subtask { get; set; }
    //    public int avatarId { get; set; }

    //    public override string ToString()
    //    {
    //        return name; 
    //    }
    //}

    //public class AvatarUrls
    //{
    //    public string __invalid_name__48x48 { get; set; }
    //    public string __invalid_name__24x24 { get; set; }
    //    public string __invalid_name__16x16 { get; set; }
    //    public string __invalid_name__32x32 { get; set; }
    //}

    //public class Project
    //{
    //    public string self { get; set; }
    //    public string id { get; set; }
    //    public string key { get; set; }
    //    public string name { get; set; }
    //    public AvatarUrls avatarUrls { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Resolution
    //{
    //    public string self { get; set; }
    //    public string id { get; set; }
    //    public string description { get; set; }
    //    public string name { get; set; }

    //    public override string ToString()
    //    {
    //        return description;
    //    }
    //}

    //public class Watches
    //{
    //    public string self { get; set; }
    //    public int watchCount { get; set; }
    //    public bool isWatching { get; set; }

    //    public override string ToString()
    //    {
    //        return self;
    //    }
    //}

    //public class Priority
    //{
    //    public string self { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public string id { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class AvatarUrls2
    //{
    //    public string __invalid_name__48x48 { get; set; }
    //    public string __invalid_name__24x24 { get; set; }
    //    public string __invalid_name__16x16 { get; set; }
    //    public string __invalid_name__32x32 { get; set; }
    //}

    //public class Assignee
    //{
    //    public string self { get; set; }
    //    public string name { get; set; }
    //    public string key { get; set; }
    //    public string emailAddress { get; set; }
    //    public AvatarUrls2 avatarUrls { get; set; }
    //    public string displayName { get; set; }
    //    public bool active { get; set; }
    //    public string timeZone { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class StatusCategory
    //{
    //    public string self { get; set; }
    //    public int id { get; set; }
    //    public string key { get; set; }
    //    public string colorName { get; set; }
    //    public string name { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Status
    //{
    //    public string self { get; set; }
    //    public string description { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public string id { get; set; }
    //    public StatusCategory statusCategory { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class AvatarUrls3
    //{
    //    public string __invalid_name__48x48 { get; set; }
    //    public string __invalid_name__24x24 { get; set; }
    //    public string __invalid_name__16x16 { get; set; }
    //    public string __invalid_name__32x32 { get; set; }
    //}

    //public class Creator
    //{
    //    public string self { get; set; }
    //    public string name { get; set; }
    //    public string key { get; set; }
    //    public string emailAddress { get; set; }
    //    public AvatarUrls3 avatarUrls { get; set; }
    //    public string displayName { get; set; }
    //    public bool active { get; set; }
    //    public string timeZone { get; set; }



    //    public override string ToString()
    //    {
    //        return name; 
    //    }
    //}

    //public class AvatarUrls4
    //{
    //    public string __invalid_name__48x48 { get; set; }
    //    public string __invalid_name__24x24 { get; set; }
    //    public string __invalid_name__16x16 { get; set; }
    //    public string __invalid_name__32x32 { get; set; }
    //}

    //public class Reporter
    //{
    //    public string self { get; set; }
    //    public string name { get; set; }
    //    public string key { get; set; }
    //    public string emailAddress { get; set; }
    //    public AvatarUrls4 avatarUrls { get; set; }
    //    public string displayName { get; set; }
    //    public bool active { get; set; }
    //    public string timeZone { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Aggregateprogress
    //{
    //    public int progress { get; set; }
    //    public int total { get; set; }
    //    public int percent { get; set; }
    //}

    //public class Progress
    //{
    //    public int progress { get; set; }
    //    public int total { get; set; }
    //    public int percent { get; set; }
    //}

    //public class Votes
    //{
    //    public string self { get; set; }
    //    public int votes { get; set; }
    //    public bool hasVoted { get; set; }
    //}

    //public class StatusCategory2
    //{
    //    public string self { get; set; }
    //    public int id { get; set; }
    //    public string key { get; set; }
    //    public string colorName { get; set; }
    //    public string name { get; set; }
    //}

    //public class Status2
    //{
    //    public string self { get; set; }
    //    public string description { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public string id { get; set; }
    //    public StatusCategory2 statusCategory { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Priority2
    //{
    //    public string self { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public string id { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Issuetype2
    //{
    //    public string self { get; set; }
    //    public string id { get; set; }
    //    public string description { get; set; }
    //    public string iconUrl { get; set; }
    //    public string name { get; set; }
    //    public bool subtask { get; set; }
    //    public int avatarId { get; set; }

    //    public override string ToString()
    //    {
    //        return name;
    //    }
    //}

    //public class Fields2
    //{
    //    public string summary { get; set; }
    //    public Status2 status { get; set; }
    //    public Priority2 priority { get; set; }
    //    public Issuetype2 issuetype { get; set; }

    //    public override string ToString()
    //    {
    //        return summary;
    //    }
    //}

    //public class Parent
    //{
    //    public string id { get; set; }
    //    public string key { get; set; }
    //    public string self { get; set; }
    //    public Fields2 fields { get; set; }
    //}

    //public class Fields
    //{
    //    public Issuetype issuetype { get; set; }
    //    public object timespent { get; set; }
    //    public Project project { get; set; }
    //    public List<object> fixVersions { get; set; }
    //    public object aggregatetimespent { get; set; }
    //    public Resolution resolution { get; set; }
    //    public string resolutiondate { get; set; }
    //    public int workratio { get; set; }
    //    public object lastViewed { get; set; }
    //    public Watches watches { get; set; }
    //    public string created { get; set; }
    //    public Priority priority { get; set; }
    //    public List<object> labels { get; set; }
    //    public int? aggregatetimeoriginalestimate { get; set; }
    //    public int? timeestimate { get; set; }
    //    public List<object> versions { get; set; }
    //    public List<object> issuelinks { get; set; }
    //    public Assignee assignee { get; set; }
    //    public string updated { get; set; }
    //    public Status status { get; set; }
    //    public List<object> components { get; set; }
    //    public int? timeoriginalestimate { get; set; }
    //    public string description { get; set; }
    //    public int? aggregatetimeestimate { get; set; }
    //    public string summary { get; set; }
    //    public Creator creator { get; set; }
    //    public List<object> subtasks { get; set; }
    //    public Reporter reporter { get; set; }
    //    public Aggregateprogress aggregateprogress { get; set; }
    //    public string environment { get; set; }
    //    public object duedate { get; set; }
    //    public Progress progress { get; set; }
    //    public Votes votes { get; set; }
    //    public Parent parent { get; set; }
    //}

    //public class Issue
    //{
    //    public string expand { get; set; }
    //    public string id { get; set; }
    //    public string self { get; set; }
    //    public string key { get; set; }
    //    public Fields fields { get; set; }
    //}

    //public class RootObject
    //{
    //    public string expand { get; set; }
    //    public int startAt { get; set; }
    //    public int maxResults { get; set; }
    //    public int total { get; set; }
    //    public List<Issue> issues { get; set; }
    //}


    //public static class ExtensionMethods
    //{
    //    public static string ToCSV(this DataTable table, string delimator)
    //    {
    //        var result = new StringBuilder();
    //        for (int i = 0; i < table.Columns.Count; i++)
    //        {
    //            result.Append(table.Columns[i].ColumnName);
    //            result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
    //        }
    //        foreach (DataRow row in table.Rows)
    //        {
    //            for (int i = 0; i < table.Columns.Count; i++)
    //            {
    //                result.Append(row[i].ToString());
    //                result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
    //            }
    //        }
    //        return result.ToString().TrimEnd(new char[] { '\r', '\n' });
    //        //return result.ToString();
    //    }


    //    public static DataTable ToDataTable<T>(this List<T> list)
    //    {
    //        var entityType = typeof(T);

    //        // Lists of type System.String and System.Enum (which includes enumerations and structs) must be handled differently 
    //        // than primitives and custom objects (e.g. an object that is not type System.Object).
    //        if (entityType == typeof(String))
    //        {
    //            var dataTable = new DataTable(entityType.Name);
    //            dataTable.Columns.Add(entityType.Name);

    //            // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
    //            foreach (T item in list)
    //            {
    //                var row = dataTable.NewRow();
    //                row[0] = item;
    //                dataTable.Rows.Add(row);
    //            }

    //            return dataTable;
    //        }
    //        else if (entityType.BaseType == typeof(Enum))
    //        {
    //            var dataTable = new DataTable(entityType.Name);
    //            dataTable.Columns.Add(entityType.Name);

    //            // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
    //            foreach (string namedConstant in Enum.GetNames(entityType))
    //            {
    //                var row = dataTable.NewRow();
    //                row[0] = namedConstant;
    //                dataTable.Rows.Add(row);
    //            }

    //            return dataTable;
    //        }

    //        // Check if the type of the list is a primitive type or not. Note that if the type of the list is a custom 
    //        // object (e.g. an object that is not type System.Object), the underlying type will be null.
    //        var underlyingType = Nullable.GetUnderlyingType(entityType);
    //        var primitiveTypes = new List<Type>
    //{
    //    typeof (Byte),
    //    typeof (Char),
    //    typeof (Decimal),
    //    typeof (Double),
    //    typeof (Int16),
    //    typeof (Int32),
    //    typeof (Int64),
    //    typeof (SByte),
    //    typeof (Single),
    //    typeof (UInt16),
    //    typeof (UInt32),
    //    typeof (UInt64),
    //};

    //        var typeIsPrimitive = primitiveTypes.Contains(underlyingType);

    //        // If the type of the list is a primitive, perform a simple conversion.
    //        // Otherwise, map the object's properties to columns and fill the cells with the properties' values.
    //        if (typeIsPrimitive)
    //        {
    //            var dataTable = new DataTable(underlyingType.Name);
    //            dataTable.Columns.Add(underlyingType.Name);

    //            // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
    //            foreach (T item in list)
    //            {
    //                var row = dataTable.NewRow();
    //                row[0] = item;
    //                dataTable.Rows.Add(row);
    //            }

    //            return dataTable;
    //        }
    //        else
    //        {
    //            // TODO:
    //            // 1. Convert lists of type System.Object to a data table.
    //            // 2. Handle objects with nested objects (make the column name the name of the object and print "system.object" as the value).

    //            var dataTable = new DataTable(entityType.Name);
    //            var propertyDescriptorCollection = TypeDescriptor.GetProperties(entityType);

    //            // Iterate through each property in the object and add that property name as a new column in the data table.
    //            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
    //            {
    //                // Data tables cannot have nullable columns. The cells can have null values, but the actual columns themselves cannot be nullable.
    //                // Therefore, if the current property type is nullable, use the underlying type (e.g. if the type is a nullable int, use int).
    //                var propertyType = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType) ?? propertyDescriptor.PropertyType;
    //                dataTable.Columns.Add(propertyDescriptor.Name, propertyType);
    //            }

    //            // Iterate through each object in the list adn add a new row in the data table.
    //            // Then iterate through each property in the object and add the property's value to the current cell.
    //            // Once all properties in the current object have been used, add the row to the data table.
    //            foreach (T item in list)
    //            {
    //                var row = dataTable.NewRow();

    //                foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
    //                {
    //                    var value = propertyDescriptor.GetValue(item);
    //                    row[propertyDescriptor.Name] = value ?? DBNull.Value;
    //                }

    //                dataTable.Rows.Add(row);
    //            }

    //            return dataTable;
    //        }
    //    }
    //}


}