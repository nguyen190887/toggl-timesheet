using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace TogglTimesheet.TimesheetGenerators
{
    public class TaskGenerator
    {
        private readonly List<TaskRule> _taskRules = new()
            {
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"interview\")", TaskName = TaskConstants.DC_Interview },
                new() { Expression = "description.StartsWith(\"DC\") && (description.Contains(\"productivity\") || description.Contains(\"initiatives\"))", TaskName = TaskConstants.DC_Productivity },
                new() { Expression = "description.StartsWith(\"DC\") && (description.Contains(\"propo\") || description.Contains(\"pro.\"))", TaskName = TaskConstants.DC_Proposal },
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"support\")", TaskName = TaskConstants.DC_Support },
                new() { Expression = "description.StartsWith(\"DC\") && description.Contains(\"iqbr\")", TaskName = TaskConstants.DC_IQRB },
                new() { Expression = "description.StartsWith(\"A -\") && description.Contains(\"night meeting\")", TaskName = TaskConstants.PRX_NightMeeting },
                new() { Expression = "description.StartsWith(\"A -\")", TaskName = TaskConstants.PRX_Tasks },
                new() { Expression = "description.StartsWith(\"Atd -\") && description.Contains(\"night meeting\")", TaskName = TaskConstants.ATD_NightMeeting },
                new() { Expression = "description.StartsWith(\"Atd -\")", TaskName = TaskConstants.ATD_Tasks },
                new() { Expression = "project == \"Self-Development\" || description.ContainsIgnoreCase(\"learning\")", TaskName = TaskConstants.Learning },
                new() { Expression = "project == \"Innovation\"", TaskName = TaskConstants.Innovation }
            };

        private static readonly ParsingConfig _parsingConfig = new()
        {
            CustomTypeProvider = new CustomTypeProvider(ParsingConfig.Default)
        };

        public TaskGenerator()
        {
            // Constructor logic here
        }

        public class TaskRule
        {
            public string Expression { get; set; } = string.Empty;
            public string TaskName { get; set; } = string.Empty;
        }

        public class TimesheetTask
        {
            public string Project { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
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

            var foundRule = _taskRules.FirstOrDefault(r => taskQuery.Any(_parsingConfig, r.Expression));
            return foundRule?.TaskName ?? TaskConstants.Unknown;
        }
    }

    public static class TaskConstants
    {
        public const string DC_Interview = "DC - Itv";
        public const string DC_Productivity = "DC - 15% Prd";
        public const string DC_Proposal = "DC - Pro.";
        public const string DC_Support = "DC - Support";
        public const string DC_IQRB = "DC - iQRB";
        public const string PRX_NightMeeting = "Prx - night meeting";
        public const string PRX_Tasks = "Prx - tasks";
        public const string ATD_NightMeeting = "Atd - night meeting";
        public const string ATD_Tasks = "Atd - tasks";
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