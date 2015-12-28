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
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("JIRA TO CSV");
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Enter the URL of JIRA Server (e.g. https://jira.atlassian.com/):");
                string jiraURL = Console.ReadLine();
                ValidateURL(jiraURL);
                Console.WriteLine();
                
                Console.Write("Enter the Login/password info. Leave blank for Anonymous Authentication, Enter Username:");
                string userName = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Password:");
                string password = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Enter the Date range. (e.g. 2015-01-01; Leave empty for no range.");
                Console.WriteLine();
                Console.Write("Start Date:");
                string startDate = Console.ReadLine();
                Console.WriteLine();
                Console.Write("End Date:");
                string endDate = Console.ReadLine();

                DateTime startDateTime;
                DateTime endDateTime;
                ValidateDateRange(startDate, endDate, out startDateTime, out endDateTime);

                Console.WriteLine();
                Console.Write("Enter the CSV Save location (e.g. c:\\temp\\issues.csv): ");
                string csvlocation = Console.ReadLine();
                Console.WriteLine();
                ValidatePath(csvlocation);
                
                Console.Write("Enter the Project  name:");
                string projectName = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    Console.WriteLine("Please wait...");
                    RunAsync(jiraURL, projectName, userName, password, startDateTime, endDateTime, csvlocation).Wait();
                }
                catch (AggregateException agx)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception");
                    Console.WriteLine(agx.Flatten());
                }
            }
            catch (ArgumentException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception occured. Details" + ex);
            }
            Console.ReadLine();
        }

        private static void ValidatePath(string csvlocation)
        {
                if (!String.IsNullOrWhiteSpace(csvlocation))
                {
                    string directoryPath = Path.GetDirectoryName(csvlocation);
                    if (!Directory.Exists(directoryPath))
                    {
                        throw new ArgumentException("Please enter a valid 'Directory' location for CSV file");
                    }
                }
        }

        private static void ValidateDateRange(string startDate, string endDate, out DateTime startDateTime, out DateTime endDateTime)
        {
            startDateTime = DateTime.MinValue;
            endDateTime = DateTime.MinValue;

            if (! String.IsNullOrWhiteSpace(startDate) &&
                ! String.IsNullOrWhiteSpace(endDate))
            {
                bool isvalidDateStart = DateTime.TryParse(startDate, out startDateTime);
                bool isvalidDateEnd = DateTime.TryParse(endDate, out endDateTime);

                if (!isvalidDateStart)
                {
                    throw new ArgumentException("Invalid Start Date. Please enter correct start date");
                }

                if (!isvalidDateEnd)
                {
                    throw new ArgumentException("Invalid End Date. Please enter correct end date");
                }
            }
        }

        private static void ValidateURL(string jiraURL)
        {
            bool isValidURI = Uri.IsWellFormedUriString(jiraURL, UriKind.Absolute);
            if (!isValidURI)
            {
                throw new ArgumentException("Please enter a valid URL");
            }
        }

        static async Task RunAsync(string jiraURL, string projectName, string userName, string password, DateTime startDateTime, DateTime endDateTime, string csvlocation)
        {
            var mergedCredentials = string.Format("{0}:{1}", userName, password);
            var byteCredentials = Encoding.UTF8.GetBytes(mergedCredentials);
            var encodedCredentials = Convert.ToBase64String(byteCredentials);

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri("https://jira.atlassian.com/");
                client.BaseAddress = new Uri(jiraURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!String.IsNullOrWhiteSpace(userName) && !String.IsNullOrWhiteSpace(password))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
                }
               
                // HTTP GET
                HttpResponseMessage responseProjects = await client.GetAsync("rest/api/latest/project");
                if (responseProjects.IsSuccessStatusCode)
                {
                    var projects = await responseProjects.Content.ReadAsStringAsync();
                    var listProjects = JsonConvert.DeserializeObject<List<RootobjectProject>>(projects);
                    var project = listProjects.FirstOrDefault(x => x.name.ToUpper() == projectName.ToUpper());
                    string projectKey = string.Empty;
                    if (project != null)
                    {
                        projectKey = project.key;

                        string query = "rest/api/latest/search?jql=project=" + projectKey;
                        if (startDateTime != DateTime.MinValue && endDateTime != DateTime.MinValue)
                        {
                            string dateQuery = "AND created >= \"" + startDateTime.ToString("yyyy-MM-dd") + " \" AND created <= \"" + endDateTime.ToString("yyyy-MM-dd") + "\"";
                            query = query + " " +dateQuery;
                        }

                        HttpResponseMessage response = await client.GetAsync(query);
                        if (response.IsSuccessStatusCode)
                        {
                            string issuesJson = await response.Content.ReadAsStringAsync();
                            try
                            {
                                var issueObjectList = JsonConvert.DeserializeObject<Rootobject>(issuesJson);

                                var resultlist = issueObjectList.issues.Select(x => x.fields);

                                StringBuilder header = new StringBuilder();
                                header.Append("IssueID,");


                                var listOfProperties = typeof(Fields).GetProperties();
                                var listOfPropertiesOfIssue = listOfProperties.Select(x => x.Name);

                                header.Append(string.Join(",", listOfPropertiesOfIssue.ToList()));
                                header.Append(Environment.NewLine);

                                StringBuilder values = new StringBuilder();

                                foreach (var issue in issueObjectList.issues)
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
                                            if (valueOfProperty != null && valueOfProperty.ToString().Contains(Environment.NewLine))
                                            {
                                                valueOfProperty = valueOfProperty.ToString().Replace("\"", "");
                                                valueOfProperty = "\"" + valueOfProperty + "\"";
                                            }
                                            values.Append(valueOfProperty);
                                        }
                                        values.Append(",");
                                    }
                                    values.AppendLine(Environment.NewLine);
                                }
                                header.Append(values.ToString());
                                File.WriteAllText(csvlocation, header.ToString());
                                Console.WriteLine("File Created sucessfully at " + csvlocation);
                            }

                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("A Problem occured while connecting to server" + ex);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("A Problem occured while connecting to server. Status Code:" + responseProjects.StatusCode + " .Reason Phrase: " + responseProjects.ReasonPhrase);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No Project found with Name ' " +  projectName + "'" );
                    }

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("A Problem occured while connecting to server. Status Code:" + responseProjects.StatusCode + " .Reason Phrase: " + responseProjects.ReasonPhrase);
                }

            }
        }
    }


}