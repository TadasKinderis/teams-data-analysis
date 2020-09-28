using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace teams_data_analysis
{
    class Program
    {
        private static readonly Dictionary<string, List<string>> _memberToGroup = new Dictionary<string, List<string>>();
        private static readonly Dictionary<string, List<string>> _groupToMember = new Dictionary<string, List<string>>();
        private static List<HashSet<string>> _memberConnections = new List<HashSet<string>>();

        static void Main()
        {
            LogMessage("Program start.");
            string currentDir = Directory.GetCurrentDirectory();
            string fileName = Path.Combine(currentDir, "Data.csv");
            if (!File.Exists(fileName))
            {
                LogMessage($"Data File not found. Press any key to exit..");
                Console.ReadKey();
                return;
            }

            LogMessage($"Data File found.");

            LoadFile(fileName);
            LogMessage("Loaded data from file.");

            var notConnectedUsers = new HashSet<string>(_memberToGroup.Keys);
            var result = new List<HashSet<string>>();
            while (notConnectedUsers.Count > 1)
            {
                string initialUser = notConnectedUsers.FirstOrDefault();
                GetConnections(new HashSet<string>() { initialUser }, new HashSet<string>() { });

                var connectedUsers = new HashSet<string>();
                foreach (var set in _memberConnections)
                {
                    notConnectedUsers.ExceptWith(set);
                    connectedUsers.UnionWith(set);
                }
                LogMessage($"{result.Count} Group found - {connectedUsers.Count} members");

                result.Add(connectedUsers);
                _memberConnections = new List<HashSet<string>>();
            }

            var csv = new StringBuilder();
            csv.Append("Member;Group").Append(Environment.NewLine);
            for (int i = 0; i < result.Count; i++)
            {
                foreach (var member in result[i])
                {
                    csv.Append($"{member};{i}").Append(Environment.NewLine);
                }
            }

            string resultFileName = Path.Combine(currentDir, $"Report {DateTime.Now:yyyyMMddHHmmss}.csv");
            using (var writer = new StreamWriter(resultFileName))
            {
                writer.Write(csv);
                writer.Flush();
                writer.Close();
            }

            LogMessage($"Detailed info written to {resultFileName}");
            LogMessage("Program finished. Press any key to exit..");
            Console.ReadKey();
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now:hh:mm:ss.FFF}\t{message}");
        }

        private static void LoadFile(string fileName)
        {
            using (var file = new StreamReader(fileName))
            {
                file.ReadLine(); // skip first line
                string line = file.ReadLine();
                while (line != null)
                {
                    string[] splitLine = line.Split(';', 2);
                    AddToDict(_groupToMember, splitLine[0], splitLine[1]);
                    AddToDict(_memberToGroup, splitLine[1], splitLine[0]);
                    line = file.ReadLine();
                }
            }
        }

        private static void AddToDict(Dictionary<string, List<string>> dict, string key, string value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(value);
            }
            else
            {
                dict.Add(key, new List<string>() { value });
            }
        }

        private static HashSet<string> GetConnections(HashSet<string> data, HashSet<string> currentMembers)
        {
            var nextIterationData = new HashSet<string>();
            foreach (var member in data)
            {
                foreach (var group in _memberToGroup[member])
                {
                    var memberConnections = _groupToMember[group].ToHashSet();
                    memberConnections.ExceptWith(currentMembers);
                    nextIterationData.UnionWith(memberConnections);
                }
            }
            currentMembers.UnionWith(nextIterationData);
            if (nextIterationData.Count > 0)
            {
                _memberConnections.Add(nextIterationData);
                return GetConnections(nextIterationData, currentMembers);
            }
            return nextIterationData;
        }
    }
}
