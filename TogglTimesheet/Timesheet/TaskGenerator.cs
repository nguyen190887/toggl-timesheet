using System.IO;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Text.Json;

namespace TogglTimesheet.Timesheet
{
    public class TaskGenerator : ITaskGenerator
    {
        private readonly List<TaskRule> _taskRules;

        private static readonly ParsingConfig _parsingConfig = new()
        {
            CustomTypeProvider = new CustomTypeProvider(ParsingConfig.Default)
        };

        public TaskGenerator(string jsonFilePath)
        {
            _taskRules = LoadTaskRulesFromJson(jsonFilePath);
        }

        public TaskGenerator(List<TaskRule> taskRules)
        {
            _taskRules = taskRules;
        }

        public class TimesheetTask
        {
            public string Project { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        private static List<TaskRule> LoadTaskRulesFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"The file {jsonFilePath} does not exist.");
            }

            var json = File.ReadAllText(jsonFilePath);
            return JsonSerializer.Deserialize<List<TaskRule>>(json) ?? [];
        }

        public string GenerateTask(string description, string project)
        {
            var taskQuery = new List<TimesheetTask>()
            {
                new()
                {
                    Description = description,
                    Project = project
                }
            }.AsQueryable();

            var foundRule = _taskRules.Find(r => taskQuery.Any(_parsingConfig, r.Expression));
            return foundRule?.TaskName ?? TaskConstants.Unknown;
        }
    }

    public class TaskRule
    {
        public string Expression { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
    }

    public static class TaskConstants
    {
        public const string DC_Itv = "DC - Itv";
        public const string DC_Prod = "DC - 15% Prd";
        public const string DC_Pro = "DC - Pro.";
        public const string DC_Support = "DC - Support";
        public const string DC_IQ = "DC - iQ";
        public const string P1_NM = "P1 - night meeting";
        public const string P1_Tasks = "P1 -tasks";
        public const string A1_NM = "A1 - night meeting";
        public const string A1_Tasks = "A1 - tasks";
        public const string Learning = "Learning";
        public const string Innovation = "Innovation";
        public const string Unknown = "Unknown";
    }

    public static class CollectionExtensions
    {
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        public CustomTypeProvider(ParsingConfig config, bool cacheCustomTypes = true)
            : base(config, cacheCustomTypes)
        {

        }

        public override HashSet<Type> GetCustomTypes()
        {
            var customTypes = base.GetCustomTypes();
            customTypes.Add(typeof(CollectionExtensions));
            return customTypes;
        }
    }
}
